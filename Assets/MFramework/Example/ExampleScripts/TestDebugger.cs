using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：测试日志系统
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.
    /// 版本：1.0
    /// </summary>
    public class TestDebugger : MonoBehaviour
    {
        private void Start()
        {
            GameObject go = new GameObject();
            Debugger.Log("001temp" + go);
            Debugger.Log("002temp", LogTag.Temp);
            Debugger.Log("003test", LogTag.Test);
            Debugger.Log("004test", LogTag.Forever);

            Debugger.LogWarning("005temp" + go);
            Debugger.LogWarning("006temp", LogTag.Temp);
            Debugger.LogWarning("007test", LogTag.Test);
            Debugger.LogWarning("008test", LogTag.Forever);

            Debugger.LogError("111temp" + go);
            Debugger.LogError("222temp", LogTag.Temp);
            Debugger.LogError("333test", LogTag.Test);
            Debugger.LogError("444test", LogTag.Forever);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debugger.Log("xxxxx");
            }
        }
    }
}