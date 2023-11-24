using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MouseControlObj
{
    /// <summary>
    /// 当前坐标模式
    /// </summary>
    public enum AxisModel
    {
        /// <summary>
        /// 位置轴
        /// </summary>
        PosAxis,
        /// <summary>
        /// 缩放轴
        /// </summary>
        ScaleAxis,
        /// <summary>
        /// 旋转轴
        /// </summary>
        RotAxis

    }
    /// <summary>
    /// 鼠标可控绘制物体基类
    /// </summary>
    public class BaseMouseControlObj : MFramework.SingletonByMono<BaseMouseControlObj>
    {


        /// <summary>
        /// 是否已经选中物体
        /// </summary>
        private bool isSelect;
        /// <summary>
        /// 初始的位置
        /// </summary>
        private Vector3 originalPos;
        /// <summary>
        /// 初始旋转
        /// </summary>
        private Vector3 originalRot;
        /// <summary>
        /// 初始缩放
        /// </summary>
        private Vector3 originalScale;

        private static BaseMouseControlObj current_BaseMouseControlObj;

        private static AxisModel currentAxisModel;
        /// <summary>
        /// 当前轴模式
        /// </summary>
        public static AxisModel CurrentAxisModel
        {
            get
            {
                return currentAxisModel;
            }
            set
            {
                currentAxisModel = value;
                if (current_BaseMouseControlObj != null)
                {
                    current_BaseMouseControlObj.SetAxisState(true);
                }

            }
        }
        private List<Coroutine> m_ListCoroutine = new List<Coroutine>();
        #region Change 20231123
        /// <summary>
        /// 是否显示控制面板
        /// </summary>
        private static bool m_IsShowControlPanel = false;
        [Tooltip("使用坐标轴移动物体的速度")]
        [SerializeField]
        [Range(0.01f, 1f)]
        private float m_MoveSpeedAxis = 0.1f;
        [Tooltip("使用平面移动物体的速度")]
        [SerializeField]
        [Range(0.01f, 1f)]
        private float m_MoveSpeedPlane = 0.5f;
        [Tooltip("旋转物体的速度")]
        [SerializeField]
        [Range(1f, 1000f)]
        private float m_RotateSpeed = 200f;
        #endregion


        private static MouseControlObjAxis objAxis;
        /// <summary>
        /// 坐标系
        /// </summary>
        protected MouseControlObjAxis ObjAxis
        {
            get
            {
                if (objAxis == null)
                {
                    objAxis = GameObject.FindObjectOfType<MouseControlObjAxis>();
                    if (objAxis == null)
                    {
                        objAxis = Instantiate(Resources.Load<MouseControlObjAxis>("AxisModel"));
                    }
                }

                objAxis.transform.position = transform.position;
                return objAxis;
            }
        }

        private static MouseControlObjScaleAxis objScaleAxis;
        /// <summary>
        /// 缩放坐标系
        /// </summary>
        private MouseControlObjScaleAxis ObjScaleAxis
        {
            get
            {
                if (objScaleAxis == null)
                {
                    objScaleAxis = GameObject.FindObjectOfType<MouseControlObjScaleAxis>();
                    if (objScaleAxis == null)
                    {
                        objScaleAxis = Instantiate(Resources.Load<MouseControlObjScaleAxis>("ScaleAxisModel"));
                    }
                }
                return objScaleAxis;
            }
        }


        private static MouseControlObjRotAxis objRotAxis;
        /// <summary>
        /// 旋转坐标系
        /// </summary>
        private MouseControlObjRotAxis ObjRotAxis
        {
            get
            {
                if (objRotAxis == null)
                {
                    objRotAxis = GameObject.FindObjectOfType<MouseControlObjRotAxis>();
                    if (objRotAxis == null)
                    {
                        objRotAxis = Instantiate(Resources.Load<MouseControlObjRotAxis>("RotAxisModel"));
                    }
                }
                return objRotAxis;
            }
        }
        private Transform mouseControlUIPos;
        /// <summary>
        /// 操作位置信息的世界UI的位置
        /// </summary>
        protected Transform MouseControlUIPos
        {
            get
            {
                if (mouseControlUIPos == null)
                {
                    mouseControlUIPos = transform.FindChildForName("MouseControlUIPos");
                    if (mouseControlUIPos == null)
                    {
                        GameObject go = new GameObject();
                        go.transform.parent = transform;

                        mouseControlUIPos = go.transform;
                        mouseControlUIPos.localPosition = transform.localPosition + Vector3.one * 3;

                    }
                }
                return mouseControlUIPos;
            }
        }

        private static MouseControlUI _mouseControlUI;
        /// <summary>
        /// UI控制物体的transform
        /// </summary>
        public MouseControlUI mouseControlUI
        {
            get
            {
                if (_mouseControlUI == null)
                {
                    _mouseControlUI = GameObject.FindObjectOfType<MouseControlUI>();
                    if (_mouseControlUI == null)
                    {
                        _mouseControlUI = Instantiate(Resources.Load<MouseControlUI>("MouseControlUI"));
                        _mouseControlUI.gameObject.SetActive(m_IsShowControlPanel);
                    }
                    _mouseControlUI.On_CloseButton_ClickAction += delegate { ObjAxis.gameObject.SetActive(false); ObjRotAxis.gameObject.SetActive(false); ObjScaleAxis.gameObject.SetActive(false); };
                }

                _mouseControlUI.transform.position = MouseControlUIPos.position;
                return _mouseControlUI;
            }
        }
        /// <summary>
        /// 拖动的时候的回调
        /// </summary>
        public Action<Vector3, Vector3, float> dragingAction;
        // Start is called before the first frame update
        void Start()
        {

            //ObjAxis.gameObject.SetActive(false);
            //mouseControlUI.gameObject.SetActive(false);
            // mouseControlUI.Init(transform);
        }
        /// <summary>
        /// 设置操作世界UI的状态
        /// </summary>
        /// <param name="state"></param>
        public virtual void Set_mouseControlUIState(bool state, List<string> stringList, string currentModelName)
        {
            if (m_IsShowControlPanel)
            {
                mouseControlUI.gameObject.SetActive(state);
            }
            ObjAxis.gameObject.SetActive(state);
            if (state)
            {
                mouseControlUI.Init(transform, currentModelName);
                ObjAxis.Set_mouseControlObj(this);
            }
        }
        /// <summary>
        /// 设置操作世界UI的状态
        /// </summary>
        /// <param name="state"></param>
        public virtual MouseControlUI Set_mouseControlUIState(bool state, string currentModelName = null)
        {
            if (m_IsShowControlPanel)
            {
                mouseControlUI.gameObject.SetActive(state);

            }
            if (state)
            {
                mouseControlUI.Init(transform, currentModelName);
            }
            return mouseControlUI;
        }
        /// <summary>
        /// 设置轴操作轴的状态
        /// </summary>
        /// <param name="state"></param>
        public virtual MouseControlObjAxis Set_ObjAxisState(bool state)
        {
            ObjAxis.gameObject.SetActive(state);
            if (state)
            {
                ObjScaleAxis.gameObject.SetActive(false);
                ObjRotAxis.gameObject.SetActive(false);
            }
            return ObjAxis;
        }
        /// <summary>
        /// 设置缩放轴的状态
        /// </summary>
        /// <param name="state"></param>
        public virtual MouseControlObjScaleAxis Set_ObjScaleAxisState(bool state)
        {
            ObjScaleAxis.gameObject.SetActive(state);
            if (state)
            {
                ObjAxis.gameObject.SetActive(false);
                ObjRotAxis.gameObject.SetActive(false);
            }
            return ObjScaleAxis;
        }

        /// <summary>
        /// 设置旋转轴的状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public virtual MouseControlObjRotAxis Set_ObjRotAxisState(bool state)
        {
            ObjRotAxis.gameObject.SetActive(state);
            if (state)
            {
                ObjAxis.gameObject.SetActive(false);
                ObjScaleAxis.gameObject.SetActive(false);
            }
            if (ObjRotAxis.Get_isGlobal())
            {
                ObjRotAxis.transform.localRotation = new Quaternion(0, 0, 0, 0);
            }
            else
            {
                ObjRotAxis.transform.localRotation = transform.localRotation;
            }
            return ObjRotAxis;
        }

        /// <summary>
        /// 鼠标点击之后
        /// </summary>
        private void OnMouseDown()
        {

        }
        /// <summary>
        /// 停止所有移动
        /// </summary>
        public void StopAllIEnumerator()
        {
            //StopAllCoroutines();
            #region Change 20231123
            foreach (var item in m_ListCoroutine)
            {
                StopCoroutine(item);
            }
            m_ListCoroutine.Clear();
            #endregion
        }
        /// <summary>
        /// 开始在某个轴方向上移动
        /// </summary>
        public void Start_IMoveXYZ(Vector3 currentAxisScreenDir, Vector3 objMoveDir, bool isGlobal)
        {
            m_ListCoroutine.Add(StartCoroutine(IMoveXYZ(currentAxisScreenDir, objMoveDir, isGlobal)));
        }
        /// <summary>
        /// 开始在某个平面上移动
        /// </summary>
        /// <param name="controlAxis"></param>
        /// <param name="isGlobal"></param>
        public void Start_IMovePanel(ControlAxis controlAxis, bool isGlobal)
        {
            m_ListCoroutine.Add(StartCoroutine(IMovePanel(controlAxis, isGlobal)));
        }
        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="currentAxisScreenDir"></param>
        public void Start_IScaleXYZ(Vector3 currentAxisScreenDir, Vector3 scaleChangeAxis, Transform X, Transform Y, Transform Z)
        {
            m_ListCoroutine.Add(StartCoroutine(IScaleXYZ(currentAxisScreenDir, scaleChangeAxis, X, Y, Z)));
        }
        /// <summary>
        /// 旋转
        /// </summary>
        /// <param name="firstPointDir"></param>
        public void Start_IRotXYZ(Vector3 firstPointDir, Axis axis, Transform controlObjRotAxis)
        {
            m_ListCoroutine.Add(StartCoroutine(IRotXYZ(firstPointDir, axis, controlObjRotAxis)));
        }
        private IEnumerator IMoveXYZ(Vector3 currentAxisScreenDir, Vector3 objMoveDir, bool isGlobal)
        {
            while (true)
            {
                Vector3 firstMousescreenPos1 = Input.mousePosition;
                yield return 0;
                Vector3 firstMousescreenPos2 = Input.mousePosition;
                float moveDis = (Vector3.Dot(currentAxisScreenDir, (firstMousescreenPos2 - firstMousescreenPos1)));
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, transform.localPosition + objMoveDir * moveDis, m_MoveSpeedAxis);
                if (dragingAction != null)
                {
                    dragingAction(transform.localPosition, transform.localRotation.eulerAngles, transform.lossyScale.x);
                }
            }

        }
        /// <summary>
        /// 在某个平面上移动
        /// </summary>
        /// <param name="controlAxis"></param>
        /// <param name="currentAxisScreenDir"></param>
        /// <param name="objMoveDir"></param>
        /// <param name="isGlobal"></param>
        /// <returns></returns>
        private IEnumerator IMovePanel(ControlAxis controlAxis, bool isGlobal)
        {
            Vector3 objScreenPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 firstMouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, objScreenPos.z));
            Vector3 distance = transform.position - firstMouseWorldPos;

            while (true)
            {

                yield return 0;
                if (Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0)
                {
                    continue;
                }
                Vector3 curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, objScreenPos.z);
                Vector3 CurPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + distance;
                Vector3 dir;
                switch (controlAxis)
                {

                    case ControlAxis.XYPanel:
                        if (isGlobal)
                        {
                            dir = Vector3.ProjectOnPlane((CurPosition - transform.position), Vector3.forward);
                        }
                        else
                        {
                            dir = Vector3.ProjectOnPlane((CurPosition - transform.position), transform.forward);
                        }
                        transform.localPosition = Vector3.MoveTowards(transform.localPosition, transform.localPosition + dir, m_MoveSpeedPlane);
                        break;
                    case ControlAxis.XZPanel:
                        if (isGlobal)
                        {
                            dir = Vector3.ProjectOnPlane((CurPosition - transform.position), Vector3.up);
                        }
                        else
                        {
                            dir = Vector3.ProjectOnPlane((CurPosition - transform.position), transform.up);
                        }
                        transform.localPosition = Vector3.MoveTowards(transform.localPosition, transform.localPosition + dir, m_MoveSpeedPlane);
                        break;
                    case ControlAxis.YZPanel:
                        if (isGlobal)
                        {
                            dir = Vector3.ProjectOnPlane((CurPosition - transform.position), Vector3.right);
                        }
                        else
                        {
                            dir = Vector3.ProjectOnPlane((CurPosition - transform.position), transform.right);
                        }

                        transform.localPosition = Vector3.MoveTowards(transform.localPosition, transform.localPosition + dir, m_MoveSpeedPlane);
                        break;
                    default:
                        break;
                }
                if (dragingAction != null)
                {
                    dragingAction(transform.localPosition, transform.localRotation.eulerAngles, transform.lossyScale.x);
                }
            }
        }
        /// <summary>
        /// 缩放协程
        /// </summary>
        /// <param name="scaleChangeDir"></param>
        /// <param name="scaleChangeAxis"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <returns></returns>
        private IEnumerator IScaleXYZ(Vector3 scaleChangeDir, Vector3 scaleChangeAxis, Transform X, Transform Y, Transform Z)
        {
            Vector3 scale = transform.localScale;
            while (true)
            {
                Vector3 firstMousescreenPos1 = Input.mousePosition;
                yield return 0;
                Vector3 firstMousescreenPos2 = Input.mousePosition;
                float moveDis = 0;
                if (X == null || Y == null || Z == null)
                {
                    moveDis = (Vector3.Dot(scaleChangeDir, (firstMousescreenPos2 - firstMousescreenPos1))) * Time.deltaTime;

                }
                else
                {
                    float x = Input.GetAxis("Mouse X");
                    float y = Input.GetAxis("Mouse Y");
                    float f = Mathf.Abs(x) > Mathf.Abs(y) ? x : y;
                    moveDis = f;
                }
                transform.localScale += scaleChangeAxis * moveDis;// Vector3.MoveTowards(transform.localPosition, transform.localPosition + objMoveDir * moveDis, 0.5f); 
                float multiple = 1;
                if (moveDis == 0)
                {
                    continue;
                }
                if (X != null)
                {
                    multiple = transform.localScale.x / Mathf.Abs(scale.x);
                    X.localScale = new Vector3(multiple, 1, 1);
                }
                if (Y != null)
                {
                    multiple = transform.localScale.y / Mathf.Abs(scale.y);
                    Y.localScale = new Vector3(1, 1, multiple);
                }
                if (Z != null)
                {
                    multiple = transform.localScale.z / Mathf.Abs(scale.z);
                    Z.localScale = new Vector3(1, 1, multiple);
                }
            }

        }
        /// <summary>
        /// 旋转协程
        /// </summary>
        /// <param name="firstPointDir"></param>
        /// <param name="axis"></param>
        /// <param name="controlObjRotAxis"></param>
        /// <returns></returns>
        private IEnumerator IRotXYZ(Vector3 firstPointDir, Axis axis, Transform controlObjRotAxis)
        {
            float screenPoint_Z = Camera.main.WorldToScreenPoint(axis.transform.position).z;
            Space space = controlObjRotAxis.GetComponent<MouseControlObjRotAxis>().Get_isGlobal() ? Space.World : Space.Self;
            while (true)
            {
                float x = Input.GetAxis("Mouse X");
                float y = Input.GetAxis("Mouse Y");

                yield return 0;
                if (x == y && y == 0)
                {
                    continue;
                }
                float f = Mathf.Abs(x) >= Mathf.Abs(y) ? x : y;

                Vector3 dir = Vector3.zero;

                float camToObjAngle = 0;
                switch (axis.Get_axis())
                {
                    case ControlAxis.X:
                        camToObjAngle = Vector3.Angle(transform.right, Camera.main.transform.position - transform.position);
                        dir = Vector3.right;
                        break;
                    case ControlAxis.Y:
                        camToObjAngle = Vector3.Angle(transform.up, Camera.main.transform.position - transform.position);
                        dir = -Vector3.up;
                        break;
                    case ControlAxis.Z:
                        camToObjAngle = Vector3.Angle(transform.forward, Camera.main.transform.position - transform.position);
                        dir = Vector3.forward;
                        break;
                }

                if (camToObjAngle > 90)
                {
                    dir *= -1;
                }
                transform.Rotate(dir, f * m_RotateSpeed * Time.deltaTime, space);
                controlObjRotAxis.transform.Rotate(dir, f * m_RotateSpeed * Time.deltaTime, space);
                if (dragingAction != null)
                {
                    dragingAction(transform.localPosition, transform.localRotation.eulerAngles, transform.lossyScale.x);
                }
            }
        }
        /// <summary>
        /// 设置坐标轴状态
        /// </summary>
        /// <param name="axisState"></param>
        public void SetAxisState(bool axisState, BaseMouseControlObj baseMouseControlObj = null)
        {
            if (baseMouseControlObj == null)
            {
                baseMouseControlObj = this;
            }
            if (axisState)
            {
                current_BaseMouseControlObj = this;
                switch (CurrentAxisModel)
                {
                    case AxisModel.PosAxis:

                        Set_ObjAxisState(true).Set_mouseControlObj(baseMouseControlObj);
                        Set_mouseControlUIState(true);
                        break;
                    case AxisModel.ScaleAxis:
                        Set_ObjScaleAxisState(true).Set_mouseControlObj(baseMouseControlObj);
                        Set_mouseControlUIState(true);
                        break;
                    case AxisModel.RotAxis:
                        Set_ObjRotAxisState(true).Set_mouseControlObj(baseMouseControlObj);
                        Set_mouseControlUIState(true);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                current_BaseMouseControlObj = null;
                switch (CurrentAxisModel)
                {
                    case AxisModel.PosAxis:
                        Set_ObjAxisState(false).Set_mouseControlObj(null);
                        Set_mouseControlUIState(false);
                        break;
                    case AxisModel.ScaleAxis:
                        Set_ObjScaleAxisState(false).Set_mouseControlObj(null);
                        Set_mouseControlUIState(false);
                        break;
                    case AxisModel.RotAxis:
                        Set_ObjRotAxisState(false).Set_mouseControlObj(null);
                        Set_mouseControlUIState(false);
                        break;
                    default:
                        break;
                }
            }
        }

        private void OnGUI()
        {
            #region Change 20231124
#if UNITY_EDITOR
            if (GUI.Button(new Rect(200, 100, 100, 100), "SetPos"))
            {
                current_BaseMouseControlObj = this;
                CurrentAxisModel = AxisModel.PosAxis;
            }
            if (GUI.Button(new Rect(200, 200, 100, 100), "SetRot"))
            {
                current_BaseMouseControlObj = this;
                CurrentAxisModel = AxisModel.RotAxis;
            }
            if (GUI.Button(new Rect(200, 300, 100, 100), "SetScale"))
            {
                current_BaseMouseControlObj = this;
                CurrentAxisModel = AxisModel.ScaleAxis;
            }  
#endif
            #endregion
        }

        #region Change 20231124
        /// <summary>
        /// 切换坐标轴类型并显示坐标轴模型
        /// </summary>
        public void SetPos()
        {
            current_BaseMouseControlObj = this;
            CurrentAxisModel = AxisModel.PosAxis;
        }
        /// <summary>
        /// 切换坐标轴类型并显示坐标轴模型
        /// </summary>
        public void SetRot()
        {
            current_BaseMouseControlObj = this;
            CurrentAxisModel = AxisModel.RotAxis;
        }
        /// <summary>
        /// 切换坐标轴类型并显示坐标轴模型
        /// </summary>
        public void SetScale()
        {
            current_BaseMouseControlObj = this;
            CurrentAxisModel = AxisModel.ScaleAxis;
        }
        /// <summary>
        /// 使用键盘切换操作类型
        /// </summary>
        private void ControlByInputKeys()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SetPos();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                SetRot();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                SetScale();
            }
        }
        private void Update()
        {
            ControlByInputKeys();
        }

        /// <summary>
        /// 显示坐标轴模型
        /// </summary>
        /// <param name="axisModel"></param>
        /// <param name="isShow"></param>
        public void ShowAxis(AxisModel axisModel, bool isShow)
        {
            switch (axisModel)
            {
                case AxisModel.PosAxis:
                    objAxis?.gameObject.SetActive(isShow);
                    break;
                case AxisModel.ScaleAxis:
                    ObjScaleAxis?.gameObject.SetActive(isShow);
                    break;
                case AxisModel.RotAxis:
                    ObjRotAxis?.gameObject.SetActive(isShow);
                    break;
            }
        }
        #endregion
    }
}
