using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static GenerateRoomData;
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
    public Button btnHalfRandomGenerate;
    public Button btnFullRandomGenerate;
    private void Start()
    {
        //固定生成
        btnFixedGenerate?.onClick.AddListener(() => { TestGenerateFixed(); });
        //半随机，需要指定各个房间大小以及邻接方位,系统自动随机所在位置
        btnHalfRandomGenerate?.onClick.AddListener(() => { TestGenerateHalfRandom(); });
        //完全随机
        btnFullRandomGenerate?.onClick?.AddListener(() => { TestGenerateFullRandom(); });
    }

    private void TestGenerateFixed()
    {
        ////TEST 模拟数据
        //GenerateRoomModel.GetInstance.RoomGroup = transform;
        //RoomInfo roomA = new RoomInfo()
        //{
        //    roomType = RoomType.LivingRoom,
        //    roomSize = new uint[2] { 4, 3 },
        //    roomPosMin = new Vector2(0, 0),
        //    roomPosMax = new Vector2(4, 3),
        //    listDoorPosInfo = new List<BorderEntityData>()
        //    {
        //        new BorderEntityData
        //        {
        //            entityModelType = EntityModelType.Door,
        //            pos = new Vector2(0, 2),
        //            entityAxis = 1
        //        },
        //        new BorderEntityData
        //        {
        //            entityModelType = EntityModelType.Door,
        //            pos =new Vector2(1, 3),
        //            entityAxis = 0
        //        },
        //        new BorderEntityData
        //        {
        //            entityModelType = EntityModelType.Door,
        //            pos =new Vector2(3, 3),
        //            entityAxis = 0
        //        },
        //        new BorderEntityData
        //        {
        //            entityModelType = EntityModelType.Door,
        //            pos =new Vector2(2, 0),
        //            entityAxis = 0
        //        }
        //    },
        //    listEmptyPosInfo = new List<BorderEntityData>()
        //    {
        //         new BorderEntityData
        //         {
        //              entityModelType = EntityModelType.Null,
        //              pos = new Vector2(4, 0),
        //              entityAxis = 1
        //         },
        //         new BorderEntityData
        //         {
        //              entityModelType = EntityModelType.Null,
        //              pos = new Vector2(4, 1),
        //              entityAxis = 1
        //         }
        //    }
        //};
        //RoomInfo roomB = new RoomInfo()
        //{
        //    roomType = RoomType.BalconyRoom,
        //    roomSize = new uint[2] { 3, 4 },
        //    roomPosMin = new Vector2(-3, 0),
        //    roomPosMax = new Vector2(0, 4),
        //    listDoorPosInfo = new List<BorderEntityData>()
        //    {
        //        new BorderEntityData
        //        {
        //            entityModelType = EntityModelType.Door,
        //            pos = new Vector2(0, 2),
        //            entityAxis = 1
        //        }
        //    }
        //};
        //RoomInfo roomC = new RoomInfo()
        //{
        //    roomType = RoomType.FirstBedRoom,
        //    roomSize = new uint[2] { 3, 4 },
        //    roomPosMin = new Vector2(0, 3),
        //    roomPosMax = new Vector2(3, 8),
        //    listDoorPosInfo = new List<BorderEntityData>()
        //    {
        //         new BorderEntityData
        //         {
        //             entityModelType = EntityModelType.Door,
        //             pos =new Vector2(1, 3),
        //             entityAxis = 0
        //         }
        //    }
        //};
        //RoomInfo roomD = new RoomInfo()
        //{
        //    roomType = RoomType.KitChenRoom,
        //    roomSize = new uint[2] { 3, 3 },
        //    roomPosMin = new Vector2(3, 3),
        //    roomPosMax = new Vector2(6, 6),
        //    listDoorPosInfo = new List<BorderEntityData>()
        //    {
        //        new BorderEntityData
        //        {
        //            entityModelType = EntityModelType.Door,
        //            pos =new Vector2(3, 3),
        //            entityAxis = 0
        //        }
        //    }
        //};
        //RoomInfo roomE = new RoomInfo()
        //{
        //    roomType = RoomType.StorageRoom,
        //    roomSize = new uint[2] { 3, 3 },
        //    roomPosMin = new Vector2(3, -3),
        //    roomPosMax = new Vector2(6, 0),
        //    listDoorPosInfo = new List<BorderEntityData>()
        //    {
        //        new BorderEntityData
        //        {
        //            entityModelType = EntityModelType.Door,
        //            pos =new Vector2(4, 0),
        //            entityAxis = 0
        //        }
        //    }
        //};
        //RoomInfo roomF = new RoomInfo()
        //{
        //    roomType = RoomType.SecondBedRoom,
        //    roomSize = new uint[2] { 2, 2 },
        //    roomPosMin = new Vector2(4, 0),
        //    roomPosMax = new Vector2(6, 2),
        //    listDoorPosInfo = new List<BorderEntityData>()
        //    {
        //        new BorderEntityData
        //        {
        //            entityModelType = EntityModelType.Door,
        //            pos =new Vector2(4, 0),
        //            entityAxis = 0
        //        }
        //    },
        //    listEmptyPosInfo = new List<BorderEntityData>()
        //    {
        //         new BorderEntityData
        //         {
        //              entityModelType = EntityModelType.Null,
        //              pos = new Vector2(4, 0),
        //              entityAxis = 1
        //         },
        //         new BorderEntityData
        //         {
        //              entityModelType = EntityModelType.Null,
        //              pos = new Vector2(4, 1),
        //              entityAxis = 1
        //         }
        //    }
        //};
        //RoomInfo roomG = new RoomInfo()
        //{
        //    roomType = RoomType.StudyRoom,
        //    roomSize = new uint[2] { 4, 2 },
        //    roomPosMin = new Vector2(-1, -2),
        //    roomPosMax = new Vector2(3, 0),
        //    listDoorPosInfo = new List<BorderEntityData>()
        //    {
        //        new BorderEntityData
        //        {
        //            entityModelType = EntityModelType.Door,
        //            pos = new Vector2(2, 0),
        //            entityAxis = 0
        //        }
        //    }
        //};
        //GenerateRoomModel.GetInstance.GenerateRoom(roomA, roomB, roomC, roomD, roomE, roomF, roomG);
    }

    private void TestGenerateHalfRandom()
    {
        List<RoomBaseInfo> roomBaseInfos = new()
        {
            new RoomBaseInfo
            {
                 curRoomType = RoomType.LivingRoom,
                 roomSize = new uint[]{ 4,6 },
                 targetRoomsDirRelation = new List<RoomsDirRelation>()
                 {
                     new RoomsDirRelation{ targetRoomType = RoomType.FirstBedRoom , locationRelation = DirEnum.Right , isCommonWall = true},
                     new RoomsDirRelation{ targetRoomType = RoomType.KitChenRoom , locationRelation = DirEnum.Down  , isCommonWall = true},
                 }
            },
            new RoomBaseInfo
            {
                 curRoomType = RoomType.FirstBedRoom,
                 roomSize = new uint[]{ 3,5 },
                 //targetRoomsDirRelation = new List<RoomsDirRelation>()
                 //{
                 //    new RoomsDirRelation{ targetRoomType = RoomType.LivingRoom , locationRelation = DirEnum.Left },
                 //}
            },
            new RoomBaseInfo
            {
                 curRoomType = RoomType.KitChenRoom,
                 roomSize = new uint[]{ 2,4 },
                 //targetRoomsDirRelation = new List<RoomsDirRelation>()
                 //{
                 //    new RoomsDirRelation{ targetRoomType = RoomType.LivingRoom , locationRelation = DirEnum.Up },
                 //}
            },
            new RoomBaseInfo
            {
                 curRoomType = RoomType.RestRoom,
                 roomSize = new uint[]{3,4 },
                 targetRoomsDirRelation = new List<RoomsDirRelation>()
                 {
                     new RoomsDirRelation{ targetRoomType = RoomType.LivingRoom , locationRelation = DirEnum.Right , isCommonWall = true},
                 }
            },
              new RoomBaseInfo
            {
                 curRoomType = RoomType.StudyRoom,
                 roomSize = new uint[]{2,3 },
                 targetRoomsDirRelation = new List<RoomsDirRelation>()
                 {
                     new RoomsDirRelation{ targetRoomType = RoomType.RestRoom , locationRelation = DirEnum.Right , isCommonWall = true},
                 }
            },
            new RoomBaseInfo
            {
                 curRoomType = RoomType.BalconyRoom,
                 roomSize = new uint[]{2,5 },
                 targetRoomsDirRelation = new List<RoomsDirRelation>()
                 {
                     new RoomsDirRelation{ targetRoomType = RoomType.FirstBedRoom , locationRelation = DirEnum.Up , isCommonWall = true},
                 }
            },
            new RoomBaseInfo
            {
                 curRoomType = RoomType.SecondBedRoom,
                 roomSize = new uint[]{4,5 },
                 targetRoomsDirRelation = new List<RoomsDirRelation>()
                 {
                     new RoomsDirRelation{ targetRoomType = RoomType.LivingRoom , locationRelation = DirEnum.Down , isCommonWall = true},
                 }
            },
              new RoomBaseInfo
            {
                 curRoomType = RoomType.StorageRoom,
                 roomSize = new uint[]{3,2 },
                 targetRoomsDirRelation = new List<RoomsDirRelation>()
                 {
                     new RoomsDirRelation{ targetRoomType = RoomType.SecondBedRoom , locationRelation = DirEnum.Right , isCommonWall = true},
                 }
            },
        };
        GenerateRoomData.GetInstance.GenerateRandomRoomInfoData(roomBaseInfos, (p) =>
        {
            if (p != null)
            {
                GenerateRoomModel.GetInstance.GenerateRoom(p);
            }
            else
            {
                TestGenerateFullRandom();
            }
        });
    }


    private RoomType GetRandomRoomType(RoomType curRoomType)
    {
        RoomType res = RoomType.Null;
        do
        {
            res = (RoomType)Random.Range(1, 9);
        } while (res == curRoomType);
        return res;
    }

    private DirEnum GetRandomDir()
    {
        return (DirEnum)Random.Range(0, 4);
    }


    private void TestGenerateFullRandom()
    {
        Vector2 roomXRange = new Vector2(3, 8);
        Vector2 roomYRange = new Vector2(3, 6);

        List<RoomBaseInfo> roomBaseInfos = new List<RoomBaseInfo>();

        for (int i = 1; i < 8; i++)
        {
            roomBaseInfos.Add(new RoomBaseInfo
            {
                curRoomType = (RoomType)i,
                roomSize = new uint[] { (uint)Random.Range(roomXRange.x, roomXRange.y), (uint)Random.Range(roomYRange.x, roomYRange.y) },
                targetRoomsDirRelation = new List<RoomsDirRelation>()
                {
                    new RoomsDirRelation { targetRoomType = GetRandomRoomType((RoomType)i), locationRelation = GetRandomDir(), isCommonWall = true },
                },
            });
        }
        GenerateRoomData.GetInstance.GenerateRandomRoomInfoData(roomBaseInfos, (p) =>
        {
            if (p != null)
            {
                GenerateRoomModel.GetInstance.GenerateRoom(p);
            }
            else
            {
                TestGenerateFullRandom();
            }
        });

    }

}
