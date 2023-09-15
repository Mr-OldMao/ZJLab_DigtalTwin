using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 标题：UI窗体配置表
    /// 功能：绑定映射UI窗体实体与mono脚本
    /// 三种绑定方式
    /// 1.自动绑定
    /// 2.通过内部代码手动绑定
    /// 3.通过配置xml表手动绑定
    /// 作者：毛俊峰
    /// 时间：2023.01.04、2023.09.04
    /// </summary>
    public class UIFormConfig
    {
        /// <summary>
        /// UIForm预设实体根目录
        /// </summary>
        public const string UIFormRootDir = "Assets/GameMain/AB/Prefab/UI/UIForms";

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

        public UIFormConfig()
        {
            Bind();
        }

        /// <summary>
        /// 绑定UI预制体信息
        /// </summary>
        public void Bind()
        {
            //内部代码手动绑定
            //Add<UIFormTest>(new UIEntityConfigInfo(UIFormRootDir + "/Main/UIFormTest.prefab", UILayerType.Common));
            Add<UIFormMain>(new UIEntityConfigInfo(UIFormRootDir + "/Main/UIFormMain.prefab", UILayerType.Common));
            Add<UIFormCameraHint>(new UIEntityConfigInfo(UIFormRootDir + "/Main/UIFormCameraHint.prefab", UILayerType.Common));

            //根据xml配置表手动绑定UI窗体信息
            BindUIFormInfoByXml();

            //自动绑定UI窗体信息，实测Webgl平台无法自动绑定Ui窗体信息，需要手动绑定
            //AutoBindUIFormInfo();
        }

        /// <summary>
        /// 自动绑定UI窗体信息
        /// 优点：自动化配置，外部无需手动配置
        /// 缺点：通过反射遍历所有Assembly程序集下的类，找到所有UIFormBase的派生类进行配置，可能存在性能问题
        /// </summary>
        public void AutoBindUIFormInfo()
        {
            Type[] typeArr = Assembly.GetCallingAssembly().GetTypes();
            Type targetType = typeof(UIFormBase);

            foreach (Type type in typeArr)
            {
                if (type.BaseType == null)
                {
                    continue;
                }
                if (type.BaseType?.Name == targetType.Name)
                {
                    UIFormBase uIForm = System.Activator.CreateInstance(type) as UIFormBase;
                    Add(type, new UIEntityConfigInfo(uIForm.AssetPath, uIForm.GetUIFormLayer));
                    Debug.Log("Auto Bind UIForm，Name：" + type.Name);
                }
            }
        }

        //TODO根据xml配置表手动绑定UI窗体信息
        private void BindUIFormInfoByXml()
        {

        }

        /// <summary>
        /// 新增UI预制体信息到容器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uiEntityConfigInfo"></param>
        private void Add<T>(UIEntityConfigInfo uiEntityConfigInfo) where T : UIFormBase
        {
            Add(typeof(T), uiEntityConfigInfo);
        }

        /// <summary>
        /// 新增UI预制体信息到容器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uiEntityConfigInfo"></param>
        private void Add(Type type, UIEntityConfigInfo uiEntityConfigInfo)
        {
            string uiFormLogicName = type.Name;
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
