using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 开发环境类型
    /// </summary>
    public enum EnvironmentMode
    {
        /// <summary>
        /// 开发阶段
        /// </summary>
        Developing,
        /// <summary>
        /// 测试阶段
        /// </summary>
        Test,
        /// <summary>
        /// 发布阶段
        /// </summary>
        Product
    }
    /// <summary>
    /// 标题：管理器入口
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.07.16
    /// 版本：1.0
    /// </summary>
    public abstract class MainManager : MonoBehaviour
    {
        public EnvironmentMode mode;

        private static EnvironmentMode m_EnvironmentMode;
        private static bool m_SetEnvironmentMode = true;
        private void Start()
        {
            if (m_SetEnvironmentMode)
            {
                m_EnvironmentMode = mode;
                m_SetEnvironmentMode = !m_SetEnvironmentMode;
            }
            switch (mode)
            {
                case EnvironmentMode.Developing:
                    LaunchInDevelopingModel();
                    break;
                case EnvironmentMode.Test:
                    LaunchInTestModel();
                    break;
                case EnvironmentMode.Product:
                    LaunchInProductModel();
                    break;
                default:
                    break;
            }
        }


        protected abstract void LaunchInDevelopingModel();
        protected abstract void LaunchInTestModel();
        protected abstract void LaunchInProductModel();
    }

}