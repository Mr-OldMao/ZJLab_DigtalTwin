
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework
{
    /// <summary>
    /// 标题：UI管理器
    /// 功能：UI资源的加载、卸载、面板层级配置、屏幕适配
    /// 作者：毛俊峰
    /// 时间：2022.04.20
    /// 版本：1.0
    /// </summary>
    public class UIManager : SingletonByMono<UIManager>
    {
        /// <summary>
        /// 缓存加载后的面板  key-面板名称（唯一标识） value-UI窗体数据
        /// </summary>
        private Dictionary<string, UIFormInfo> m_DicUIPanelInfoContainer = new Dictionary<string, UIFormInfo>();


        class UIFormInfo
        {
            /// <summary>
            /// UI窗体实体
            /// </summary>
            public GameObject UIFormEntity;
            /// <summary>
            /// UI窗体所挂载的逻辑脚本
            /// </summary>
            public UIFormBase UIFormLogicScript;
            public UIFormInfo(GameObject UIForm, UIFormBase UIFormLogicScript)
            {
                this.UIFormEntity = UIForm;
                this.UIFormLogicScript = UIFormLogicScript;
            }
        }

        private UIFormConfig m_UIFormConfig;
        private static GameObject m_UIRoot;
        public static GameObject UIRoot
        {
            get
            {
                if (!m_UIRoot)
                {
                    if (GameObject.Find("UIRoot"))
                    {
                        m_UIRoot = GameObject.Find("UIRoot");
                    }
                    else
                    {
                        m_UIRoot = Instantiate(Resources.Load<GameObject>("UI/Canvas/UIRoot"));
                    }
                }
                return m_UIRoot;
            }
        }

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            m_UIFormConfig = new UIFormConfig();
        }

        /// <summary>
        /// 显示UI窗体
        /// </summary>
        /// <typeparam name="T">UIFormBase的派生类</typeparam>
        /// <returns></returns>
        public T Show<T>() where T : UIFormBase
        {
            UIFormConfig.UIEntityConfigInfo uiEntityConfigInfo = m_UIFormConfig.GetUIFormConfigData<T>();
            if (uiEntityConfigInfo != null)
            {
                return Show<T>(uiEntityConfigInfo.uiFormEntityName, uiEntityConfigInfo.uiLayerType);

            }
            else
            {
                Debugger.LogError("UIForm Show Fail ，uiFormEntity is Null， Type：" + typeof(T).Name);
            }
            return default;
        }

        /// <summary>
        /// 显示UI窗体
        /// </summary>
        /// <typeparam name="T">UIFormBase的派生类</typeparam>
        /// <param name="uiFormName">UI窗体路径："Assets/xxx/xx.prefab"</param>
        /// <returns></returns>
        public T Show<T>(string uiFormName, UILayerType uILayerType = UILayerType.Common) where T : UIFormBase
        {
            if (string.IsNullOrEmpty(uiFormName))
            {
                Debugger.LogError("UIFormName is null");
                return default;
            }
            if (!uiFormName.EndsWith(".prefab"))
            {
                Debugger.LogError("UIFormName Not Exist .prefab ，Path：" + uiFormName);
                return default;
            }
            T UIFormLogicScript = GetUIFormLogicScript<T>();
            UIFormLogicScript.Show();
            return UIFormLogicScript;
        }

        /// <summary>
        /// 获取UI窗体是否显示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsShow<T>() where T : UIFormBase
        {
            return GetUIFormLogicScript<T>().IsShow;
        }

        //获取UI窗体所在层级
        public UILayerType GetUIFromLayer<T>() where T : UIFormBase
        {
            return GetUIFormLogicScript<T>().GetUIFormLayer;
        }


        /// <summary>
        /// 隐藏UI窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Hide<T>() where T : UIFormBase
        {
            GetUIFormLogicScript<T>()?.Hide();
        }


        /// <summary>
        /// 获取UI窗体实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public GameObject GetUIFormEntity<T>() where T : UIFormBase
        {
            if (m_DicUIPanelInfoContainer.ContainsKey(typeof(T).Name))
            {
                return m_DicUIPanelInfoContainer[typeof(T).Name].UIFormEntity;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 获取UI窗体的逻辑脚本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetUIFormLogicScript<T>() where T : UIFormBase
        {
            T UIFormLogicScript = null;
            if (m_DicUIPanelInfoContainer.ContainsKey(typeof(T).Name))
            {
                UIFormLogicScript = m_DicUIPanelInfoContainer[typeof(T).Name].UIFormLogicScript as T;
            }
            else
            {
                UIFormConfig.UIEntityConfigInfo uiEntityConfigInfo = m_UIFormConfig.GetUIFormConfigData<T>();
                if (uiEntityConfigInfo != null)
                {
                    string uiFormName = uiEntityConfigInfo.uiFormEntityName;
                    UILayerType uILayerType = uiEntityConfigInfo.uiLayerType;

                    //根据当前项目运行模式 获取UI窗体资源实体
                    LoadMode resType = GameLaunch.GetInstance.LaunchModel == LaunchModel.EditorModel ? LoadMode.ResEditor : LoadMode.ResAssetBundleAsset;
                    //Debug.Log("resType " + resType + "," + GameLaunch.GetInstance.LaunchModel + "," + LaunchModel.EditorModel + "," + (GameLaunch.GetInstance.LaunchModel == LaunchModel.EditorModel));
                    //GameObject UIForm = ResManager.LoadSync<GameObject>(uiFormName, resType);
                    string[] uiFormNameSplit = uiFormName.Split(new char[] { '/', '.' });
                    string assetName = uiFormNameSplit[uiFormNameSplit.Length - 2];
                    GameObject UIForm = Instantiate(LoadAssetsByAddressable.GetInstance.GetEntityRes(assetName));

                    //修改UI窗体名称
                    UIForm.name = assetName;
                    //设置UI窗体位置
                    RectTransform cloneRect = UIForm.GetComponent<RectTransform>();
                    Transform parent = UIRoot.transform.Find(uILayerType.ToString());
                    cloneRect.SetParent(parent);
                    cloneRect.offsetMax = Vector3.zero;
                    cloneRect.offsetMin = Vector3.zero;
                    cloneRect.anchoredPosition3D = Vector3.zero;
                    cloneRect.anchorMin = Vector2.zero;
                    cloneRect.anchorMax = Vector2.one;
                    cloneRect.pivot = new Vector2(0.5f, 0.5f);
                    cloneRect.localScale = Vector3.one;
                    //为UI窗体添加指定UIFormBase的派生类
                    UIFormLogicScript = UIForm.AddComponent<T>();
                    //缓存至资源池
                    m_DicUIPanelInfoContainer.Add(assetName, new UIFormInfo(UIForm, UIFormLogicScript));
                }
                else
                {
                    Debugger.LogError("GetPanelLogic Fail，name：" + typeof(T).Name);
                }
            }
            return UIFormLogicScript;
        }


        /// <summary>
        /// 设置Canvas分辨率
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="matchValue"></param>
        public static void SetUIRootCanvas(float width, float height, float matchValue = 0)
        {
            CanvasScaler canvasScaler = UIRoot.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(width, height);
            canvasScaler.matchWidthOrHeight = matchValue;
        }
    }
}