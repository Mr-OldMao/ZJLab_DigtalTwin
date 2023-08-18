using System.Collections.Generic;
using UnityEngine;
using static GenerateRoomBorderModel;
using MFramework;
using static GenerateRoomData;
using System;
using System.Linq;
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

    private Transform ItemEntityGroupNode;

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


    private void Awake()
    {
        ItemEntityGroupNode = GameObject.Find("ItemEntityGroupNode")?.transform;
        if (ItemEntityGroupNode == null)
        {
            ItemEntityGroupNode = new GameObject("ItemEntityGroupNode").transform;
        }
    }

    /// <summary>
    /// 生成房间内道具
    /// </summary>
    /// <param name="borderEntityDatas">所有房间边界信息</param>
    public void GenerateRoomItem(List<RoomInfo> roomInfos)
    {
        //CacheItenEntityModel();

        /*根据边界信息找到各个房间的可放置坐标节点，屏蔽"门"模型前后坐标节点，避免物体堵门*/
        ChcheItemModelTypeInfo(roomInfos);

        //拿到当前房间所需要放置的道具实体，实体名称、实体大小
        Dictionary<RoomType, List<GameObject>> dicRoomInsideItemEntity = GetRoomInsideItemEntity(roomInfos);

        //根据当前实体大小等信息找到合适的位置放置
        SetRandomRoomInsideItemEntity(dicRoomInsideItemEntity);



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
            res = curPos.y < centerPos.y ? DirEnum.Up : DirEnum.Down;
        }
        return res;
    }


    /// <summary>
    /// 获取各个房间内部所有物体实体
    /// </summary>
    /// <param name="roomInfos"></param>
    /// <param name="obj">TODO 根据接口数据配置各个房间物品</param>
    /// <returns></returns>
    private Dictionary<RoomType, List<GameObject>> GetRoomInsideItemEntity(List<RoomInfo> roomInfos,object obj = null)
    {
        var res = new Dictionary<RoomType, List<GameObject>>();
        for (int i = 0; i < roomInfos.Count; i++)
        {
            List<string> itemModels = new List<string>();
            switch (roomInfos[i].roomType)
            {
                case RoomType.LivingRoom://Add原则 尽量占地面积大的物品放在前列，后续随机放置物品时会按此顺序顺次放置
                    //TODO 后面需要随机取 ItemSofaBig1，ItemSofaBig2，ItemSofaBig3
                    itemModels.Add("SofaBig");
                    itemModels.Add("ItemComputerDeskChair");
                    itemModels.Add("Bin");
                    itemModels.Add("Bin");
                    itemModels.Add("Bin");
                    break;
                case RoomType.BathRoom:
                    itemModels.Add("Bin");
                    itemModels.Add("Bin");
                    break;
                case RoomType.BedRoom:
                    itemModels.Add("Bin");
                    break;
                //case RoomType.SecondBedRoom:
                //    itemModels.Add(ItemModelType.ItemBed);
                //    itemModels.Add(ItemModelType.ItemBin);
                //    break;
                case RoomType.KitChenRoom:
                    itemModels.Add("ItemKitchenCase1"); //TODO 后面随机取ItemKitchenCase1，ItemKitchenCase2
                    itemModels.Add("ItemRefrigerator1");//TODO 后面随机取ItemRefrigerator1,ItemRefrigerator2
                    itemModels.Add("Bin");
                    break;
                case RoomType.StudyRoom:
                    break;
                //case RoomType.BalconyRoom:
                //    break;
                case RoomType.StorageRoom:
                    break;
            }
            res.Add(roomInfos[i].roomType, GetItemEntity(itemModels.ToArray()));
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
            GameObject go = ResourcesLoad.GetInstance.GetEntityRes(itemModelTypes[i].ToString());
            if (go != null)
            {
                res.Add(go);
            }
            else
            {
                Debug.LogError("Get ItemModelEntity Fail, name:" + itemModelTypes[i].ToString());
            }
        }
        return res;
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
                            case DirEnum.Up://up  当前物体锚点在物体的右上角 朝向下方
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
                            case DirEnum.Down://down 当前物体锚点在物体的左下角 朝向上方
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
                                    needItemModelInfoArr[k].itemModelType = GetItemModelType(itemEntityArr[i].name);
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
            Destroy(ItemEntityGroupNode?.GetChild(i).gameObject);
        }
    }

    //获取实体对象对应的枚举
    private string GetItemModelType(string itemName)
    {
        //@string res = @string.Null;
        //if (Enum.TryParse(typeof(@string), itemName, out object p))
        //{
        //    res = (@string)p;
        //}
        return itemName;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //foreach (var roomType in m_DicItemModelInfo.Keys)
            //{
            //    Debug.Log("roomType:" + roomType);
            //    foreach (var item in m_DicItemModelInfo[roomType])
            //    {
            //        Debug.Log("pos:" + item.pos + ",itemType:" + item.itemModelType);
            //    }
            //}
        }
    }
}
