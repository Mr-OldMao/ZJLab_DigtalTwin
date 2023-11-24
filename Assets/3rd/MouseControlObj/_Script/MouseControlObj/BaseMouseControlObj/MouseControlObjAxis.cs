using MFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MouseControlObj
{
    /// <summary>
    /// 控制的轴
    /// </summary>
    public enum ControlAxis
    {
        None = 0,
        /// <summary>
        /// X轴
        /// </summary>
        X,
        /// <summary>
        /// Y轴
        /// </summary>
        Y,
        /// <summary>
        /// Z轴
        /// </summary>
        Z,
        /// <summary>
        /// XY平面
        /// </summary>
        XYPanel,
        /// <summary>
        /// XZ平面
        /// </summary>
        XZPanel,
        /// <summary>
        /// YZ平面
        /// </summary>
        YZPanel,
        /// <summary>
        /// 原点
        /// </summary>
        OriginPoint
    }
    /// <summary>
    /// 坐标系
    /// </summary>
    public class MouseControlObjAxis : MonoBehaviour
    {
        [SerializeField]
        private bool isGlobal;
        [SerializeField]
        /// <summary>
        /// 射线检测的时候的轴的层
        /// </summary>
        private int rayLayer = 12;
        private Transform originPoint;
        /// <summary>
        /// 原点
        /// </summary>
        private Transform OriginPoint
        {
            get
            {
                if (originPoint == null)
                {
                    originPoint = transform.FindChildForName("OriginPoint");
                }
                return originPoint;
            }
        }
        private Transform x;
        /// <summary>
        /// X
        /// </summary>
        private Transform X
        {
            get
            {
                if (x == null)
                {
                    x = transform.FindChildForName("X");
                }
                return x;
            }
        }
        private Transform y;
        /// <summary>
        /// Y
        /// </summary>
        private Transform Y
        {
            get
            {
                if (y == null)
                {
                    y = transform.FindChildForName("Y");
                }
                return y;
            }
        }
        private Transform z;
        /// <summary>
        /// Z
        /// </summary>
        private Transform Z
        {
            get
            {
                if (z == null)
                {
                    z = transform.FindChildForName("Z");
                }
                return z;
            }
        }
        /// <summary>
        /// 鼠标可控制物体
        /// </summary>
        private BaseMouseControlObj mouseControlObj;
        /// <summary>
        /// 当前点击的轴的正方向
        /// </summary>
        private Vector3 currentAxisScreenDir;
        /// <summary>
        /// 物体移动的方向
        /// </summary>
        private Vector3 objMoveDir;
        private ControlAxis currentAxis;
        /// <summary>
        /// 当前控制的坐标轴
        /// </summary>
        public ControlAxis CurrentAxis
        {
            get
            {
                return currentAxis;
            }
            set
            {
                if (mouseControlObj == null)
                {
                    return;
                }


                currentAxis = value;
            }
        }

        private List<Axis> axisList;

        private List<Axis> AxisList
        {
            get
            {
                if (axisList == null)
                {
                    axisList = new List<Axis>();
                    axisList.AddRange(GetComponentsInChildren<Axis>());
                }
                return axisList;
            }
        }
        void Start()
        {

        }
        /// <summary>
        /// 停止所有移动
        /// </summary>
        public void StopAllIEnumerator()
        {
            mouseControlObj.StopAllIEnumerator();
        }
        private void Update()
        {

            if (isGlobal)
            {
                transform.localRotation = new Quaternion(0, 0, 0, 0);
            }
            if (mouseControlObj == null)
            {
                gameObject.SetActive(false);
                return;
            }
            if (!isGlobal)
            {
                transform.localRotation = Quaternion.Euler(mouseControlObj.transform.localEulerAngles);
            }
            transform.position = mouseControlObj.transform.position;

            if (!Input.GetMouseButton(0))
            {
                Ray rayMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfoMouse;
                //如果鼠标所在位置在可放置区域，则物体在可放置区域表面
                if (Physics.Raycast(rayMouse, out hitInfoMouse, 1000, 1 << rayLayer))
                {
                    //缩放中心为鼠标所在位置的物体
                    Axis axis = hitInfoMouse.transform.GetComponent<Axis>();
                    Set_mouseInAxisMats(axis);

                }
                else
                {
                    Set_mouseInAxisMats(null);
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                Ray rayMouseDown = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfoMouseDown;
                //如果鼠标所在位置在可放置区域，则物体在可放置区域表面
                bool b = Physics.Raycast(rayMouseDown, out hitInfoMouseDown, 1000, 1 << rayLayer);
                if (b)
                {
                    Axis axis = hitInfoMouseDown.transform.GetComponent<Axis>();
                    if (axis != null)
                    {

                        Vector3 originScreenPoint = Camera.main.WorldToScreenPoint(OriginPoint.position);
                        if ((int)axis.Get_axis() <= 3)
                        {
                            Vector3 axisScreenPoint = Camera.main.WorldToScreenPoint(axis.transform.position);
                            currentAxisScreenDir = axisScreenPoint - originScreenPoint;//原点到轴的方向为点击的轴的正方向
                            objMoveDir = (axis.transform.position - OriginPoint.position).normalized;
                            CurrentAxis = axis.Get_axis();
                            mouseControlObj.Start_IMoveXYZ(currentAxisScreenDir, objMoveDir, isGlobal);

                        }
                        else
                        {
                            mouseControlObj.Start_IMovePanel(axis.Get_axis(), isGlobal);
                        }
                        Set_mouseDragAxisMats(axis);
                        SelectObjByMouse.GetInstance.callbackMoveStart?.Invoke();
                    }
                    return;
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                StopAllIEnumerator();
                SelectObjByMouse.GetInstance.callbackMoveEnd?.Invoke();
            }
        }
        /// <summary>
        /// 设置此坐标轴可操作的物体
        /// </summary>
        /// <param name="mouseControlObj"></param>
        public void Set_mouseControlObj(BaseMouseControlObj mouseControlObj)
        {
            this.mouseControlObj = mouseControlObj;
        }

        /// <summary>
        /// 设置材质为鼠标进入
        /// </summary>
        /// <param name="axis"></param>
        private void Set_mouseInAxisMats(Axis axis)
        {
            //Debug.Log("axis " + axis);
            for (int i = 0; i < AxisList.Count; i++)
            {
                int index = i;
                if (AxisList[index] != axis)
                {
                    AxisList[index].SetTo_originalMats();
                }
                else
                {
                    axis.SetTo_mouseInMats();
                }
            }
        }

        /// <summary>
        /// 设置材质为拖拽材质
        /// </summary>
        /// <param name="axis"></param>
        private void Set_mouseDragAxisMats(Axis axis)
        {

            for (int i = 0; i < AxisList.Count; i++)
            {
                int index = i;
                if (AxisList[index] != axis)
                {
                    AxisList[index].SetTo_unActiveMats();
                }
                else
                {
                    axis.SetTo_mouseDragMats();
                }
            }
        }


    }
}
