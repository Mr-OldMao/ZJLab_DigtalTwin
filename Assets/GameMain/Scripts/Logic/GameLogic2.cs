using MFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Feature_Robot_Pos;

/// <summary>
/// 标题：数字孪生程序主逻辑
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.10.23
/// </summary>
public class GameLogic2 : SingletonByMono<GameLogic2>
{
    public GameObject robotPrefab;
    public GameObject peoplePrefab;
    public Dictionary<string, EntityInfo> m_DicEntityArray = new Dictionary<string, EntityInfo>();
    private Transform m_RobotRootNode;
    public class EntityInfo
    {
        public string id;
        public int type;
        public GameObject entity;
        public Transform cvsRobot;
    }

    public void Init()
    {
        GameObject RobotRootNode = GameObject.Find("RobotRootNode");
        if (RobotRootNode != null)
        {
            m_RobotRootNode = RobotRootNode.transform;
        }
        else
        {
            m_RobotRootNode = new GameObject("RobotRootNode").transform;
        }

        //异步加载ab资源
        LoadAssetsByAddressable.GetInstance.LoadAssetsAsyncByLable(new List<string> { "Scene2" }, () =>
        {
            Debug.Log("ab资源加载完毕回调");
            new ReadConfigFile(() =>
            {
                //接入网络通信 
                NetworkMQTT();

                
                UIManager.GetInstance.Show<UIFormScene2>().Init();

            });
        });

        //LoadAssetsByAddressable.GetInstance.LoadAssetsAsyncByDirectory("/GameMain/AB/Prefab/Robot/", () =>
        //{
        //    Debug.Log("ab资源加载完毕回调");
        //    new ReadConfigFile(() =>
        //    {
        //        //接入网络通信 
        //        NetworkMQTT();
        //    });
        //});
    }
    private void NetworkMQTT()
    {
        InterfaceDataCenter.GetInstance.InitMQTT();
    }

    private void Update()
    {
        if (MainData.feature_robot_pos?.Count > 0)
        {
            Feature_Robot_Pos feature_Robot_Pos = MainData.feature_robot_pos.Dequeue();
            DisposeRobotPos(feature_Robot_Pos);
            DisposeRobotUI(feature_Robot_Pos);
        }
        if (MainData.feature_People_Perceptions?.Count > 0)
        {
            Feature_People_Perception feature_People_Perception = MainData.feature_People_Perceptions.Dequeue();
            DisposePeoplePerception(feature_People_Perception);
        }

        if (Input.GetKey(KeyCode.F6))
        {
            int timestamp = int.Parse(System.DateTime.Now.ToString("HHmmss"));


            string testJson = @"{
    ""robotId"":""test"",
    ""timestamp"":" + timestamp + @",
    ""data"":{
        ""feature"":{
            ""orientation"":[0.0,30.0,0.0],
            ""position"":[0.0,0.0,1.0]
        }
    },
    ""clientId"":""test01""
}";
            NetworkMqtt.GetInstance.Publish(InterfaceDataCenter.TOPIC_ROBOT_POS,
                testJson);
            //NetworkMqtt.GetInstance.Publish(InterfaceDataCenter.TOPIC_PEOPLE_PERCEPTION,
            //    testJson);
        }

        if (Input.GetKey(KeyCode.F7))
        {
            int timestamp = int.Parse(System.DateTime.Now.ToString("HHmmss"));


            string testJson = @"
{""robotId"":""iben_a03_3"",""timestamp"":1701852288,""data"":{""feature"":[{""gender"":-1,""bbox"":[821.6388750059427,437.8141886913876,931.2474431189472,570.2288415655232],""visitor_id"":-1,""person_point"":{""left_judge"":0,""right_judge"":0,""left_dir"":[0.619624535276613,0.5723316805379481,-0.48834458490702637],""right_dir"":[-0.1853649763112824,-0.0011125552023106833,-0.9594120687701156],""left_pos"":[3.500957763671875,2.19164599609375,13.062],""right_pos"":[4.62186474609375,2.477716796875,13.75]},""face_box"":[],""location_world"":[10.382868690331652,-2.4299413594398414,2.5257156912569],""loss_reason"":1,""detect_id"":-1,""face_pose"":[],""person_2d_keypoint"":[[882.796875,448.8697509765625,0.6998059749603271],[882.796875,442.828125,0.5206851959228516],[881.2864379882812,441.31768798828125,0.7208263874053955],[852.5885620117188,439.8072509765625,0.3227178454399109],[861.6510620117188,441.31768798828125,0.7856888175010681],[823.890625,468.50518798828125,0.34711381793022156],[867.6926879882812,476.0572509765625,0.4382656216621399],[900.921875,515.328125,0.0802328884601593],[903.9426879882812,521.3697509765625,0.2825605273246765],[885.8176879882812,542.515625,0.2447684109210968],[932.640625,539.4948120117188,0.09929623454809189],[832.953125,562.1510620117188,0.11995057016611099],[860.140625,566.6823120117188,0.23573744297027588],[928.109375,539.4948120117188,0.03720126301050186],[929.6198120117188,541.0051879882812,0.03803718835115433],[870.7135620117188,550.0676879882812,0.04773281142115593],[857.875,597.6458129882812,0.04069367051124573]],""angle"":-1.8910229142568662,""speak"":-1,""mask"":-1,""glass"":-1,""body_pose"":[0,0,0],""person_3d_keypoint"":[[4.9411279296875,1.880501953125,13.75],[4.99359326171875,1.771135986328125,13.896],[5.025396484375,1.76834033203125,14.045],[4.80858740234375,1.8871123046875,15.367],[4.90729541015625,1.8903426513671875,15.014],[3.500957763671875,2.19164599609375,13.062],[4.62186474609375,2.477716796875,13.75],[4.64041650390625,2.884485107421875,11.984],[4.48994921875,2.864522216796875,11.458],[4.52814208984375,3.51526171875,12.44],[0.0,0.0,0.0],[2.0126136474609373,2.238487060546875,7.138],[1.1935257568359374,1.1733692626953125,3.669],[1.4556383056640625,0.9453521118164063,3.411],[1.4196095581054688,0.9393148498535157,3.315],[2.330590576171875,2.017415771484375,6.839],[1.2428634033203125,1.4259561767578126,3.876]],""velocity"":[1.1429935375821256,0.6060377558084772,0.7855822508907959],""person_action"":{""hand_shake"":null,""take_a_photo"":null,""point_to_an_object"":null,""read"":null,""talk_to_a_person"":null,""touch_an_object"":null,""grab_a_person"":null,""hand_wave"":null,""text_on_look_at_a_cellphone"":null,""watch_TV"":null,""watch_a_person"":null,""drink"":null,""unknown"":null,""hug_a_person"":null,""hand_clap"":null,""turn_a_screwdriver"":null,""listen_to_a_person"":null,""answer_phone"":null,""stand"":null,""walk"":null,""sit"":null},""camera_location"":[0,0,0],""mouth"":-1,""gaze"":{""conf"":1.0,""location"":null,""target"":""""},""track_id"":5,""move_dir_x"":""leave"",""location_confidence"":1,""loss_track_time"":0,""move_dir_y"":""left"",""location"":[10.545017509411593,-2.9341758405228022,2.5257156912569],""time"":{""nsecs"":110935926,""secs"":1701852288},""face_pose_world"":[0,0,0],""intention_info"":{""body_left_refer"":-1,""engagement_with_location"":false,""engagement"":false,""body_right_refer"":-1,""head_refer"":-1},""age"":-1,""status"":2}],""featureId"":""people_perception""},""clientId"":null}
";
            NetworkMqtt.GetInstance.Publish(InterfaceDataCenter.TOPIC_PEOPLE_PERCEPTION,
                testJson);
        }
    }

    /// <summary>
    /// 处理机器人坐标
    /// </summary>
    private void DisposeRobotPos(Feature_Robot_Pos feature_Robot_Pos)
    {
        GameObject entity = GetEntityInfo(feature_Robot_Pos.robotId).entity;
        if (entity != null)
        {
            Feature_Robot_Pos_data_feature transInfo = feature_Robot_Pos.data.feature;
            entity.transform.localPosition = new Vector3(transInfo.position[0], transInfo.position[1], transInfo.position[2]);
            entity.transform.rotation = Quaternion.Euler(new Vector3(transInfo.orientation[0], transInfo.orientation[1], transInfo.orientation[2]));
        }
    }

    private void DisposeRobotUI(Feature_Robot_Pos feature_Robot_Pos)
    {
        Transform cvsRobot = GetEntityInfo(feature_Robot_Pos.robotId).cvsRobot;
        if (cvsRobot != null)
        {
            cvsRobot.Find<Text>("txtID").text = feature_Robot_Pos.robotId;
            cvsRobot.Find<Text>("txtTime").text = TransformTime(feature_Robot_Pos.timestamp);
        }
    }

    /// <summary>
    /// 时间戳转时间
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    private string TransformTime(double timestamp)
    {
        DateTime startTime1 = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Local);
        DateTime dt1 = startTime1.AddMilliseconds(timestamp);//传入的时间戳
        return dt1.ToString();
    }

    private EntityInfo GetEntityInfo(string id, bool isRobot = true)
    {
        EntityInfo res = null;
        if (m_DicEntityArray.ContainsKey(id))
        {
            res = m_DicEntityArray[id];
        }
        else
        {
            GameObject prefab = isRobot ? LoadAssetsByAddressable.GetInstance.dicCacheAssets["RobotPrefab"]?.items[0] : LoadAssetsByAddressable.GetInstance.dicCacheAssets["PeoplePrefab"]?.items[0];
            GameObject obj = Instantiate(prefab);
            res = new EntityInfo
            {
                entity = obj,
                id = id,
                cvsRobot = obj.transform.Find("CvsRobot")
            };
            obj.transform.parent = m_RobotRootNode;
            obj.name = id;
            m_DicEntityArray.Add(id, res);
        }
        return res;
    }

    /// <summary>
    /// 处理访客坐标信息
    /// </summary>
    private void DisposePeoplePerception(Feature_People_Perception feature_People_Perception)
    {
        GameObject entity = GetEntityInfo(feature_People_Perception.robotId,false).entity;
        if (entity != null)
        {
            float[] pos = feature_People_Perception.data.feature[0].location;
            if (pos[1] < 0)
            {
                pos[1] = 0;
            }
            entity.transform.localPosition = new Vector3(pos[0], pos[1], pos[2]);

            if (feature_People_Perception.data.feature[0].angle != null)
            {
                float rotAngle = (float)feature_People_Perception.data.feature[0].angle;
                entity.transform.rotation = Quaternion.Euler(new Vector3(0, rotAngle, 0));
            }
        }
    }
}
