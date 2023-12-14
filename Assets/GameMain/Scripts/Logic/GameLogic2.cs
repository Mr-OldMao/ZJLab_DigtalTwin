using MFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Feature_People_Perception;
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
    public Dictionary<string, EntityInfo> m_DicRobotEntityArray = new Dictionary<string, EntityInfo>();

    public Dictionary<string, EntityInfo> m_DicPeopleEntityArray = new Dictionary<string, EntityInfo>();



    private Transform m_RobotRootNode;
    private Transform m_PeopleRootNode;
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
        GameObject PeopleRootNode = GameObject.Find("PeopleRootNode");
        if (PeopleRootNode != null)
        {
            m_PeopleRootNode = PeopleRootNode.transform;
        }
        else
        {
            m_PeopleRootNode = new GameObject("PeopleRootNode").transform;
        }

        //异步加载ab资源
        LoadAssetsByAddressable.GetInstance.LoadAssetsAsyncByLable(new List<string> { "Scene2", "PeopleEntity" }, () =>
        {
            Debug.Log("ab资源加载完毕回调");
            new ReadConfigFile(() =>
            {
                //接入网络通信 
                NetworkMQTT();
                Debugger.Log(LoadAssetsByAddressable.GetInstance.dicCacheAssets);

                UIManager.GetInstance.Show<UIFormScene2>().Init();

            });
        });


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
            Debugger.Log("feature_People_Perception.robotId：" + feature_People_Perception.robotId);
            DisposePeoplePos(feature_People_Perception);
            DisposePeopleUI(feature_People_Perception);
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


            string testJson = "{\r\n    \"robotId\": \"x_biped_upper_part_0\",\r\n    \"timestamp\": 1702437482,\r\n    \"data\": {\r\n        \"feature\": [\r\n            {\r\n                \"gender\": 0,\r\n                \"bbox\": [],\r\n                \"visitor_id\": -1,\r\n                \"person_point\": {\r\n                    \"left_judge\": -1,\r\n                    \"right_judge\": -1,\r\n                    \"left_dir\": [\r\n                        0,\r\n                        0,\r\n                        0\r\n                    ],\r\n                    \"right_dir\": [\r\n                        0,\r\n                        0,\r\n                        0\r\n                    ],\r\n                    \"left_pos\": [\r\n                        0,\r\n                        0,\r\n                        0\r\n                    ],\r\n                    \"right_pos\": [\r\n                        0,\r\n                        0,\r\n                        0\r\n                    ]\r\n                },\r\n                \"face_box\": [],\r\n                \"location_world\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"loss_reason\": 1,\r\n                \"detect_id\": -1,\r\n                \"face_pose\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"person_2d_keypoint\": [],\r\n                \"angle\": null,\r\n                \"speak\": -1,\r\n                \"mask\": -1,\r\n                \"glass\": -1,\r\n                \"body_pose\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"person_3d_keypoint\": [],\r\n                \"velocity\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"person_action\": {\r\n                    \"hand_shake\": null,\r\n                    \"take_a_photo\": null,\r\n                    \"point_to_an_object\": null,\r\n                    \"read\": null,\r\n                    \"talk_to_a_person\": null,\r\n                    \"touch_an_object\": null,\r\n                    \"grab_a_person\": null,\r\n                    \"hand_wave\": null,\r\n                    \"text_on_look_at_a_cellphone\": null,\r\n                    \"watch_TV\": null,\r\n                    \"watch_a_person\": null,\r\n                    \"drink\": null,\r\n                    \"unknown\": null,\r\n                    \"hug_a_person\": null,\r\n                    \"hand_clap\": null,\r\n                    \"turn_a_screwdriver\": null,\r\n                    \"listen_to_a_person\": null,\r\n                    \"answer_phone\": null,\r\n                    \"stand\": null,\r\n                    \"walk\": null,\r\n                    \"sit\": null\r\n                },\r\n                \"camera_location\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"mouth\": -1,\r\n                \"gaze\": {\r\n                    \"conf\": null,\r\n                    \"location\": null,\r\n                    \"target\": null\r\n                },\r\n                \"track_id\": 1,\r\n                \"move_dir_x\": \"unknown\",\r\n                \"location_confidence\": 1,\r\n                \"loss_track_time\": 0,\r\n                \"move_dir_y\": \"unknown\",\r\n                \"location\": [\r\n                    0.4911286132130158,\r\n                    1.8817616074412558,\r\n                    0\r\n                ],\r\n                \"time\": {\r\n                    \"nsecs\": 160089600,\r\n                    \"secs\": 1702437482\r\n                },\r\n                \"face_pose_world\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"intention_info\": {\r\n                    \"body_left_refer\": -1,\r\n                    \"engagement_with_location\": null,\r\n                    \"engagement\": null,\r\n                    \"body_right_refer\": -1,\r\n                    \"head_refer\": -1\r\n                },\r\n                \"age\": -1,\r\n                \"status\": 2\r\n            },\r\n            {\r\n                \"gender\": 0,\r\n                \"bbox\": [],\r\n                \"visitor_id\": -1,\r\n                \"person_point\": {\r\n                    \"left_judge\": -1,\r\n                    \"right_judge\": -1,\r\n                    \"left_dir\": [\r\n                        0,\r\n                        0,\r\n                        0\r\n                    ],\r\n                    \"right_dir\": [\r\n                        0,\r\n                        0,\r\n                        0\r\n                    ],\r\n                    \"left_pos\": [\r\n                        0,\r\n                        0,\r\n                        0\r\n                    ],\r\n                    \"right_pos\": [\r\n                        0,\r\n                        0,\r\n                        0\r\n                    ]\r\n                },\r\n                \"face_box\": [],\r\n                \"location_world\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"loss_reason\": 1,\r\n                \"detect_id\": -1,\r\n                \"face_pose\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"person_2d_keypoint\": [],\r\n                \"angle\": null,\r\n                \"speak\": -1,\r\n                \"mask\": -1,\r\n                \"glass\": -1,\r\n                \"body_pose\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"person_3d_keypoint\": [],\r\n                \"velocity\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"person_action\": {\r\n                    \"hand_shake\": null,\r\n                    \"take_a_photo\": null,\r\n                    \"point_to_an_object\": null,\r\n                    \"read\": null,\r\n                    \"talk_to_a_person\": null,\r\n                    \"touch_an_object\": null,\r\n                    \"grab_a_person\": null,\r\n                    \"hand_wave\": null,\r\n                    \"text_on_look_at_a_cellphone\": null,\r\n                    \"watch_TV\": null,\r\n                    \"watch_a_person\": null,\r\n                    \"drink\": null,\r\n                    \"unknown\": null,\r\n                    \"hug_a_person\": null,\r\n                    \"hand_clap\": null,\r\n                    \"turn_a_screwdriver\": null,\r\n                    \"listen_to_a_person\": null,\r\n                    \"answer_phone\": null,\r\n                    \"stand\": null,\r\n                    \"walk\": null,\r\n                    \"sit\": null\r\n                },\r\n                \"camera_location\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"mouth\": -1,\r\n                \"gaze\": {\r\n                    \"conf\": null,\r\n                    \"location\": null,\r\n                    \"target\": null\r\n                },\r\n                \"track_id\": 2,\r\n                \"move_dir_x\": \"unknown\",\r\n                \"location_confidence\": 1,\r\n                \"loss_track_time\": 0,\r\n                \"move_dir_y\": \"unknown\",\r\n                \"location\": [\r\n                    0.5339549631637497,\r\n                    1.928978804075479,\r\n                    0\r\n                ],\r\n                \"time\": {\r\n                    \"nsecs\": 160089600,\r\n                    \"secs\": 1702437482\r\n                },\r\n                \"face_pose_world\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"intention_info\": {\r\n                    \"body_left_refer\": -1,\r\n                    \"engagement_with_location\": null,\r\n                    \"engagement\": null,\r\n                    \"body_right_refer\": -1,\r\n                    \"head_refer\": -1\r\n                },\r\n                \"age\": -1,\r\n                \"status\": 2\r\n            },\r\n            {\r\n                \"gender\": 0,\r\n                \"bbox\": [],\r\n                \"visitor_id\": -1,\r\n                \"person_point\": {\r\n                    \"left_judge\": -1,\r\n                    \"right_judge\": -1,\r\n                    \"left_dir\": [\r\n                        0,\r\n                        0,\r\n                        0\r\n                    ],\r\n                    \"right_dir\": [\r\n                        0,\r\n                        0,\r\n                        0\r\n                    ],\r\n                    \"left_pos\": [\r\n                        0,\r\n                        0,\r\n                        0\r\n                    ],\r\n                    \"right_pos\": [\r\n                        0,\r\n                        0,\r\n                        0\r\n                    ]\r\n                },\r\n                \"face_box\": [],\r\n                \"location_world\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"loss_reason\": 1,\r\n                \"detect_id\": -1,\r\n                \"face_pose\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"person_2d_keypoint\": [],\r\n                \"angle\": null,\r\n                \"speak\": -1,\r\n                \"mask\": -1,\r\n                \"glass\": -1,\r\n                \"body_pose\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"person_3d_keypoint\": [],\r\n                \"velocity\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"person_action\": {\r\n                    \"hand_shake\": null,\r\n                    \"take_a_photo\": null,\r\n                    \"point_to_an_object\": null,\r\n                    \"read\": null,\r\n                    \"talk_to_a_person\": null,\r\n                    \"touch_an_object\": null,\r\n                    \"grab_a_person\": null,\r\n                    \"hand_wave\": null,\r\n                    \"text_on_look_at_a_cellphone\": null,\r\n                    \"watch_TV\": null,\r\n                    \"watch_a_person\": null,\r\n                    \"drink\": null,\r\n                    \"unknown\": null,\r\n                    \"hug_a_person\": null,\r\n                    \"hand_clap\": null,\r\n                    \"turn_a_screwdriver\": null,\r\n                    \"listen_to_a_person\": null,\r\n                    \"answer_phone\": null,\r\n                    \"stand\": null,\r\n                    \"walk\": null,\r\n                    \"sit\": null\r\n                },\r\n                \"camera_location\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"mouth\": -1,\r\n                \"gaze\": {\r\n                    \"conf\": null,\r\n                    \"location\": null,\r\n                    \"target\": null\r\n                },\r\n                \"track_id\": 3,\r\n                \"move_dir_x\": \"unknown\",\r\n                \"location_confidence\": 1,\r\n                \"loss_track_time\": 0,\r\n                \"move_dir_y\": \"unknown\",\r\n                \"location\": [\r\n                    0.5805256439172308,\r\n                    1.8731614227999107,\r\n                    0\r\n                ],\r\n                \"time\": {\r\n                    \"nsecs\": 160089600,\r\n                    \"secs\": 1702437482\r\n                },\r\n                \"face_pose_world\": [\r\n                    0,\r\n                    0,\r\n                    0\r\n                ],\r\n                \"intention_info\": {\r\n                    \"body_left_refer\": -1,\r\n                    \"engagement_with_location\": null,\r\n                    \"engagement\": null,\r\n                    \"body_right_refer\": -1,\r\n                    \"head_refer\": -1\r\n                },\r\n                \"age\": -1,\r\n                \"status\": 2\r\n            }\r\n        ],\r\n        \"featureId\": \"people_perception\"\r\n    },\r\n    \"clientId\": null\r\n}\r\n\r\n";
            NetworkMqtt.GetInstance.Publish(InterfaceDataCenter.TOPIC_PEOPLE_PERCEPTION,
                testJson);
        }
    }

    /// <summary>gen 
    /// 处理机器人坐标
    /// </summary>
    private void DisposeRobotPos(Feature_Robot_Pos feature_Robot_Pos)
    {
        GameObject entity = GetEntityInfo(feature_Robot_Pos.robotId).entity;
        if (entity != null)
        {
            Feature_Robot_Pos_data_feature transInfo = feature_Robot_Pos.data.feature;
            entity.transform.localPosition = new Vector3(transInfo.position[1], transInfo.position[2], transInfo.position[0]);
            //entity.transform.rotation = Quaternion.Euler(new Vector3(transInfo.orientation[2], transInfo.orientation[1], transInfo.orientation[0]));
            entity.transform.rotation = Quaternion.Euler(new Vector3(0, transInfo.orientation[0], 0));
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
        //DateTime startTime1 = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Local);
        //DateTime dt1 = startTime1.AddMilliseconds(timestamp);//传入的时间戳
        //return dt1.ToString();

        System.DateTime startTime = System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));//获取时间戳
        System.DateTime dt = startTime.AddSeconds(timestamp);
        string t = dt.ToString("yyyy/MM/dd HH:mm:ss");//转化为日期时间
        return t;
    }

    private EntityInfo GetEntityInfo(string id, bool isRobot = true)
    {
        //id = isRobot ? "R_" + id : "P_" + id;

        EntityInfo res = null;

        if (isRobot)
        {
            if (m_DicRobotEntityArray.ContainsKey(id))
            {
                res = m_DicRobotEntityArray[id];
            }
            else
            {
                GameObject prefab = LoadAssetsByAddressable.GetInstance.dicCacheAssets["RobotPrefab"]?.items[0];
                GameObject obj = Instantiate(prefab);
                res = new EntityInfo
                {
                    entity = obj,
                    id = id,
                    cvsRobot = obj.transform.Find("CvsRobot")
                };
                obj.transform.parent = m_RobotRootNode;
                obj.name = id;
                m_DicRobotEntityArray.Add(id, res);
            }
        }
        else
        {
            if (m_DicPeopleEntityArray.ContainsKey(id))
            {
                res = m_DicPeopleEntityArray[id];
            }
            else
            {
                //peoplePrefab
                GameObject prefab = LoadAssetsByAddressable.GetInstance.dicCacheAssets["PeoplePrefab"]?.items[0];
                GameObject obj = Instantiate(prefab);
                //body
                if (int.TryParse(id, out int botyIndex))
                {
                    botyIndex = botyIndex % LoadAssetsByAddressable.GetInstance.dicCacheAssets["Humano"].items.Count;
                }
                MFramework.Debugger.Log("botyIndex:" + botyIndex);
                GameObject body = LoadAssetsByAddressable.GetInstance.dicCacheAssets["Humano"]?.items[botyIndex];
                GameObject bodyClone = Instantiate(body);
                bodyClone.transform.parent = obj?.transform.Find("BodyContainer");
                bodyClone.transform.localPosition = Vector3.zero;
                //cvs
                res = new EntityInfo
                {
                    entity = obj,
                    id = id,
                    cvsRobot = obj.transform.Find("CvsRobot")
                };
                obj.transform.parent = m_PeopleRootNode;
                obj.name = id;
                m_DicPeopleEntityArray.Add(id, res);
            }
        }

        return res;
    }

    /// <summary>
    /// 处理访客坐标信息
    /// </summary>
    private void DisposePeoplePos(Feature_People_Perception feature_People_Perception)
    {
        if (feature_People_Perception.data.feature?.Length > 0)
        {
            //访客信息
            Feature_People_Perception_data_feature[] peopleInfos = feature_People_Perception.data.feature;

            for (int i = 0; i < peopleInfos?.Length; i++)
            {
                Feature_People_Perception_data_feature peopleInfo = peopleInfos[i];
                GameObject entity = GetEntityInfo(peopleInfo.track_id.ToString(), false).entity;
                if (entity != null)
                {
                    float[] pos = peopleInfo.location;
                    if (pos[1] < 0)
                    {
                        pos[1] = 0;
                    }
                    entity.transform.localPosition = new Vector3(pos[1], pos[2], pos[0]);


                    if (peopleInfo.angle != null)
                    {
                        float rotAngle = (float)peopleInfo.angle;
                        entity.transform.rotation = Quaternion.Euler(new Vector3(0, rotAngle, 0));
                    }
                }
            }


        }
    }

    /// <summary>
    /// 处理访客UI数据
    /// </summary>
    /// <param name="feature_People_Perception"></param>
    private void DisposePeopleUI(Feature_People_Perception feature_People_Perception)
    {
        if (feature_People_Perception.data.feature?.Length > 0)
        {
            //访客信息
            Feature_People_Perception_data_feature[] peopleInfos = feature_People_Perception.data.feature;
            for (int i = 0; i < peopleInfos?.Length; i++)
            {
                Feature_People_Perception_data_feature peopleInfo = peopleInfos[i];
                Transform cvsRobot = GetEntityInfo(peopleInfo.track_id.ToString(), false).cvsRobot;
                if (cvsRobot != null)
                {
                    cvsRobot.Find<Text>("txtPeopleID").text = peopleInfo.track_id.ToString();
                    cvsRobot.Find<Text>("txtRobotID").text = feature_People_Perception.robotId;
                    cvsRobot.Find<Text>("txtTime").text = TransformTime(feature_People_Perception.timestamp);
                }
            }
        }


    }
}
