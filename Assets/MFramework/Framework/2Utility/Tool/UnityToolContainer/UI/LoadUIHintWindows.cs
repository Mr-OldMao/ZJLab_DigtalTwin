using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace MFramework
{
    /// <summary>
    /// 描述：自动加载提示框UI窗体
    /// 作者：毛俊峰
    /// 时间：2022.05.10
    /// 版本：1.0
    /// </summary>
    public class LoadUIHintWindows : SingletonByMono<LoadUIHintWindows>
    {
        /// <summary>
        ///UI预设 根目录
        /// </summary>
        public static string hintBorderRootPath = "Assets/GameMain/AB/UI/HintWindow/HintBorder";
        /// <summary>
        /// 无按钮
        /// </summary>
        public static string hintSubRootPath_NotBtn = "imgBorderNotBtn";
        /// <summary>
        /// 一个按钮
        /// </summary>
        public static string hintSubRootPath_OneBtn = "imgBorderOneBtn";
        /// <summary>
        /// 两个按钮
        /// </summary>
        public static string hintSubRootPath_TwoBtn = "imgBorderTwoBtn";
        /// <summary>
        /// 简单Log提示
        /// </summary>
        public static string hintSubRootPath_Simple = "imgHintSimple";


        /// <summary>
        /// 允许自动销毁
        /// </summary>
        public static bool m_CanAutoDestroy = true;
        /// <summary>
        /// 自动销毁的剩余时间
        /// </summary>
        public static float m_AutoDrestroyRemainTime = -1;
        /// <summary>
        /// 自动销毁后回调
        /// </summary>
        public static Action m_AutoDestroyCallback;

        /// <summary>
        /// 文本UI配置
        /// </summary>
        public struct TextSetting
        {
            /// <summary>
            /// 顶部标题内容
            /// </summary>
            public string txtTopContext;
            /// <summary>
            /// 提示内容
            /// </summary>
            public string txtHintContext;
            /// <summary>
            /// 顶部标题内容颜色
            /// </summary>
            public Color colorTxtTopContext;
            /// <summary>
            /// 提示内容颜色
            /// </summary>
            public Color colorTxtHintContext;
        }

        /// <summary>
        /// 按钮UI配置
        /// </summary>
        public struct ButtonSetting
        {
            /// <summary>
            /// 按钮下文本名称
            /// </summary>
            public string txtContextName;
            /// <summary>
            /// 按钮事件
            /// </summary>
            public UnityAction btnEvent;
            /// <summary>
            /// 按钮下文本颜色
            /// </summary>
            public Color txtColor;
            /// <summary>
            /// 按钮颜色
            /// </summary>
            public Color btnColor;
        }

        /// <summary>
        /// 无按钮 提示窗体
        /// 返回值：自动销毁 返回空，反之返回提示窗体实例
        /// </summary>
        /// <param name="textSetting">文本UI结构体</param>
        /// <param name="autoCloseTime">自动销毁的时间  负数为不自动销毁,生成的窗体作为返回值返回</param>
        /// <param name="autoCloseCallback">自动销毁后回调</param>
        /// <param name="maskFullSceneColorA">全屏遮罩不透明度， 默认为0.3半透明遮罩   范围：【负无穷，1】负数为取消遮罩，0全透明 1不透明</param>
        public GameObject LoadHintBorderNotBtn(TextSetting textSetting, float autoCloseTime = 2f, Action autoCloseCallback = null, float maskFullSceneColorA = 0.3f)
        {
            //窗体位置
            string path = hintBorderRootPath + hintSubRootPath_NotBtn;
            //创建提示窗体
            GameObject clone = CreateHintWindows(hintBorderRootPath, hintSubRootPath_NotBtn);
            //获取组件
            Transform txtHintContextTrans = clone.transform.Find("txtHintContext");
            Transform txtTopContextTrans = clone.transform.Find("txtTopContext");
            Transform imgMask = clone.transform.Find("imgMask");

            //配置文本
            if (txtHintContextTrans != null)
            {
                Text txtHintContext = txtHintContextTrans.GetComponent<Text>();
                if (txtHintContext != null)
                {
                    txtHintContext.text = string.IsNullOrEmpty(textSetting.txtHintContext) ? string.Empty : textSetting.txtHintContext;
                    txtHintContext.color = textSetting.colorTxtHintContext != Color.clear ? textSetting.colorTxtHintContext : txtHintContext.color;
                }
                Debug.Log("提示框：" + txtHintContextTrans.GetComponent<Text>().text);
            }
            if (txtTopContextTrans != null)
            {
                Text txtTopContext = txtTopContextTrans.GetComponent<Text>();
                if (txtTopContext != null)
                {
                    txtTopContext.text = string.IsNullOrEmpty(textSetting.txtTopContext) ? string.Empty : textSetting.txtTopContext;
                    txtTopContext.color = textSetting.colorTxtTopContext != Color.clear ? textSetting.colorTxtTopContext : txtTopContext.color;
                }
            }
            //窗体自动销毁
            if (autoCloseTime > 0)
            {
                StartCoroutine(DelayCloseLogInfoWin(clone, autoCloseTime, autoCloseCallback));

                //clone.AddComponent<LoadUIHintWindows>();
                //m_CanAutoDestroy = true;
                //m_AutoDrestroyRemainTime = autoDestroyTime;
                //m_AutoDestroyCallback = autoDestroyCallback;
            }
            //遮罩
            if (imgMask != null)
            {
                if (maskFullSceneColorA >= 0)
                {
                    imgMask.GetComponent<Image>().color = new Color(0, 0, 0, maskFullSceneColorA);
                }
                imgMask.gameObject.SetActive(maskFullSceneColorA >= 0);
            }
            return autoCloseTime > 0 ? null : clone;
        }

        /// <summary>
        /// 无按钮 提示窗体简易版
        /// 返回值：自动销毁 返回空，反之返回提示窗体实例
        /// </summary>
        /// <param name="txtContext">文本内容 0提示内容 1-顶部窗体标题(不需要可省略)</param>
        /// <param name="autoCloseTime">自动销毁的时间  负数为不自动销毁,生成的窗体作为返回值返回</param>
        /// <param name="autoCloseCallback">自动销毁后回调</param>
        /// <param name="maskFullSceneColorA">全屏遮罩不透明度， 默认为0.3半透明遮罩   范围：【负无穷，1】负数为取消遮罩，0全透明 1不透明</param>
        public GameObject LoadHintBorderNotBtn(string[] txtContext, float autoCloseTime = 2f, Action autoCloseCallback = null, float maskFullSceneColorA = 0.3f)
        {
            string txtHintContext = txtContext.Length >= 1 ? txtContext[0] : string.Empty;
            string txtTopContext = txtContext.Length >= 2 ? txtContext[1] : string.Empty;
            TextSetting textSetting = new TextSetting
            {
                txtHintContext = txtHintContext,
                txtTopContext = txtTopContext,
                colorTxtHintContext = Color.black,
                colorTxtTopContext = Color.black
            };
            return LoadHintBorderNotBtn(textSetting, autoCloseTime, autoCloseCallback, maskFullSceneColorA);
        }


        /// <summary>
        /// 俩按钮 提示窗体
        /// </summary>
        /// <param name="txtContext">文本UI结构体</param>
        /// <param name="leftButtonSetting">左按钮UI结构体</param>
        /// <param name="rightButtonSetting">右按钮UI结构体</param>
        /// <param name="maskFullSceneColorA">全屏遮罩不透明度， 默认为0.3半透明遮罩   范围：【负无穷，1】负数为取消遮罩，0全透明 1不透明</param>
        /// <returns></returns>
        public GameObject LoadHintBorderTwoBtn(TextSetting txtContext, ButtonSetting leftButtonSetting, ButtonSetting rightButtonSetting, float maskFullSceneColorA = 0.3f)
        {
            //窗体位置
            string path = hintBorderRootPath + hintSubRootPath_TwoBtn;
            //创建提示窗体
            GameObject clone = CreateHintWindows(hintBorderRootPath, hintSubRootPath_TwoBtn);
            //获取组件
            Transform txtHintContextTrans = clone.transform.Find("txtHintContext");
            Transform txtTopContextTrans = clone.transform.Find("txtTopContext");
            Transform leftBtnTrans = clone.transform.Find("btnGroup/btnLeft");
            Transform rightBtnTrans = clone.transform.Find("btnGroup/btnRight");
            Transform imgMask = clone.transform.Find("imgMask");
            #region 配置文本
            if (txtHintContextTrans != null)
            {
                Text txtHintContext = txtHintContextTrans.GetComponent<Text>();
                if (txtHintContext != null)
                {
                    txtHintContext.text = string.IsNullOrEmpty(txtContext.txtHintContext) ? string.Empty : txtContext.txtHintContext;
                    txtHintContext.color = txtContext.colorTxtHintContext != Color.clear ? txtContext.colorTxtHintContext : txtHintContext.color;
                }
            }
            else
            {
                Debug.LogError(path + "  txtHintContext is Null");
            }
            if (txtTopContextTrans != null)
            {
                Text txtTopContext = txtTopContextTrans.GetComponent<Text>();
                if (txtTopContext != null)
                {
                    txtTopContext.text = string.IsNullOrEmpty(txtContext.txtTopContext) ? string.Empty : txtContext.txtTopContext;
                    txtTopContext.color = txtContext.colorTxtTopContext != Color.clear ? txtContext.colorTxtTopContext : txtTopContext.color;
                }
            }
            else
            {
                Debug.LogError(path + "  txtTopContext is Null");
            }
            #endregion

            #region 配置按钮
            if (leftBtnTrans != null)
            {
                Button leftBtn = leftBtnTrans.GetComponent<Button>();
                if (leftBtn != null)
                {
                    leftBtn.onClick.AddListenerCustom(() =>
                    {
                        leftButtonSetting.btnEvent?.Invoke();
                        Destroy(clone.gameObject); //TODO 后期优化 使用对象池
                    });
                    leftBtn.GetComponent<Image>().color = leftButtonSetting.btnColor != Color.clear ? leftButtonSetting.btnColor : leftBtn.GetComponent<Image>().color;
                }
                Text leftText = leftBtnTrans.GetComponentInChildren<Text>();
                if (leftText != null)
                {
                    leftText.text = string.IsNullOrEmpty(leftButtonSetting.txtContextName) ? string.Empty : leftButtonSetting.txtContextName;
                    leftText.color = leftButtonSetting.txtColor != Color.clear ? leftButtonSetting.txtColor : leftText.color;
                }
            }
            else
            {
                Debug.LogError(path + "  btnLeft is Null");
            }
            if (rightBtnTrans != null)
            {
                Button rightBtn = rightBtnTrans.GetComponent<Button>();
                if (rightBtn != null)
                {
                    rightBtn.onClick.AddListenerCustom(() =>
                    {
                        rightButtonSetting.btnEvent?.Invoke();
                        Destroy(clone.gameObject); //TODO 后期优化 使用对象池
                    });
                    rightBtn.GetComponent<Image>().color = rightButtonSetting.btnColor != Color.clear ? rightButtonSetting.btnColor : rightBtn.GetComponent<Image>().color;
                }
                Text rightText = rightBtnTrans.GetComponentInChildren<Text>();
                if (rightText != null)
                {
                    rightText.text = string.IsNullOrEmpty(rightButtonSetting.txtContextName) ? string.Empty : rightButtonSetting.txtContextName;
                    rightText.color = rightButtonSetting.txtColor != Color.clear ? rightButtonSetting.txtColor : rightText.color;
                }
            }
            else
            {
                Debug.LogError(path + "  btnRight is Null");
            }
            #endregion

            //遮罩
            if (imgMask != null)
            {
                if (maskFullSceneColorA >= 0)
                {
                    imgMask.GetComponent<Image>().color = new Color(0, 0, 0, maskFullSceneColorA);
                }
                imgMask.gameObject.SetActive(maskFullSceneColorA >= 0);
            }
            return clone;
        }
        /// <summary>
        /// 俩按钮 提示窗体简易版
        /// </summary>
        /// <param name="txtContext">文本内容 0提示内容 1-顶部窗体标题(不需要可省略)</param>
        /// <param name="leftBtnContext">左按钮内容</param>
        /// <param name="rightBtnContext">右按钮内容</param>
        /// <param name="btnEventLeft">左按钮事件</param>
        /// <param name="btnEventRight">右按钮事件</param>
        /// <param name="maskFullSceneColorA">全屏遮罩不透明度， 默认为0.3半透明遮罩   范围：【负无穷，1】负数为取消遮罩，0全透明 1不透明</param>
        /// <returns></returns>
        public GameObject LoadHintBorderTwoBtn(string[] txtContext, string leftBtnContext, string rightBtnContext, UnityAction btnEventLeft, UnityAction btnEventRight, float maskFullSceneColorA = 0.3f)
        {
            string txtHintContext = txtContext.Length >= 1 ? txtContext[0] : string.Empty;
            string txtTopContext = txtContext.Length >= 2 ? txtContext[1] : string.Empty;
            TextSetting textSetting = new TextSetting
            {
                txtHintContext = txtHintContext,
                txtTopContext = txtTopContext,
                colorTxtHintContext = Color.black,
                colorTxtTopContext = Color.black
            };
            ButtonSetting leftButtonSetting = new ButtonSetting
            {
                btnEvent = btnEventLeft,
                btnColor = Color.white,
                txtContextName = leftBtnContext,
                txtColor = Color.black,
            };
            ButtonSetting rightButtonSetting = new ButtonSetting
            {
                btnEvent = btnEventRight,
                btnColor = Color.white,
                txtContextName = rightBtnContext,
                txtColor = Color.black,
            };
            return LoadHintBorderTwoBtn(textSetting, leftButtonSetting, rightButtonSetting, maskFullSceneColorA);
        }
        /// <summary>
        /// 俩按钮 提示窗体简易版
        /// </summary>
        /// <param name="txtContext">文本内容 0提示内容 1-顶部窗体标题(不需要可省略)</param>
        /// <param name="leftBtnContext">左按钮内容</param>
        /// <param name="rightBtnContext">右按钮内容</param>
        /// <param name="btnEventLeft">左按钮事件</param>
        /// <param name="btnEventRight">右按钮事件</param>
        /// <param name="maskFullSceneColorA">全屏遮罩不透明度， 默认为0.3半透明遮罩   范围：【负无穷，1】负数为取消遮罩，0全透明 1不透明</param>
        /// <returns></returns>
        public GameObject LoadHintBorderTwoBtn(string txtContext, string txtTitle, string leftBtnContext, string rightBtnContext, UnityAction btnEventLeft, UnityAction btnEventRight, float maskFullSceneColorA = 0.3f)
        {
            string txtHintContext = txtContext;
            string txtTopContext = txtTitle;
            TextSetting textSetting = new TextSetting
            {
                txtHintContext = txtHintContext,
                txtTopContext = txtTopContext,
                colorTxtHintContext = Color.black,
                colorTxtTopContext = Color.black
            };
            ButtonSetting leftButtonSetting = new ButtonSetting
            {
                btnEvent = btnEventLeft,
                btnColor = Color.white,
                txtContextName = leftBtnContext,
                txtColor = Color.black,
            };
            ButtonSetting rightButtonSetting = new ButtonSetting
            {
                btnEvent = btnEventRight,
                btnColor = Color.white,
                txtContextName = rightBtnContext,
                txtColor = Color.black,
            };
            return LoadHintBorderTwoBtn(textSetting, leftButtonSetting, rightButtonSetting, maskFullSceneColorA);
        }
        /// <summary>
        /// 一按钮 提示窗体
        /// </summary>
        /// <param name="txtContext">文本UI结构体</param>
        /// <param name="buttonSetting">按钮UI结构体</param>
        /// <param name="maskFullSceneColorA">全屏遮罩不透明度， 默认为0.3半透明遮罩   范围：【负无穷，1】负数为取消遮罩，0全透明 1不透明</param>
        /// <returns></returns>
        public GameObject LoadHintBorderOneBtn(TextSetting txtContext, ButtonSetting buttonSetting, float maskFullSceneColorA = 0.3f)
        {
            //窗体位置
            string path = hintBorderRootPath + hintSubRootPath_OneBtn;
            //创建提示窗体
            GameObject clone = CreateHintWindows(hintBorderRootPath, hintSubRootPath_OneBtn);
            //获取组件
            Text txtHintContext = clone.transform.Find<Text>("txtHintContext");
            Text txtTopContext = clone.transform.Find<Text>("txtTopContext");
            Button btnCenter = clone.transform.Find<Button>("btnCenter");
            Image imgMask = clone.transform.Find<Image>("imgMask");
            #region 配置文本
            if (txtHintContext != null)
            {
                if (txtHintContext != null)
                {
                    txtHintContext.text = string.IsNullOrEmpty(txtContext.txtHintContext) ? string.Empty : txtContext.txtHintContext;
                    txtHintContext.color = txtContext.colorTxtHintContext != Color.clear ? txtContext.colorTxtHintContext : txtHintContext.color;
                }
            }
            else
            {
                Debug.LogError(path + "  txtHintContext is Null");
            }
            if (txtTopContext != null)
            {
                if (txtTopContext != null)
                {
                    txtTopContext.text = string.IsNullOrEmpty(txtContext.txtTopContext) ? string.Empty : txtContext.txtTopContext;
                    txtTopContext.color = txtContext.colorTxtTopContext != Color.clear ? txtContext.colorTxtTopContext : txtTopContext.color;
                }
            }
            else
            {
                Debug.LogError(path + "  txtTopContext is Null");
            }
            #endregion

            #region 配置按钮
            if (btnCenter != null)
            {
                btnCenter.onClick.AddListenerCustom(() =>
                {
                    buttonSetting.btnEvent?.Invoke();
                    Destroy(clone.gameObject); //TODO 后期优化 使用对象池
                });

                btnCenter.GetComponent<Image>().color = buttonSetting.btnColor != Color.clear ? buttonSetting.btnColor : btnCenter.GetComponent<Image>().color;
                Text btnChindText = btnCenter.GetComponentInChildren<Text>();
                if (btnChindText != null)
                {
                    btnChindText.text = string.IsNullOrEmpty(buttonSetting.txtContextName) ? string.Empty : buttonSetting.txtContextName;
                    btnChindText.color = buttonSetting.txtColor != Color.clear ? buttonSetting.txtColor : btnChindText.color;
                }
            }
            else
            {
                Debug.LogError(path + "  btnLeft is Null");
            }
            #endregion

            //遮罩
            if (imgMask != null)
            {
                if (maskFullSceneColorA >= 0)
                {
                    imgMask.color = new Color(0, 0, 0, maskFullSceneColorA);
                }
                imgMask.gameObject.SetActive(maskFullSceneColorA >= 0);
            }
            return clone;
        }
        /// <summary>
        /// 一按钮 提示窗体简易版
        /// </summary>
        /// <param name="txtContext">文本内容 0提示内容 1-顶部窗体标题(不需要可省略)</param>
        /// <param name="btnContext">左按钮内容</param>
        /// <param name="btnEvent">左按钮事件</param>
        /// <param name="maskFullSceneColorA">全屏遮罩不透明度， 默认为0.3半透明遮罩   范围：【负无穷，1】负数为取消遮罩，0全透明 1不透明</param>
        /// <returns></returns>
        public GameObject LoadHintBorderOneBtn(string[] txtContext, string btnContext, UnityAction btnEvent, float maskFullSceneColorA = 0.3f)
        {
            string txtHintContext = txtContext.Length >= 1 ? txtContext[0] : string.Empty;
            string txtTopContext = txtContext.Length >= 2 ? txtContext[1] : string.Empty;
            TextSetting textSetting = new TextSetting
            {
                txtHintContext = txtHintContext,
                txtTopContext = txtTopContext,
                colorTxtHintContext = Color.black,
                colorTxtTopContext = Color.black
            };
            ButtonSetting btnSetting = new ButtonSetting
            {
                btnEvent = btnEvent,
                txtContextName = btnContext,
                btnColor = Color.white,
                txtColor = Color.black
            };
            return LoadHintBorderOneBtn(textSetting, btnSetting, maskFullSceneColorA);
        }
        /// <summary>
        /// 一按钮 提示窗体简易版
        /// </summary>
        /// <param name="txtContext">文本内容 0提示内容 1-顶部窗体标题(不需要可省略)</param>
        /// <param name="btnContext">左按钮内容</param>
        /// <param name="btnEvent">左按钮事件</param>
        /// <param name="maskFullSceneColorA">全屏遮罩不透明度， 默认为0.3半透明遮罩   范围：【负无穷，1】负数为取消遮罩，0全透明 1不透明</param>
        /// <returns></returns>
        public GameObject LoadHintBorderOneBtn(string txtContext, string btnContext, UnityAction btnEvent, float maskFullSceneColorA = 0.3f)
        {
            string txtHintContext = txtContext;
            string txtTopContext = "提示";
            TextSetting textSetting = new TextSetting
            {
                txtHintContext = txtHintContext,
                txtTopContext = txtTopContext,
                colorTxtHintContext = Color.black,
                colorTxtTopContext = Color.black
            };
            ButtonSetting btnSetting = new ButtonSetting
            {
                btnEvent = btnEvent,
                txtContextName = btnContext,
                btnColor = Color.white,
                txtColor = Color.black
            };
            return LoadHintBorderOneBtn(textSetting, btnSetting, maskFullSceneColorA);
        }

        /// <summary>
        /// 日志信息
        /// </summary>
        /// <param name="txtContext">文本UI结构体</param>
        /// <param name="autoCloseWinTime">自动关闭窗体时间</param>
        /// <param name="closeWinCallback">回调 关闭窗体后回调</param>
        /// <param name="maskFullSceneColorA">全屏遮罩不透明度， 默认为0.3半透明遮罩   范围：【负无穷，1】负数为取消遮罩，0全透明 1不透明</param>
        /// <returns></returns>
        public GameObject LoadHintLogInfo(TextSetting txtContext, float autoCloseWinTime = 1f, Action closeWinCallback = null, float maskFullSceneColorA = -1)
        {
            //窗体预设位置
            string path = hintBorderRootPath + hintSubRootPath_Simple;
            //创建提示窗体
            GameObject clone = CreateHintWindows(hintBorderRootPath, hintSubRootPath_Simple);
            //获取组件
            Text txtHintContextTrans = clone.transform.Find("txtHintContext").GetComponent<Text>();
            Image imgMask = clone.transform.Find("imgMask").GetComponent<Image>();
            Image imgContextBg = clone.transform.Find("imgContextBg").GetComponent<Image>();
            #region 配置文本
            txtHintContextTrans.text = string.IsNullOrEmpty(txtContext.txtHintContext) ? string.Empty : txtContext.txtHintContext;
            txtHintContextTrans.color = txtContext.colorTxtHintContext != Color.clear ? txtContext.colorTxtHintContext : txtHintContextTrans.color;
            #endregion
            //窗体自动关闭  ---TODo 后期需要优化成对象池
            if (autoCloseWinTime > 0)
            {
                StartCoroutine(DelayCloseLogInfoWin(clone, autoCloseWinTime, closeWinCallback));
            }
            //遮罩
            SetMaskImg(ref imgMask, maskFullSceneColorA);
            return clone;
        }


        /// <summary>
        /// 日志信息  简易版
        /// </summary>
        /// <param name="txtContext">文本UI结构体</param>
        /// <param name="autoCloseWinTime">自动关闭窗体时间</param>
        /// <param name="closeWinCallback">回调 关闭窗体后回调</param>
        /// <param name="maskFullSceneColorA">全屏遮罩不透明度， 默认为0.3半透明遮罩   范围：【负无穷，1】负数为取消遮罩，0全透明 1不透明</param>
        public GameObject LoadHintLogInfo(string txtContext, float autoCloseWinTime = 2f, Action closeWinCallback = null, float maskFullSceneColorA = -1)
        {
            string txtHintContext = txtContext.Length >= 1 ? txtContext : string.Empty;
            TextSetting textSetting = new TextSetting
            {
                txtHintContext = txtHintContext,
                colorTxtHintContext = Color.white
            };
            return LoadHintLogInfo(textSetting, autoCloseWinTime, closeWinCallback, maskFullSceneColorA);
        }

        /// <summary>
        /// 创建提示窗体
        /// </summary>
        /// <param name="resourcesPath">窗体预设Resources位置</param>
        private static GameObject CreateHintWindows(string hintBorderRootPath, string hintSubPath)
        {
            //TODO 后期优化 使用对象池
            //生成提示框
            GameObject clone = ResManager.LoadSync<GameObject>(hintBorderRootPath + "/" + hintSubPath + ".prefab");
            if (clone != null)
            {
                //配置窗体位置、大小信息
                Canvas cloneParent = FindObjectOfType<Canvas>();
                for (int i = 0; i < FindObjectsOfType<Canvas>().Length; i++)
                {
                    if (FindObjectsOfType<Canvas>()[i].name == "CvsUIHint")
                    {
                        cloneParent = FindObjectsOfType<Canvas>()[i];
                        break;
                    }
                }
                if (cloneParent == null)
                {
                    GameObject imgHintSimple = ResManager.LoadSync<GameObject>(hintBorderRootPath + "/cvsUIHint.prefab");
                    cloneParent = imgHintSimple.GetComponent<Canvas>();
                    cloneParent.name = "CvsUIHint";
                    DontDestroyOnLoad(cloneParent);
                }
                clone.transform.SetParent(cloneParent.transform);

                //锚点设置 todo

                //位置设置   
                clone.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                clone.GetComponent<RectTransform>().localScale = Vector3.one;
                return clone;
            }
            return null;
        }


        /// <summary>
        /// 设置窗体的全屏遮罩
        /// </summary>
        /// <param name="imgMask">窗体背景遮罩</param>
        /// <param name="maskFullSceneColorA">全屏遮罩不透明度， 默认为0.3半透明遮罩   范围：【负无穷，1】负数为取消遮罩，0全透明 1不透明</param>
        private void SetMaskImg(ref Image imgMask, float maskFullSceneColorA)
        {
            if (imgMask != null)
            {
                if (maskFullSceneColorA >= 0)
                {
                    imgMask.GetComponent<Image>().color = new Color(0, 0, 0, maskFullSceneColorA);
                }
                imgMask.gameObject.SetActive(maskFullSceneColorA >= 0);
            }
        }

        /// <summary>
        /// 延时关闭窗体
        /// </summary>
        /// <param name="hintWin"></param>
        /// <param name="autoCloseWinTime"></param>
        /// <param name="closeWinCallback"></param>
        /// <returns></returns>
        IEnumerator DelayCloseLogInfoWin(GameObject hintWin, float autoCloseWinTime = 2f, Action closeWinCallback = null)
        {
            yield return new WaitForSeconds(autoCloseWinTime);
            Destroy(hintWin); //todo 后期优化为对象池
            closeWinCallback?.Invoke();
        }


        ///// <summary>
        ///// 延时关闭 日志信息窗体
        ///// </summary>
        ///// <param name="delayAction"></param>
        ///// <param name="waitTime"></param>
        ///// <returns></returns>
        //IEnumerator DelayCloseLogInfoWin(GameObject hintWin, Image imgTextBg, Text txtHintContextTrans, float autoCloseWinTime = 2f, Action closeWinCallback = null)
        //{
        //    yield return new WaitForSeconds(autoCloseWinTime);

        //    ////渐变消失 todo
        //    //float imgTextBgColorA = imgTextBg.color.a;
        //    //while (imgTextBgColorA >0)
        //    //{
        //    //    imgTextBgColorA -= 0.01f;
        //    //    if (imgTextBgColorA >= 0)
        //    //    {
        //    //        imgTextBg.color = new Color(imgTextBg.color.r, imgTextBg.color.g, imgTextBg.color.b, imgTextBgColorA);
        //    //    }
        //    //    else
        //    //    {
        //    //        Destroy(hintWin); //todo 后期优化为对象池
        //    //        closeWinCallback?.Invoke();
        //    //    }
        //    //    yield return new WaitForFixedUpdate();
        //    //}
        //}
    }
}
