using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static GenerateRoomData;
using static GenerateRoomBorderModel;
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

    public InputField txtFullRandomCount;
    public InputField txtFullRandomLengthRangeMin;
    public InputField txtFullRandomLengthRangeMax;
    public InputField txtFullRandomWidthRangeMin;
    public InputField txtFullRandomWidthRangeMax;

    private void Awake()
    {
        txtFullRandomCount.text = "3";
        txtFullRandomLengthRangeMin.text = "4";
        txtFullRandomLengthRangeMax.text = "7";
        txtFullRandomWidthRangeMin.text = "4";
        txtFullRandomWidthRangeMax.text= "8";
    }

    private void Start()
    {
        ////固定生成
        //btnFixedGenerate?.onClick.AddListener(() => { TestGenerateFixed(); });
        //半随机，需要指定各个房间大小以及邻接方位,系统自动随机所在位置
        btnHalfRandomGenerate?.onClick.AddListener(() => { TestGenerateHalfRandom(); });


        //完全随机
        btnFullRandomGenerate?.onClick?.AddListener(() =>
        {
            if (uint.TryParse(txtFullRandomCount.text, out uint roomCount)
            && uint.TryParse(txtFullRandomLengthRangeMin.text, out uint lengthRangeMin)
            && uint.TryParse(txtFullRandomLengthRangeMax.text, out uint lengthRangeMax)
            && uint.TryParse(txtFullRandomWidthRangeMin.text, out uint widthRangeMin)
            && uint.TryParse(txtFullRandomWidthRangeMax.text, out uint widthRangeMax))
            {
                TestGenerateFullRandom(new FullRandomData
                {
                    roomCount = roomCount,
                    lengthRangeMin = lengthRangeMin,
                    lengthRangeMax = lengthRangeMax,
                    widthRangeMax = widthRangeMax,
                    widthRangeMin = widthRangeMin,
                });
            }
        });

        Transform RoomBorderGroupNode = GameObject.Find("RoomBorderGroupNode")?.transform;
        if (RoomBorderGroupNode == null)
        {
            RoomBorderGroupNode = new GameObject("RoomBorderGroupNode").transform;
        }
        GenerateRoomBorderModel.GetInstance.RoomGroup = RoomBorderGroupNode;
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
        GenerateRoomData.GetInstance.GenerateRandomRoomInfoData(roomBaseInfos, (p, k) =>
        {
            if (p == null || k == null)
            {
                Debug.LogError("helf random is fail");
            }
            else
            {
                GenerateRoomBorderModel.GetInstance.GenerateRoomBorder(p);
                GenerageRoomItemModel.GetInstance.GenerateRoomItem(k);
            }
            
        });
    }

    #region FullRandom

    private RoomType GetRandomRoomType(RoomType curRoomType)
    {
        RoomType res = RoomType.Null;
        do
        {
            res = (RoomType)Random.Range(1, m_RoomCount + 1);
        } while (res == curRoomType);
        return res;
    }

    private DirEnum GetRandomDir()
    {
        return (DirEnum)Random.Range(0, 4);
    }


    private uint m_RoomCount = 4;
    private void TestGenerateFullRandom(FullRandomData fullRandomData)
    {
        m_RoomCount = fullRandomData.roomCount;
        Vector2 roomXRange = new Vector2(fullRandomData.lengthRangeMin, fullRandomData.lengthRangeMax);
        Vector2 roomYRange = new Vector2(fullRandomData.widthRangeMin, fullRandomData.widthRangeMax);

        List<RoomBaseInfo> roomBaseInfos = new List<RoomBaseInfo>();

        for (int i = 1; i <= fullRandomData.roomCount; i++)
        {
            roomBaseInfos.Add(new RoomBaseInfo
            {
                curRoomType = (RoomType)i,
                roomSize = new uint[] { (uint)Random.Range(roomXRange.x, roomXRange.y + 1), (uint)Random.Range(roomYRange.x, roomYRange.y + 1) },
                targetRoomsDirRelation = new List<RoomsDirRelation>()
                {
                    new RoomsDirRelation { targetRoomType = GetRandomRoomType((RoomType)i), locationRelation = GetRandomDir(), isCommonWall = true },
                },
            });
        }

        GenerateRoomData.GetInstance.GenerateRandomRoomInfoData(roomBaseInfos, (p, k) =>
        {
            if (p == null || k == null)
            {
                Debug.LogError("FullRandom Fail Regenerate...");
                TestGenerateFullRandom(fullRandomData);
            }
            else
            {
                GenerateRoomBorderModel.GetInstance.GenerateRoomBorder(p);
                GenerageRoomItemModel.GetInstance.GenerateRoomItem(k);
            }
        });
    }

    class FullRandomData
    {
        public uint roomCount;
        public uint lengthRangeMin;
        public uint lengthRangeMax;
        public uint widthRangeMin;
        public uint widthRangeMax;
    }
    #endregion

}
