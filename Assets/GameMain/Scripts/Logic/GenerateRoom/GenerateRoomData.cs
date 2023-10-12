using System.Collections.Generic;
using UnityEngine;
using static GenerateRoomBorderModel;
using MFramework;
using System.Linq;
using System;

/// <summary>
/// 标题：生成房间信息数据，为生成房间模型服务
/// 功能：根据各个房间之间的联系，创建合理的随机数据
/// 作者：毛俊峰
/// 功能：提供随机生成合理的房间墙、门、地砖数据
/// 规则：随机房间：1.随机墙，分横墙和竖墙，两者锚点都在左下侧，按边长为1的坐标来划分，2.根据之前已生成房间的位置判定当前生成位置的合理性
///       随机门逻辑：1.每个房间有且只生成一个属于自己房间的门，但是一个房间允许出现多个们，2.门的位置必须在相邻房间的公共墙里，3.若一个房间邻接多个房间，多个房间中包含客厅则在邻接客厅墙中创建门，否则在多个房间邻接墙中随机创建门
/// 时间：2023.07.21-2023.07.28
/// </summary>
public class GenerateRoomData : SingletonByMono<GenerateRoomData>
{
    /// <summary>
    /// 缓存房间所有边界(墙、门、地板)数据
    /// </summary>
    public List<BorderEntityData> listRoomBuilderInfo = new List<BorderEntityData>();

    /// <summary>
    /// 缓存房间基础信息
    /// </summary>
    private List<RoomBaseInfo> m_ListBaseRoomInfos = new List<RoomBaseInfo>();
    /// <summary>
    /// 缓存已经建造的房间信息
    /// </summary>
    public List<RoomInfo> m_ListRoomInfo = new List<RoomInfo>();

    /// <summary>
    /// 各个坐标的实体信息，供生成对于模型使用， k-实体坐标(唯一) v-当前实体坐标对于的实体信息
    /// </summary>
    private Dictionary<Vector2, BorderInfo> m_DicRoomWallInfo = new Dictionary<Vector2, BorderInfo>();

    /// <summary>
    /// 房间基础信息
    /// </summary>
    public class RoomBaseInfo
    {
        public RoomType curRoomType;
        /// <summary>
        /// 当前房间ID（唯一标识）
        /// </summary>
        public string curRoomID;
        /// <summary>
        /// 房间长宽，单位米(正整数)
        /// </summary>
        public uint[] roomSize;
        /// <summary>
        /// 与其他房间关系
        /// </summary>
        public List<RoomsDirRelation> targetRoomsDirRelation;
    }

    /// <summary>
    /// 当前房间与目标房间之间的方位关系
    /// </summary>
    public class RoomsDirRelation
    {
        /// <summary>
        /// 目标房间的类型
        /// </summary>
        public RoomType targetRoomType;
        /// <summary>
        /// 目标房间ID（唯一标识）
        /// </summary>
        public string targetRoomID;
        /// <summary>
        /// 当前房间在目标房间的方位关系
        /// </summary>
        public DirEnum locationRelation;
        /// <summary>
        /// 当前房间与目标房间共用一面墙（字段未使用暂时保留）
        /// </summary>
        public bool isCommonWall;
    }

    /// <summary>
    /// 方位标识
    /// </summary>
    public enum DirEnum
    {
        Top = 0,
        Left,
        Bottom,
        Right
    }


    /// <summary>
    /// 生成随机房间信息，根据当前房间和与其他房间的关系信息 生成RoomInfo对象
    /// </summary>
    /// <param name="callback">随机生成房间基础数据</param>
    /// <param name="roomBaseInfos">生成完成回调 P-房间各个节点位置的详细信息，生成失败则为null</param>
    public void GenerateRandomRoomInfoData(List<RoomBaseInfo> roomBaseInfos, Action<List<BorderEntityData>, List<RoomInfo>> callback)
    {
        if (roomBaseInfos == null || roomBaseInfos.Count == 0)
        {
            callback(null, null);
            return;
        }

        ClearRoom();
        GenerateRoomBorderModel.GetInstance.ClearRoom();

        UpadteRoomsDirRelation(ref roomBaseInfos);

        List<RoomBaseInfo> roomBaseInfosClone = new List<RoomBaseInfo>();
        for (int i = 0; i < roomBaseInfos.Count; i++)
        {
            roomBaseInfosClone.Add(roomBaseInfos[i]);
        }

        bool isGenerateSuc = true;

        m_ListRoomInfo = new List<RoomInfo>();

        //找到客厅房间放置在坐标系，房间左下角在原点，向第一象限延申
        RoomBaseInfo livingRoomBaseInfo = roomBaseInfos.Find((p) => { return p.curRoomType == RoomType.LivingRoom; });
        string firstGenerateID = livingRoomBaseInfo.curRoomID;
        RoomInfo livingRoomInfo = new RoomInfo
        {
            roomType = livingRoomBaseInfo.curRoomType,
            roomSize = livingRoomBaseInfo.roomSize,
            roomPosMin = new Vector2(0, 0),
            roomPosMax = new Vector2(livingRoomBaseInfo.roomSize[0], livingRoomBaseInfo.roomSize[1]),
            listDoorPosInfo = null, //TODO
            listEmptyPosInfo = null, //TODO
            roomID = firstGenerateID,
        };
        m_ListRoomInfo.Add(livingRoomInfo);
        UpdateRoomBuilderInfo(livingRoomInfo);
        roomBaseInfos.Remove(livingRoomBaseInfo);

        int loopCpuntMax = 30;
        int curLoopCount = 0;
        //遍历其他房间
        for (int i = 0; roomBaseInfos.Count > 0;)
        {
            curLoopCount++;
            //容错 避免死循环
            if (curLoopCount > loopCpuntMax)
            {
                callback(null, null);
                return;
            }
            //邻接的房间是否全部都已生成，未完成则先创建其他的房间
            bool isExist = true;
            if (i >= roomBaseInfos.Count)
            {
                i = 0;
            }
            foreach (var item in roomBaseInfos[i].targetRoomsDirRelation)
            {
                //如果当前房间有邻接房间已创建，则允许该当前房间创建
                if (m_ListRoomInfo.Find((p) => { return p.roomType == item.targetRoomType; }) != null)
                {
                    isExist = true;
                    break;
                }
                else
                {
                    isExist = false;
                    //当前房间所邻接的房间未创建时，优先创建所邻接的房间
                    for (int j = 0; j < roomBaseInfos.Count; j++)
                    {
                        if (roomBaseInfos[j].curRoomType == item.targetRoomType)
                        {
                            i = j;
                            break;
                        }
                    }
                }
            }
            if (!isExist)
            {
                //i++;
                continue;
            }
            //Debug.Log(" Cur Generate RoomType:" + roomBaseInfos[i].curRoomType);
            //根据房间邻接的边界信息，给出roomPosMin，roomPosMax
            List<Vector2> pos = GetRandomRoomPosByDirRelaateion(roomBaseInfos[i], (p) =>
            {
                if (!p)
                {
                    isGenerateSuc = false;
                }
            });
            RoomInfo roomInfo = new RoomInfo
            {
                roomType = roomBaseInfos[i].curRoomType,
                roomSize = roomBaseInfos[i].roomSize,
                roomPosMin = pos[0],
                roomPosMax = pos[1],
                listDoorPosInfo = null, //TODO
                listEmptyPosInfo = null, //TODO
                roomID = roomBaseInfos[i].curRoomID
            };
            m_ListRoomInfo.Add(roomInfo);
            UpdateRoomBuilderInfo(roomInfo);
            roomBaseInfos.Remove(roomBaseInfos[i]);
        }
        if (!isGenerateSuc)
        {
            callback(null, null);
            return;
        }


        CacheRoomWallInfo(m_ListRoomInfo.ToArray());

        GenerateDoorData(roomBaseInfosClone, firstGenerateID, ref m_ListRoomInfo);

        //Debug.Log("房间邻接的边界信息生成成功！");
        callback(listRoomBuilderInfo, m_ListRoomInfo);
    }

    /// <summary>
    /// 清理房间数据
    /// </summary>
    public void ClearRoom()
    {
        m_ListRoomInfo.Clear();
        m_ListBaseRoomInfos.Clear();
        listRoomBuilderInfo.Clear();
        m_DicRoomWallInfo.Clear();
    }

    #region 边界信息

    /// <summary>
    /// 更新房间已建造的房间位置信息
    /// </summary>
    /// <param name="roomInfo"></param>
    private void UpdateRoomBuilderInfo(RoomInfo roomInfo)
    {
        //更新房间边界信息
        List<BorderEntityData> listBorderEntityData = GetRoomBorderInfo(roomInfo);
        listRoomBuilderInfo = listRoomBuilderInfo.Union(listBorderEntityData).ToList();

        //缓存房间矩形内部占地信息，为下个房间随机位置是否合法做准备
        CacheRoomFloorBorderEntiryData(roomInfo);
    }

    /// <summary>
    /// 获取房间横向x轴所占矩形的坐标
    /// 根据左下，右上位置，获取整个矩形各个坐标，坐标间隔1米
    /// 左闭右开 [roomPosMin，roomPosMax)
    /// </summary>
    /// <param name="roomPosMin"></param>
    /// <param name="roomPosMax"></param>
    /// <returns></returns>
    public List<BorderEntityData> GetRoomBorderInfo(RoomInfo roomInfo)
    {
        List<BorderEntityData> borderItemPosInfos = new List<BorderEntityData>();

        for (int i = (int)roomInfo.roomPosMin.x; i < (int)roomInfo.roomPosMax.x; i++)
        {
            int curX = i;
            borderItemPosInfos.Add(new BorderEntityData
            {
                entityAxis = 0,
                pos = new Vector2(curX, roomInfo.roomPosMin.y),
                entityModelType = EntityModelType.Wall,
                listRoomType = new List<RoomType> { roomInfo.roomType }
            });
            borderItemPosInfos.Add(new BorderEntityData
            {
                entityAxis = 0,
                pos = new Vector2(curX, roomInfo.roomPosMax.y),
                entityModelType = EntityModelType.Wall,
                listRoomType = new List<RoomType> { roomInfo.roomType }
            });
        }

        for (int i = (int)roomInfo.roomPosMin.y; i < (int)roomInfo.roomPosMax.y; i++)
        {
            int curY = i;
            borderItemPosInfos.Add(new BorderEntityData
            {
                entityAxis = 1,
                pos = new Vector2(roomInfo.roomPosMin.x, curY),
                entityModelType = EntityModelType.Wall,
                listRoomType = new List<RoomType> { roomInfo.roomType }
            });
            borderItemPosInfos.Add(new BorderEntityData
            {
                entityAxis = 1,
                pos = new Vector2(roomInfo.roomPosMax.x, curY),
                entityModelType = EntityModelType.Wall,
                listRoomType = new List<RoomType> { roomInfo.roomType }
            });
        }

        for (int i = 0; i < roomInfo.listDoorPosInfo?.Count; i++)
        {
            int temp = i;
            BorderEntityData borderItemPosInfo = borderItemPosInfos.Find((p) => { return p.pos == roomInfo.listDoorPosInfo[temp].pos && p.entityAxis == roomInfo.listDoorPosInfo[temp].entityAxis; });
            borderItemPosInfo.entityModelType = roomInfo.listDoorPosInfo[temp].entityModelType;
            //Debug.Log("更正实体类型：" + borderItemPosInfo.pos + "," + borderItemPosInfo.entityModelType);
        }

        for (int i = 0; i < roomInfo.listEmptyPosInfo?.Count; i++)
        {
            int temp = i;
            BorderEntityData borderItemPosInfo = borderItemPosInfos.Find((p) => { return p.pos == roomInfo.listEmptyPosInfo[temp].pos && p.entityAxis == roomInfo.listEmptyPosInfo[temp].entityAxis; });
            borderItemPosInfo.entityModelType = roomInfo.listEmptyPosInfo[temp].entityModelType;
            //Debug.Log("更正实体类型：" + borderItemPosInfo.pos + "," + borderItemPosInfo.entityModelType);
        }
        return borderItemPosInfos;

    }

    /// <summary>
    /// 缓存地砖实体数据信息到字典
    /// </summary>
    public void CacheRoomFloorBorderEntiryData(params RoomInfo[] roomInfos)
    {
        for (int m = 0; m < roomInfos.Length; m++)
        {
            RoomInfo roomInfo = roomInfos[m];
            for (int i = (int)roomInfo.roomPosMin.x; i < (int)roomInfo.roomPosMax.x; i++)
            {
                for (int j = (int)roomInfo.roomPosMin.y; j < (int)roomInfo.roomPosMax.y; j++)
                {
                    Vector2 pos = new Vector2(i, j);
                    listRoomBuilderInfo.Add(new BorderEntityData
                    {
                        entityAxis = 0,
                        pos = pos,
                        listRoomType = new List<RoomType> { roomInfo.roomType },
                        entityModelType = EntityModelType.Floor
                    });
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// 获取房间随机坐标min、max根据房间的方位信息
    /// </summary>
    /// <param name="roomBaseInfo"></param>
    /// <param name="callback">生成随机坐标完成回调，p=> T-生成成功 F-生成失败</param>
    private List<Vector2> GetRandomRoomPosByDirRelaateion(RoomBaseInfo roomBaseInfo, Action<bool> callback)
    {
        List<Vector2> res = new List<Vector2>();
        Vector2 minPos = Vector2.zero;//左下角pos
        Vector2 maxPos = Vector2.zero;//右上角pos


        uint roomWidth = roomBaseInfo.roomSize[0];
        uint roomHeight = roomBaseInfo.roomSize[1];

        //当前房间的坐标信息范围参考
        List<Vector2> posLeftDownArr = new List<Vector2>();
        List<Vector2> posRightDownArr = new List<Vector2>();
        List<Vector2> posLeftUpArr = new List<Vector2>();
        List<Vector2> posRightUpArr = new List<Vector2>();

        //当前房间与各个房间的邻接关系
        for (int i = 0; i < roomBaseInfo.targetRoomsDirRelation.Count; i++)
        {
            //List<BorderEntityData> borderEntityDatas = GetRoomBuilderInfo(roomBaseInfo.curRoomType);
            RoomInfo otherRoomInfo = m_ListRoomInfo.Find((p) => { return p.roomType == roomBaseInfo.targetRoomsDirRelation[i].targetRoomType; });
            if (otherRoomInfo != null)
            {
                switch (roomBaseInfo.targetRoomsDirRelation[i].locationRelation)
                {
                    case DirEnum.Top:
                        posLeftUpArr.Add(new Vector2(otherRoomInfo.roomPosMin.x, otherRoomInfo.roomPosMax.y));
                        posRightUpArr.Add(new Vector2(otherRoomInfo.roomPosMax.x, otherRoomInfo.roomPosMax.y));
                        break;
                    case DirEnum.Left:
                        posLeftUpArr.Add(new Vector2(otherRoomInfo.roomPosMin.x, otherRoomInfo.roomPosMin.y));
                        posLeftDownArr.Add(new Vector2(otherRoomInfo.roomPosMin.x, otherRoomInfo.roomPosMax.y));
                        break;
                    case DirEnum.Bottom:
                        posLeftDownArr.Add(new Vector2(otherRoomInfo.roomPosMin.x, otherRoomInfo.roomPosMin.y));
                        posRightDownArr.Add(new Vector2(otherRoomInfo.roomPosMax.x, otherRoomInfo.roomPosMin.y));
                        break;
                    case DirEnum.Right:
                        posRightUpArr.Add(new Vector2(otherRoomInfo.roomPosMax.x, otherRoomInfo.roomPosMax.y));
                        posRightDownArr.Add(new Vector2(otherRoomInfo.roomPosMax.x, otherRoomInfo.roomPosMin.y));
                        break;
                    default:
                        break;
                }
            }
        }
        //邻接房间的坐标信息，矩形的四个顶点
        Vector2? otherPosLeftDown = null;
        Vector2? otherPosRightDown = null;
        Vector2? otherPosLeftUp = null;
        Vector2? otherPosRightUp = null;
        int tempX = 0;
        int tempY = 0;
        int otherPosCount = 0;
        for (int i = 0; i < posLeftDownArr.Count; i++)
        {
            if (i == 0)
            {
                tempX = (int)posLeftDownArr[i].x;
                tempY = (int)posLeftDownArr[i].y;
                otherPosCount++;
            }
            else
            {
                tempX = (int)posLeftDownArr[i].x > tempX ? (int)posLeftDownArr[i].x : tempX;
                tempY = (int)posLeftDownArr[i].y > tempY ? (int)posLeftDownArr[i].y : tempY;
            }
            otherPosLeftDown = new Vector2(tempX, tempY);

        }
        for (int i = 0; i < posRightDownArr.Count; i++)
        {
            if (i == 0)
            {
                tempX = (int)posRightDownArr[i].x;
                tempY = (int)posRightDownArr[i].y;
                otherPosCount++;
            }
            else
            {
                tempX = (int)posRightDownArr[i].x < tempX ? (int)posRightDownArr[i].x : tempX;
                tempY = (int)posRightDownArr[i].y > tempY ? (int)posRightDownArr[i].y : tempY;
            }
            otherPosRightDown = new Vector2(tempX, tempY);
        }
        for (int i = 0; i < posLeftUpArr.Count; i++)
        {
            if (i == 0)
            {
                tempX = (int)posLeftUpArr[i].x;
                tempY = (int)posLeftUpArr[i].y;
                otherPosCount++;
            }
            else
            {
                tempX = (int)posLeftUpArr[i].x > tempX ? (int)posLeftUpArr[i].x : tempX;
                tempY = (int)posLeftUpArr[i].y < tempY ? (int)posLeftUpArr[i].y : tempY;
            }
            otherPosLeftUp = new Vector2(tempX, tempY);
        }
        for (int i = 0; i < posRightUpArr.Count; i++)
        {
            if (i == 0)
            {
                tempX = (int)posRightUpArr[i].x;
                tempY = (int)posRightUpArr[i].y;
                otherPosCount++;
            }
            else
            {
                tempX = (int)posRightUpArr[i].x < tempX ? (int)posRightUpArr[i].x : tempX;
                tempY = (int)posRightUpArr[i].y < tempY ? (int)posRightUpArr[i].y : tempY;
            }
            otherPosRightUp = new Vector2(tempX, tempY);
        }

        //根据边界信息 生成合理的随机数
        //根据上述2~4个点位信息，生成连续的1~3条线作为边界

        //Debug.Log("otherPosCount：" + otherPosCount);
        int randomMin;
        int randomMax;

        int curRandomCount = 0;
        int maxRandomCount = 30;
        switch (otherPosCount)
        {
            case 2:
                //1.单边限制，已知两个点位信息
                if (otherPosLeftDown != null && otherPosRightDown != null) //当前房间在目标房间下侧
                {
                    randomMin = (int)otherPosLeftDown.Value.x - (int)roomBaseInfo.roomSize[0] + 1;
                    randomMax = (int)otherPosRightDown.Value.x;//考虑到砖的3d模型锚点坐标在模型最左边所以要再-1,后面同理

                    //容错机制，避免实体随机生成到已经创建的房间里
                    do
                    {
                        int randomMinX = (int)UnityEngine.Random.Range(randomMin, randomMax);
                        //Debug.Log("down [" + randomMin + "," + randomMax + ")，randomMinX：" + randomMinX + ",curRoomType" + roomBaseInfo.curRoomType);
                        minPos = new Vector2(randomMinX, otherPosLeftDown.Value.y - roomBaseInfo.roomSize[1]);
                        maxPos = new Vector2(randomMinX + roomBaseInfo.roomSize[0], otherPosLeftDown.Value.y);
                    } while (!JudgeRandomPosIsRight(minPos, maxPos) && ++curRandomCount < maxRandomCount);
                }
                else if (otherPosLeftUp != null && otherPosRightUp != null)//上侧
                {
                    randomMin = (int)otherPosLeftUp.Value.x - (int)roomBaseInfo.roomSize[0] + 1;
                    randomMax = (int)otherPosRightUp.Value.x;

                    do
                    {
                        int randomMinX = (int)UnityEngine.Random.Range(randomMin, randomMax);
                        //Debug.Log("up [" + randomMin + "," + randomMax + ")，randomMinX：" + randomMinX + ",curRoomType" + roomBaseInfo.curRoomType + ",otherPosRightUp:" + otherPosRightUp);
                        minPos = new Vector2(randomMinX, otherPosLeftUp.Value.y);
                        maxPos = new Vector2(randomMinX + roomBaseInfo.roomSize[0], otherPosLeftUp.Value.y + roomBaseInfo.roomSize[1]);
                    } while (!JudgeRandomPosIsRight(minPos, maxPos) && ++curRandomCount < maxRandomCount);
                }
                else if (otherPosLeftUp != null && otherPosLeftDown != null)//左侧
                {
                    //目标房间左下y值- 当前房间的宽-1~other左下点y值+当前房间的宽-2
                    randomMin = (int)otherPosLeftDown.Value.y - (int)roomBaseInfo.roomSize[1] + 1;
                    randomMax = (int)otherPosLeftUp.Value.y - 2;
                    do
                    {
                        int randomMinY = (int)UnityEngine.Random.Range(randomMin, randomMax);
                        //Debug.Log("left [" + randomMin + "," + randomMax + ")，randomMinY：" + randomMinY + ",curRoomType" + roomBaseInfo.curRoomType);
                        minPos = new Vector2(otherPosLeftUp.Value.x - roomBaseInfo.roomSize[0], randomMinY);
                        maxPos = new Vector2(otherPosLeftUp.Value.x, randomMinY + roomBaseInfo.roomSize[1]);
                    } while (!JudgeRandomPosIsRight(minPos, maxPos) && ++curRandomCount < maxRandomCount);
                }
                else if (otherPosRightUp != null && otherPosRightDown != null)//右侧
                {
                    //目标房间左下y值- 当前房间的宽-1~other左下点y值+当前房间的宽-2
                    randomMin = (int)otherPosRightDown.Value.y - (int)roomBaseInfo.roomSize[1] + 1;
                    randomMax = (int)otherPosRightUp.Value.y - 2;
                    do
                    {
                        int randomMinY = (int)UnityEngine.Random.Range(randomMin, randomMax);
                        //Debug.Log("right [" + randomMin + "," + randomMax + ")，randomMinY：" + randomMinY + ",curRoomType" + roomBaseInfo.curRoomType);
                        minPos = new Vector2(otherPosRightUp.Value.x, randomMinY);
                        maxPos = new Vector2(otherPosRightUp.Value.x + roomBaseInfo.roomSize[0], randomMinY + roomBaseInfo.roomSize[1]);
                    } while (!JudgeRandomPosIsRight(minPos, maxPos) && ++curRandomCount < maxRandomCount);
                }
                callback(curRandomCount < maxRandomCount);
                break;
            //case 3://2.双边限制，已知三个点位信息 TODO
            //    break;
            //case 4://3.三边限制，已知四个点位信息 TODO
            //    break;
            default:
                Debug.Log("获取当前房间随机坐标数据失败 roomType" + roomBaseInfo.curRoomType + ", otherPosCount：" + otherPosCount);
                callback(false);
                break;
        }
        res.Add(minPos);
        res.Add(maxPos);
        return res;
    }

    /// <summary>
    /// 更新修复并检测房间与房间的邻接关系数据信息
    /// 例如1：A房间已标记在B房间的上方，但在B房间未标记A房在自己下方位置，此时应修复新增B房间下方存在A房间信息，若B房间标记A房间在自己右侧，则为房间邻接关系冲突会报错
    /// </summary>
    /// <param name="roomBaseInfos"></param>
    /// <returns></returns>
    public void UpadteRoomsDirRelation(ref List<RoomBaseInfo> roomBaseInfos)
    {
        for (int i = 0; i < roomBaseInfos.Count; i++)
        {
            for (int j = 0; j < roomBaseInfos[i].targetRoomsDirRelation.Count; j++)
            {
                //房间A的邻接房间信息
                RoomType curTargetRoomType = roomBaseInfos[i].targetRoomsDirRelation[j].targetRoomType;
                string curTargetRoomID = roomBaseInfos[i].targetRoomsDirRelation[j].targetRoomID;
                DirEnum curDirEnum = roomBaseInfos[i].targetRoomsDirRelation[j].locationRelation;
                bool curIsCommonWall = roomBaseInfos[i].targetRoomsDirRelation[j].isCommonWall;

                //邻接房间A的房间B信息
                RoomBaseInfo targetRoomBaseInfo = roomBaseInfos.Find((p) => { return p.curRoomType == curTargetRoomType && p.curRoomID == curTargetRoomID; });
                if (targetRoomBaseInfo != null)
                {
                    //判定房间A是否存在房间B的邻接信息中
                    //房间B无邻接房间信息
                    if (targetRoomBaseInfo.targetRoomsDirRelation == null)
                    {
                        targetRoomBaseInfo.targetRoomsDirRelation = new List<RoomsDirRelation>
                        {
                            new RoomsDirRelation
                            {
                                targetRoomType =  roomBaseInfos[i].curRoomType,
                                locationRelation = (DirEnum)(((int)curDirEnum + 2) % 4),// 取反方向
                                isCommonWall = curIsCommonWall,
                                targetRoomID = roomBaseInfos[i].curRoomID
                            }
                        };
                    }
                    else
                    {
                        RoomsDirRelation roomsDirRelation = targetRoomBaseInfo.targetRoomsDirRelation?.Find((p) => { return p.targetRoomType == curTargetRoomType && p.targetRoomID == curTargetRoomID; });
                        if (roomsDirRelation == null)
                        {
                            //Debug.Log("更新邻接关系，targetRoomBaseInfo：" + targetRoomBaseInfo + "，");
                            targetRoomBaseInfo.targetRoomsDirRelation.Add(new RoomsDirRelation
                            {
                                targetRoomType = roomBaseInfos[i].curRoomType,
                                locationRelation = (DirEnum)(((int)curDirEnum + 2) % 4),// 取反方向
                                isCommonWall = curIsCommonWall,
                                targetRoomID = roomBaseInfos[i].curRoomID
                            });
                        }
                        else//邻接房间邻接信息存在当前房间类型
                        {
                            //验证两个房间方向指向是否正确   (DirEnum)(((int)curDirEnum + 2) % 4)// 取反方向
                            if (roomsDirRelation.locationRelation != (DirEnum)(((int)curDirEnum + 2) % 4))
                            {
                                Debug.LogError("房间邻接关系冲突,请检查！ roomTypeA:" + curTargetRoomType + ",DirA:" + curDirEnum + "，roomTypeB" + targetRoomBaseInfo.curRoomType + "，DirB：" + roomsDirRelation.locationRelation);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("roomTypeModel is null ,curTargetRoomType: " + curTargetRoomType + "，curTargetRoomID：" + curTargetRoomID);
                    foreach (var item in roomBaseInfos[i].targetRoomsDirRelation)
                    {
                        Debug.LogError("otherRoomType:" + item.targetRoomType + ",targetRoomID：" + item.targetRoomID + ",locationRelation: " + item.locationRelation);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 判定随机数是否存在可用 T-可用 F-不可用
    /// </summary>
    /// <param name="minPos"></param>
    /// <param name="maxPos"></param>
    /// <returns></returns>
    private bool JudgeRandomPosIsRight(Vector2 minPos, Vector2 maxPos)
    {
        List<Vector2> targetAllPosArr = new List<Vector2>();
        for (int i = (int)minPos.x; i < (int)maxPos.x; i++)//i <= (int)maxPos.y 加等会避免模型穿帮
        {
            for (int j = (int)minPos.y; j < (int)maxPos.y; j++)
            {
                targetAllPosArr.Add(new Vector2(i, j));
            }
        }
        bool isRight = true;

        foreach (Vector2 pos in targetAllPosArr)
        {
            if (listRoomBuilderInfo.Find((p) => { return p.pos == pos && p.entityAxis == 0 && p.entityModelType == EntityModelType.Floor; }) != null)
            {
                isRight = false;
                break;
            }
        }
        if (minPos.x >= maxPos.x || minPos.y >= maxPos.y)
        {
            isRight = false;
        }

        //Debug.Log("isRight:" + isRight + "，minPos：" + minPos + "，maxPos：" + maxPos);
        return isRight;
    }

    #region Door

    /// <summary>
    /// 生成随机实体门数据，修改房间实体信息
    /// </summary>
    /// <param name="roomBaseInfos"></param>
    /// <param name="roomInfos"></param>
    private void GenerateDoorData(List<RoomBaseInfo> roomBaseInfos, string firstGenerateID, ref List<RoomInfo> roomInfos)
    {
        //获取家庭所有房间的公共墙信息
        List<BorderEntityData> commonWallEntityData = new List<BorderEntityData>();
        foreach (var item in m_DicRoomWallInfo.Values)
        {
            if (item.borderItemPosInfoX?.listRoomType?.Count > 1)
            {
                commonWallEntityData.Add(item.borderItemPosInfoX);
                //Debug.Log(item.borderItemPosInfoX?.pos + ",entityAxis:" + item.borderItemPosInfoX?.entityAxis + ",otherRoomType:" + item.borderItemPosInfoX?.listRoomType[0] + "," + item.borderItemPosInfoX?.listRoomType[1]);
            }
            if (item.borderItemPosInfoY?.listRoomType?.Count > 1)
            {
                commonWallEntityData.Add(item.borderItemPosInfoY);
                //Debug.Log(item.borderItemPosInfoY?.pos + ",entityAxis:" + item.borderItemPosInfoY?.entityAxis + ",otherRoomType:" + item.borderItemPosInfoY?.listRoomType[0] + "," + item.borderItemPosInfoY?.listRoomType[1]);
            }
        }

        for (int i = 0; i < roomBaseInfos?.Count; i++)
        {
            if (roomBaseInfos[i].curRoomType == RoomType.LivingRoom && roomBaseInfos[i].curRoomID == firstGenerateID)
            {
                continue;
            }
            //获取当前房间的所有邻接房间信息
            List<RoomsDirRelation> roomsDirRelations = roomBaseInfos[i].targetRoomsDirRelation;

            //寻找模型门的可用的公共墙位置
            List<BorderEntityData> listDoorRandomPosInfo = commonWallEntityData.FindAll((p) =>
            {
                //return p.listRoomType.Contains(roomBaseInfos[i].curRoomType);
                return p.listRoomTypeID.Contains(roomBaseInfos[i].curRoomID);
            });
            //缩小门模型随机位置范围，如果里面包含与客厅房间的公共墙，则删除与其他的公共墙
            bool isExistLivingCommonWall = false;
            for (int j = 0; j < listDoorRandomPosInfo.Count;)
            {
                //判定模型门随机位置范围中是否存在与客厅的公共墙
                if (!isExistLivingCommonWall)
                {
                    if (listDoorRandomPosInfo[j].listRoomType.Contains(RoomType.LivingRoom))
                    {
                        isExistLivingCommonWall = true;
                        j = 0;
                    }
                    j++;
                }
                else
                {
                    if (listDoorRandomPosInfo[j].listRoomType.Contains(RoomType.LivingRoom))
                    {
                        j++;
                    }
                    else
                    {
                        listDoorRandomPosInfo.Remove(listDoorRandomPosInfo[j]);
                    }
                }
            }

            ////log 当前房间允许放置门的随机位置
            //foreach (BorderEntityData item in listDoorRandomPosInfo)
            //{
            //    Debug.Log("curRoomType:" + roomBaseInfos[i].curRoomType + ",允许放置门的位置：" + item.pos + ",entityAxis:" + item.entityAxis);
            //}

            //当前房间门的位置
            if (listDoorRandomPosInfo.Count == 0)
            {
                Debug.LogError("listDoorRandomPosInfo is null");
                return;
            }
            int doorRandomIndex = UnityEngine.Random.Range(0, listDoorRandomPosInfo.Count);
            BorderEntityData doorData = listDoorRandomPosInfo[doorRandomIndex];
            //Debug.Log("curRoomType:" + roomBaseInfos[i].curRoomType + ",doorPos：" + doorData.pos + ",entityAxis:" + doorData.entityAxis);

            //替换边界信息 wall=》door
            BorderEntityData doorBorderEntityData = new BorderEntityData
            {
                pos = doorData.pos,
                entityAxis = doorData.entityAxis,
                entityModelType = EntityModelType.Door,
                listRoomType = doorData.listRoomType,
                listRoomTypeID = doorData.listRoomTypeID,
            };
            roomInfos[i].listDoorPosInfo = new List<BorderEntityData> { doorBorderEntityData };

            //清楚当前坐标位置的墙体
            List<BorderEntityData> borderItemPosInfos = listRoomBuilderInfo.FindAll((p) =>
            {
                return p.pos == doorData.pos && p.entityAxis == doorData.entityAxis && p.entityModelType == EntityModelType.Wall;
            });
            for (int j = 0; j < borderItemPosInfos.Count; j++)
            {
                listRoomBuilderInfo.Remove(borderItemPosInfos[j]);
            }
            listRoomBuilderInfo.Add(doorBorderEntityData);
        }
    }

    #endregion

    #region CacheWallData
    /// <summary>
    /// 缓存房间实体墙的信息，根据所有房间信息
    /// </summary>
    /// <param name="roomInfoArr">所有房间信息数据</param>
    public void CacheRoomWallInfo(params RoomInfo[] roomInfoArr)
    {
        for (int j = 0; j < roomInfoArr.Length; j++)
        {
            RoomInfo roomInfo = roomInfoArr[j];
            //横向边界实体信息
            for (int i = (int)roomInfo.roomPosMin.x; i < (int)roomInfo.roomPosMax.x; i++)
            {
                int curX = i;
                Vector2 pos1 = new Vector2(curX, roomInfo.roomPosMin.y);
                if (!m_DicRoomWallInfo.ContainsKey(pos1))
                {
                    m_DicRoomWallInfo.Add(pos1, new BorderInfo
                    {
                        borderItemPosInfoX = new BorderEntityData
                        {
                            entityAxis = 0,
                            pos = pos1,
                            entityModelType = EntityModelType.Wall,
                            listRoomType = new List<RoomType> { roomInfo.roomType },
                            listRoomTypeID = new List<string> { roomInfo.roomID }
                        }
                    });
                }
                else
                {
                    if (m_DicRoomWallInfo[pos1].borderItemPosInfoX == null)
                    {
                        m_DicRoomWallInfo[pos1].borderItemPosInfoX = new BorderEntityData
                        {
                            entityAxis = 0,
                            pos = pos1,
                            entityModelType = EntityModelType.Wall,
                            listRoomType = new List<RoomType> { roomInfo.roomType },
                            listRoomTypeID = new List<string> { roomInfo.roomID }
                        };
                    }
                    else
                    {
                        if (!m_DicRoomWallInfo[pos1].borderItemPosInfoX.listRoomTypeID.Contains(roomInfo.roomID))
                        {
                            m_DicRoomWallInfo[pos1].borderItemPosInfoX.listRoomType.Add(roomInfo.roomType);
                            m_DicRoomWallInfo[pos1].borderItemPosInfoX.listRoomTypeID.Add(roomInfo.roomID);
                        }
                    }
                }

                Vector2 pos2 = new Vector2(curX, roomInfo.roomPosMax.y);
                if (!m_DicRoomWallInfo.ContainsKey(pos2))
                {
                    m_DicRoomWallInfo.Add(pos2, new BorderInfo
                    {
                        borderItemPosInfoX = new BorderEntityData
                        {
                            entityAxis = 0,
                            pos = pos2,
                            entityModelType = EntityModelType.Wall,
                            listRoomType = new List<RoomType> { roomInfo.roomType },
                            listRoomTypeID = new List<string> { roomInfo.roomID }
                        }
                    });
                }
                else
                {
                    if (m_DicRoomWallInfo[pos2].borderItemPosInfoX == null)
                    {
                        m_DicRoomWallInfo[pos2].borderItemPosInfoX = new BorderEntityData
                        {
                            entityAxis = 0,
                            pos = pos2,
                            entityModelType = EntityModelType.Wall,
                            listRoomType = new List<RoomType> { roomInfo.roomType },
                            listRoomTypeID = new List<string> { roomInfo.roomID }
                        };
                    }
                    else
                    {
                        if (!m_DicRoomWallInfo[pos2].borderItemPosInfoX.listRoomTypeID.Contains(roomInfo.roomID))
                        {
                            m_DicRoomWallInfo[pos2].borderItemPosInfoX.listRoomType.Add(roomInfo.roomType);
                            m_DicRoomWallInfo[pos2].borderItemPosInfoX.listRoomTypeID.Add(roomInfo.roomID);
                        }
                    }
                }
            }
            //纵向边界实体信息
            for (int i = (int)roomInfo.roomPosMin.y; i < (int)roomInfo.roomPosMax.y; i++)
            {
                int curY = i;
                Vector2 pos1 = new Vector2(roomInfo.roomPosMin.x, curY);
                if (!m_DicRoomWallInfo.ContainsKey(pos1))
                {
                    m_DicRoomWallInfo.Add(pos1, new BorderInfo
                    {
                        borderItemPosInfoY = new BorderEntityData
                        {
                            entityAxis = 1,
                            pos = pos1,
                            entityModelType = EntityModelType.Wall,
                            listRoomType = new List<RoomType> { roomInfo.roomType },
                            listRoomTypeID = new List<string> { roomInfo.roomID }
                        }
                    });
                }
                else
                {
                    if (m_DicRoomWallInfo[pos1].borderItemPosInfoY == null)
                    {
                        m_DicRoomWallInfo[pos1].borderItemPosInfoY = new BorderEntityData
                        {
                            entityAxis = 1,
                            pos = pos1,
                            entityModelType = EntityModelType.Wall,
                            listRoomType = new List<RoomType> { roomInfo.roomType },
                            listRoomTypeID = new List<string> { roomInfo.roomID }
                        };
                    }
                    else
                    {
                        if (!m_DicRoomWallInfo[pos1].borderItemPosInfoY.listRoomTypeID.Contains(roomInfo.roomID))
                        {
                            m_DicRoomWallInfo[pos1].borderItemPosInfoY.listRoomType.Add(roomInfo.roomType);
                            m_DicRoomWallInfo[pos1].borderItemPosInfoY.listRoomTypeID.Add(roomInfo.roomID);
                        }
                    }
                }

                Vector2 pos2 = new Vector2(roomInfo.roomPosMax.x, curY);
                if (!m_DicRoomWallInfo.ContainsKey(pos2))
                {
                    m_DicRoomWallInfo.Add(pos2, new BorderInfo
                    {
                        borderItemPosInfoY = new BorderEntityData
                        {
                            entityAxis = 1,
                            pos = pos2,
                            entityModelType = EntityModelType.Wall,
                            listRoomType = new List<RoomType> { roomInfo.roomType },
                            listRoomTypeID = new List<string> { roomInfo.roomID }
                        }
                    });
                }
                else
                {
                    if (m_DicRoomWallInfo[pos2].borderItemPosInfoY == null)
                    {
                        m_DicRoomWallInfo[pos2].borderItemPosInfoY = new BorderEntityData
                        {
                            entityAxis = 1,
                            pos = pos2,
                            entityModelType = EntityModelType.Wall,
                            listRoomType = new List<RoomType> { roomInfo.roomType },
                            listRoomTypeID = new List<string> { roomInfo.roomID }
                        };
                    }
                    else
                    {
                        if (!m_DicRoomWallInfo[pos2].borderItemPosInfoY.listRoomTypeID.Contains(roomInfo.roomID))
                        {
                            m_DicRoomWallInfo[pos2].borderItemPosInfoY.listRoomType.Add(roomInfo.roomType);
                            m_DicRoomWallInfo[pos2].borderItemPosInfoY.listRoomTypeID.Add(roomInfo.roomID);
                        }
                    }
                }
            }
        }
    }
    #endregion

    public List<BorderEntityData> GetDoorInfoByRoomType(RoomType roomType,string roomID)
    {
        return listRoomBuilderInfo.FindAll((p) => { return p.entityModelType == EntityModelType.Door && p.listRoomType.Contains(roomType) && p.listRoomTypeID.Contains(roomID); });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //CacheRoomWallInfo(m_ListRoomInfo.ToArray());
            //GenerateDoorData(roomBaseInfosClone, ref m_ListRoomInfo);

            Debug.Log(listRoomBuilderInfo);
            //foreach (var item in listRoomBuilderInfo)
            //{
            //    Debug.Log(item.pos + ",entityModelType" + item.entityModelType + ",entityAxis:" + item.entityAxis + ",otherRoomType:" + item.listRoomType[0]);

        }
    }
}
