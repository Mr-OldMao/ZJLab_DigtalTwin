using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：测试特性
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.12.10
    /// 版本：1.0
    /// </summary>
    public class TestAttribute : MonoBehaviour
    {
#if UNITY_EDITOR
        [EnumLabel("场景名称")]
#endif
        public SceneName sceneName;

        public enum SceneName
        {
            [Header("场景入口")]
            SceneLaunch,
            [Header("主场景")]
            SceneMain,
            [Header("战斗场景")]
            SceneAction,
        }
    }
}