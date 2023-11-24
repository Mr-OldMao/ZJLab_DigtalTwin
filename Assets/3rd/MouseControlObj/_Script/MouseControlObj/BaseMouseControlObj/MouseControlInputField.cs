using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;
using System;
using UnityEngine.EventSystems;

namespace MouseControlObj
{
    /// <summary>
    /// 当输入框Text改变时执行
    /// </summary>
    /// <returns></returns>
    public delegate void On_MouseControlInputFieldTextChange(float f);
    /// <summary>
    /// 按下增或者建按钮之后的回调(每一帧执行)
    /// </summary>
    public delegate void On_ButtonDown();
    /// <summary>
    /// 鼠标移动物体的输入框
    /// </summary>
    public class MouseControlInputField : InputField
    {
        bool isAddButtonDown = false;

        bool isSubButtonDown = false;
        /// <summary>
        /// 当输入框Text改变时执行,通过Text修改物体transform属性
        /// </summary>
        public On_MouseControlInputFieldTextChange on_MouseControlInputFieldTextChange;
        /// <summary>
        /// 加按下事件
        /// </summary>
        public event PointEventHandler on_AddButton_Click;
        /// <summary>
        /// 减按下事件
        /// </summary>
        public event PointEventHandler on_SubButton_Click;

        /// <summary>
        /// 鼠标一直点击加按钮
        /// </summary>
        public On_ButtonDown on_AddButton_Down_EveryFrame;
        /// <summary>
        /// 鼠标一直点击减按钮
        /// </summary>
        public On_ButtonDown on_SubButton_Down_EveryFrame;

        private Transform addButton;
        /// <summary>
        /// 加按钮
        /// </summary>
        private Transform AddButton
        {
            get
            {
                if (addButton == null)
                {
                    addButton = transform.FindChildForName("AddButton");
                }
                return addButton;
            }
        }

        private Transform subButton;
        /// <summary>
        /// 减按钮
        /// </summary>
        private Transform SubButton
        {
            get
            {
                if (subButton == null)
                {
                    subButton = transform.FindChildForName("SubButton");
                }

                return subButton;
            }
        }
        /// <summary>
        /// 更新Text
        /// </summary>
        /// <param name="value"></param>
        public void UpdateText(float value)
        {
            if (currentSelectionState == SelectionState.Selected)
            {
                return;
            }
            text = value.ToString();
        }
        /// <summary>
        /// 更新Text
        /// </summary>
        /// <param name="value"></param>
        public void UpdateText(string value)
        {
            if (currentSelectionState == SelectionState.Selected)
            {
                return;
            }
            text = value.ToString();
        }
        protected override void Awake()
        {
            AddListener();
        }
        /// <summary>
        /// 添加点击事件
        /// </summary>
        private void AddListener()
        {
            UIEventListener.GetUIEventListener(AddButton).pointClickHandler += On_AddButton_Click;
            UIEventListener.GetUIEventListener(SubButton).pointClickHandler += On_SubButton_Click;

            UIEventListener.GetUIEventListener(AddButton).pointDownHandler += On_AddButton_Down;
            UIEventListener.GetUIEventListener(AddButton).pointUpHandler += On_AddButton_Up;

            UIEventListener.GetUIEventListener(SubButton).pointDownHandler += On_SubButton_Down;
            UIEventListener.GetUIEventListener(SubButton).pointUpHandler += On_SubButton_Up;
        }

        private void On_SubButton_Down(PointerEventData eventData)
        {
            isSubButtonDown = true;
        }

        private void On_SubButton_Up(PointerEventData eventData)
        {
            isSubButtonDown = false;
        }

        private void On_AddButton_Up(PointerEventData eventData)
        {
            isAddButtonDown = false;
        }

        private void On_AddButton_Down(PointerEventData eventData)
        {
            isAddButtonDown = true;
        }

        private void Update()
        {
            if (currentSelectionState == SelectionState.Selected)
            {
                if (on_MouseControlInputFieldTextChange != null)
                {
                    float f;
                    if (float.TryParse(text, out f))
                    {
                        on_MouseControlInputFieldTextChange(f);
                    }

                }
            }
            else if (isAddButtonDown && on_AddButton_Down_EveryFrame != null)
            {
                on_AddButton_Down_EveryFrame();
            }
            else if (isSubButtonDown && on_SubButton_Down_EveryFrame != null)
            {
                on_SubButton_Down_EveryFrame();
            }

        }
        /// <summary>
        /// 清除所有回调
        /// </summary>
        public void ClearAllListener()
        {
            on_MouseControlInputFieldTextChange = null;
            on_SubButton_Click = null;
            on_AddButton_Click = null;
            on_SubButton_Down_EveryFrame = null;
            on_AddButton_Down_EveryFrame = null;
        }
        /// <summary>
        /// 加按钮按下
        /// </summary>
        /// <param name="eventData"></param>
        private void On_AddButton_Click(PointerEventData eventData)
        {
            if (on_AddButton_Click != null)
            {
                on_AddButton_Click(eventData);
            }
        }
        /// <summary>
        /// 减按钮按下
        /// </summary>
        /// <param name="eventData"></param>
        private void On_SubButton_Click(PointerEventData eventData)
        {
            if (on_SubButton_Click != null)
            {
                on_SubButton_Click(eventData);
            }
        }


    }
}
