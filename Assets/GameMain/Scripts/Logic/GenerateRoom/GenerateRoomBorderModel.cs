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
        public GameObject entity = null;
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

        for (int i = 0; i < GenerateRoomData.GetInstance.listRoomBuilderInfo.Count; i++)
        {
            BorderEntityData borderEntityData = GenerateRoomData.GetInstance.listRoomBuilderInfo[i];
            GameObject entityModel = null;
            switch (borderEntityData.entityModelType)
            {
                case EntityModelType.Null:
                    break;
                case EntityModelType.Wall:
                    entityModel = borderEntityData.entityAxis == 0 ? ResourcesLoad.GetInstance.GetEntityRes("WallX") : ResourcesLoad.GetInstance.GetEntityRes("WallY");
                    break;
                case EntityModelType.Door:
                    entityModel = borderEntityData.entityAxis == 0 ? ResourcesLoad.GetInstance.GetEntityRes("DoorX") : ResourcesLoad.GetInstance.GetEntityRes("DoorY");
                    break;
                case EntityModelType.Floor:
                    entityModel = ResourcesLoad.GetInstance.GetEntityRes("Floor");
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
            if (borderEntityData.entityModelType == EntityModelType.Wall || borderEntityData.entityModelType == EntityModelType.Door)
            {
                GameObject modelWallSmall = borderEntityData.entityAxis == 0 ? ResourcesLoad.GetInstance.GetEntityRes("WallSmallX") : ResourcesLoad.GetInstance.GetEntityRes("WallSmallY");
                GameObject clone = Instantiate(modelWallSmall, new Vector3(borderEntityData.pos.x, 0, borderEntityData.pos.y), Quaternion.identity);
                //clone.name = entityModel?.name + "_" + borderEntityData.pos.x + "_" + borderEntityData.pos.y;
                clone.name = modelWallSmall.name + "_" + borderEntityData.pos.x + "_" + borderEntityData.pos.y;
                clone.transform.parent = GetRoomRootNode(borderEntityData.listRoomType[0]).transform; //TODO listRoomType[0]
            }
        }

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
    Null,
    /// <summary>
    /// 客厅
    /// </summary>
    LivingRoom,
    /// <summary>
    /// 浴室
    /// </summary>
    BathRoom,
    /// <summary>
    /// 主卧
    /// </summary>
    BedRoom,
    ///// <summary>
    ///// 侧卧
    ///// </summary>
    //SecondBedRoom,
    /// <summary>
    /// 厨房
    /// </summary>
    KitchenRoom,
    /// <summary>
    /// 书房
    /// </summary>
    StudyRoom,
    ///// <summary>
    ///// 阳台
    ///// </summary>
    //BalconyRoom,
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
