using System.Collections.Generic;
using UnityEngine;
using MFramework;

/// <summary>
/// 标题：房间边界模型(墙壁、门、地砖)自动生成器
/// 功能：1.生成模型，根据房间信息数据生成与之匹配的3D房间模型；2.清空场景模型
/// 作者：毛俊峰
/// 时间：2023.07.18-2023.07.20
/// </summary>
public class GenerateRoomBorderModel : SingletonByMono<GenerateRoomBorderModel>
{
    //TODO 后面根据ab包加载资源
    public GameObject modelWallX;
    public GameObject modelWallY;
    public GameObject modelDoorX;
    public GameObject modelDoorY;
    public GameObject modelFloor;
    public GameObject modelWallSmallX;
    public GameObject modelWallSmallY;

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
    public class RoomInfo
    {
        public RoomType roomType;
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
    public class BorderEntityData
    {
        public Vector2 pos;
        public EntityModelType entityModelType;
        /// <summary>
        /// 边界实体轴向，0-横向 1-纵向
        /// </summary>
        public int entityAxis;
        /// <summary>
        /// 当前实体所属房间类型，一个边界实体最多可属于两个房间，listRoomType.count<=2
        /// </summary>
        public List<RoomType> listRoomType;
    }

    private void Awake()
    {
        m_DicBorderItemInfo = new Dictionary<Vector2, BorderInfo>();
        m_DicRoomEntity = new Dictionary<RoomType, GameObject>();
    }
    /// <summary>
    /// 生成房间边界模型，对外接口
    /// </summary>
    /// <param name="roomInfo">需要传入房间的详细信息</param>
    /// <param name="borderEntityDatas">房间所有边界(墙、门、地板)数据</param>
    public void GenerateRoomBorder(List<BorderEntityData> borderEntityDatas)
    {
        ClearRoom();

        for (int i = 0; i < GenerateRoomData.GetInstance.listRoomBuilderInfo.Count; i++)
        {
            BorderEntityData borderEntityData = GenerateRoomData.GetInstance.listRoomBuilderInfo[i];
            GameObject entityModel = null;
            switch (borderEntityData.entityModelType)
            {
                case EntityModelType.Null:
                    break;
                case EntityModelType.Wall:
                    entityModel = borderEntityData.entityAxis == 0 ? modelWallX : modelWallY;
                    break;
                case EntityModelType.Door:
                    entityModel = borderEntityData.entityAxis == 0 ? modelDoorX : modelDoorY;
                    break;
                case EntityModelType.Floor:
                    entityModel = modelFloor;
                    break;
                default:
                    break;
            }
            if (entityModel != null)
            {
                GameObject clone = Instantiate(entityModel, new Vector3(borderEntityData.pos.x, 0, borderEntityData.pos.y), Quaternion.identity);
                clone.name = entityModel?.name + "_" + borderEntityData.pos.x + "_" + borderEntityData.pos.y;
                clone.transform.parent = GetRoomRootNode(borderEntityData.listRoomType[0]).transform; //TODO listRoomType[0]
            }
            if (borderEntityData.entityModelType == EntityModelType.Wall || borderEntityData.entityModelType == EntityModelType.Door)
            {
                GameObject modelWallSmall = borderEntityData.entityAxis == 0 ? modelWallSmallX : modelWallSmallY;
                GameObject clone = Instantiate(modelWallSmall, new Vector3(borderEntityData.pos.x, 0, borderEntityData.pos.y), Quaternion.identity);
                clone.name = entityModel?.name + "_" + borderEntityData.pos.x + "_" + borderEntityData.pos.y;
                clone.transform.parent = GetRoomRootNode(borderEntityData.listRoomType[0]).transform; //TODO listRoomType[0]
            }
        }

    }

    ///// <summary>
    ///// 生成房间模型，对外接口
    ///// </summary>
    ///// <param name="roomInfo">需要传入房间的详细信息</param>
    //public void GenerateRoom(params RoomInfo[] roomInfoArr)
    //{
    //    if (roomInfoArr.Length > 0)
    //    {
    //        ClearRoom();
    //    }
    //    for (int i = 0; i < roomInfoArr.Length; i++)
    //    {
    //        RoomInfo roomInfo = roomInfoArr[i];
    //        if (roomInfo.roomType == RoomType.Null || roomInfo.roomSize == null || roomInfo.roomPosMin == null || roomInfo.roomPosMax == null)
    //        {
    //            Debug.LogError("Generate Fail，roomType：" + roomInfo.roomType);
    //            Debug.LogError("Generate Fail，roomSize：" + roomInfo.roomSize);
    //            Debug.LogError("Generate Fail，roomPosMin：" + roomInfo.roomPosMin);
    //            Debug.LogError("Generate Fail，roomPosMax：" + roomInfo.roomPosMax);
    //            continue;
    //        }
    //        GameObject roomRootNode = GetRoomRootNode(roomInfoArr[i].roomType);

    //        //获取当前房间的所有边界实体信息
    //        List<BorderEntityData> borderItemPosInfoData = GetRoomBorderInfo(roomInfo);
    //        for (int j = 0; j < borderItemPosInfoData?.Count; j++)
    //        {
    //            //判定当前位置是否已存在同种类型实体，存在则弹出并缓存其所属的房间类型，反之不重复生成实体。判定条件 实体坐标、轴向、类型一致
    //            if (JudgeBorderEntityDataIsExist(borderItemPosInfoData[j], roomInfo.roomType))
    //            {
    //                continue;
    //            }

    //            GameObject targetItem = null;
    //            switch (borderItemPosInfoData[j].entityModelType)
    //            {
    //                case EntityModelType.Null:
    //                    break;
    //                case EntityModelType.Wall:
    //                    targetItem = borderItemPosInfoData[j].entityAxis == 0 ? wallX : wallY;
    //                    break;
    //                case EntityModelType.Door:
    //                    targetItem = borderItemPosInfoData[j].entityAxis == 0 ? doorX : doorY;
    //                    break;
    //            }
    //            if (targetItem != null)
    //            {
    //                GameObject clone = Instantiate(targetItem, new Vector3(borderItemPosInfoData[j].pos.x, 0, borderItemPosInfoData[j].pos.y), Quaternion.identity);
    //                clone.name = targetItem?.name + "_" + borderItemPosInfoData[j].pos.x + "_" + borderItemPosInfoData[j].pos.y;
    //                clone.transform.parent = roomRootNode.transform;
    //            }
    //            //缓存当前生成的边界实体信息
    //            CacheBorderEntityData(borderItemPosInfoData[j], roomInfo.roomType);
    //        }
    //    }

    //    foreach (BorderInfo borderInfo in GenerateRoomData.GetInstance.dicRoomFloorInfo.Values)
    //    {
    //        GenerateFloorEntity(borderInfo.borderItemPosInfoX.pos, borderInfo.borderItemPosInfoX.listRoomType[0]);
    //    }
    //}

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

    ///// <summary>
    ///// 判定当前位置是否已存在同种类型实体，存在则弹出并缓存其所属的房间类型，反之不重复生成实体。判定条件 实体坐标、轴向、类型一致
    ///// </summary>
    ///// <param name="borderEntityData"></param>
    ///// <returns></returns>
    //public bool JudgeBorderEntityDataIsExist(BorderEntityData borderEntityData, RoomType curRoomType)
    //{
    //    bool curEntityIsExist = false;
    //    //判定当前位置是否已生成实体对象，根据之前生成实体时所缓存的信息查询
    //    if (m_DicBorderItemInfo.ContainsKey(borderEntityData.pos))
    //    {
    //        BorderInfo curCacheBorderInfo = m_DicBorderItemInfo[borderEntityData.pos];

    //        //判定当前位置是否已存在同种类型实体，存在则弹出不重复生成实体，判定条件 实体坐标、轴向、类型一致
    //        if (borderEntityData.entityAxis == 0 && curCacheBorderInfo?.borderItemPosInfoX?.entityModelType == borderEntityData.entityModelType)
    //        {
    //            Debug.Log("cur pos exist entityX:" + curCacheBorderInfo.borderItemPosInfoX?.pos + "," + curCacheBorderInfo.borderItemPosInfoX?.entityAxis);
    //            if (!curCacheBorderInfo.borderItemPosInfoX.listRoomType.Contains(curRoomType))
    //            {
    //                curCacheBorderInfo.borderItemPosInfoX.listRoomType.Add(curRoomType);
    //            }
    //            curEntityIsExist = true;
    //        }
    //        else if (borderEntityData.entityAxis == 1 && curCacheBorderInfo?.borderItemPosInfoY?.entityModelType == borderEntityData.entityModelType)
    //        {
    //            Debug.Log("cur pos exist entityY:" + curCacheBorderInfo.borderItemPosInfoY?.pos + "," + curCacheBorderInfo.borderItemPosInfoY?.entityAxis);
    //            if (!curCacheBorderInfo.borderItemPosInfoY.listRoomType.Contains(curRoomType))
    //            {
    //                curCacheBorderInfo.borderItemPosInfoY.listRoomType.Add(curRoomType);
    //            }
    //            curEntityIsExist = true;
    //        }
    //    }
    //    return curEntityIsExist;
    //}

    ///// <summary>
    ///// 缓存实体数据信息到字典
    ///// </summary>
    ///// <param name="borderEntityData"></param>
    ///// <param name="curRoomType"></param>
    //public void CacheBorderEntityData(BorderEntityData borderEntityData, RoomType curRoomType)
    //{
    //    if (!m_DicBorderItemInfo.ContainsKey(borderEntityData.pos))
    //    {
    //        //Debug.LogError("pos:" + borderItemPosInfoData[i].pos + " data is exist");
    //        BorderInfo borderInfo = new BorderInfo();
    //        m_DicBorderItemInfo.Add(borderEntityData.pos, borderInfo);
    //    }
    //    if (borderEntityData.entityAxis == 0)
    //    {
    //        m_DicBorderItemInfo[borderEntityData.pos].borderItemPosInfoX = new BorderEntityData
    //        {
    //            entityAxis = borderEntityData.entityAxis,
    //            pos = borderEntityData.pos,
    //            entityModelType = borderEntityData.entityModelType,
    //            listRoomType = new List<RoomType> { curRoomType }
    //        };
    //    }
    //    else if (borderEntityData.entityAxis == 1)
    //    {
    //        m_DicBorderItemInfo[borderEntityData.pos].borderItemPosInfoY = new BorderEntityData
    //        {
    //            entityAxis = borderEntityData.entityAxis,
    //            pos = borderEntityData.pos,
    //            entityModelType = borderEntityData.entityModelType,
    //            listRoomType = new List<RoomType> { curRoomType }
    //        };
    //    }
    //}

    ///// <summary>
    ///// 生成地砖实体模型
    ///// </summary>
    ///// <param name="pos"></param>
    ///// <param name="roomType"></param>
    //public void GenerateFloorEntity(Vector2 pos, RoomType roomType)
    //{
    //    GameObject clone = Instantiate(modelFloor, new Vector3(pos.x, 0, pos.y), Quaternion.identity);
    //    GameObject floorGroup = GetRoomRootNode(roomType);

    //    clone.name = "floor_" + pos.x + "_" + pos.y;
    //    clone.transform.SetParent(floorGroup.transform);
    //}
}

public enum RoomType
{
    Null,
    /// <summary>
    /// 客厅
    /// </summary>
    LivingRoom,
    /// <summary>
    /// 浴室
    /// </summary>
    RestRoom,
    /// <summary>
    /// 主卧
    /// </summary>
    FirstBedRoom,
    /// <summary>
    /// 侧卧
    /// </summary>
    SecondBedRoom,
    /// <summary>
    /// 厨房
    /// </summary>
    KitChenRoom,
    /// <summary>
    /// 书房
    /// </summary>
    StudyRoom,
    /// <summary>
    /// 阳台
    /// </summary>
    BalconyRoom,
    /// <summary>
    /// 储藏室
    /// </summary>
    StorageRoom,
}

/// <summary>
/// 实体模型类型
/// </summary>
public enum EntityModelType
{
    //无墙
    Null,
    Wall,//轴心点在左下侧
    Door,
    //地砖
    Floor
}
