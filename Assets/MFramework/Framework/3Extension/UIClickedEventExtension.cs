using UnityEngine.Events;
using static UnityEngine.UI.Button;
using static UnityEngine.UI.Toggle;

namespace MFramework
{
    /// <summary>
    /// 标题：UI点击事件静态扩展
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.10.18
    /// 版本：1.0
    /// </summary>
    public static class UIClickedEventExtension
    {
        /// <summary>
        /// btn点击事件静态扩展
        /// </summary>
        /// <param name="btnEvent"></param>
        /// <param name="unityAction"></param>
        /// <param name="btnClickAudioType"></param>
        public static void AddListenerCustom(this ButtonClickedEvent btnEvent, UnityAction unityAction)
        {
            unityAction += () =>
            {
                //todo  eg：AudioManager.GetInstance.Play();
            };
            btnEvent.AddListener(unityAction);
        }

        public static void AddListenerCustom(this ToggleEvent tgeEvent, UnityAction<bool> unityAction)
        {
            unityAction += (b) =>
            { 
                //todo  eg：if (b) { AudioManager.GetInstance.Play(); }
            };
            tgeEvent.AddListener(unityAction);
        }
    }
}