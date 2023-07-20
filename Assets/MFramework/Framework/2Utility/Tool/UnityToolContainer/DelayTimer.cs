using System;
using System.Collections;
using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 标题：Unity工具类
    /// 功能：定时工具延时调用
    /// 作者：毛俊峰
    /// 时间：2022.09.27
    /// 版本：1.0
    /// </summary>
    public class DelayTimer : SingletonByMono<DelayTimer>
    {
        #region 定时工具 延时调用
        /// <summary>
        /// 延时调用
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="callback"></param>
        public Coroutine DelayCoroutine(float seconds, Action callback)
        {
            return StartCoroutine(IEDelayCoroutine(seconds, callback));
        }
        /// <summary>
        /// 延时调用 等待当前帧结束调用
        /// </summary>
        public Coroutine DelayCoroutineWaitForEndOfFrame(Action callback)
        {
            return StartCoroutine(IEDelayCoroutineWaitForEndOfFrame(callback));
        }
        /// <summary>
        /// 延时调用 等待下一帧调用
        /// </summary>
        public Coroutine DelayCoroutineWaitNextFrame(params Action[] callback)
        {
            return StartCoroutine(IEDelayCoroutineWaitNextFrame(callback));
        }
        /// <summary>
        /// 延时调用 等待Func返回true   执行回调
        /// </summary>
        /// <param name="func"></param>
        /// <param name="callback"></param>
        public Coroutine DelayCoroutineWaitReturnTrue(Func<bool> func, Action callback)
        {
            return StartCoroutine(IEDelayCoroutineWaitReturnTrue(func, callback));
        }
        /// <summary>
        /// 延时调用 等待Func返回false   执行回调
        /// </summary>
        /// <param name="func"></param>
        /// <param name="callback"></param>
        public Coroutine DelayCoroutineWaitReturnFalse(Func<bool> func, Action callback)
        {
            return StartCoroutine(IEDelayCoroutineWaitReturnFalse(func, callback));
        }

        /// <summary>
        /// 计数器
        /// </summary>
        /// <param name="func">p1-当前时间(单位秒)，p2-返回值为false时终止计数，调用频率-每hzSecond秒回调一次</param>
        /// <param name="hzSecond">频率 单位秒</param>
        public Coroutine DelayCoroutineTimer(Func<float, bool> func, float hzSecond = 1.0f)
        {
            return StartCoroutine(IDelayCoroutineTimer(func, hzSecond));
        }
        #endregion

        #region IEnumerator
        private IEnumerator IEDelayCoroutine(float seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }
        private IEnumerator IEDelayCoroutineWaitForEndOfFrame(Action callback)
        {
            yield return new WaitForEndOfFrame();
            callback?.Invoke();
        }
        private IEnumerator IEDelayCoroutineWaitNextFrame(params Action[] callback)
        {
            for (int i = 0; i < callback.Length; i++)
            {
                yield return null;
                callback[i]?.Invoke();
            }
        }
        private IEnumerator IEDelayCoroutineWaitReturnTrue(Func<bool> func, Action callback)
        {
            yield return new WaitUntil(func);
            callback?.Invoke();
        }

        private IEnumerator IEDelayCoroutineWaitReturnFalse(Func<bool> func, Action callback)
        {
            yield return new WaitWhile(func);
            callback?.Invoke();
        }
        private IEnumerator IDelayCoroutineTimer(Func<float, bool> func, float hzSecond)
        {
            float curTime = 0;
            while (func.Invoke(curTime))
            {
                yield return new WaitForSeconds(hzSecond);
                curTime += hzSecond;
            }
        }
        #endregion
    }

    /// <summary>
    /// UnityTool工具类扩展-定时工具延时调用
    /// </summary>
    public partial class UnityTool
    {
        #region 定时工具 延时调用
        /// <summary>
        /// 延时调用
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="callback"></param>
        public Coroutine DelayCoroutine(float seconds, Action callback)
        {
            return DelayTimer.GetInstance.DelayCoroutine(seconds, callback);
        }
        /// <summary>
        /// 延时调用 等待当前帧结束调用
        /// </summary>
        public Coroutine DelayCoroutineWaitForEndOfFrame(Action callback)
        {
            return DelayTimer.GetInstance.DelayCoroutineWaitForEndOfFrame(callback);
        }
        /// <summary>
        /// 延时调用 等待下一帧调用
        /// </summary>
        public Coroutine DelayCoroutineWaitNextFrame(params Action[] callback)
        {
            return DelayTimer.GetInstance.DelayCoroutineWaitNextFrame(callback);
        }
        /// <summary>
        /// 延时调用 等待Func返回true   执行回调
        /// </summary>
        /// <param name="func"></param>
        /// <param name="callback"></param>
        public Coroutine DelayCoroutineWaitReturnTrue(Func<bool> func, Action callback)
        {
            return DelayTimer.GetInstance.DelayCoroutineWaitReturnTrue(func, callback);
        }
        /// <summary>
        /// 延时调用 等待Func返回false   执行回调
        /// </summary>
        /// <param name="func"></param>
        /// <param name="callback"></param>
        public Coroutine DelayCoroutineWaitReturnFalse(Func<bool> func, Action callback)
        {
            return DelayTimer.GetInstance.DelayCoroutineWaitReturnFalse(func, callback);
        }

        /// <summary>
        /// 计数器
        /// </summary>
        /// <param name="func">p1-当前时间(单位秒)，p2-返回值为false时终止计数，调用频率-每hzSecond秒回调一次</param>
        /// <param name="hzSecond">频率 单位秒</param>
        public Coroutine DelayCoroutineTimer(Func<float, bool> func, float hzSecond = 1.0f)
        {
            return DelayTimer.GetInstance.DelayCoroutineTimer(func, hzSecond);
        }
        #endregion
    }
}
