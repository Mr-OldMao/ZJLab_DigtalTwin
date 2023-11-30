using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework;
using System;
using static GenerateRoomBorderModel;
/// <summary>
/// 标题：上帝视角摄像机
/// 功能：根据生成的场景调整大小位置
/// 作者：毛俊峰
/// 时间：2023.09.11
/// </summary>
public class TopCameraConfig : MonoBehaviour
{
    private Camera m_CameraTop;
    private void Awake()
    {
        m_CameraTop = GetComponent<Camera>();
    }
    private void Start()
    {
        RegisterMsgEvent();
    }

    private void RegisterMsgEvent()
    {
        MsgEvent.RegisterMsgEvent(MsgEventName.GenerateSceneComplete, () =>
        {
            //size = 1 => RoomSize(2.5,1.5)
            Vector2 roomSize = GetAllRoomSize();
            float cameraSize = roomSize.x / 2.5f > roomSize.y / 1.5f ? roomSize.x / 2.5f : roomSize.y / 1.5f;
            m_CameraTop.orthographicSize = cameraSize;
            m_CameraTop.transform.position = new Vector3(roomSize.x/2, 50f, roomSize.y/2);
         //   Debug.Log("RoomSize：" + GetAllRoomSize() + "，cameraSize：" + cameraSize);
        });
    }

    private Vector2 GetAllRoomSize()
    {
        int minX = 0, minY = 0, maxX = 0, maxY = 0;
        List<RoomInfo> roomData = GenerateRoomData.GetInstance.m_ListRoomInfo;
       
        foreach (RoomInfo ri in roomData)
        {
            if (ri.roomPosMin.x < minX)
            {
                minX = (int)ri.roomPosMin.x;
            }
            if (ri.roomPosMin.y < minY)
            {
                minY = (int)ri.roomPosMin.y;
            }
            if (ri.roomPosMax.x > maxX)
            {
                maxX = (int)ri.roomPosMax.x;
            }
            if (ri.roomPosMax.y > maxY)
            {
                maxY = (int)ri.roomPosMax.y;
            }
        }
        return new Vector3(maxX - minX, maxY - minY);
    }
}
