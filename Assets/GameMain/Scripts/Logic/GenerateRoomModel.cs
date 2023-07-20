using System.Collections.Generic;
using UnityEngine;
using MFramework;

/// <summary>
/// 标题：房间模型布局自动生成器
/// 功能：1.生成模型，根据房间信息数据生成与之匹配的3D房间模型；2.清空场景模型
/// 作者：毛俊峰
/// 时间：2023.07.18-2023.07.20
/// </summary>
public class GenerateRoomModel : SingletonByMono<GenerateRoomModel>
{
    //TODO 后面根据ab包加载资源
    public GameObject wallX;
    public GameObject wallY;
    public GameObject doorX;
    public GameObject doorY;
    private Transform m_RoomGroup;
    public Transform RoomGroup
    {
        get { return m_RoomGroup; }
        set { m_RoomGroup = value; }
    }
    /// <summary>
    /// 缓存已生成实体的数据信息 k-实体坐标(唯一) v-当前实体坐标对于的实体信息
    /// </summary>
    private Dictionary<Vector2, BorderInfo> m_DicBorderItemInfo;

    /// <summary>
    /// 缓存所有房间实体    k-房间类型 v-实体
    /// </summary>
    private Dictionary<RoomType, GameObject> m_DicRoomEntity;
    public struct RoomInfo
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
    /// 生成房间模型，对外接口
    /// </summary>
    /// <param name="roomInfo"></param>
    public void GenerateRoom(RoomInfo roomInfo)
    {
        if (roomInfo.roomType == RoomType.Null || roomInfo.roomSize == null || roomInfo.roomPosMin == null || roomInfo.roomPosMax == null)
        {
            Debug.LogError("Generate Fail，roomType：" + roomInfo.roomType);
            Debug.LogError("Generate Fail，roomSize：" + roomInfo.roomSize);
            Debug.LogError("Generate Fail，roomPosMin：" + roomInfo.roomPosMin);
            Debug.LogError("Generate Fail，roomPosMax：" + roomInfo.roomPosMax);
        }
        //当前房间的根节点
        GameObject roomRoot = new GameObject(roomInfo.roomType.ToString());
        roomRoot.transform.parent = m_RoomGroup;
        roomRoot.transform.position = Vector3.zero;
        roomRoot.transform.rotation = Quaternion.identity;
        roomRoot.transform.localScale = Vector3.one;
        if (!m_DicRoomEntity.ContainsKey(roomInfo.roomType))
        {
            m_DicRoomEntity.Add(roomInfo.roomType, roomRoot);
        }
        else
        {
            Debug.LogError("cur roomType is exist,roomType:" + roomInfo.roomType);
        }

        //获取当前房间的所有边界实体信息
        List<BorderEntityData> borderItemPosInfoData = GetRoomBorderInfo(roomInfo);
        for (int i = 0; i < borderItemPosInfoData?.Count; i++)
        {
            //判定当前位置是否已存在同种类型实体，存在则弹出并缓存其所属的房间类型，反之不重复生成实体。判定条件 实体坐标、轴向、类型一致
            if (JudgeBorderEntityDataIsExist(borderItemPosInfoData[i], roomInfo.roomType))
            {
                continue;
            }

            GameObject targetItem = null;
            switch (borderItemPosInfoData[i].entityModelType)
            {
                case EntityModelType.Null:
                    break;
                case EntityModelType.Wall:
                    targetItem = borderItemPosInfoData[i].entityAxis == 0 ? wallX : wallY;
                    break;
                case EntityModelType.Door:
                    targetItem = borderItemPosInfoData[i].entityAxis == 0 ? doorX : doorY;
                    break;
            }
            if (targetItem != null)
            {
                GameObject clone = Instantiate(targetItem, new Vector3(borderItemPosInfoData[i].pos.x, 0, borderItemPosInfoData[i].pos.y), Quaternion.identity);
                clone.name = targetItem?.name + "_" + borderItemPosInfoData[i].pos.x + "_" + borderItemPosInfoData[i].pos.y;
                clone.transform.parent = roomRoot.transform;
            }
            //缓存当前生成的边界实体信息
            CacheBorderEntityData(borderItemPosInfoData[i], roomInfo.roomType);
        }
    }

    /// <summary>
    /// 清空指定房间
    /// </summary>
    /// <param name="roomType"></param>
    public void ClearRoom(RoomType roomType)
    {
        bool isExistTargetRoom = false;
        foreach (var item in m_DicRoomEntity.Keys)
        {
            if (item == roomType)
            {
                isExistTargetRoom = true;
                break;
            }
        }
        if (isExistTargetRoom)
        {
            Destroy(m_DicRoomEntity[roomType]);
            m_DicRoomEntity.Remove(roomType);
            
        }
        else
        {
            Debug.LogError("remove fail,targetRoomType not find,roomType:" + roomType);
        }
        //TODO
        //m_DicBorderItemInfo.Remove
    }

    /// <summary>
    /// 清空所有房间
    /// </summary>
    public void ClearRoom()
    {
        List<RoomType> roomTypes = new List<RoomType>();
        foreach (var item in m_DicRoomEntity.Keys)
        {
            roomTypes.Add(item);
        }
        for (int i = 0;i< roomTypes.Count;i++) 
        {
            Destroy(m_DicRoomEntity[roomTypes[i]]);
            m_DicRoomEntity.Remove(roomTypes[i]);
        }

        m_DicBorderItemInfo.Clear();
    }

    /// <summary>
    /// 判定当前位置是否已存在同种类型实体，存在则弹出并缓存其所属的房间类型，反之不重复生成实体。判定条件 实体坐标、轴向、类型一致
    /// </summary>
    /// <param name="borderEntityData"></param>
    /// <returns></returns>
    private bool JudgeBorderEntityDataIsExist(BorderEntityData borderEntityData, RoomType curRoomType)
    {
        bool curEntityIsExist = false;
        //判定当前位置是否已生成实体对象，根据之前生成实体时所缓存的信息查询
        if (m_DicBorderItemInfo.ContainsKey(borderEntityData.pos))
        {
            BorderInfo curCacheBorderInfo = m_DicBorderItemInfo[borderEntityData.pos];

            //判定当前位置是否已存在同种类型实体，存在则弹出不重复生成实体，判定条件 实体坐标、轴向、类型一致
            if (borderEntityData.entityAxis == 0 && curCacheBorderInfo?.borderItemPosInfoX?.entityModelType == borderEntityData.entityModelType)
            {
                Debug.Log("cur pos exist entityX:" + curCacheBorderInfo.borderItemPosInfoX?.pos + "," + curCacheBorderInfo.borderItemPosInfoX?.entityAxis);
                if (!curCacheBorderInfo.borderItemPosInfoX.listRoomType.Contains(curRoomType))
                {
                    curCacheBorderInfo.borderItemPosInfoX.listRoomType.Add(curRoomType);
                }
                curEntityIsExist = true;
            }
            else if (borderEntityData.entityAxis == 1 && curCacheBorderInfo?.borderItemPosInfoY?.entityModelType == borderEntityData.entityModelType)
            {
                Debug.Log("cur pos exist entityY:" + curCacheBorderInfo.borderItemPosInfoY?.pos + "," + curCacheBorderInfo.borderItemPosInfoY?.entityAxis);
                if (!curCacheBorderInfo.borderItemPosInfoY.listRoomType.Contains(curRoomType))
                {
                    curCacheBorderInfo.borderItemPosInfoY.listRoomType.Add(curRoomType);
                }
                curEntityIsExist = true;
            }
        }

        return curEntityIsExist;
    }

    /// <summary>
    /// 缓存实体数据信息到字典
    /// </summary>
    /// <param name="borderEntityData"></param>
    /// <param name="curRoomType"></param>
    private void CacheBorderEntityData(BorderEntityData borderEntityData, RoomType curRoomType)
    {
        if (!m_DicBorderItemInfo.ContainsKey(borderEntityData.pos))
        {
            //Debug.LogError("pos:" + borderItemPosInfoData[i].pos + " data is exist");
            m_DicBorderItemInfo.Add(borderEntityData.pos, new BorderInfo());
        }
        if (borderEntityData.entityAxis == 0)
        {
            m_DicBorderItemInfo[borderEntityData.pos].borderItemPosInfoX = new BorderEntityData
            {
                entityAxis = borderEntityData.entityAxis,
                pos = borderEntityData.pos,
                entityModelType = borderEntityData.entityModelType,
                listRoomType = new List<RoomType> { curRoomType }
            };
        }
        else if (borderEntityData.entityAxis == 1)
        {
            m_DicBorderItemInfo[borderEntityData.pos].borderItemPosInfoY = new BorderEntityData
            {
                entityAxis = borderEntityData.entityAxis,
                pos = borderEntityData.pos,
                entityModelType = borderEntityData.entityModelType,
                listRoomType = new List<RoomType> { curRoomType }
            };
        }
    }

    /// <summary>
    /// 获取房间横向x轴所占矩形的坐标
    /// 根据左下，右上位置，获取整个矩形各个坐标，坐标间隔1米
    /// 左闭右开 [roomPosMin，roomPosMax)
    /// </summary>
    /// <param name="roomPosMin"></param>
    /// <param name="roomPosMax"></param>
    /// <returns></returns>
    private List<BorderEntityData> GetRoomBorderInfo(RoomInfo roomInfo)
    {
        List<BorderEntityData> borderItemPosInfos = new List<BorderEntityData>();

        for (int i = (int)roomInfo.roomPosMin.x; i < (int)roomInfo.roomPosMax.x; i++)
        {
            int curX = i;
            borderItemPosInfos.Add(new BorderEntityData
            {
                entityAxis = 0,
                pos = new Vector2(curX, roomInfo.roomPosMin.y),
                entityModelType = EntityModelType.Wall
            });
            borderItemPosInfos.Add(new BorderEntityData
            {
                entityAxis = 0,
                pos = new Vector2(curX, roomInfo.roomPosMax.y),
                entityModelType = EntityModelType.Wall
            });


        }

        for (int i = (int)roomInfo.roomPosMin.y; i < (int)roomInfo.roomPosMax.y; i++)
        {
            int curY = i;
            borderItemPosInfos.Add(new BorderEntityData
            {
                entityAxis = 1,
                pos = new Vector2(roomInfo.roomPosMin.x, curY),
                entityModelType = EntityModelType.Wall
            });
            borderItemPosInfos.Add(new BorderEntityData
            {
                entityAxis = 1,
                pos = new Vector2(roomInfo.roomPosMax.x, curY),
                entityModelType = EntityModelType.Wall
            });
        }

        for (int i = 0; i < roomInfo.listDoorPosInfo?.Count; i++)
        {
            int temp = i;
            BorderEntityData borderItemPosInfo = borderItemPosInfos.Find((p) => { return p.pos == roomInfo.listDoorPosInfo[temp].pos && p.entityAxis == roomInfo.listDoorPosInfo[temp].entityAxis; });
            borderItemPosInfo.entityModelType = roomInfo.listDoorPosInfo[temp].entityModelType;
            Debug.Log("更正实体类型：" + borderItemPosInfo.pos + "," + borderItemPosInfo.entityModelType);
        }

        for (int i = 0; i < roomInfo.listEmptyPosInfo?.Count; i++)
        {
            int temp = i;
            BorderEntityData borderItemPosInfo = borderItemPosInfos.Find((p) => { return p.pos == roomInfo.listEmptyPosInfo[temp].pos && p.entityAxis == roomInfo.listEmptyPosInfo[temp].entityAxis; });
            borderItemPosInfo.entityModelType = roomInfo.listEmptyPosInfo[temp].entityModelType;
            Debug.Log("更正实体类型：" + borderItemPosInfo.pos + "," + borderItemPosInfo.entityModelType);

        }
        return borderItemPosInfos;

    }

    /// <summary>
    /// 获取房间横向x轴所占矩形的坐标
    /// 根据左下，右上位置，获取整个矩形各个坐标，坐标间隔1米
    /// 左闭右开 [roomPosMin，roomPosMax)
    /// </summary>
    /// <param name="roomPosMin"></param>
    /// <param name="roomPosMax"></param>
    /// <returns></returns>
    private List<Vector2> GetRoomAllXAxisPos(Vector2 roomPosMin, Vector2 roomPosMax)
    {
        List<Vector2> result = new List<Vector2>();
        for (int i = (int)roomPosMin.x; i < (int)roomPosMax.x; i++)
        {
            int curX = i;
            result.Add(new Vector2(curX, roomPosMin.y));
            result.Add(new Vector2(curX, roomPosMax.y));
        }

        //TEST
        foreach (var item in result)
        {
            Debug.Log("pos:" + item);
        }
        return result;
    }

    /// <summary>
    /// 获取房间横向y轴所占矩形的坐标
    /// 根据左下，右上位置，获取整个矩形各个坐标，坐标间隔1米
    /// 左闭右开 [roomPosMin，roomPosMax)
    /// </summary>
    /// <param name="roomPosMin"></param>
    /// <param name="roomPosMax"></param>
    /// <returns></returns>
    private List<Vector2> GetRoomAllYAxisPos(Vector2 roomPosMin, Vector2 roomPosMax)
    {
        List<Vector2> result = new List<Vector2>();
        for (int i = (int)roomPosMin.y; i < (int)roomPosMax.y; i++)
        {
            int curY = i;
            result.Add(new Vector2(roomPosMin.x, curY));
            result.Add(new Vector2(roomPosMax.x, curY));

        }

        //TEST
        foreach (var item in result)
        {
            Debug.Log("pos:" + item);
        }
        return result;
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
    Wall,
    Door,
}
