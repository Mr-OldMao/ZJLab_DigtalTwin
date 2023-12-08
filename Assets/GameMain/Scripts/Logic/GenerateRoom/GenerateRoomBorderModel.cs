using System.Collections.Generic;
using UnityEngine;
using MFramework;
using System;
using static DataRead;
using static GenerateRoomBorderModel;
using static GenerateRoomData;

/// <summary>
/// 标题：房间边界模型(墙壁、门、地砖)自动生成器
/// 功能：1.生成模型，根据房间信息数据生成与之匹配的3D房间模型；2.清空场景模型
/// 作者：毛俊峰
/// 时间：2023.07.18-2023.07.20
/// </summary>
public class GenerateRoomBorderModel : SingletonByMono<GenerateRoomBorderModel>
{

    private Transform m_RoomRootNode;
    public Transform RoomGroup
    {
        get { return m_RoomRootNode; }
        set { m_RoomRootNode = value; }
    }

    /// <summary>
    /// 缓存已生成实体的数据信息 k-实体坐标(唯一) v-当前实体坐标对于的实体信息
    /// </summary>
    private Dictionary<Vector2, BorderInfo> m_DicBorderItemInfo;


    /// <summary>
    /// 缓存所有房间实体    k-房间类型 v-实体
    /// </summary>
    private Dictionary<RoomType, GameObject> m_DicRoomEntity;

    [Serializable]
    public class RoomInfo
    {
        public RoomType roomType;
        public string roomID;
        /// <summary>
        /// 房间长宽，单位米(正整数)  PS：暂时没用上，后面考虑删除此字段
        /// </summary>
        public uint[] roomSize;
        /// <summary>
        /// 房间左下世界坐标位置(整数)
        /// </summary>
        public Vector2 roomPosMin;
        /// <summary>
        /// 房间右上世界坐标位置(整数)
        /// </summary>
        public Vector2 roomPosMax;
        /// <summary>
        /// 当前房间门的世界坐标位置(整数)
        /// </summary>
        public List<BorderEntityData> listDoorPosInfo;
        /// <summary>
        /// 当前房间空墙的世界坐标位置(整数)
        /// </summary>
        public List<BorderEntityData> listEmptyPosInfo;
    }

    /// <summary>
    /// 边界实体数据信息数据结构，边界实体(实墙、空墙、门)
    /// </summary>
    public class BorderInfo
    {
        //横向边界实体信息
        public BorderEntityData borderItemPosInfoX;
        //纵向边界实体信息
        public BorderEntityData borderItemPosInfoY;
    }

    /// <summary>
    /// 具体边界实体数据信息
    /// </summary>
    [Serializable]
    public class BorderEntityData
    {
        public GameObject entity = null;
        ///// <summary>
        ///// 实体的材质ID [1,10]
        ///// </summary>
        //public int matID;
        public Vector2 pos;
        public EntityModelType entityModelType;
        /// <summary>
        /// 边界实体轴向，0-横向 1-纵向
        /// </summary>
        public int entityAxis;

        /// <summary>
        /// 边界实体方位，相对于当前房间中心点的方位
        /// </summary>
        public BorderDir borderDir;

        /// <summary>
        /// 当前实体所属房间类型，一个边界实体最多可属于两个房间，listRoomType.count<=2 ,目前没有一个边界实体属于两个房间说法，只属于一个房间
        /// </summary>
        public List<RoomType> listRoomType;

        /// <summary>
        /// 当前实体所属房间类型ID,可代替listRoomType使用，一个边界实体最多可属于两个房间，listRoomType.count<=2,目前没有一个边界实体属于两个房间说法，只属于一个房间
        /// </summary>
        public List<string> listRoomTypeID;
    }

    /// <summary>
    /// 实体方位
    /// </summary>
    public enum BorderDir
    {
        Up,
        Down,
        Left,
        Right
    }

    private void Awake()
    {
        m_DicBorderItemInfo = new Dictionary<Vector2, BorderInfo>();
        m_DicRoomEntity = new Dictionary<RoomType, GameObject>();
        m_RoomRootNode = GameLogic.GetInstance.staticModelRootNode?.transform;
    }
    /// <summary>
    /// 生成房间边界模型，对外接口
    /// </summary>
    /// <param name="roomInfo">需要传入房间的详细信息</param>
    /// <param name="borderEntityDatas">房间所有边界(墙、门、地板)数据</param>
    public void GenerateRoomBorder()
    {
        ClearRoom();
        //获取材质信息
        List<RoomMatData> roomMatDatas = null;
        if (MainData.CanReadFile)
        {
            roomMatDatas = DataRead.GetInstance.ReadRoomMatData();
        }
        else
        {
            roomMatDatas = GenerateRoomData.GetInstance.roomMatDatas;
        }
        if (roomMatDatas == null)
        {
            roomMatDatas = new List<RoomMatData>();
        }

        int matID;
        string matKey;
        Material mat;
        for (int i = 0; i < GenerateRoomData.GetInstance.listRoomBuilderInfo.Count; i++)
        {
            BorderEntityData borderEntityData = GenerateRoomData.GetInstance.listRoomBuilderInfo[i];
            GameObject entityModel = null;
            switch (borderEntityData.entityModelType)
            {
                case EntityModelType.Null:
                    break;
                case EntityModelType.Wall:
                    entityModel = LoadAssetsByAddressable.GetInstance.GetEntityRes("WallPrefab");
                    for (int j = 0; j < entityModel.transform.childCount; j++)
                    {
                        bool isTrue = entityModel.transform.GetChild(j).name == borderEntityData.borderDir.ToString();
                        entityModel.transform.GetChild(j).SetActive(isTrue);
                        if (isTrue)
                        {
                            matID = GetRoomMatID(borderEntityData.listRoomType[0], borderEntityData.listRoomTypeID[0], borderEntityData.entityModelType, ref roomMatDatas);
                            matKey = "Wall_" + matID;
                            mat = LoadAssetsByAddressable.GetInstance.GetRes<Material>(matKey, false);
                            entityModel.transform.GetChild(j).GetComponentInChildren<MeshRenderer>().material = mat;
                        }
                    }
                    break;
                case EntityModelType.SmallWall:
                    entityModel = LoadAssetsByAddressable.GetInstance.GetEntityRes("WallSmallPrefab");
                    for (int j = 0; j < entityModel.transform.childCount; j++)
                    {
                        bool isTrue = entityModel.transform.GetChild(j).name == borderEntityData.borderDir.ToString();
                        entityModel.transform.GetChild(j).SetActive(isTrue);
                        if (isTrue)
                        {
                            matID = GetRoomMatID(borderEntityData.listRoomType[0], borderEntityData.listRoomTypeID[0], borderEntityData.entityModelType, ref roomMatDatas);
                            matKey = "SmallWall_" + matID;
                            mat = LoadAssetsByAddressable.GetInstance.GetRes<Material>(matKey, false);
                            entityModel.transform.GetChild(j).GetComponentInChildren<MeshRenderer>().material = mat;
                        }
                    }
                    break;
                case EntityModelType.Door:
                    entityModel = borderEntityData.entityAxis == 0 ? LoadAssetsByAddressable.GetInstance.GetEntityRes("DoorX") : LoadAssetsByAddressable.GetInstance.GetEntityRes("DoorY");
                    break;
                case EntityModelType.Floor:
                    entityModel = LoadAssetsByAddressable.GetInstance.GetEntityRes("Floor");
                    matID = GetRoomMatID(borderEntityData.listRoomType[0], borderEntityData.listRoomTypeID[0], borderEntityData.entityModelType, ref roomMatDatas);
                    matKey = "Floor_" + matID;
                    mat = LoadAssetsByAddressable.GetInstance.GetRes<Material>(matKey, false);
                    entityModel.GetComponentInChildren<MeshRenderer>().material = mat;
                    break;
                case EntityModelType.Ceiling:
                    entityModel = LoadAssetsByAddressable.GetInstance.GetEntityRes("Ceiling");
                    break;
                default:
                    break;
            }
            if (entityModel != null)
            {
                GameObject clone = Instantiate(entityModel, new Vector3(borderEntityData.pos.x, 0, borderEntityData.pos.y), Quaternion.identity);
                clone.name = entityModel?.name + "_" + borderEntityData.pos.x + "_" + borderEntityData.pos.y;
                clone.transform.parent = GetRoomRootNode(borderEntityData.listRoomType[0]).transform; //TODO listRoomType[0]
                borderEntityData.entity = clone;
            }
        }

        //缓存地板、墙壁材质
        DataSave.GetInstance.SaveRoomMatData(roomMatDatas);
    }

    /// <summary>
    /// 获取房间的地板、墙壁材质ID
    /// </summary>
    /// <param name="roomType"></param>
    /// <param name="entityModelType"></param>
    /// <param name="roomMatDatas"></param>
    /// <returns></returns>
    private int GetRoomMatID(RoomType roomType, string roomID, EntityModelType entityModelType, ref List<RoomMatData> roomMatDatas)
    {
        //0-随机材质 1-10指定材质
        int curRoomMatIndex = 0;
        int maxID = 6;
        if (roomMatDatas == null)
        {
            roomMatDatas = new List<RoomMatData>();
        }
        RoomMatData roomMatData = roomMatDatas?.Find((p) => { return p.roomID == roomID; });
        switch (entityModelType)
        {
            case EntityModelType.Wall:
            case EntityModelType.SmallWall:
                if (roomMatData == null)
                {
                    Debugger.LogWarning("未找到该房间类型的墙壁材质 roomID:" + roomID + ",roomType:" + roomType);
                    curRoomMatIndex = UnityEngine.Random.Range(1, maxID + 1);
                    roomMatDatas.Add(new RoomMatData { roomType = roomType, roomID = roomID, matIDWall = curRoomMatIndex, matIDFloor = -1 });
                }
                else
                {
                    if (roomMatData?.matIDWall <= 0)
                    {
                        curRoomMatIndex = UnityEngine.Random.Range(1, maxID + 1);
                        roomMatData.matIDWall = curRoomMatIndex;
                    }
                    else
                    {
                        curRoomMatIndex = roomMatData.matIDWall;
                    }
                }
                break;
            case EntityModelType.Floor:
                if (roomMatData == null)
                {
                    Debugger.LogError("未找到该房间类型的地板材质 roomType:" + roomType);
                    curRoomMatIndex = UnityEngine.Random.Range(1, maxID + 1);
                    roomMatDatas.Add(new RoomMatData { roomType = roomType, roomID = roomID, matIDWall = -1, matIDFloor = curRoomMatIndex });
                }
                else
                {
                    if (roomMatData?.matIDFloor <= 0)
                    {
                        curRoomMatIndex = UnityEngine.Random.Range(1, maxID + 1);
                        roomMatData.matIDFloor = curRoomMatIndex;
                    }
                    else
                    {
                        curRoomMatIndex = roomMatData.matIDFloor;
                    }
                }
                break;
            default:
                Debugger.LogError("entityModelType is border , entityModelType:" + entityModelType);
                break;
        }


        return curRoomMatIndex;

    }

    private GameObject GetRoomRootNode(RoomType roomType)
    {
        if (!m_DicRoomEntity.ContainsKey(roomType))
        {
            //当前房间的根节点
            GameObject roomRoot = new GameObject(roomType.ToString());
            roomRoot.transform.parent = m_RoomRootNode;
            roomRoot.transform.position = Vector3.zero;
            roomRoot.transform.rotation = Quaternion.identity;
            roomRoot.transform.localScale = Vector3.one;
            m_DicRoomEntity.Add(roomType, roomRoot);
        }
        return m_DicRoomEntity[roomType];
    }


    /// <summary>
    /// 清空所有房间模型、数据
    /// </summary>
    public void ClearRoom()
    {
        List<RoomType> roomTypes = new List<RoomType>();
        foreach (var item in m_DicRoomEntity.Keys)
        {
            roomTypes.Add(item);
        }
        for (int i = 0; i < roomTypes.Count; i++)
        {
            Destroy(m_DicRoomEntity[roomTypes[i]]);
            m_DicRoomEntity.Remove(roomTypes[i]);
        }
        m_DicBorderItemInfo.Clear();
    }
}

public enum RoomType
{
    Null = 0,
    /// <summary>
    /// 客厅
    /// </summary>
    LivingRoom,
    /// <summary>
    /// 浴室
    /// </summary>
    BathRoom,
    /// <summary>
    /// 卧室
    /// </summary>
    BedRoom,
    /// <summary>
    /// 厨房
    /// </summary>
    KitchenRoom,
    /// <summary>
    /// 书房
    /// </summary>
    StudyRoom,
    /// <summary>
    /// 储藏室
    /// </summary>
    StorageRoom,
    /// <summary>
    /// 办公室
    /// </summary>
    OfficeRoom,
    /// <summary>
    /// 茶水间
    /// </summary>
    TeaRoom,
    /// <summary>
    /// 大厅
    /// </summary>
    LobbyRoom,
    /// <summary>
    /// 实验室
    /// </summary>
    LabRoom
}

/// <summary>
/// 实体模型类型
/// </summary>
public enum EntityModelType
{
    //无墙
    Null,
    Wall,//轴心点在左下侧
    SmallWall,
    Door,
    //地砖
    Floor,
    //天花板
    Ceiling
}
