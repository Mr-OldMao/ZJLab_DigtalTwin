using System.Collections.Generic;
using UnityEngine;
using MFramework;
using static GenerateRoomBorderModel;
using static GenerateRoomData;
using static GetEnvGraph;
using static GetThingGraph;
using System.Linq;
/// <summary>
/// 标题：更新房间布局信息
/// 功能：用于运行时房间布局发生变化时，手动更新房间布局所关联的所有数据并存档
/// 作者：毛俊峰
/// 时间：2023.11.27
/// </summary>
public class UpdateRoomData : SingletonByMono<UpdateRoomData>
{
    private List<RoomInfo> m_RoomInfos;
    private List<RoomBaseInfo> m_RoomBaseInfos;
    private List<BorderEntityData> m_BorderEntityDatas;
    private PostThingGraph m_PostThingGraph;
    private GetEnvGraph_data m_GetEnvGraphData;
    //private List<RoomMatData> m_ListRoomMatData;

    public bool CanUpdateRoomData { get; private set; } = true;
    private JsonChangeRoomLayout m_JsonChangeRoomLayout;

    public bool CanRefresh { get; private set; } = false;
    /// <summary>
    /// 更新所有房间数据
    /// </summary>
    /// <param name="jsonChangeRoomLayout"></param>
    public void UpdateAllRoomData(JsonChangeRoomLayout jsonChangeRoomLayout)
    {
        Debugger.Log("UpdateAllRoomData");
        CanUpdateRoomData = false;
        GetNowData();
        m_JsonChangeRoomLayout = jsonChangeRoomLayout;
        //更新数据
        UpdateDataRoomInfo();
        UpdateRoomBaseInfos();
        UpdateDataPostThingGraph();
        UpdateDataListRoomMatData();

        //重新生成“门”实体相关数据
        RegenerateDoorData();

        //存档
        DataSave.GetInstance.SaveRoomInfos(m_RoomInfos);
        DataSave.GetInstance.SaveRoomBaseInfos(m_RoomBaseInfos);
        DataSave.GetInstance.SaveListRoomBuilderInfo(m_BorderEntityDatas);
        DataSave.GetInstance.SaveGetThingGraph_data_items(m_PostThingGraph);
        DataSave.GetInstance.SaveGetEnvGraph_data(m_GetEnvGraphData);
        DataSave.GetInstance.Save(() =>
        {
            CanUpdateRoomData = true;

#if UNITY_WEBGL
            UIManager.GetInstance.GetUIFormLogicScript<UIFormHintOneBtn>().Show(new UIFormHintOneBtn.ShowParams
            {
                txtHintContent = "检测到房间布局发生变动，点击确认即可重新打开程序刷新场景",
                btnConfirmContent = "确认",
                isFullMask = true,
            }, () =>
            {
                CanRefresh = true;
                Debugger.Log("尝试刷新页面");
#if !UNITY_EDITOR
                MqttWebglCenter.GetInstance.RefreshWeb();
#endif
            });


#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            UIManager.GetInstance.GetUIFormLogicScript<UIFormHintNotBtn>().Show(new UIFormHintNotBtn.ShowParams
            {
                txtHintContent = "检测到房间布局发生变动，请手动重新打开该程序刷新场景",
                delayCloseUIFormTime = 0
            });
#endif
        });
    }

    /// <summary>
    /// 获取最新数据
    /// </summary>
    private void GetNowData()
    {
        m_RoomInfos = DataRead.GetInstance.ReadRoomInfos();
        m_RoomBaseInfos = DataRead.GetInstance.ReadRoomBaseInfos();
        m_BorderEntityDatas = DataRead.GetInstance.ReadListRoomBuilderInfo();
        //m_PostThingGraph = DataRead.GetInstance.ReadGetThingGraph_data_items();
        m_PostThingGraph = MainData.CacheSceneItemsInfo;
        m_GetEnvGraphData = DataRead.GetInstance.ReadGetEnvGraph_data();
        //m_ListRoomMatData = DataRead.GetInstance.ReadRoomMatData();
    }

    private bool UpdateDataRoomInfo()
    {
        RoomInfo roomInfo = m_RoomInfos.Find((p) => { return p.roomID == m_JsonChangeRoomLayout.roomID; });
        // RoomBaseInfo roomBaseInfo = m_RoomBaseInfos.Find((p) => { return p.curRoomID == m_JsonChangeRoomLayout.roomID; });
        Debugger.Log("oldPos:" + roomInfo.roomPosMin + "," + roomInfo.roomPosMax);
        //Vector2 originOffset = GameLogic.GetInstance.GetOriginOffset();
        Vector2 newRoomPosMin = new Vector2(roomInfo.roomPosMin.x + m_JsonChangeRoomLayout.offsetPos.x, roomInfo.roomPosMin.y + m_JsonChangeRoomLayout.offsetPos.y);
        Vector2 newRoomPosMax = new Vector2(roomInfo.roomPosMax.x + m_JsonChangeRoomLayout.offsetPos.x, roomInfo.roomPosMax.y + m_JsonChangeRoomLayout.offsetPos.y);

        if (roomInfo != null)
        {
            roomInfo.roomPosMin = newRoomPosMin;
            roomInfo.roomPosMax = newRoomPosMax;
        }
        return roomInfo != null;
    }

    private bool UpdateRoomBaseInfos()
    {
        Debugger.Log("更新房间布局信息：" + m_RoomBaseInfos);
        string curRoomID = m_JsonChangeRoomLayout.roomID;

        //改变布局
        //删除当前roomID原来的临界关系数据
        foreach (var item in m_RoomBaseInfos)
        {
            if (item.curRoomID == curRoomID)
            {
                item.targetRoomsDirRelation = new List<RoomsDirRelation>();
                continue;
            }
            RoomsDirRelation roomsDirRelation = item.targetRoomsDirRelation.Find((p) => { return p.targetRoomID == curRoomID; });
            if (roomsDirRelation != null)
            {
                Debugger.Log("curRoom:" + item.curRoomType + "_" + item.curRoomID + ", try remove :" + roomsDirRelation.targetRoomType + "_" + roomsDirRelation.targetRoomID);
                item.targetRoomsDirRelation.Remove(roomsDirRelation);
            }
        }

        Vector2 originOffsetValue = GameLogic.GetInstance.GetOriginOffset();
        //寻找当前roomID的新临界的房间
        RoomBaseInfo roomBaseInfo = m_RoomBaseInfos.Find((p) => { return p.curRoomID == curRoomID; });
        RoomInfo curRoomInfo = m_RoomInfos.Find((p) => { return p.roomID == m_JsonChangeRoomLayout.roomID; });
        if (roomBaseInfo != null)
        {
            foreach (var item in m_RoomBaseInfos)
            {
                if (item.curRoomID == curRoomID)
                {
                    continue;
                }
                RoomInfo targetRoomInfo = m_RoomInfos.Find((p) => { return p.roomID == item.curRoomID; });
                if (targetRoomInfo != null)
                {
                    Vector2 targetRoomMinPos = targetRoomInfo.roomPosMin + originOffsetValue;
                    Vector2 targetRoomMaxPos = targetRoomInfo.roomPosMax + originOffsetValue;
                    int dirEnum = JudgeAdjoinRelation(curRoomInfo.roomPosMin, curRoomInfo.roomPosMax, targetRoomMinPos, targetRoomMaxPos);
                    if (dirEnum >= 0 && dirEnum <= 3)
                    {
                        Debugger.Log("当前房间与目标房间有邻接关系，curRoom:" + curRoomInfo.roomType + ",targetRoom:" + targetRoomInfo.roomType + ",dirEnum：" + (DirEnum)dirEnum);
                        roomBaseInfo.targetRoomsDirRelation.Add(new RoomsDirRelation
                        {
                            targetRoomID = targetRoomInfo.roomID,
                            targetRoomType = targetRoomInfo.roomType,
                            locationRelation = (DirEnum)dirEnum,
                        });
                    }
                    else
                    {
                        Debugger.Log("当前房间与目标房间无邻接关系，curRoom:" + curRoomInfo.roomType + ",targetRoom:" + targetRoomInfo.roomType);
                    }
                }
                else
                {
                    Debugger.LogError("targetRoomInfo is null, targetId:" + item.curRoomID);

                }

            }
        }
        return roomBaseInfo != null;
    }
    private void UpdateDataBorderEntityDatas()
    {
        Debugger.Log(m_BorderEntityDatas);
        RoomInfo roomInfo = m_RoomInfos.Find((p) => { return p.roomID == m_JsonChangeRoomLayout.roomID; });

        float offsetValueX = m_JsonChangeRoomLayout.offsetPos.x;
        float offsetValueY = m_JsonChangeRoomLayout.offsetPos.y;
        //当前房间的所有实体信息
        List<BorderEntityData> curRommAllBorderEntityDataArr = m_BorderEntityDatas.FindAll((p) => { return p.listRoomTypeID?[0] == roomInfo.roomID; });
        //List<BorderEntityData> curRommAllBorderEntityDataArr = m_BorderEntityDatas.FindAll((p) => { return p.listRoomType?[0] == roomInfo.roomType; });
        foreach (var item in curRommAllBorderEntityDataArr)
        {
            item.pos = new Vector2(item.pos.x + offsetValueX, item.pos.y + offsetValueY);
        }
    }
    private void UpdateDataPostThingGraph()
    {
        //判断房间偏移量是否发生改变
        Vector2 oldOriginOffset = new Vector2(GameLogic.GetInstance.staticModelRootNode.transform.position.x, GameLogic.GetInstance.staticModelRootNode.transform.position.z);
        Vector2 newOriginOffset = GameLogic.GetInstance.GetOriginOffset(m_RoomInfos);
        Debugger.Log($"房间原点偏移量 oldOriginOffset:{oldOriginOffset},newOriginOffset{newOriginOffset}");
        GetThingGraph_data_items roomItems = m_PostThingGraph.items.Find((P) => { return P.id == m_JsonChangeRoomLayout.roomID; });
        //foreach (var item in roomItems?.relatedThing)
        //{
        //    Vector3 newItemPos = new Vector3(item.target.position[0] + m_JsonChangeRoomLayout.offsetPos.x, item.target.position[1], item.target.position[2] + m_JsonChangeRoomLayout.offsetPos.y);
        //    item.target.position = new float[3] { newItemPos[0], newItemPos[1], newItemPos[2] };

        //    //房间里的物体的子物体
        //    if (item.target.relatedThing?.Count > 0)
        //    {
        //        foreach (var chindItem in item.target.relatedThing)
        //        {
        //            Vector3 newChindItemPos = new Vector3(chindItem.target.position[0] + m_JsonChangeRoomLayout.offsetPos.x, chindItem.target.position[1], chindItem.target.position[2] + m_JsonChangeRoomLayout.offsetPos.y);
        //            chindItem.target.position = new float[3] { newChindItemPos[0], newChindItemPos[1], newChindItemPos[2] };
        //        }
        //    }
        //}
        string roomName = roomItems.name + "_" + roomItems.id;
        GameObject roomObj = GenerateRoomItemModel.GetInstance.ItemEntityGroupNode.Find(roomName)?.gameObject;
        if (roomObj != null)
        {
            roomObj.transform.position = new Vector3(m_JsonChangeRoomLayout.offsetPos.x, 0, m_JsonChangeRoomLayout.offsetPos.y);
        }
        else
        {
            Debugger.LogError("roomObj is null ,roomName : " + roomName);
        }

        //房间原点偏移量改变 所有物体整体偏移
        if (oldOriginOffset != newOriginOffset)
        {
            float x = newOriginOffset.x - oldOriginOffset.x;
            float y = newOriginOffset.y - oldOriginOffset.y;
            Debugger.Log($"房间原点偏移量改变 x:{x},y{y}");
            Vector3 oldPosItemEntityGroupNode = GenerateRoomItemModel.GetInstance.ItemEntityGroupNode.position;
            Vector3 newPosItemEntityGroupNode = new Vector3(oldPosItemEntityGroupNode[0] + x, oldPosItemEntityGroupNode[1], oldPosItemEntityGroupNode[2] + y);
            GenerateRoomItemModel.GetInstance.ItemEntityGroupNode.position = newPosItemEntityGroupNode;
            //foreach (var item1 in m_PostThingGraph.items)
            //{
            //    float[] oldItem1Pos = item1.position;
            //    float[] newItem1Pos = new float[] { oldItem1Pos[0] + x, oldItem1Pos[1] + oldItem1Pos[2] + y };
            //    item1.position = newItem1Pos;
            //    //第二层
            //    if (item1.relatedThing?.Count > 0)
            //    {
            //        foreach (var item2 in item1.relatedThing)
            //        {
            //            float[] oldItem2Pos = item2.target.position;
            //            float[] newItem2Pos = new float[] { oldItem2Pos[0] + x, oldItem2Pos[1] + oldItem2Pos[2] + y };
            //            item2.target.position = newItem2Pos;
            //            //第三层 todo
            //        }
            //    }
            //}
            //MainData.CacheSceneItemsInfo = m_PostThingGraph;
            //Debugger.Log(MainData.CacheSceneItemsInfo);


        }
        UpdateEnityInfoTool.GetInstance.UpdateSceneEntityInfo();
        ////存档
        //DataSave.GetInstance.SaveGetThingGraph_data_items(MainData.CacheSceneItemsInfo);
    }
    private void UpdateDataListRoomMatData()
    {
        Debugger.Log(m_GetEnvGraphData);

        RoomBaseInfo curRoomBaseInfo = m_RoomBaseInfos.Find((p) => { return p.curRoomID == m_JsonChangeRoomLayout.roomID; });

        foreach (var item in m_GetEnvGraphData.items)
        {
            if (item.id == m_JsonChangeRoomLayout.roomID)
            {
                item.relatedThing = new GetEnvGraph_data_items_relatedThing[curRoomBaseInfo.targetRoomsDirRelation.Count];
                //更新当前房间的临界关系
                for (int i = 0; i < curRoomBaseInfo.targetRoomsDirRelation.Count; i++)
                {
                    item.relatedThing[i] = new GetEnvGraph_data_items_relatedThing
                    {
                        target = new GetEnvGraph_data_items_relatedThing_target
                        {
                            id = curRoomBaseInfo.targetRoomsDirRelation[i].targetRoomID,
                            name = curRoomBaseInfo.targetRoomsDirRelation[i].targetRoomType.ToString()
                        },
                        relationship = curRoomBaseInfo.targetRoomsDirRelation[i].locationRelation.ToString()
                    };
                }
                continue;
            }
            for (int i = 0; i < item.relatedThing.Length;)
            {
                if (item.relatedThing[i].target.id == m_JsonChangeRoomLayout.roomID)
                {
                    //移除当前房间布局信息
                    item.relatedThing = item.relatedThing.Except(new[] { item.relatedThing[i] }).ToArray();
                }
                else
                {
                    i++;
                }
            }
        }
        Debugger.Log(m_GetEnvGraphData);

    }

    /// <summary>
    /// 重新生成“门”实体相关数据
    /// </summary>
    private void RegenerateDoorData()
    {
        //更新门实体信息
        List<BorderEntityData> oldDoorEntityData = m_BorderEntityDatas.FindAll((p) => { return p.entityModelType == EntityModelType.Door; });
        //Debugger.Log(oldDoorEntityData);

        //删除原来的一扇门，变为两面墙
        for (int i = 0; i < oldDoorEntityData.Count; i++)
        {



            BorderEntityData targetBorderEntityData = oldDoorEntityData[i];

            //判断当前“墙”相对于当前房间的位置关系
            BorderDir borderDir1 = BorderDir.Up;
            BorderDir borderDir2 = BorderDir.Up;
            if (targetBorderEntityData.listRoomTypeID?.Count == 2)
            {
                borderDir1 = GetBorderDir(targetBorderEntityData, targetBorderEntityData.listRoomTypeID[0]);
                borderDir2 = GetBorderDir(targetBorderEntityData, targetBorderEntityData.listRoomTypeID[1]);
            }
            else
            {
                Debugger.LogError("targetBorderEntityData.listRoomTypeID Count != 2 ,count:" + targetBorderEntityData.listRoomTypeID?.Count + "," + targetBorderEntityData.pos);
            }


            BorderEntityData targetBorderEntityData1 = new BorderEntityData
            {
                listRoomType = new List<RoomType> { targetBorderEntityData.listRoomType[0] },
                listRoomTypeID = new List<string> { targetBorderEntityData.listRoomTypeID[0] },
                borderDir = borderDir1,
                pos = targetBorderEntityData.pos,
                entityAxis = targetBorderEntityData.entityAxis,
                entityModelType = EntityModelType.Wall,
                entity = null
            };

            BorderEntityData targetBorderEntityData2 = new BorderEntityData
            {
                listRoomType = new List<RoomType> { targetBorderEntityData.listRoomType[1] },
                listRoomTypeID = new List<string> { targetBorderEntityData.listRoomTypeID[1] },
                borderDir = borderDir2,
                pos = targetBorderEntityData.pos,
                entityAxis = targetBorderEntityData.entityAxis,
                entityModelType = EntityModelType.Wall,
                entity = null
            };
            //更新边界实体方位，相对于当前房间中心点的方位
            targetBorderEntityData1.borderDir = UpdateWallEntityDir(targetBorderEntityData.listRoomTypeID[0], targetBorderEntityData.entityAxis, targetBorderEntityData.pos);
            targetBorderEntityData2.borderDir = UpdateWallEntityDir(targetBorderEntityData.listRoomTypeID[1], targetBorderEntityData.entityAxis, targetBorderEntityData.pos);


            m_BorderEntityDatas.Add(targetBorderEntityData1);
            m_BorderEntityDatas.Add(targetBorderEntityData2);

            m_BorderEntityDatas.Remove(targetBorderEntityData);
        }

        //更新所移动的房间的实体信息
        UpdateDataBorderEntityDatas();

        foreach (var item in m_RoomInfos)
        {
            item.listDoorPosInfo = null;
            item.listEmptyPosInfo = null;
        }

        //更新所有房间公共墙壁数据
        Dictionary<Vector2, BorderInfo> dicBorDerInfo = GenerateRoomData.GetInstance.CacheRoomWallInfo(m_RoomInfos.ToArray());

        //重新生成门位置信息
        GenerateRoomData.GetInstance.GenerateDoorData(m_RoomBaseInfos, "", ref m_RoomInfos, dicBorDerInfo);
        //更新实体数据
        foreach (var item in m_RoomInfos)
        {
            if (item?.listDoorPosInfo != null)
            {
                foreach (var doors in item?.listDoorPosInfo)
                {
                    Vector2 newDoorPos = doors.pos;
                    Debugger.Log("newDoorPos:" + newDoorPos + " ,entityAxis: " + doors?.entityAxis + ",roomType: " + doors?.listRoomType[0] + "," + doors?.listRoomType[1]);

                    //删除当前新“门”坐标位置原有的“墙体”，在当前坐标位置新增：“门”实体
                    List<BorderEntityData> borderEntityDataWall = m_BorderEntityDatas.FindAll((p) => { return p.pos == newDoorPos && p.entityModelType == EntityModelType.Wall && p.entityAxis == doors.entityAxis; });
                    for (int i = 0; i < borderEntityDataWall?.Count; i++)
                    {
                        m_BorderEntityDatas.Remove(borderEntityDataWall[i]);
                    }
                    m_BorderEntityDatas.Add(doors);
                }
            }
        }
    }

    private BorderDir UpdateWallEntityDir(string roomTypeID, int entityAxis, Vector2 wallPos)
    {
        RoomInfo roomInfo = m_RoomInfos.Find((p) => { return p.roomID == roomTypeID; });
        Debugger.Log("roomTypeID:" + roomTypeID + "," + roomInfo.roomPosMin + "," + roomInfo.roomPosMax + ",wallPos:" + wallPos);
        if (entityAxis == 0)
        {
            if (wallPos.y == roomInfo.roomPosMax.y)
            {
                return BorderDir.Up;
            }
            else
            {
                return BorderDir.Down;
            }

        }
        else if (entityAxis == 1)
        {
            if (wallPos.x == roomInfo.roomPosMin.x)
            {
                return BorderDir.Left;
            }
            else
            {
                return BorderDir.Right;
            }
        }
        else
        {
            return default;
        }
    }

    //当前“墙”相对于当前房间的位置关系
    private BorderDir GetBorderDir(BorderEntityData borderEntityData, string roomTypeID)
    {
        BorderDir res = BorderDir.Up;
        Vector2 sidePos1;
        Vector2 sidePos2;
        if (borderEntityData.entityAxis == 0)
        {
            //相邻的坐标
            sidePos1 = new Vector2(borderEntityData.pos.x + 1, borderEntityData.pos.y);
            sidePos2 = new Vector2(borderEntityData.pos.x - 1, borderEntityData.pos.y);
        }
        else
        {
            sidePos1 = new Vector2(borderEntityData.pos.x, borderEntityData.pos.y + 1);
            sidePos2 = new Vector2(borderEntityData.pos.x, borderEntityData.pos.y - 1);
        }
        BorderEntityData borderEntityData1 = m_BorderEntityDatas.Find(p => p.listRoomTypeID.Contains(roomTypeID) && p.entityAxis == borderEntityData.entityAxis && p.pos == sidePos1);
        BorderEntityData borderEntityData2 = m_BorderEntityDatas.Find(p => p.listRoomTypeID.Contains(roomTypeID) && p.entityAxis == borderEntityData.entityAxis && p.pos == sidePos2);
        if (borderEntityData1 != null)
        {
            res = borderEntityData1.borderDir;
            Debugger.Log("borderEntityData1   roomTypeID:" + roomTypeID + "," + borderEntityData1.entityAxis + ",borderDir:" + res);
        }
        else if (borderEntityData2 != null)
        {
            res = borderEntityData2.borderDir;
            Debugger.Log("borderEntityData2   roomTypeID:" + roomTypeID + "," + borderEntityData2.entityAxis + ",borderDir:" + res);
        }
        return res;
    }

    /// <summary>
    /// 判断两个房间的临界关系
    /// </summary>
    /// <param name="curRoomMinPos"></param>
    /// <param name="curRoomMaxPos"></param>
    /// <param name="targetRoomMinPos"></param>
    /// <param name="targetRoomMaxPos"></param>
    /// <returns>[0,3]</returns>
    private int JudgeAdjoinRelation(Vector2 curRoomMinPos, Vector2 curRoomMaxPos, Vector2 targetRoomMinPos, Vector2 targetRoomMaxPos)
    {
        int res = -1;
        //Top
        if (curRoomMinPos.y == targetRoomMaxPos.y && curRoomMinPos.x <= (targetRoomMaxPos.x - 1) && curRoomMaxPos.x >= (targetRoomMinPos.x + 1))
        {
            res = (int)DirEnum.Top;
        }
        else if (curRoomMaxPos.y == targetRoomMinPos.y && curRoomMaxPos.x >= (targetRoomMinPos.x + 1) && curRoomMinPos.x <= (targetRoomMaxPos.x - 1)) //bottom
        {
            res = (int)DirEnum.Bottom;

        }
        else if (curRoomMaxPos.x == targetRoomMinPos.x && curRoomMinPos.y <= targetRoomMaxPos.y - 1 && curRoomMaxPos.y >= targetRoomMinPos.y + 1)//left
        {
            res = (int)DirEnum.Left;

        }
        else if (curRoomMinPos.x == targetRoomMaxPos.x && curRoomMinPos.y <= targetRoomMaxPos.y - 1 && curRoomMaxPos.y >= targetRoomMinPos.y + 1)//right
        {
            res = (int)DirEnum.Right;
        }
        return res;
    }
}
