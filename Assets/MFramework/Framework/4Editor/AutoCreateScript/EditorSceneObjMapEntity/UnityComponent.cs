using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace MFramework
{
    /// <summary>
    /// 描述：自动获取当前脚本下所有Unity组件，为了解决的问题：搭建完UI需要 动态创建每个UI对象的实例映射
    /// 功能：1.获取当前脚本下的所有UI对象，根据对象所挂载的组件类型进行分类
    ///       2.根据获取到的场景对象名、UI组件类型，直接访问字典容器获取组件
    ///       3.场景UI对象 与 新建变量 映射一一对应，为逻辑层服务，方便调用
    /// 作者：毛俊峰
    /// 时间：2022.07.13
    /// 版本：1.0
    /// </summary>
    public class UnityComponent : MonoBehaviour
    {
        //常用UI组件的容器
        public Dictionary<string, Button> dicBtn = new Dictionary<string, Button>();
        public Dictionary<string, Toggle> dicTge = new Dictionary<string, Toggle>();
        public Dictionary<string, Text> dicText = new Dictionary<string, Text>();
        public Dictionary<string, Image> dicImg = new Dictionary<string, Image>();
        public Dictionary<string, Slider> dicSlider = new Dictionary<string, Slider>();
        public Dictionary<string, ScrollRect> dicScrollRect = new Dictionary<string, ScrollRect>();
        public Dictionary<string, Scrollbar> dicScrollbar = new Dictionary<string, Scrollbar>();
        public Dictionary<string, InputField> dicInputField = new Dictionary<string, InputField>();
        public Dictionary<string, TMP_InputField> dicInputFieldPro = new Dictionary<string, TMP_InputField>();
        public Dictionary<string, TextMeshProUGUI> dicTextPro = new Dictionary<string, TextMeshProUGUI>();
        public Dictionary<string, Transform> dicTrans = new Dictionary<string, Transform>();
        /// <summary>
        /// 常用UI组件容器的容器
        /// </summary>
        public List<object> listComponentContainer = new List<object>();

        /// <summary>
        /// UI事件
        /// </summary>
        public class UIEvent
        {
            /// <summary>
            /// 按钮事件
            /// </summary>
            public UnityAction btnEvent;
            /// <summary>
            /// Toggle事件
            /// </summary>
            public UnityAction<bool> tgeEvent;
        }
        public new GameObject GetComponent(string uiObjName)
        {
            return GetComponent<Transform>(uiObjName).gameObject;
        }

        private void Awake()
        {
            GetUIComponent();
        }

        [ContextMenu("测试获取UI组件")]
        public void GetUIComponent()
        {
            SaveAllUIComponent(transform);
            Debug.Log("btnCount:" + dicBtn.Count + ",tgeCount:" + dicTge.Count + ",textCount:" + dicText.Count + ",imgCount:" + dicImg.Count);
        }

        /// <summary>
        /// 获取UI组件
        /// T：支持 RectTransForm，Image，Button，Toggle，Text类型
        /// </summary>
        /// <param name="uiObjName">场景挂载UI的游戏对象名称</param>
        /// <returns></returns>
        public T GetComponent<T>(string uiObjName) where T : Component
        {
            T res = null;
            if (typeof(T) == typeof(Transform) || typeof(T) == typeof(RectTransform))
            {
                if (dicTrans.ContainsKey(uiObjName))
                {
                    res = dicTrans[uiObjName] as T;
                }
            }
            else if (typeof(T) == typeof(Text))
            {
                if (dicText.ContainsKey(uiObjName))
                {
                    res = dicText[uiObjName] as T;
                }
            }
            else if (typeof(T) == typeof(Button))
            {
                if (dicBtn.ContainsKey(uiObjName))
                {
                    res = dicBtn[uiObjName] as T;
                }
            }
            else if (typeof(T) == typeof(Toggle))
            {
                if (dicTge.ContainsKey(uiObjName))
                {
                    res = dicTge[uiObjName] as T;
                }
            }
            else if (typeof(T) == typeof(Image))
            {
                if (dicImg.ContainsKey(uiObjName))
                {
                    res = dicImg[uiObjName] as T;
                }
            }
            else if (typeof(T) == typeof(Slider))
            {
                if (dicSlider.ContainsKey(uiObjName))
                {
                    res = dicSlider[uiObjName] as T;
                }
            }
            else if (typeof(T) == typeof(ScrollRect))
            {
                if (dicScrollRect.ContainsKey(uiObjName))
                {
                    res = dicScrollRect[uiObjName] as T;
                }
            }
            else if (typeof(T) == typeof(Scrollbar))
            {
                if (dicScrollbar.ContainsKey(uiObjName))
                {
                    res = dicScrollbar[uiObjName] as T;
                }
            }
            else if (typeof(T) == typeof(InputField))
            {
                if (dicInputField.ContainsKey(uiObjName))
                {
                    res = dicInputField[uiObjName] as T;
                }
            }
            else if (typeof(T) == typeof(TMP_InputField))
            {
                if (dicInputFieldPro.ContainsKey(uiObjName))
                {
                    res = dicInputFieldPro[uiObjName] as T;
                }
            }
            else if (typeof(T) == typeof(TextMeshProUGUI))
            {
                if (dicTextPro.ContainsKey(uiObjName))
                {
                    res = dicTextPro[uiObjName] as T;
                }
            }
            else
            {
                Debug.LogError("未找到组件  类型：" + typeof(T) + " 对象名:" + uiObjName);
            }
            if (!res)
            {
                Debug.LogError("未找到组件  类型：" + typeof(T) + " 对象名:" + uiObjName);
            }

            return res;
        }

        /// <summary>
        /// 注册UI事件
        /// 调用时序：Start()中调用
        /// </summary>
        /// <typeparam name="T">UI组件类型 支持Button，Toggle、Slider类型</typeparam>
        /// <param name="uiObjName">场景挂载UI的游戏对象名称</param>
        /// <param name="btnCallback">按钮回调事件</param>
        /// <param name="tgeCallback">开关回调事件</param>
        public void RegisterUIEvent<T>(string uiObjName, UIEvent uIEvent) where T : Component
        {
            if (typeof(T) == typeof(Button) || typeof(T) == typeof(Toggle))
            {
                T uiComponent = GetComponent<T>(uiObjName);
                if (uiComponent != null)
                {
                    if (typeof(T) == typeof(Button))
                    {
                        Button btn = uiComponent as Button;
                        if (uIEvent.btnEvent != null)
                        {
                            btn.onClick.AddListener(uIEvent.btnEvent);

                        }
                        else
                        {
                            Debug.LogError("注册UI事件失败，回调为空  对象名：" + uiObjName + "，组件类型：" + typeof(T));
                        }
                    }
                    else if (typeof(T) == typeof(Toggle))
                    {
                        Toggle tge = uiComponent as Toggle;
                        if (uIEvent.tgeEvent != null)
                        {
                            tge.onValueChanged.AddListener(uIEvent.tgeEvent);
                        }
                        else
                        {
                            Debug.LogError("注册UI事件失败，回调为空  对象名：" + uiObjName + "，组件类型：" + typeof(T));
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("注册UI事件失败  对象名：" + uiObjName + "，组件类型：" + typeof(T));
            }
        }



        /// <summary>
        /// 缓存当前对象下所有UI组件  一个游戏对象可缓存在多个容器中
        /// 规则：场景挂载UI的游戏对象名需要唯一，自动屏蔽默认的游戏对象名 如Text、Text (TMP)、Button
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="isSaveHideObj">是否缓存未激活对象</param>
        public void SaveAllUIComponent(Transform trans, bool isSaveHideObj = true)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                Transform child = trans.GetChild(i);
                if (!isSaveHideObj && !child.gameObject.activeSelf)
                {
                    //重复迭代
                    if (child.childCount > 0)
                    {
                        SaveAllUIComponent(child);
                    }
                    continue;
                }
                //屏蔽未重命名的UI组件
                if (child.name == "Canvas" || child.name == "Text" || child.name == "Button" || child.name == "Toggle" || child.name == "Image" || child.name == "Slider"
                    || child.name == "Text (TMP)" || child.name == "RawImage" || child.name == "EventSystem" || child.name == "Viewport" || child.name == "Background"
                    || child.name == "Checkmark" || child.name == "Label" || child.name == "Content" || child.name == "Fill Area" || child.name == "Fill" || child.name == "Handle Slide Area"
                    || child.name == "Handle" || child.name == "Scrollbar Horizontal" || child.name == "Scrollbar Vertical" || child.name == "Sliding Area")
                {
                    //重复迭代
                    if (child.childCount > 0)
                    {
                        SaveAllUIComponent(child);
                    }
                    continue;
                }
                Button childBtn = child.GetComponent<Button>();
                Toggle childTge = child.GetComponent<Toggle>();
                Text childtext = child.GetComponent<Text>();
                Image childImg = child.GetComponent<Image>();
                Transform childTrans = child.GetComponent<Transform>();
                Slider childSlider = child.GetComponent<Slider>();
                ScrollRect childScrollRect = child.GetComponent<ScrollRect>();
                Scrollbar childScrollbar = child.GetComponent<Scrollbar>();
                InputField childInputField = child.GetComponent<InputField>();
                TMP_InputField childInputFieldPro = child.GetComponent<TMP_InputField>();
                TextMeshProUGUI childTxtPro = child.GetComponent<TextMeshProUGUI>();

                if (childTrans != null && !dicTrans.ContainsKey(child.name))
                {
                    dicTrans.Add(child.name, childTrans);
                }
                if (childBtn != null && !dicBtn.ContainsKey(child.name))
                {
                    dicBtn.Add(child.name, childBtn);
                }
                if (childTge != null && !dicTge.ContainsKey(child.name))
                {
                    dicTge.Add(child.name, childTge);
                }
                if (childImg != null && !dicImg.ContainsKey(child.name))
                {
                    dicImg.Add(child.name, childImg);
                }
                if (childtext != null && !dicText.ContainsKey(child.name))
                {
                    dicText.Add(child.name, childtext);
                }
                if (childSlider && !dicSlider.ContainsKey(child.name))
                {
                    dicSlider.Add(child.name, childSlider);
                }
                if (childScrollRect && !dicScrollRect.ContainsKey(child.name))
                {
                    dicScrollRect.Add(child.name, childScrollRect);
                }
                if (childScrollbar && !dicScrollbar.ContainsKey(child.name))
                {
                    dicScrollbar.Add(child.name, childScrollbar);
                }
                if (childInputField && !dicInputField.ContainsKey(child.name))
                {
                    dicInputField.Add(child.name, childInputField);
                }
                if (childInputFieldPro && !dicInputFieldPro.ContainsKey(child.name))
                {
                    dicInputFieldPro.Add(child.name, childInputFieldPro);
                }
                if (childTxtPro && !dicTextPro.ContainsKey(child.name))
                {
                    dicTextPro.Add(child.name, childTxtPro);
                }
                //重复迭代
                if (child.childCount > 0)
                {
                    SaveAllUIComponent(child);
                }
            }
        }


        /// <summary>
        /// 缓存当前对象下所有UI组件  每种对象缓存容器唯一
        /// 类型：按钮、文本、开关
        /// 规则：场景挂载UI的游戏对象名需要唯一，自动屏蔽默认的游戏对象名如Text、Text (TMP)、Button
        /// 缓存容器优先级  Button > Image , Scrollbar > image , Toggle > image 
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="isSaveHideObj">是否缓存未激活对象</param>
        public void SaveAllUIComponentContainerOnly(Transform trans, bool isSaveHideObj = true)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                Transform child = trans.GetChild(i);
                if (!isSaveHideObj && !child.gameObject.activeSelf)
                {
                    //重复迭代
                    if (child.childCount > 0)
                    {
                        SaveAllUIComponentContainerOnly(child);
                    }
                    continue;
                }

                if (!MaskCustomUIObjAndChinds(child.name))
                {
                    continue;
                }
                if (!MaskCustomUIObj(child.name) || !JudgeUIName(child.name))
                {
                    //重复迭代
                    if (child.childCount > 0)
                    {
                        SaveAllUIComponentContainerOnly(child);
                    }
                    continue;
                }
                Button childBtn = child.GetComponent<Button>();
                Toggle childTge = child.GetComponent<Toggle>();
                Text childtext = child.GetComponent<Text>();
                Image childImg = child.GetComponent<Image>();
                Transform childTrans = child.GetComponent<Transform>();
                Slider childSlider = child.GetComponent<Slider>();
                ScrollRect childScrollRect = child.GetComponent<ScrollRect>();
                Scrollbar childScrollbar = child.GetComponent<Scrollbar>();
                InputField childInputField = child.GetComponent<InputField>();
                TMP_InputField childInputFieldPro = child.GetComponent<TMP_InputField>();
                TextMeshProUGUI childTxtPro = child.GetComponent<TextMeshProUGUI>();

                if (childBtn && !dicBtn.ContainsKey(child.name))
                {
                    dicBtn.Add(child.name, childBtn);
                }
                else if (childTge && !dicTge.ContainsKey(child.name))
                {
                    dicTge.Add(child.name, childTge);
                }
                else if (childSlider && !dicSlider.ContainsKey(child.name))
                {
                    dicSlider.Add(child.name, childSlider);
                }
                else if (childScrollRect && !dicScrollRect.ContainsKey(child.name))
                {
                    dicScrollRect.Add(child.name, childScrollRect);
                }
                else if (childScrollbar && !dicScrollbar.ContainsKey(child.name))
                {
                    dicScrollbar.Add(child.name, childScrollbar);
                }
                else if (childInputField && !dicInputField.ContainsKey(child.name))
                {
                    dicInputField.Add(child.name, childInputField);
                }
                else if (childInputFieldPro && !dicInputFieldPro.ContainsKey(child.name))
                {
                    dicInputFieldPro.Add(child.name, childInputFieldPro);
                }
                else if (childTxtPro && !dicTextPro.ContainsKey(child.name))
                {
                    dicTextPro.Add(child.name, childTxtPro);
                }
                else if (childImg && !dicImg.ContainsKey(child.name))
                {
                    dicImg.Add(child.name, childImg);
                }
                else if (childtext && !dicText.ContainsKey(child.name))
                {
                    dicText.Add(child.name, childtext);
                }
                else if (childTrans && !dicTrans.ContainsKey(child.name))
                {
                    dicTrans.Add(child.name, childTrans);
                }
                else
                {
                    Debug.LogError("未知错误 child：" + child);
                }
                //重复迭代
                if (child.childCount > 0)
                {
                    SaveAllUIComponentContainerOnly(child);
                }
            }
        }

        /// <summary>
        /// 屏蔽指定UI游戏对象 如果当前游戏对象名称不合法，是否连带子对象也一同屏蔽   T-合法
        /// </summary>
        /// <param name="uiObjName"></param>
        /// <returns></returns>
        private bool MaskCustomUIObj(string uiObjName)
        {
            if (string.IsNullOrEmpty(uiObjName))
            {
                return false;
            }
            //屏蔽未重命名的UI组件
            if (
                    uiObjName == "Canvas" || uiObjName == "Text" || uiObjName == "Button" || uiObjName == "Toggle" || uiObjName == "Image" || uiObjName == "Slider"
                    || uiObjName == "Text (TMP)" || uiObjName == "RawImage" || uiObjName == "EventSystem" || uiObjName == "Viewport" || uiObjName == "Background"
                    || uiObjName == "Checkmark" || uiObjName == "Label" || uiObjName == "Content" || uiObjName == "Fill Area" || uiObjName == "Fill" || uiObjName == "Handle Slide Area"
                    || uiObjName == "Handle" || uiObjName == "Scrollbar Horizontal" || uiObjName == "Scrollbar Vertical" || uiObjName == "Sliding Area")
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 屏蔽指定UI游戏对象以及子对象 T-合法
        /// </summary>
        /// <param name="uiObjName"></param>
        /// <returns></returns>
        private bool MaskCustomUIObjAndChinds(string uiObjName)
        {
            if (string.IsNullOrEmpty(uiObjName))
            {
                return false;
            }

            if (uiObjName.Contains("LineChart"))
            {
                return false;
            }


            return true;
        }

        /// <summary>
        /// 判定游戏名称是否合法 屏蔽不合法的ui组件根据游戏对象名称 T-合法
        /// </summary>
        /// <param name="uiObjName">ui游戏对象名称</param>
        /// <returns></returns>
        private bool JudgeUIName(string uiObjName)
        {
            if (string.IsNullOrEmpty(uiObjName))
            {
                return false;
            }

            char firstChar = uiObjName[0];
            //大写字母开头
            if (firstChar >= 'A' && firstChar <= 'Z')
            {
                return false;
            }

            //数字开头
            if (firstChar >= '0' && firstChar <= '9')
            {
                return false;
            }

            //名称中有空格
            if (uiObjName.Split(' ').Length > 1)
            {
                return false;
            }

            //_开头
            if (uiObjName.StartsWith("_"))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// 获取UI组件容器
        /// </summary>
        /// <returns></returns>
        public List<object> GetListComponentContainer()
        {
            listComponentContainer.Clear();
            listComponentContainer.Add(dicBtn);
            listComponentContainer.Add(dicTge);
            listComponentContainer.Add(dicText);
            listComponentContainer.Add(dicImg);
            listComponentContainer.Add(dicSlider);
            listComponentContainer.Add(dicScrollRect);
            listComponentContainer.Add(dicScrollbar);
            listComponentContainer.Add(dicInputField);
            listComponentContainer.Add(dicInputFieldPro);
            listComponentContainer.Add(dicTextPro);
            listComponentContainer.Add(dicTrans);
            return listComponentContainer;
        }
    }
}
