using System.Collections.Generic;
using UnityEngine;
using static GenerateRoomBorderModel;
using MFramework;
using static GenerateRoomData;
using System;
using System.Linq;
using static GetThingGraph;
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

    public Transform ItemEntityGroupNode { get; private set; }

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

    private void Awake()
    {
        ItemEntityGroupNode = GameObject.Find("ItemEntityGroupNode")?.transform;
        if (ItemEntityGroupNode == null)
        {
            ItemEntityGroupNode = new GameObject("ItemEntityGroupNode").transform;
        }
        ItemEntityGroupNode.parent = GameLogic.GetInstance.staticModelRootNode?.transform;
    }

    /// <summary>
    /// 生成房间内道具
    /// </summary>
    /// <param name="roomInfos">所有房间边界信息</param>
    /// <param name="getThingGraph">物体与房间的邻接关系数据</param>
    public void GenerateRoomItem(List<RoomInfo> roomInfos, GetThingGraph getThingGraph = null)
    {
        /*根据边界信息找到各个房间的可放置坐标节点，屏蔽"门"模型前后坐标节点，避免物体堵门*/
        ChcheItemModelTypeInfo(roomInfos);

        ////拿到当前房间所需要放置的道具实体，实体名称、实体大小
        //Dictionary<RoomType, List<ItemDependInfo>> dicRoomInsideItemEntity = GetRoomInsideItemEntity(roomInfos, getThingGraph);

        ////根据当前实体大小等信息找到合适的位置放置
        //SetRandomRoomInsideItemEntity(dicRoomInsideItemEntity);
        SetRandomRoomInsideItemEntity(getThingGraph);
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
                        Debug.Log("屏蔽掉门周边的放置信息，门口不可放置物体 targetPos:" + targetPos);
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
        //    Debug.Log("roomType:" + roomType);
        //    foreach (var item in m_DicItemModelInfo[roomType])
        //    {
        //        if (doorPosArr.Contains(item.pos))
        //        {
        //            Debug.Log("delA pos:" + item.pos + ",itemType:" + item.itemModelType);
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
                        //Debug.Log("delB pos:" + m_DicItemModelInfo[(RoomType)tempI][j].pos + ",itemType:" + m_DicItemModelInfo[(RoomType)tempI][j].itemModelType);
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
    /// <param name="itemModelTypes"></param>
    /// <returns></returns>
    private List<GameObject> GetItemEntity(params string[] itemModelTypes)
    {
        var res = new List<GameObject>();
        for (int i = 0; i < itemModelTypes?.Length; i++)
        {
            //改为大驼峰原则
            string itemName = itemModelTypes[i].ToString().Substring(0, 1).ToUpper() + itemModelTypes[i].ToString().Substring(1, itemModelTypes[i].ToString().Length - 1).ToLower();
            GameObject go = LoadAssetsByAddressable.GetInstance.GetEntityRes(itemName);
            if (go != null)
            {
                res.Add(go);
            }
            else
            {
                Debug.LogError("Get ItemModelEntity Fail, name:" + itemName);
            }
        }
        return res;
    }


    //随机放置各个房间的实体
    private void SetRandomRoomInsideItemEntity(GetThingGraph getThingGraph)
    {
        ClearItemEntity();

        if (getThingGraph != null && getThingGraph.data?.items?.Count > 0)
        {
            for (int i = 0; i < getThingGraph.data?.items?.Count; i++)
            {
                var data = getThingGraph.data?.items?[i];
                RoomType roomType = (RoomType)Enum.Parse(typeof(RoomType), data.name);
                //为每个实体找位置放置
                PutItem(roomType, data.id, data.relatedThing, new ItemDependInfo
                {
                    isDepend = false,
                    dependItemID = data.id,
                    dependItemName = data.name,
                });
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
            //Debug.Log("cur put item:" + relatedThingArr[i].target.name
            //    + ",id" + relatedThingArr[i].target.id
            //    + ", isDepend :" + itemDependInfo.isDepend
            //    + ",dependItemName:" + itemDependInfo?.dependItemName
            //    + ",dependItemID:" + itemDependInfo?.dependItemID);
            //当前实体信息
            string entityName = relatedThingArr[i].target.name;
            //剔除Door信息
            if (entityName.EndsWith("Door"))
            {
                continue;
            }
            GameObject clone = Instantiate(GetItemEntity(entityName)?[0]);
            Transform parentTrans = ItemEntityGroupNode.transform.Find(roomType.ToString() + "_" + roomID);
            if (parentTrans == null)
            {
                parentTrans = new GameObject(roomType.ToString() + "_" + roomID).transform;
                parentTrans.SetParent(ItemEntityGroupNode.transform);
            }
            clone.transform.SetParent(parentTrans, false);
            clone.name = entityName + "_" + relatedThingArr[i].target.id;

            clone.SetActive(false);
            Transform model = clone.transform.Find<Transform>("Model");
            Transform size = clone.transform.Find<Transform>("Size"); //模型在Model节点旋转角度0°时的长宽
            int itemLength;
            int itemWidch;

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
                    clone.transform.position = putAreaTrans.position;
                    clone.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("item is null ,itemName : " + parentItemName);
                }
            }
            //无放置限制
            else
            {
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
                                Debug.Log("roomType:" + roomType + ",itemEntity:" + relatedThingArr[i].target.name);
                                for (int k = 0; k < needItemModelInfoArr.Count; k++)
                                {
                                    needItemModelInfoArr[k].itemModelType =relatedThingArr[i].target.name;
                                    //Debug.Log("pos:" + needItemModelInfoArr[k].pos);
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

                if (relatedThingArr[i].target.relatedThing?.Count > 0)
                {
                    PutItem(roomType, roomID, relatedThingArr[i].target.relatedThing,
                        new ItemDependInfo
                        {
                            isDepend = true,
                            dependItemName = relatedThingArr[i].target.name,
                            dependItemID = relatedThingArr[i].target.id,
                            posRelation = (PosRelation)Enum.Parse(typeof(PosRelation), relatedThingArr[i].relationship)
                        });
                }
            }
        }
    }


    //随机放置各个房间的实体
    private void SetRandomRoomInsideItemEntity(Dictionary<RoomType, List<GameObject>> dicRoomInsideItemEntity)
    {
        ClearItemEntity();
        foreach (RoomType roomType in dicRoomInsideItemEntity?.Keys)
        {
            List<GameObject> itemEntityArr = dicRoomInsideItemEntity[roomType];
            //为每个实体找位置放置
            for (int i = 0; i < itemEntityArr?.Count; i++)
            {
                //当前实体信息
                GameObject clone = Instantiate(itemEntityArr[i]);
                clone.transform.SetParent(ItemEntityGroupNode.transform, false); //TODO 加载一层房间名称


                clone.SetActive(false);
                Transform model = clone.transform.Find<Transform>("Model");
                Transform size = clone.transform.Find<Transform>("Size"); //模型在Model节点旋转角度0°时的长宽
                int itemLength;
                int itemWidch;
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
                                Debug.Log("roomType:" + roomType + ",itemEntity:" + itemEntityArr[i].name);
                                for (int k = 0; k < needItemModelInfoArr.Count; k++)
                                {
                                    needItemModelInfoArr[k].itemModelType = itemEntityArr[i].name;
                                    Debug.Log("pos:" + needItemModelInfoArr[k].pos);
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
            }
        }
    }

    //清理实体对象
    private void ClearItemEntity()
    {
        for (int i = 0; i < ItemEntityGroupNode?.childCount; i++)
        {
            //Debug.Log("DEL " + ItemEntityGroupNode?.GetChild(i));
            int typeI = i;
            ItemEntityGroupNode.GetChild(i).gameObject.name = "del_"+ ItemEntityGroupNode.GetChild(i).gameObject.name;
            ItemEntityGroupNode.GetChild(i).gameObject.SetActive(false);
            
            UnityTool.GetInstance.DelayCoroutine(1f,()=> Destroy(ItemEntityGroupNode.GetChild(typeI).gameObject));
        }
    }
}
