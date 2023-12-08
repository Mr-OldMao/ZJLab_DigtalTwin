using MFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制相机移动(长按滚轮平移)、旋转（右键）、视角缩放（滚轮滑动）
/// </summary>
public class CameraFreeMove : MonoBehaviour
{
    public Vector3 movePosMin = new Vector3(0, 0f, 0f);
    public Vector3 movePosMax = new Vector3(50f, 30f, 50f);



    private int MouseWheelSensitivity = 5; //滚轮灵敏度设置
    private int MouseZoomMin = -20; //相机距离最小值
    private int MouseZoomMax = 50; //相机距离最大值

    private float moveSpeed = 10; //相机跟随速度（中键平移时），采用平滑模式时起作用，越大则运动越平滑

    private float xSpeed = 250.0f; //旋转视角时相机x轴转速
    private float ySpeed = 120.0f; //旋转视角时相机y轴转速

    private int yMinLimit = -360;
    private int yMaxLimit = 360;

    private float x = 0.0f; //存储相机的euler角
    private float y = 0.0f; //存储相机的euler角

    private float Distance = 0; //相机和target之间的距离，因为相机的Z轴总是指向target，也就是相机z轴方向上的距离

    private Vector3 targetOnScreenPosition; //目标的屏幕坐标，第三个值为z轴距离
    private Quaternion storeRotation; //存储相机的姿态四元数
    private Vector3 initPosition; //平移时用于存储平移的起点位置
    private Vector3 cameraX; //相机的x轴方向向量
    private Vector3 cameraY; //相机的y轴方向向量
    private Vector3 cameraZ; //相机的z轴方向向量

    private Vector3 initScreenPos; //中键刚按下时鼠标的屏幕坐标（第三个值其实没什么用）
    private Vector3 curScreenPos; //当前鼠标的屏幕坐标（第三个值其实没什么用）
    void Start()
    {
        //这里就是设置一下初始的相机视角以及一些其他变量，这里的x和y。。。是和下面getAxis的mouse x与mouse y对应
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        storeRotation = Quaternion.Euler(y, x, 0);
        // Debug.Log("Camera x: "+transform.right);
        // Debug.Log("Camera y: "+transform.up);
        // Debug.Log("Camera z: "+transform.forward);

        // //-------------TEST-----------------
        // testScreenToWorldPoint();
    }

    /// <summary>
    /// 更新相机信息 (相机非鼠标移动需要更新相机缓存信息)
    /// </summary>
    public void UpdateCameraInfo()
    {
        storeRotation = transform.rotation;
        x = storeRotation.eulerAngles.y;
        y = storeRotation.eulerAngles.x;
    }

    void Update()
    {
        //鼠标右键旋转功能
        if (Input.GetMouseButton(1))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            storeRotation = Quaternion.Euler(y, x, 0);
            var position = storeRotation * new Vector3(0.0f, 0.0f, -Distance) + transform.position;

            transform.rotation = storeRotation;
            //transform.position = position;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") != 0) //鼠标滚轮缩放功能
        {
            if (Distance >= MouseZoomMin && Distance <= MouseZoomMax)
            {
                Distance = Input.GetAxis("Mouse ScrollWheel") * MouseWheelSensitivity;
                //Debug.Log("Distance " + Distance);
            }
            //if (Distance < MouseZoomMin)
            //{
            //    Distance = MouseZoomMin;
            //}
            //if (Distance > MouseZoomMax)
            //{
            //    Distance = MouseZoomMax;
            //}

            if (JudegTargetPos(storeRotation * new Vector3(0.0F, 0.0F, Distance) + transform.position))
            {
                transform.position = storeRotation * new Vector3(0.0F, 0.0F, Distance) + transform.position;
            }
        }

        //鼠标中键平移
        if (Input.GetMouseButtonDown(2))
        {
            cameraX = transform.right;
            cameraY = transform.up;
            cameraZ = transform.forward;

            initScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, targetOnScreenPosition.z);
            //Debugger.Log("鼠标中间按下：" + initScreenPos);

            //targetOnScreenPosition.z为目标物体到相机xmidbuttonDownPositiony平面的法线距离
            targetOnScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
            initPosition = transform.position;
        }

        if (Input.GetMouseButton(2))
        {
            curScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, targetOnScreenPosition.z);
            //Debugger.Log("鼠标中间持续按下，curScreenPos：" + curScreenPos + "，initScreenPos：" + initScreenPos);
            if (JudegTargetPos(initPosition - 0.01f * ((curScreenPos.x - initScreenPos.x) * cameraX + (curScreenPos.y - initScreenPos.y) * cameraY)))
            {
                //0.01这个系数是控制平移的速度，要根据相机和目标物体的distance来灵活选择
                transform.position = initPosition - 0.01f * ((curScreenPos.x - initScreenPos.x) * cameraX + (curScreenPos.y - initScreenPos.y) * cameraY);
            }



            ////重新计算位置
            //Vector3 mPosition = storeRotation * new Vector3(0.0F, 0.0F, -Distance) + transform.position;
            //transform.position = mPosition;

            // //用这个会让相机的平移变得更平滑，但是可能在你buttonup时未使相机移动到应到的位置，导致再进行旋转与缩放操作时出现短暂抖动
            //transform.position=Vector3.Lerp(transform.position,mPosition,Time.deltaTime*moveSpeed);

        }
        if (Input.GetMouseButtonUp(2))
        {
            Debugger.Log("upOnce");
        }

        InputKey();
    }

    private int m_KeyACount = 0;
    private int m_KeyDCount = 0;
    /// <summary>
    /// 键盘横向移动数据
    /// </summary>
    private float m_MoveSpeedInputAD = 20f;
    /// <summary>
    /// 键盘WASD移动相机
    /// </summary>
    private void InputKey()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Distance = 0.5f;
            if (JudegTargetPos(storeRotation * new Vector3(0.0F, 0.0F, Distance) + transform.position))
            {
                transform.position = storeRotation * new Vector3(0.0F, 0.0F, Distance) + transform.position;
            }
        }
        if (Input.GetKey(KeyCode.S))
        {
            Distance = -0.5f;
            if (JudegTargetPos(storeRotation * new Vector3(0.0F, 0.0F, Distance) + transform.position))
            {
                transform.position = storeRotation * new Vector3(0.0F, 0.0F, Distance) + transform.position;
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            cameraX = transform.right;
            cameraY = transform.up;
            cameraZ = transform.forward;

            initScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, targetOnScreenPosition.z);
            //Debugger.Log("键盘A键按下：" + initScreenPos);

            //targetOnScreenPosition.z为目标物体到相机xmidbuttonDownPositiony平面的法线距离
            targetOnScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
            initPosition = transform.position;

            m_KeyACount = 0;
        }
        if (Input.GetKey(KeyCode.A))
        {
            m_KeyACount++;
            //curScreenPos = new Vector3(Input.mousePosition.x - (m_KeyACount * 10f), Input.mousePosition.y, targetOnScreenPosition.z);
            curScreenPos = new Vector3(initScreenPos.x + (m_KeyACount * m_MoveSpeedInputAD), initScreenPos.y, initScreenPos.z);
            //Debugger.Log("键盘A键持续按下，curScreenPos：" + curScreenPos + "，initScreenPos：" + initScreenPos);
            if (JudegTargetPos(initPosition - 0.01f * ((curScreenPos.x - initScreenPos.x) * cameraX + (curScreenPos.y - initScreenPos.y) * cameraY)))
            {
                //0.01这个系数是控制平移的速度，要根据相机和目标物体的distance来灵活选择
                transform.position = initPosition - 0.01f * ((curScreenPos.x - initScreenPos.x) * cameraX + (curScreenPos.y - initScreenPos.y) * cameraY);
            }
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            cameraX = transform.right;
            cameraY = transform.up;
            cameraZ = transform.forward;

            initScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, targetOnScreenPosition.z);
            //Debugger.Log("键盘A键按下：" + initScreenPos);

            //targetOnScreenPosition.z为目标物体到相机xmidbuttonDownPositiony平面的法线距离
            targetOnScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
            initPosition = transform.position;

            m_KeyDCount = 0;
        }
        if (Input.GetKey(KeyCode.D))
        {
            m_KeyDCount++;
            //curScreenPos = new Vector3(Input.mousePosition.x - (m_KeyDCount * 10f), Input.mousePosition.y, targetOnScreenPosition.z);
            curScreenPos = new Vector3(initScreenPos.x - (m_KeyDCount * m_MoveSpeedInputAD), initScreenPos.y, initScreenPos.z);
            //Debugger.Log("键盘A键持续按下，curScreenPos：" + curScreenPos + "，initScreenPos：" + initScreenPos);
            if (JudegTargetPos(initPosition - 0.01f * ((curScreenPos.x - initScreenPos.x) * cameraX + (curScreenPos.y - initScreenPos.y) * cameraY)))
            {
                //0.01这个系数是控制平移的速度，要根据相机和目标物体的distance来灵活选择
                transform.position = initPosition - 0.01f * ((curScreenPos.x - initScreenPos.x) * cameraX + (curScreenPos.y - initScreenPos.y) * cameraY);
            }
        }
    }

   


    //将angle限制在min~max之间
    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    /// <summary>
    /// 判断摄像机目标位置是否可用
    /// </summary>
    private bool JudegTargetPos(Vector3 pos)
    {
        if (pos.x > movePosMin.x && pos.x < movePosMax.x
            && pos.y > movePosMin.y && pos.y < movePosMax.y
            && pos.z > movePosMin.z && pos.z < movePosMax.z)
        {
            return true;
        }
        return false;
    }


    void testScreenToWorldPoint()
    {
        //第三个坐标指的是在相机z轴指向方向上的距离
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        Debug.Log("ScreenPoint: " + screenPoint);

        // var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(1,1,10));
        // Debug.Log("worldPosition: "+worldPosition);
    }
}