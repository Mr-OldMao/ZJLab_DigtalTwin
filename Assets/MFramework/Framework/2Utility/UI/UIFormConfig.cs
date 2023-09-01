using System;
using System.Collections.Generic;
namespace MFramework
{
    /// <summary>
    /// 标题：UI窗体配置表
    /// 功能：绑定映射UI窗体实体与mono脚本
    /// 作者：毛俊峰
    /// 时间：2023.01.04
    /// </summary>
    public class UIFormConfig
    {
        /// <summary>
        /// UI窗体实体映射表   key-mono脚本名 value-UI实体信息路径
        /// </summary>
        private Dictionary<string, UIEntityConfigInfo> m_DictUIFormConfig = new Dictionary<string, UIEntityConfigInfo>();

        /// <summary>
        /// UI实体信息
        /// </summary>
        public class UIEntityConfigInfo
        {
            /// <summary>
            /// UI窗体资源路径
            /// </summary>
            public string uiFormEntityName;
            /// <summary>
            /// UI窗体层级
            /// </summary>
            public UILayerType uiLayerType;

            public UIEntityConfigInfo(string uiFormEntityName, UILayerType uiLayerType = UILayerType.Common)
            {
                this.uiFormEntityName = uiFormEntityName;
                this.uiLayerType = uiLayerType;
            }
        }

        /// <summary>
        /// UIForm根路径
        /// </summary>
        private string m_UIFormsPath = "Assets/GameMain/UI/UIForms/";
        public UIFormConfig()
        {
            RegisterUIForm();
        }

        /// <summary>
        /// 注册UI预制体信息
        /// </summary>
        public void RegisterUIForm()
        {
            //Add<UIFormTest>(new UIEntityConfigInfo(m_UIFormsPath + "Main/UIFormTest.prefab", UILayerType.Common));
            Add<UIFormMain>(new UIEntityConfigInfo(m_UIFormsPath + "UIFormMain.prefab", UILayerType.Common));
        }

        /// <summary>
        /// 新增UI预制体信息到容器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uiEntityConfigInfo"></param>
        private void Add<T>(UIEntityConfigInfo uiEntityConfigInfo) where T : UIFormBase
        {
            string uiFormLogicName = typeof(T).Name;
            if (!m_DictUIFormConfig.ContainsKey(uiFormLogicName))
            {
                m_DictUIFormConfig.Add(uiFormLogicName, uiEntityConfigInfo);
            }
            else
            {
                Debugger.LogError("UIFormConfig is Exist， uiFormLogicName：" + uiFormLogicName + "，uiFormEntityName：" + uiEntityConfigInfo.uiFormEntityName);
            }
        }

        /// <summary>
        /// 获取UI窗体配置数据，根据UIFormBase派生类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public UIEntityConfigInfo GetUIFormConfigData<T>() where T : UIFormBase
        {
            UIEntityConfigInfo res = null;
            if (m_DictUIFormConfig.ContainsKey(typeof(T).Name))
            {
                res = m_DictUIFormConfig[typeof(T).Name];
            }
            return res;
        }
    }
}
