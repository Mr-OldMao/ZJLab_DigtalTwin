using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace  MFramework
{
    /// <summary>
    /// 标题：程序逻辑入口
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.12.28
    /// </summary>
    public class GameLogic : SingletonByMono<GameLogic>
    {
        public void Init()
        {
            Debug.Log("Init GameLogic");
            this.EnterMainScene();
        }

        private void EnterMainScene()
        {
            //加载UI、实体
        }
    } 
}
