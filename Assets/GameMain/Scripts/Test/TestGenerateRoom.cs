using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static GenerateRoomModel;
using static UnityEditor.Progress;

/// <summary>
/// 标题：测试动态生成场景房间
/// 功能：
/// 作者：毛俊峰
/// 时间：20230719
/// </summary>
public class TestGenerateRoom : MonoBehaviour
{ 
    public Button btnFixedGenerate;
    public Button btnRandomGenerate;
    private void Start()
    {
        btnFixedGenerate?.onClick.AddListener(() => { TestGenerateFixed(); });
        btnRandomGenerate?.onClick.AddListener(() => { TestGenerateRandom(); });
        //TestGenerateFixed();
        //TestGenerateRandom();
    }

    private void TestGenerateFixed()
    {
        GenerateRoomModel.GetInstance.ClearRoom();
        //TEST 模拟数据
        GenerateRoomModel.GetInstance.RoomGroup = transform;
        RoomInfo roomA = new RoomInfo()
        {
            roomType = RoomType.LivingRoom,
            roomSize = new uint[2] { 4, 3 },
            roomPosMin = new Vector2(0, 0),
            roomPosMax = new Vector2(4, 3),
            listDoorPosInfo = new List<BorderEntityData>()
            {
                new BorderEntityData
                {
                    entityModelType = EntityModelType.Door,
                    pos = new Vector2(0, 2),
                    entityAxis = 1
                },
                new BorderEntityData
                {
                    entityModelType = EntityModelType.Door,
                    pos =new Vector2(1, 3),
                    entityAxis = 0
                },
                new BorderEntityData
                {
                    entityModelType = EntityModelType.Door,
                    pos =new Vector2(3, 3),
                    entityAxis = 0
                },
                new BorderEntityData
                {
                    entityModelType = EntityModelType.Door,
                    pos =new Vector2(2, 0),
                    entityAxis = 0
                }
            },
            listEmptyPosInfo = new List<BorderEntityData>()
            {
                 new BorderEntityData
                 {
                      entityModelType = EntityModelType.Null,
                      pos = new Vector2(4, 0),
                      entityAxis = 1
                 },
                 new BorderEntityData
                 {
                      entityModelType = EntityModelType.Null,
                      pos = new Vector2(4, 1),
                      entityAxis = 1
                 }
            }
        };
        RoomInfo roomB = new RoomInfo()
        {
            roomType = RoomType.BalconyRoom,
            roomSize = new uint[2] { 3, 4 },
            roomPosMin = new Vector2(-3, 0),
            roomPosMax = new Vector2(0, 4),
            listDoorPosInfo = new List<BorderEntityData>()
            {
                new BorderEntityData
                {
                    entityModelType = EntityModelType.Door,
                    pos = new Vector2(0, 2),
                    entityAxis = 1
                }
            }
        };
        RoomInfo roomC = new RoomInfo()
        {
            roomType = RoomType.FirstBedRoom,
            roomSize = new uint[2] { 3, 4 },
            roomPosMin = new Vector2(0, 3),
            roomPosMax = new Vector2(3, 8),
            listDoorPosInfo = new List<BorderEntityData>()
            {
                 new BorderEntityData
                 {
                     entityModelType = EntityModelType.Door,
                     pos =new Vector2(1, 3),
                     entityAxis = 0
                 }
            }
        };
        RoomInfo roomD = new RoomInfo()
        {
            roomType = RoomType.KitChenRoom,
            roomSize = new uint[2] { 3, 3 },
            roomPosMin = new Vector2(3, 3),
            roomPosMax = new Vector2(6, 6),
            listDoorPosInfo = new List<BorderEntityData>()
            {
                new BorderEntityData
                {
                    entityModelType = EntityModelType.Door,
                    pos =new Vector2(3, 3),
                    entityAxis = 0
                }
            }
        };
        RoomInfo roomE = new RoomInfo()
        {
            roomType = RoomType.StorageRoom,
            roomSize = new uint[2] { 3, 3 },
            roomPosMin = new Vector2(3, -3),
            roomPosMax = new Vector2(6, 0),
            listDoorPosInfo = new List<BorderEntityData>()
            {
                new BorderEntityData
                {
                    entityModelType = EntityModelType.Door,
                    pos =new Vector2(4, 0),
                    entityAxis = 0
                }
            }
        };
        RoomInfo roomF = new RoomInfo()
        {
            roomType = RoomType.SecondBedRoom,
            roomSize = new uint[2] { 2, 2 },
            roomPosMin = new Vector2(4, 0),
            roomPosMax = new Vector2(6, 2),
            listDoorPosInfo = new List<BorderEntityData>()
            {
                new BorderEntityData
                {
                    entityModelType = EntityModelType.Door,
                    pos =new Vector2(4, 0),
                    entityAxis = 0
                }
            },
            listEmptyPosInfo = new List<BorderEntityData>()
            {
                 new BorderEntityData
                 {
                      entityModelType = EntityModelType.Null,
                      pos = new Vector2(4, 0),
                      entityAxis = 1
                 },
                 new BorderEntityData
                 {
                      entityModelType = EntityModelType.Null,
                      pos = new Vector2(4, 1),
                      entityAxis = 1
                 }
            }
        };
        RoomInfo roomG = new RoomInfo()
        {
            roomType = RoomType.StudyRoom,
            roomSize = new uint[2] { 4, 2 },
            roomPosMin = new Vector2(-1, -2),
            roomPosMax = new Vector2(3, 0),
            listDoorPosInfo = new List<BorderEntityData>()
            {
                new BorderEntityData
                {
                    entityModelType = EntityModelType.Door,
                    pos = new Vector2(2, 0),
                    entityAxis = 0
                }
            }
        };
        GenerateRoomModel.GetInstance.GenerateRoom(roomA);
        GenerateRoomModel.GetInstance.GenerateRoom(roomB);
        GenerateRoomModel.GetInstance.GenerateRoom(roomC);
        GenerateRoomModel.GetInstance.GenerateRoom(roomD);
        GenerateRoomModel.GetInstance.GenerateRoom(roomE);
        GenerateRoomModel.GetInstance.GenerateRoom(roomF);
        GenerateRoomModel.GetInstance.GenerateRoom(roomG);
    }

    private void TestGenerateRandom()
    {
    }
}
