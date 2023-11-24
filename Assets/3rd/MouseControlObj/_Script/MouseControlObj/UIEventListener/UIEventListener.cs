using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace UIFramework
{
    //2.定义委托数据类型
    /// <summary>
    /// 鼠标响应委托
    /// </summary>
    /// <param name="eventData"></param>
    public delegate void PointEventHandler(PointerEventData eventData);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseEventData"></param>
    public delegate void BaseEventHandler(BaseEventData baseEventData);

    public delegate void AxisEventHandler(AxisEventData axisEventData);
    /// <summary>
    /// UI事件监听类，管理所有UGUI事件，提供事件参数类, 
    /// 附加到需要交互的UI元素上，用于监听用户的操作，与EventTrigger类事件类似
    /// </summary>

    public class UIEventListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler
    {
        //3.声明事件
        /// <summary>
        /// 鼠标点击事件
        /// </summary>
        public event PointEventHandler pointClickHandler;
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public event PointEventHandler pointDownHandler;
        /// <summary>
        /// 鼠标抬起事件
        /// </summary>
        public event PointEventHandler pointUpHandler;
        /// <summary>
        /// 鼠标进入事件
        /// </summary>
        public event PointEventHandler pointEnterHandler;
        /// <summary>
        /// 鼠标移出事件
        /// </summary>
        public event PointEventHandler pointExitHandler;
        /// <summary>
        /// 鼠标在A对象按下还没开始拖拽时 A对象响应此事件
        /// </summary>
        public event PointEventHandler pointInitializePotentialDragHandler;
        /// <summary>
        /// 鼠标开始拖拽
        /// </summary>
        public event PointEventHandler pointBeginDragHandler;
        /// <summary>
        /// 鼠标拖拽
        /// </summary>
        public event PointEventHandler pointDragHandler;
        /// <summary>
        /// 鼠标拖拽完成
        /// </summary>
        public event PointEventHandler pointEndHandler;
        /// <summary>
        /// 拖拉结束，拖拉开始的地方必须先实现IDragHandler，该事件在拖拉结束的对象上发生(但不能是拖拉开始的对象)
        /// </summary>
        public event PointEventHandler pointDropHandler;
        /// <summary>
        /// 鼠标中键滚动
        /// </summary>
        public event PointEventHandler pointScrollHandler;
        /// <summary>
        ///  当对象被选中，则每帧都会发生
        /// </summary>
        public event BaseEventHandler updateSelectedHandler;
        /// <summary>
        ///  当对象被选中，则每帧都会发生
        /// </summary>
        public event BaseEventHandler slectHandler;
        /// <summary>
        ///  不再选中该对象,点击对象外的地方就会变成不选中
        /// </summary>
        public event BaseEventHandler deselectHandler;
        /// <summary>
        /// 点击方向键	对象被选中才会发生
        /// </summary>
        public event AxisEventHandler moveHandler;
        /// <summary>
        ///  点击Submit键(默认是Enter键)	对象被选中才会发生
        /// </summary>
        public event BaseEventHandler submitHandler;
        /// <summary>
        ///  点击Cancel键(默认是Esc键)	对象被选中才会发生
        /// </summary>
        public event BaseEventHandler cancelHandler;
        

        /// <summary>
        /// 获取事件监听器
        /// </summary>
        /// <param name="tf"></param>
        /// <returns></returns>
        public static UIEventListener GetUIEventListener(Transform tf)
        {
            UIEventListener uiEventListener = tf.GetComponent<UIEventListener>();
            if (uiEventListener==null)
            {
                uiEventListener = tf.gameObject.AddComponent<UIEventListener>();
            }
            return uiEventListener;
        }

        //1.继承接口
        public void OnPointerClick(PointerEventData eventData)
        {
            //4.引发事件
            if (pointClickHandler!=null)
            {
                pointClickHandler(eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pointDownHandler != null)
            {
                pointDownHandler(eventData);
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (pointUpHandler != null)
            {
                pointUpHandler(eventData);
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (pointEnterHandler != null)
            {
                pointEnterHandler(eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (pointExitHandler != null)
            {
                pointExitHandler(eventData);
            }
        }
        /// <summary>
        /// 鼠标在A对象按下还没开始拖拽时 A对象响应此事件
        /// </summary>
        /// <param name="eventData"></param>
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (pointInitializePotentialDragHandler != null)
            {
                pointInitializePotentialDragHandler(eventData);
            }
        }
        /// <summary>
        /// 开始拖拽
        /// </summary>
        /// <param name="eventData"></param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (pointBeginDragHandler != null)
            {
                pointBeginDragHandler(eventData);
            }
            
        }
        /// <summary>
        /// 拖拽
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            if (pointDragHandler != null)
            {
                pointDragHandler(eventData);
            }
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (pointEndHandler != null)
            {
                pointEndHandler(eventData);
            }
        }
        /// <summary>
        /// 拖拉结束，拖拉开始的地方必须先实现IDragHandler，该事件在拖拉结束的对象上发生(但不能是拖拉开始的对象)
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrop(PointerEventData eventData)
        {
            if (pointDropHandler != null)
            {
                pointDropHandler(eventData);
            }
        }
        /// <summary>
        /// 鼠标中键滚动
        /// </summary>
        /// <param name="eventData"></param>
        public void OnScroll(PointerEventData eventData)
        {
            if (pointScrollHandler != null)
            {
                pointScrollHandler(eventData);
            }
        }
        /// <summary>
        /// 当对象被选中，则每帧都会发生
        /// </summary>
        /// <param name="eventData"></param>
        public void OnUpdateSelected(BaseEventData eventData)
        {
            if (updateSelectedHandler != null)
            {
                updateSelectedHandler(eventData);
            }
        }
        /// <summary>
        /// 当EventSystem选中该对象,使用EventSystem中的SetSelectedGameObject方法来选中
        /// </summary>
        /// <param name="eventData"></param>
        public void OnSelect(BaseEventData eventData)
        {
            if (slectHandler != null)
            {
                slectHandler(eventData);
            }
        }
        /// <summary>
        /// 不再选中该对象,点击对象外的地方就会变成不选中
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDeselect(BaseEventData eventData)
        {
            if (deselectHandler != null)
            {
                deselectHandler(eventData);
            }
        }
        /// <summary>
        /// 点击方向键	对象被选中才会发生
        /// </summary>
        /// <param name="eventData"></param>
        public void OnMove(AxisEventData eventData)
        {
            if (moveHandler != null)
            {
                moveHandler(eventData);
            }
        }
        /// <summary>
        /// 点击Submit键(默认是Enter键)	对象被选中才会发生
        /// </summary>
        /// <param name="eventData"></param>
        public void OnSubmit(BaseEventData eventData)
        {
            if (submitHandler != null)
            {
                submitHandler(eventData);
            }
        }
        /// <summary>
        /// 点击Cancel键(默认是Esc键)	对象被选中才会发生
        /// </summary>
        /// <param name="eventData"></param>
        public void OnCancel(BaseEventData eventData)
        {
            if (cancelHandler != null)
            {
                cancelHandler(eventData);
            }
        }
    }
}
