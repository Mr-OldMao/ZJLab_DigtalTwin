using System.Collections.Generic;
using UnityEngine;
using static GenerateRoomBorderModel;
using MFramework;
using static GenerateRoomData;
using System;
using static GetThingGraph;
using static GetEnvGraph;
/// <summary>
/// 标题：生成各个房间内所有道具(床桌椅板凳等)
/// 功能：针对每个房间生成合理的道具，摆放位置随机且合理(1.各个房间内部物品都朝向当前房间的中心点)
/// 作者：毛俊峰
/// 时间：2023.07.31
/// </summary>
public class GenerateRoomItemModel : SingletonByMono<GenerateRoomItemModel>
{
    //各个房间的可放置坐标节点  v-各个坐标点 放置信息
    private Dictionary<RoomType, List<ItemModelInfo>> m_DicItemModelInfo;

    /// <summary>
    /// 是否限制门周边两米位置放置道具
    /// </summary>
    private bool m_IsLimitPutPos = false;

    public Transform ItemEntityGroupNode { get; set; }

    /// <summary>
    /// 坐标点的放置信息
    /// </summary>
    public class ItemModelInfo
    {
        public Vector2 pos;
        /// <summary>
        /// 当前坐标相对于当前房间的相对位置
        /// </summary>
        public DirEnum posDir;
        /// <summary>
        /// 当前坐标放置的物体,Null则表示当前当前位置未放置物体
        /// </summary>
        public string itemModelType;
    }
    /// <summary>
    /// 物品依赖关系，用于约束物品放置的位置
    /// 例如，当前物品为食物，所依赖物品为锅，空间关系为in，则表示食物放置在锅中。反之，无依赖物品，则表示可放置在当前房间任何位置
    /// </summary>
    public class ItemDependInfo
    {
        ///// <summary>
        ///// 当前物品实体名称
        ///// </summary>
        //public string itemName;
        ///// <summary>
        ///// 当前物品实体ID
        ///// </summary>
        //public string itemID;
        /// <summary>
        /// 是否有依赖关系，为false则以下字段无意义
        /// </summary>
        public bool isDepend;
        /// <summary>
        /// 所依赖的物品实体名称
        /// </summary>
        public string dependItemName;
        /// <summary>
        /// 所依赖的物品实体ID 唯一标识
        /// </summary>
        public string dependItemID;
        /// <summary>
        /// 所依赖的物品的空间位置关系
        /// </summary>
        public PosRelation posRelation;
    }
    /// <summary>
    /// 物体空间关系
    /// </summary>
    public enum PosRelation
    {
        Null = 0,
        In,
        On,
        Below,
        Above
    }

    private int GetDefaultItemIDIndex;
    /// <summary>
    /// 获取各个房间默认物品的ID
    /// </summary>
    private string GetDefaultItemID
    {
        get
        {
            GetDefaultItemIDIndex++;
            return "sim:" + GetDefaultItemIDIndex;
        }
    }

    private void Awake()
    {
        ItemEntityGroupNode = GameObject.Find("ItemEntityGroupNode")?.transform;
        if (ItemEntityGroupNode == null)
        {
            ItemEntityGroupNode = new GameObject("ItemEntityGroupNode").transform;
        }
        //ItemEntityGroupNode.parent = GameLogic.GetInstance.staticModelRootNode?.transform;
    }

    /// <summary>
    /// 生成房间内道具
    /// </summary>
    /// <param name="roomInfos">所有房间边界信息</param>
    /// <param name="getThingGraph">物体与房间的邻接关系数据</param>
    public void GenerateRoomItem(List<RoomInfo> roomInfos, List<GetThingGraph_data_items> getThingGraph = null)
    {
        GetDefaultItemIDIndex = 1000;

        /*根据边界信息找到各个房间的可放置坐标节点，屏蔽"门"模型前后坐标节点，避免物体堵门*/
        ChcheItemModelTypeInfo(roomInfos);

        MainData.CacheItemsEntity.Clear();
        ClearItemEntity();

        if (MainData.CanReadFile || !MainData.UseTestData)
        {
            //根据服务器数据设置各个房间实体物品，位置随机
            SetRandomRoomInsideItemEntity(getThingGraph);
        }

        if (!MainData.CanReadFile)
        {
            //在每个房间天花板中心放置灯
            SetToplampInsideItemEnity();
        }

        ////在每个房间天花板中心放置灯
        //SetToplampInsideItemEnity();


        if (MainData.UseTestData)
        {
            //设置各个房间默认的实体物品，位置随机
            SetDefaultRoomInsideItemEntity();
        }
    }

    /// <summary>
    /// 缓存各个房间的坐标信息
    /// </summary>
    private void ChcheItemModelTypeInfo(List<RoomInfo> roomInfos)
    {
        m_DicItemModelInfo = new Dictionary<RoomType, List<ItemModelInfo>>();
        List<Vector2> doorPosArr = new List<Vector2>();
        for (int i = 0; i < roomInfos?.Count; i++)
        {
            if (!m_DicItemModelInfo.ContainsKey(roomInfos[i].roomType))
            {
                m_DicItemModelInfo.Add(roomInfos[i].roomType, new List<ItemModelInfo>());
            }
            Vector2 centerPos = new Vector2(roomInfos[i].roomPosMax.x - roomInfos[i].roomPosMin.x, roomInfos[i].roomPosMax.y - roomInfos[i].roomPosMin.y);
            for (int j = (int)roomInfos[i].roomPosMin.x; j <= (int)roomInfos[i].roomPosMax.x; j++)
            {
                for (int k = (int)roomInfos[i].roomPosMin.y; k <= (int)roomInfos[i].roomPosMax.y; k++)
                {
                    Vector2 targetPos = new Vector2(j, k);
                    if (doorPosArr.Contains(targetPos))
                    {
                        Debugger.Log("屏蔽掉门周边的放置信息，门口不可放置物体 targetPos:" + targetPos);
                        continue;
                    }

                    ItemModelInfo itemModelInfo = new ItemModelInfo
                    {
                        pos = targetPos,
                        itemModelType = string.Empty,
                        posDir = GetPosDir(targetPos, centerPos)
                    };
                    m_DicItemModelInfo[roomInfos[i].roomType].Add(itemModelInfo);
                }
            }

            //限制"门"模型坐标位置上以及周边信息
            for (int j = 0; j < roomInfos[i].listDoorPosInfo?.Count; j++)
            {
                Vector2 doorPos1 = roomInfos[i].listDoorPosInfo[j].pos;
                //门的坐标
                doorPosArr.Add(doorPos1);
                doorPosArr.Add(roomInfos[i].listDoorPosInfo[j].entityAxis == 0 ? new Vector2(doorPos1.x + 1, doorPos1.y) : new Vector2(doorPos1.x, doorPos1.y + 1));
                if (m_IsLimitPutPos)
                {
                    //限制已门坐标位置上的 日字型六个坐标
                    doorPosArr.Add(roomInfos[i].listDoorPosInfo[j].entityAxis == 0 ? new Vector2(doorPos1.x, doorPos1.y + 1) : new Vector2(doorPos1.x - 1, doorPos1.y));
                    doorPosArr.Add(roomInfos[i].listDoorPosInfo[j].entityAxis == 0 ? new Vector2(doorPos1.x + 1, doorPos1.y + 1) : new Vector2(doorPos1.x - 1, doorPos1.y + 1));
                    doorPosArr.Add(roomInfos[i].listDoorPosInfo[j].entityAxis == 0 ? new Vector2(doorPos1.x, doorPos1.y - 1) : new Vector2(doorPos1.x + 1, doorPos1.y));
                    doorPosArr.Add(roomInfos[i].listDoorPosInfo[j].entityAxis == 0 ? new Vector2(doorPos1.x + 1, doorPos1.y - 1) : new Vector2(doorPos1.x + 1, doorPos1.y + 1));
                }
            }

        }

        //foreach (var roomType in m_DicItemModelInfo.Keys)
        //{
        //    Debugger.Log("roomType:" + roomType);
        //    foreach (var item in m_DicItemModelInfo[roomType])
        //    {
        //        if (doorPosArr.Contains(item.pos))
        //        {
        //            Debugger.Log("delA pos:" + item.pos + ",itemType:" + item.itemModelType);
        //        }
        //    }
        //}

        for (int i = 1; i <= Enum.GetValues(typeof(RoomType)).Length; i++)
        {
            int tempI = i;
            if (m_DicItemModelInfo.ContainsKey((RoomType)tempI))
            {
                for (int j = 0; j < m_DicItemModelInfo[(RoomType)tempI].Count;)
                {
                    if (doorPosArr.Contains(m_DicItemModelInfo[(RoomType)tempI][j].pos))
                    {
                        //Debugger.Log("delB pos:" + m_DicItemModelInfo[(RoomType)tempI][j].pos + ",itemType:" + m_DicItemModelInfo[(RoomType)tempI][j].itemModelType);
                        //m_DicItemModelInfo[(RoomType)tempI].Remove(m_DicItemModelInfo[(RoomType)tempI][j]);
                        m_DicItemModelInfo[(RoomType)tempI].RemoveAt(j);
                    }
                    else
                    {
                        j++;
                    }
                }

            }
        }
    }

    /// <summary>
    /// 获取当前坐标大致方位
    /// </summary>
    /// <param name="curPos"></param>
    /// <param name="centerPos"></param>
    private DirEnum GetPosDir(Vector2 curPos, Vector2 centerPos)
    {
        DirEnum res;
        if (Mathf.Abs(curPos.x - centerPos.x) > Mathf.Abs(curPos.y - centerPos.y))
        {
            res = curPos.x < centerPos.x ? DirEnum.Left : DirEnum.Right;
        }
        else
        {
            res = curPos.y < centerPos.y ? DirEnum.Top : DirEnum.Bottom;
        }
        return res;
    }

    /// <summary>
    /// 获取模型实体
    /// </summary>
    /// <param name="itemModelTypes">实体类型</param>
    /// <param name="assetName">获取指定资源填写，默认assetName为空，获取随机资源，不为空则获取指定资源，若不为空且未找到指定资源则获取随机资源</param>
    /// <param name="isAutoInstance">自动实例化</param>
    /// <returns></returns>
    private GameObject GetItemEntity(string itemModelTypes, string assetName = "", bool isAutoInstance = true)
    {
        GameObject res = null;
        if (!string.IsNullOrEmpty(itemModelTypes))
        {
            string itemName = string.Empty;
            if (itemModelTypes == "TV" || itemModelTypes == "PC")
            {
                itemName = itemModelTypes;
            }
            else
            {
                //改为大驼峰原则
                itemName = itemModelTypes.ToString().Substring(0, 1).ToUpper() + itemModelTypes.ToString().Substring(1, itemModelTypes.ToString().Length - 1).ToLower();
            }
            res = LoadAssetsByAddressable.GetInstance.GetEntityRes(itemName, assetName);
            if (res != null && isAutoInstance)
            {
                res = Instantiate(res);
            }
            if (res == null)
            {
                Debugger.LogError("Get ItemModelEntity Fail, name:" + itemName);
            }
        }
        return res;
    }


    //随机放置各个房间的实体
    private void SetRandomRoomInsideItemEntity(List<GetThingGraph_data_items> getThingGraph)
    {
        CreateRoomContainer();
        if (getThingGraph != null && getThingGraph.Count > 0)
        {
            for (int i = 0; i < getThingGraph.Count; i++)
            {
                GetThingGraph_data_items data = getThingGraph?[i];
                RoomType roomType = (RoomType)Enum.Parse(typeof(RoomType), data.name);
                //为每个实体找位置放置
                PutItem(roomType, data.id, data.relatedThing, new ItemDependInfo
                {
                    isDepend = false,
                    dependItemID = data.id,
                    dependItemName = data.name,
                });

                Debugger.Log(data.relatedThing);
                ////on、in下的子物体 
                //for (int j = 0; j < data.relatedThing?.Count; j++)
                //{
                //    GetThingGraph_data_items_relatedThing_target chindData = data.relatedThing[j].target;
                //    RoomType chindRoomType = (RoomType)Enum.Parse(typeof(RoomType), chindData.name);
                //    //为每个实体找位置放置
                //    PutItem(chindRoomType, chindData.id, chindData.relatedThing, new ItemDependInfo
                //    {
                //        isDepend = false,
                //        dependItemID = chindData.id,
                //        dependItemName = chindData.name,
                //    });
                //}
            }
        }
    }

    /// <summary>
    /// 在每个房间天花板中心放置灯
    /// </summary>
    private void SetToplampInsideItemEnity()
    {

        for (int i = 0; i < GenerateRoomData.GetInstance.m_ListRoomInfo.Count; i++)
        {
            RoomType roomTypeStr = GenerateRoomData.GetInstance.m_ListRoomInfo[i].roomType;
            string roomIDStr = GenerateRoomData.GetInstance.m_ListRoomInfo[i].roomID;
            //在每个房间天花板中心放置灯
            Vector3 roomCenterPos = GenerateRoomData.GetInstance.GetRoomCenterPos(roomIDStr);
            if (roomCenterPos != Vector3.zero)
            {
                try
                {
                    PutItem(roomTypeStr, roomIDStr, "Toplamp", GetDefaultItemID, roomCenterPos, false);
                }
                catch (Exception e)
                {
                    Debugger.LogError("设置各个房间默认的实体物品 栈溢出,准备重新生成场景 e:" + e);
                    UnityTool.GetInstance.DelayCoroutine(0.5f, () => GameLogic.GetInstance.GenerateScene());
                    return;
                }
            }
            //在每个房间放置垃圾桶
            PutCustomItem(roomTypeStr, "Bin", GetDefaultItemID);
        }
    }

    /// <summary>
    /// 设置各个房间默认的实体物品，位置随机
    /// </summary>
    private void SetDefaultRoomInsideItemEntity()
    {
        //客厅
        PutCustomItem(RoomType.LivingRoom, "Sofa", GetDefaultItemID);
        PutCustomItem(RoomType.LivingRoom, "TV", GetDefaultItemID);

        //卧室
        PutCustomItem(RoomType.BedRoom, "TV", GetDefaultItemID);
        PutCustomItem(RoomType.BedRoom, "Bed", GetDefaultItemID);
        string brDeskItemID = GetDefaultItemID;
        Debugger.Log("brDeskItemID:" + brDeskItemID);
        PutCustomItem(RoomType.BedRoom, "Desk", brDeskItemID);
        PutCustomItem(RoomType.BedRoom, "Chair", GetDefaultItemID, new ItemDependInfo
        {
            isDepend = true,
            dependItemID = brDeskItemID,
            dependItemName = "Desk",
            posRelation = PosRelation.Below
        });
        PutCustomItem(RoomType.BedRoom, "Drink", GetDefaultItemID, new ItemDependInfo
        {
            isDepend = true,
            dependItemID = brDeskItemID,
            dependItemName = "Desk",
            posRelation = PosRelation.On
        });


        //厨房 
        PutCustomItem(RoomType.KitchenRoom, "Bigsink", GetDefaultItemID);

        //卫生间
        PutCustomItem(RoomType.BathRoom, "Bathtub", GetDefaultItemID);

        //书房
        PutCustomItem(RoomType.StudyRoom, "Sofa", GetDefaultItemID);
        string srDeskItemID = GetDefaultItemID;
        PutCustomItem(RoomType.StudyRoom, "Desk", srDeskItemID);
        PutCustomItem(RoomType.StudyRoom, "Chair", GetDefaultItemID, new ItemDependInfo
        {
            isDepend = true,
            dependItemID = srDeskItemID,
            dependItemName = "Desk",
            posRelation = PosRelation.Below
        });
        PutCustomItem(RoomType.StudyRoom, "Book", GetDefaultItemID, new ItemDependInfo
        {
            isDepend = true,
            dependItemID = srDeskItemID,
            dependItemName = "Desk",
            posRelation = PosRelation.On
        });
        PutCustomItem(RoomType.StudyRoom, "Cabinet", GetDefaultItemID);
        PutCustomItem(RoomType.StudyRoom, "Plant", GetDefaultItemID);

        //储藏室
        PutCustomItem(RoomType.StorageRoom, "Plant", GetDefaultItemID);


        PutCustomItem(RoomType.StorageRoom, "BoxPush", GetDefaultItemID);
        PutCustomItem(RoomType.StorageRoom, "BoxPull", GetDefaultItemID);
        PutCustomItem(RoomType.StorageRoom, "Wheel", GetDefaultItemID);
        PutCustomItem(RoomType.LivingRoom, "Pile", GetDefaultItemID);

    }



    /// <summary>
    /// 创建物品实体的放置房间容器
    /// </summary>
    private void CreateRoomContainer()
    {
        GetEnvGraph_data_items[] roomItemsData = MainData.getEnvGraph.data.items;

        string roomType = string.Empty;
        string roomId = string.Empty;
        string roomName = string.Empty;
        for (int i = 0; i < roomItemsData.Length; i++)
        {
            roomType = roomItemsData[i].name;
            roomId = roomItemsData[i].id;
            roomName = roomType + "_" + roomId;
            if (ItemEntityGroupNode.Find(roomName) == null)
            {
                GameObject roomContainer = new GameObject(roomName);
                roomContainer.transform.SetParent(ItemEntityGroupNode);
            }
            for (int j = 0; j < roomItemsData[i].relatedThing?.Length; j++)
            {
                roomType = roomItemsData[i].relatedThing[j].target.name;
                roomId = roomItemsData[i].relatedThing[j].target.id;
                roomName = roomType + "_" + roomId;

                if (ItemEntityGroupNode.Find(roomName) == null)
                {
                    GameObject roomContainer = new GameObject(roomName);
                    roomContainer.transform.SetParent(ItemEntityGroupNode);
                }
            }
        }
    }

    /// <summary>
    /// 为每个物品实体找位置放置
    /// </summary>
    /// <param name="roomType">房间名称</param>
    /// <param name="relatedThingArr">当前层级所有物品</param>
    /// <param name="itemDependInfo">当前物品的放置依赖限制</param>
    private void PutItem(RoomType roomType, string roomID, List<GetThingGraph_data_items_relatedThing> relatedThingArr, ItemDependInfo itemDependInfo)
    {
        for (int i = 0; i < relatedThingArr.Count; i++)
        {
            //Debugger.Log("cur put item:" + relatedThingArr[i].target.name
            //    + ",id" + relatedThingArr[i].target.id
            //    + ", isDepend :" + itemDependInfo.isDepend
            //    + ",dependItemName:" + itemDependInfo?.dependItemName
            //    + ",dependItemID:" + itemDependInfo?.dependItemID);
            string key = relatedThingArr[i].target.name + "_" + relatedThingArr[i].target.id;
            if (MainData.CacheItemsEntity.ContainsKey(key))
            {
                Debugger.LogError("当前实体已存在，name_id：" + key);
                continue;
            }
            //当前实体信息
            string entityName = relatedThingArr[i].target.name;
            //剔除Door信息和
            if (entityName.EndsWith("Door"))
            {
                continue;
            }
            ////重新生成场景后屏蔽顶灯Toplamp信息
            //if (relatedThingArr[i].target.name == "Toplamp" && !MainData.IsFirstGenerate)
            //{
            //    continue;
            //}
            //实例化实体
            GameObject clone = GetItemEntity(entityName);
            Transform parentTrans = ItemEntityGroupNode.transform.Find(roomType.ToString() + "_" + roomID);
            if (parentTrans == null)
            {
                parentTrans = new GameObject(roomType.ToString() + "_" + roomID).transform;
                parentTrans.SetParent(ItemEntityGroupNode.transform);
            }
            clone.transform.SetParent(parentTrans, false);
            if (relatedThingArr[i].target.scale != null && relatedThingArr[i].target.scale[0] != 0 && relatedThingArr[i].target.scale[1] != 0 && relatedThingArr[i].target.scale[2] != 0)
            {
                clone.transform.localScale = new Vector3(relatedThingArr[i].target.scale[0], relatedThingArr[i].target.scale[1], relatedThingArr[i].target.scale[2]);

            }
            clone.name = entityName + "_" + relatedThingArr[i].target.id;

            clone.SetActive(false);
            Transform model = clone.transform.Find<Transform>("Model");
            Transform size = clone.transform.Find<Transform>("Size"); //模型在Model节点旋转角度0°时的长宽
            int itemLength;
            int itemWidch;

            //设置实体位置
            //限制放在上一层级物品之上
            if (itemDependInfo.isDepend)
            {
                //寻找所限制的物体区域
                string parentItemName = itemDependInfo.dependItemName + "_" + itemDependInfo.dependItemID;
                GameObject parentItem = GameObject.Find(parentItemName)?.gameObject;
                if (parentItem != null)
                {
                    Transform putAreaTrans = parentItem.transform.Find("PutArea/" + itemDependInfo.posRelation.ToString());
                    clone.transform.parent = putAreaTrans;

                    //clone.transform.position = putAreaTrans.position;
                    clone.transform.localPosition = Vector3.zero;

                    clone.transform.Find<Transform>("Model").transform.localPosition = Vector3.zero;
                    clone.gameObject.SetActive(true);
                }
                else
                {
                    Debugger.LogWarning("item is null ,itemName : " + parentItemName);
                }
            }
            else
            {
                if (MainData.CanReadFile)//读档,且处于首次生成中  位置固定
                {
                    float[] pos = relatedThingArr[i].target.position;
                    float[] rot = relatedThingArr[i].target.rotation;
                    if (pos?.Length >= 3)
                    {
                        clone.transform.position = new Vector3(pos[0], pos[1], pos[2]);
                    }
                    if (rot?.Length >= 3)
                    {
                        clone.transform.rotation = Quaternion.Euler(new Vector3(rot[0], rot[1], rot[2]));
                    }

                    clone.SetActive(true);
                }
                else //非读档 位置随机
                {
                    //无放置限制
                    if (!m_DicItemModelInfo.ContainsKey(roomType))
                    {
                        Debugger.LogError("cur roomType notFind , roomType:" + roomType);
                        continue;
                    }
                    //找到当前房间所有可放置的位置信息
                    List<ItemModelInfo> itemModelInfos = m_DicItemModelInfo[roomType];

                    //已经找到可用位置
                    bool canUse = true;
                    foreach (ItemModelInfo itemInfo in itemModelInfos)
                    {
                        if (itemInfo.itemModelType != string.Empty)
                        {
                            continue;
                        }
                        Vector2 curPos = itemInfo.pos;
                        //当前坐标位置相对于当前房间的方位
                        DirEnum curPosDir = itemInfo.posDir;

                        //对当前房间每个坐标点进行上下左右是个方向查询是否都可放置，查询范围，从当前坐标点开始到物体的长宽结束，物体锚点默认在左下角
                        //优先找朝向房间中心位置的方向
                        DirEnum curDir = (DirEnum)(((int)curPosDir + 2) % Enum.GetValues(typeof(DirEnum)).Length);//初始方向与curPosDir相反
                        int curLoopCount = 0;
                        int loopMaxCount = 4;
                        while (curLoopCount < loopMaxCount)
                        {
                            //当前模型所需占用的坐标位置
                            List<Vector2> itemPosArr = new List<Vector2>();
                            canUse = true;
                            clone.transform.position = new Vector3(curPos.x, 0, curPos.y);
                            switch (curDir)
                            {
                                case DirEnum.Top://up  当前物体锚点在物体的右上角 朝向下方
                                    clone.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
                                    itemLength = (int)size.transform.localScale.x;
                                    itemWidch = (int)size.transform.localScale.z;
                                    for (int m = (int)curPos.x - itemLength; m <= (int)curPos.x; m++)
                                    {
                                        for (int n = (int)curPos.y - itemWidch; n <= (int)curPos.y; n++)
                                        {
                                            if (!itemPosArr.Contains(new Vector2(m, n)))
                                            {
                                                itemPosArr.Add(new Vector2(m, n));
                                            }
                                        }
                                    }
                                    break;
                                case DirEnum.Left://left 当前物体锚点在物体的左上角 朝向右方
                                    clone.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                                    itemLength = (int)size.transform.localScale.z;
                                    itemWidch = (int)size.transform.localScale.x;
                                    for (int m = (int)curPos.x; m <= (int)curPos.x + itemLength; m++)
                                    {
                                        for (int n = (int)curPos.y - itemWidch; n <= (int)curPos.y; n++)
                                        {
                                            if (!itemPosArr.Contains(new Vector2(m, n)))
                                            {
                                                itemPosArr.Add(new Vector2(m, n));
                                            }
                                        }
                                    }
                                    break;
                                case DirEnum.Bottom://down 当前物体锚点在物体的左下角 朝向上方
                                    clone.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                                    itemLength = (int)size.transform.localScale.x;
                                    itemWidch = (int)size.transform.localScale.z;
                                    for (int m = (int)curPos.x; m <= (int)curPos.x + itemLength; m++)
                                    {
                                        for (int n = (int)curPos.y; n <= (int)curPos.y + itemWidch; n++)
                                        {
                                            if (!itemPosArr.Contains(new Vector2(m, n)))
                                            {
                                                itemPosArr.Add(new Vector2(m, n));
                                            }
                                        }
                                    }
                                    break;
                                case DirEnum.Right://right 当前物体锚点在物体的右下角 朝向左方
                                    clone.transform.localRotation = Quaternion.Euler(new Vector3(0, -90, 0));
                                    itemLength = (int)size.transform.localScale.z;
                                    itemWidch = (int)size.transform.localScale.x;
                                    for (int m = (int)curPos.x - itemLength; m <= (int)curPos.x; m++)
                                    {
                                        for (int n = (int)curPos.y; n <= (int)curPos.y + itemWidch; n++)
                                        {
                                            if (!itemPosArr.Contains(new Vector2(m, n)))
                                            {
                                                itemPosArr.Add(new Vector2(m, n));
                                            }
                                        }
                                    }
                                    break;
                            }
                            //判定所有坐标是否全部可用
                            if (itemPosArr.Count == 0)
                            {
                                canUse = false;
                            }
                            else
                            {
                                List<ItemModelInfo> needItemModelInfoArr = new List<ItemModelInfo>();
                                foreach (Vector2 needPos in itemPosArr)
                                {
                                    ItemModelInfo needItemModelInfo = itemModelInfos.Find((p) => { return p.pos == needPos && p.itemModelType == string.Empty; });
                                    if (needItemModelInfo == null)
                                    {
                                        canUse = false;
                                        break;
                                    }
                                    else
                                    {
                                        needItemModelInfoArr.Add(needItemModelInfo);
                                    }
                                }
                                if (canUse)
                                {
                                    //放置后标记当前位置在当前房间已被放置其他物体不可重复放置在此
                                    Debugger.Log("当前实体已放置成功 name_id：" + key + " , roomType:" + roomType, LogTag.Forever);
                                    MainData.CacheItemsEntity.Add(key, clone);

                                    for (int k = 0; k < needItemModelInfoArr.Count; k++)
                                    {
                                        needItemModelInfoArr[k].itemModelType = relatedThingArr[i].target.name;
                                        //Debugger.Log("pos:" + needItemModelInfoArr[k].pos);
                                    }
                                    clone.SetActive(true);
                                    break;
                                }
                            }
                            curLoopCount++;
                            curDir = (DirEnum)(((int)curPosDir + 1) % Enum.GetValues(typeof(DirEnum)).Length);

                        }
                        if (canUse)
                        {
                            break;
                        }

                    }
                    if (!canUse)
                    {
                        Debugger.LogWarning("当前实体未找到合适位置放置，隐藏该实体，itemEntity:" + relatedThingArr[i].target.name + ",id:" + relatedThingArr[i].target.id, LogTag.Forever);
                        //Destroy(clone);
                    }

                    //if (relatedThingArr[i].target.relatedThing?.Count > 0)
                    //{
                    //    PutItem(roomType, roomID, relatedThingArr[i].target.relatedThing,
                    //        new ItemDependInfo
                    //        {
                    //            isDepend = true,
                    //            dependItemName = relatedThingArr[i].target.name,
                    //            dependItemID = relatedThingArr[i].target.id,
                    //            posRelation = (PosRelation)Enum.Parse(typeof(PosRelation), relatedThingArr[i].relationship)
                    //        });
                    //}

                }
                if (relatedThingArr[i].target.relatedThing?.Count > 0)
                {
                    //PutItem(roomType, roomID, relatedThingArr[i].target.relatedThing,
                    //    new ItemDependInfo
                    //    {
                    //        isDepend = true,
                    //        dependItemName = relatedThingArr[i].target.name,
                    //        dependItemID = relatedThingArr[i].target.id,
                    //        posRelation = (PosRelation)Enum.Parse(typeof(PosRelation), relatedThingArr[i].relationship)
                    //    });
                    for (int j = 0; j < relatedThingArr[i].target.relatedThing.Count; j++)
                    {
                        List<GetThingGraph_data_items_relatedThing> newRelatedThing = new List<GetThingGraph_data_items_relatedThing>
                        {
                            relatedThingArr[i].target.relatedThing[j]
                        };
                        PutItem(roomType, roomID, newRelatedThing,
                       new ItemDependInfo
                       {
                           isDepend = true,
                           dependItemName = relatedThingArr[i].target.name,
                           dependItemID = relatedThingArr[i].target.id,
                           posRelation = (PosRelation)Enum.Parse(typeof(PosRelation), relatedThingArr[i].target.relatedThing[j].relationship)
                       });
                    }
                }
            }
        }
    }

    /// <summary>
    /// 物体放置在指定位置
    /// </summary>
    /// <param name="roomType"></param>
    /// <param name="roomID"></param>
    /// <param name="entityName"></param>
    /// <param name="isCachePutPos">是否缓存实体所放置的位置</param>
    private void PutItem(RoomType roomType, string roomID, string entityName, string entityID, Vector3 pos, bool isCachePutPos)
    {
        Transform parentTrans = ItemEntityGroupNode.transform.Find(roomType.ToString() + "_" + roomID);
        if (parentTrans == null)
        {
            parentTrans = new GameObject(roomType.ToString() + "_" + roomID).transform;
            parentTrans.SetParent(ItemEntityGroupNode.transform);
        }
        string targetItemName = entityName + "_" + entityID;
        GameObject targetItem = parentTrans.Find<Transform>(targetItemName)?.gameObject;
        if (targetItem == null)
        {
            GameObject entityPrefab = LoadAssetsByAddressable.GetInstance.GetEntityRes(entityName);
            targetItem = Instantiate(entityPrefab);

            targetItem.transform.SetParent(parentTrans, false);
            targetItem.name = targetItemName;
            Debugger.Log("111111" + targetItem);
        }
        targetItem.transform.position = pos;
    }

    /// <summary>
    /// 放置自定义物体
    /// </summary>
    /// <param name="roomType"></param>
    /// <param name="itemDependInfo">当前物体所依赖的父物体</param>
    /// <param name="itemsName"></param>
    private void PutCustomItem(RoomType roomType, string itemName, string itemID, ItemDependInfo itemDependInfo = null)
    {
        string roomID = GenerateRoomData.GetInstance.GetRoomID(roomType);
        if (!string.IsNullOrEmpty(roomID))
        {
            List<string> itemResName = new List<string>() { itemName };

            List<GetThingGraph_data_items_relatedThing> relatedThingData = new List<GetThingGraph_data_items_relatedThing>();
            for (int i = 0; i < itemResName?.Count; i++)
            {
                relatedThingData.Add(new GetThingGraph_data_items_relatedThing
                {
                    target = new GetThingGraph_data_items_relatedThing_target
                    {
                        name = itemResName[i],
                        id = itemID
                    }
                });
            }
            if (itemDependInfo == null)
            {
                itemDependInfo = new ItemDependInfo
                {
                    isDepend = false,
                    dependItemID = "",
                    dependItemName = "",
                };
            }
            PutItem(roomType, roomID, relatedThingData, itemDependInfo);
        }
    }

    //清理实体对象
    private void ClearItemEntity()
    {
        for (int i = 0; i < ItemEntityGroupNode?.childCount; i++)
        {
            //Debugger.Log("DEL " + ItemEntityGroupNode?.GetChild(i));
            int typeI = i;
            ItemEntityGroupNode.GetChild(i).gameObject.name = "del_" + ItemEntityGroupNode.GetChild(i).gameObject.name;
            ItemEntityGroupNode.GetChild(i).gameObject.SetActive(false);
            Destroy(ItemEntityGroupNode.GetChild(typeI).gameObject);
            // UnityTool.GetInstance.DelayCoroutine(0.1f, () => Destroy(ItemEntityGroupNode.GetChild(typeI).gameObject));
        }
    }
}
