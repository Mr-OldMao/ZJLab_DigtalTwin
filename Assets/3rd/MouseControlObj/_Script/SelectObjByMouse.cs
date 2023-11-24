using MouseControlObj;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 标题：使用鼠标选择可操作（平移、旋转、缩放）的物体
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2023.11.24
    /// </summary>
    public class SelectObjByMouse : SingletonByMono<SelectObjByMouse>
    {
        /// <summary>
        /// 控制脚本 用于控制游戏对象移动旋转缩放
        /// </summary>
        private BaseMouseControlObj m_BaseMouseControlObj;
        /// <summary>
        /// 选择成功后的物体材质
        /// </summary>
        private Material m_MatSelectObj;

        private bool m_Canselect;
        /// <summary>
        /// 是否允许选择对象
        /// </summary>
        public bool CanSelect
        {
            get
            {
                return m_Canselect;
            }
            set
            {
                if (value == false)
                {
                    HideAllAxisModel();
                }
                CurSelectObjInfo = null;
                m_Canselect = value;
            }
        }
        private SelectObjInfo m_CurSelectObjInfo;
        public SelectObjInfo CurSelectObjInfo
        {
            get
            {
                return m_CurSelectObjInfo;
            }
            private set
            {
                if (m_CurSelectObjInfo != value)
                {
                    m_CurSelectObjInfo?.HideScript();
                    m_CurSelectObjInfo?.ResetMat();

                    m_CurSelectObjInfo = value;
                    SetPos();
                }
            }
        }



        /// <summary>
        /// 选择对象数据信息
        /// </summary>
        public class SelectObjInfo
        {
            public GameObject entity;
            /// <summary>
            /// 添加所要挂载控制脚本BaseMouseControlObj的对象
            /// </summary>
            public BaseMouseControlObj addScriptTrans;


            /// <summary>
            /// 前者对象位置，用于位置初始化
            /// </summary>
            private Vector3 addScriptPos;
            private Vector3 addScriptRotate;
            private Vector3 addScriptScale;
            private Material entityMat;

            public SelectObjInfo(GameObject entity, BaseMouseControlObj addScriptTrans)
            {
                this.entity = entity;
                this.addScriptTrans = addScriptTrans;
                addScriptPos = addScriptTrans.transform.position;
                addScriptRotate = addScriptTrans.transform.rotation.eulerAngles;
                addScriptScale = addScriptTrans.transform.localScale;
                entityMat = entity.GetComponent<MeshRenderer>().material;
            }

            public void ResetMat()
            {
                //还原原有实体的材质，还原entity同级下的所有实体
                if (entity != null)
                {
                    foreach (MeshRenderer meshRenderer in entity.transform.parent.GetComponentsInChildren<MeshRenderer>())
                    {
                        meshRenderer.material = entityMat;
                    }
                }
            }

            public void HideScript()
            {
                //隐藏原有的实体对象BaseMouseControlObj脚本
                if (addScriptTrans != null)
                {
                    addScriptTrans.enabled = false;
                }
            }

            public void ResetPos()
            {
                addScriptTrans.transform.position = addScriptPos;
                addScriptTrans.transform.rotation = Quaternion.Euler(addScriptRotate);
                addScriptTrans.transform.localScale = addScriptScale;
            }
        }


        public Action callbackMoveStart;
        public Action callbackMoveEnd;
        public Action callbackRotateStart;
        public Action callbackRotateEnd;
        public Action callbackScaleStart;
        public Action callbackScaleEnd;

        private void Awake()
        {
            //();
        }

        public void Init()
        {
            CanSelect = false;
            Material matTansparent = Resources.Load<Material>("MatTansparent");
            m_MatSelectObj = matTansparent;
        }


        private void Start()
        {
            RegisterAction();
        }
        /// <summary>
        /// 切换坐标轴类型并显示坐标轴模型
        /// </summary>
        public void SetPos()
        {
            if (CanSelect)
            {
                m_CurSelectObjInfo?.addScriptTrans?.GetComponent<BaseMouseControlObj>().SetPos();

            }
        }
        /// <summary>
        /// 切换坐标轴类型并显示坐标轴模型
        /// </summary>
        public void SetRot()
        {
            if (CanSelect)
            {
                m_CurSelectObjInfo?.addScriptTrans?.GetComponent<BaseMouseControlObj>().SetRot();

            }
        }
        /// <summary>
        /// 切换坐标轴类型并显示坐标轴模型
        /// </summary>
        public void SetScale()
        {
            if (CanSelect)
            {
                m_CurSelectObjInfo?.addScriptTrans?.GetComponent<BaseMouseControlObj>().SetScale();

            }
        }
        /// <summary>
        /// 重置实体模型位置
        /// </summary>
        public void ResetPos()
        {
            if (m_CurSelectObjInfo != null)
            {
                m_CurSelectObjInfo.ResetPos();
            }
        }

        /// <summary>
        /// 隐藏所有（平移、旋转、缩放）坐标轴模型
        /// </summary>
        private void HideAllAxisModel()
        {
            BaseMouseControlObj baseMouseControlObj = CurSelectObjInfo?.addScriptTrans?.GetComponent<BaseMouseControlObj>();
            if (baseMouseControlObj != null)
            {
                baseMouseControlObj.ShowAxis(AxisModel.PosAxis, false);
                baseMouseControlObj.ShowAxis(AxisModel.RotAxis, false);
                baseMouseControlObj.ShowAxis(AxisModel.ScaleAxis, false);

            }
        }

        private void RegisterAction()
        {
            callbackMoveStart += () =>
            {
                //Debugger.Log("callbackMoveStart");
            };
            callbackMoveEnd += () =>
            {
                //Debugger.Log("callbackMoveEnd");
            };
            callbackRotateStart += () =>
            {
                //Debugger.Log("callbackRotateStart");
            };
            callbackRotateEnd += () =>
            {
                //Debugger.Log("callbackRotateEnd");
            };
            callbackScaleStart += () =>
            {
                //Debugger.Log("callbackScaleStart");

            };
            callbackScaleEnd += () =>
            {
                //Debugger.Log("callbackScaleEnd");
            };
        }

        private void Update()
        {
            SelectObj();
        }

        private void SelectObj()
        {
            if (CanSelect)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Debugger.Log("尝试选择对象");
                    Ray rayMouseDown = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(rayMouseDown, out RaycastHit hitInfoMouseDown, 1000))
                    {
                        //Debugger.Log(hitInfoMouseDown.collider.name);
                        if (hitInfoMouseDown.collider.gameObject.layer == LayerMask.NameToLayer("IndoorItem"))
                        {
                            GameObject entity = hitInfoMouseDown.collider.gameObject;
                            //判断选择的对象与之前相同
                            if (CurSelectObjInfo?.addScriptTrans?.gameObject == entity.transform.parent.parent.gameObject)
                            {
                                //Debugger.Log("选择的对象与之前相同 entity:" + entity);
                                return;
                            }

                            Transform addScriptTrans = entity.transform.parent.parent;
                            if (addScriptTrans.TryGetComponent<BaseMouseControlObj>(out BaseMouseControlObj baseMouseControlObj))
                            {
                                baseMouseControlObj.enabled = true;
                            }
                            else
                            {
                                baseMouseControlObj = addScriptTrans.AddComponent<BaseMouseControlObj>();
                            }
                            CurSelectObjInfo = new SelectObjInfo(entity, baseMouseControlObj);
                            //材质替换 替换entity同级下的所有实体
                            foreach (MeshRenderer meshRenderer in entity.transform.parent.GetComponentsInChildren<MeshRenderer>())
                            {
                                meshRenderer.material = m_MatSelectObj;
                            }
                            Debugger.Log("已选中实体：" + entity + "，parent：" + addScriptTrans);
                        }
                    }
                }
            }
        }
    }
}
