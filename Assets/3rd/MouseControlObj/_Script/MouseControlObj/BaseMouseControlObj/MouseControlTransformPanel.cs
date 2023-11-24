using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MouseControlObj
{
  
    /// <summary>
    /// 可操作物体的UI控制面板中控制位置，大小，缩放面板
    /// </summary>
    public class MouseControlTransformPanel : MonoBehaviour
    {

        [SerializeField]
        /// <summary>
        /// 调整位置的时候的增量
        /// </summary>
        private float pos_increment=0.01f;
        [SerializeField]
        /// <summary>
        /// 调整旋转的时候的增量
        /// </summary>
        private float rot_increment = 0.01f;
        [SerializeField]
        /// <summary>
        /// 调整缩放的时候的增量
        /// </summary>
        private float scale_increment = 0.01f;
        /// <summary>
        /// 控制的模型Transform
        /// </summary>
        private Transform controlModel;
      

        private MouseControlInputField pos_X_InputField;
        /// <summary>
        /// 控制位置X轴InputField
        /// </summary>
        private MouseControlInputField Pos_X_InputField
        {
            get
            {
                if (pos_X_InputField == null)
                {
                    pos_X_InputField = transform.FindChildForName("Pos_X_InputField").GetComponent<MouseControlInputField>();
                }
                return pos_X_InputField;
            }
        }
        private MouseControlInputField pos_Y_InputField;
        /// <summary>
        /// 控制位置Y轴InputField
        /// </summary>
        private MouseControlInputField Pos_Y_InputField
        {
            get
            {
                if (pos_Y_InputField == null)
                {
                    pos_Y_InputField = transform.FindChildForName("Pos_Y_InputField").GetComponent<MouseControlInputField>();
                }
                return pos_Y_InputField;
            }
        }
        private MouseControlInputField pos_Z_InputField;
        /// <summary>
        /// 控制位置Z轴InputField
        /// </summary>
        private MouseControlInputField Pos_Z_InputField
        {
            get
            {
                if (pos_Z_InputField == null)
                {
                    pos_Z_InputField = transform.FindChildForName("Pos_Z_InputField").GetComponent<MouseControlInputField>();
                }
                return pos_Z_InputField;
            }
        }

        private MouseControlInputField rot_X_InputField;
        /// <summary>
        /// 控制旋转X轴InputField
        /// </summary>
        private MouseControlInputField Rot_X_InputField
        {
            get
            {
                if (rot_X_InputField == null)
                {
                    rot_X_InputField = transform.FindChildForName("Rot_X_InputField").GetComponent<MouseControlInputField>();
                }
                return rot_X_InputField;
            }
        }
        private MouseControlInputField rot_Y_InputField;
        /// <summary>
        /// 控制旋转Y轴InputField
        /// </summary>
        private MouseControlInputField Rot_Y_InputField
        {
            get
            {
                if (rot_Y_InputField == null)
                {
                    rot_Y_InputField = transform.FindChildForName("Rot_Y_InputField").GetComponent<MouseControlInputField>();
                }
                return rot_Y_InputField;
            }
        }
        private MouseControlInputField rot_Z_InputField;
        /// <summary>
        /// 控制旋转Z轴InputField
        /// </summary>
        private MouseControlInputField Rot_Z_InputField
        {
            get
            {
                if (rot_Z_InputField == null)
                {
                    rot_Z_InputField = transform.FindChildForName("Rot_Z_InputField").GetComponent<MouseControlInputField>();
                }
                return rot_Z_InputField;
            }
        }

        private MouseControlInputField scale_X_InputField;
        /// <summary>
        /// 控制缩放X轴InputField
        /// </summary>
        private MouseControlInputField Scale_X_InputField
        {
            get
            {
                if (scale_X_InputField == null)
                {
                    scale_X_InputField = transform.FindChildForName("Scale_X_InputField").GetComponent<MouseControlInputField>();
                }
                return scale_X_InputField;
            }
        }
        private MouseControlInputField scale_Y_InputField;
        /// <summary>
        /// 控制缩放Y轴InputField
        /// </summary>
        private MouseControlInputField Scale_Y_InputField
        {
            get
            {
                if (scale_Y_InputField == null)
                {
                    scale_Y_InputField = transform.FindChildForName("Scale_Y_InputField").GetComponent<MouseControlInputField>();
                }
                return scale_Y_InputField;
            }
        }
        private MouseControlInputField scale_Z_InputField;
        /// <summary>
        /// 控制缩放Z轴InputField
        /// </summary>
        private MouseControlInputField Scale_Z_InputField
        {
            get
            {
                if (scale_Z_InputField == null)
                {
                    scale_Z_InputField = transform.FindChildForName("Scale_Z_InputField").GetComponent<MouseControlInputField>();
                }
                return scale_Z_InputField;
            }
        }

      
        // Start is called before the first frame update
        void Start()
        {
           
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            ClearAllListener();
            AddAllListener();
        }
        private void Update()
        {
            UpdateAllInputField();
        }
        /// <summary>
        /// 设置控制模型
        /// </summary>
        /// <param name="t"></param>
        public void Set_controlModel(Transform t)
        {
            controlModel = t;
        }

        private void ClearAllListener()
        {
            Pos_X_InputField.ClearAllListener();
          
            Pos_Y_InputField.ClearAllListener();
            Pos_Z_InputField.ClearAllListener();



            Rot_X_InputField.ClearAllListener();
            Rot_Y_InputField.ClearAllListener();
            Rot_Z_InputField.ClearAllListener();

            Scale_X_InputField.ClearAllListener();
            Scale_Y_InputField.ClearAllListener();
            Scale_Z_InputField.ClearAllListener();
        }
        /// <summary>
        /// 根据模型实际位置来设置InputField
        /// </summary>
        private void UpdateAllInputField()
        {
            Pos_X_InputField.UpdateText(controlModel.localPosition.x);
            Pos_Y_InputField.UpdateText(controlModel.localPosition.y);
            Pos_Z_InputField.UpdateText(controlModel.localPosition.z);

            Rot_X_InputField.UpdateText(controlModel.localEulerAngles.x);
            Rot_Y_InputField.UpdateText(controlModel.localEulerAngles.y);
            Rot_Z_InputField.UpdateText(controlModel.localEulerAngles.z);

            Scale_X_InputField.UpdateText(controlModel.localScale.x);
            Scale_Y_InputField.UpdateText(controlModel.localScale.y);
            Scale_Z_InputField.UpdateText(controlModel.localScale.z);
        }
        /// <summary>
        /// 添加事件
        /// </summary>
        private void AddAllListener()
        {
            Pos_X_InputField.on_AddButton_Click += On_Pos_X_InputField_AddBtn_Click;
            Pos_X_InputField.on_SubButton_Click += On_Pos_X_InputField_SubBtn_Click;
            Pos_X_InputField.on_MouseControlInputFieldTextChange += on_MouseControlInputFieldTextChange_Pos_X;
            Pos_X_InputField.on_AddButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Pos_X_AddButton;
            Pos_X_InputField.on_SubButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Pos_X_SubButton;

            Pos_Y_InputField.on_AddButton_Click += On_Pos_Y_InputField_AddBtn_Click;
            Pos_Y_InputField.on_SubButton_Click += On_Pos_Y_InputField_SubBtn_Click;
            Pos_Y_InputField.on_MouseControlInputFieldTextChange += on_MouseControlInputFieldTextChange_Pos_Y;
            Pos_Y_InputField.on_AddButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Pos_Y_AddButton;
            Pos_Y_InputField.on_SubButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Pos_Y_SubButton;

            Pos_Z_InputField.on_AddButton_Click += On_Pos_Z_InputField_AddBtn_Click;
            Pos_Z_InputField.on_SubButton_Click += On_Pos_Z_InputField_SubBtn_Click;
            Pos_Z_InputField.on_MouseControlInputFieldTextChange += on_MouseControlInputFieldTextChange_Pos_Z;
            Pos_Z_InputField.on_AddButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Pos_Z_AddButton;
            Pos_Z_InputField.on_SubButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Pos_Z_SubButton;


            Rot_X_InputField.on_AddButton_Click += On_Rot_X_InputField_AddBtn_Click;
            Rot_X_InputField.on_SubButton_Click += On_Rot_X_InputField_SubBtn_Click;
            Rot_X_InputField.on_MouseControlInputFieldTextChange += on_MouseControlInputFieldTextChange_Rot_X;
            Rot_X_InputField.on_AddButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Rot_X_AddButton;
            Rot_X_InputField.on_SubButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Rot_X_SubButton;

            Rot_Y_InputField.on_AddButton_Click += On_Rot_Y_InputField_AddBtn_Click;
            Rot_Y_InputField.on_SubButton_Click += On_Rot_Y_InputField_SubBtn_Click;
            Rot_Y_InputField.on_MouseControlInputFieldTextChange += on_MouseControlInputFieldTextChange_Rot_Y;
            Rot_Y_InputField.on_AddButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Rot_Y_AddButton;
            Rot_Y_InputField.on_SubButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Rot_Y_SubButton;

            Rot_Z_InputField.on_AddButton_Click += On_Rot_Z_InputField_AddBtn_Click;
            Rot_Z_InputField.on_SubButton_Click += On_Rot_Z_InputField_SubBtn_Click;
            Rot_Z_InputField.on_MouseControlInputFieldTextChange += on_MouseControlInputFieldTextChange_Rot_Z;
            Rot_Z_InputField.on_AddButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Rot_Z_AddButton;
            Rot_Z_InputField.on_SubButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Rot_Z_SubButton;

            Scale_X_InputField.on_AddButton_Click += On_Scale_X_InputField_AddBtn_Click;
            Scale_X_InputField.on_SubButton_Click += On_Scale_X_InputField_SubBtn_Click;
            Scale_X_InputField.on_MouseControlInputFieldTextChange += on_MouseControlInputFieldTextChange_Scale_X;
            Scale_X_InputField.on_AddButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Scale_X_AddButton;
            Scale_X_InputField.on_SubButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Scale_X_SubButton;

            Scale_Y_InputField.on_AddButton_Click += On_Scale_Y_InputField_AddBtn_Click;
            Scale_Y_InputField.on_SubButton_Click += On_Scale_Y_InputField_SubBtn_Click;
            Scale_Y_InputField.on_MouseControlInputFieldTextChange += on_MouseControlInputFieldTextChange_Scale_Y;
            Scale_Y_InputField.on_AddButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Scale_Y_AddButton;
            Scale_Y_InputField.on_SubButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Scale_Y_SubButton;

            Scale_Z_InputField.on_AddButton_Click += On_Scale_Z_InputField_AddBtn_Click;
            Scale_Z_InputField.on_SubButton_Click += On_Scale_Z_InputField_SubBtn_Click;
            Scale_Z_InputField.on_MouseControlInputFieldTextChange += on_MouseControlInputFieldTextChange_Scale_Z;
            Scale_Z_InputField.on_AddButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Scale_Z_AddButton;
            Scale_Z_InputField.on_SubButton_Down_EveryFrame += on_AddButton_Down_EveryFrame_Scale_Z_SubButton;

           

        }

       

        #region//位移X
        private void On_Pos_X_InputField_AddBtn_Click(PointerEventData eventData)
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x + pos_increment, controlModel.localPosition.y, controlModel.localPosition.z);
        }

        private void On_Pos_X_InputField_SubBtn_Click(PointerEventData eventData)
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x -pos_increment, controlModel.localPosition.y, controlModel.localPosition.z);
        }
        private void on_AddButton_Down_EveryFrame_Pos_X_SubButton()
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x - pos_increment, controlModel.localPosition.y, controlModel.localPosition.z);
        }

        private void on_AddButton_Down_EveryFrame_Pos_X_AddButton()
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x + pos_increment, controlModel.localPosition.y, controlModel.localPosition.z);
        }
        private void on_MouseControlInputFieldTextChange_Pos_X(float f)
        {
            controlModel.localPosition = new Vector3(f, controlModel.localPosition.y, controlModel.localPosition.z);
        }
        #endregion

        #region//位移Y
        private void On_Pos_Y_InputField_AddBtn_Click(PointerEventData eventData)
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x, controlModel.localPosition.y + pos_increment, controlModel.localPosition.z);
        }

        private void On_Pos_Y_InputField_SubBtn_Click(PointerEventData eventData)
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x, controlModel.localPosition.y - pos_increment, controlModel.localPosition.z);
        }
        private void on_AddButton_Down_EveryFrame_Pos_Y_SubButton()
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x, controlModel.localPosition.y - pos_increment, controlModel.localPosition.z);
        }

        private void on_AddButton_Down_EveryFrame_Pos_Y_AddButton()
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x, controlModel.localPosition.y + pos_increment, controlModel.localPosition.z);
        }
       
        private void on_MouseControlInputFieldTextChange_Pos_Y(float f)
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x, f, controlModel.localPosition.z);
        }
        #endregion

        #region//位移Z
        private void On_Pos_Z_InputField_AddBtn_Click(PointerEventData eventData)
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x, controlModel.localPosition.y , controlModel.localPosition.z+ pos_increment);
        }

        private void On_Pos_Z_InputField_SubBtn_Click(PointerEventData eventData)
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x, controlModel.localPosition.y, controlModel.localPosition.z - pos_increment);
        }
        private void on_AddButton_Down_EveryFrame_Pos_Z_AddButton()
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x, controlModel.localPosition.y, controlModel.localPosition.z + pos_increment);
        }

        private void on_AddButton_Down_EveryFrame_Pos_Z_SubButton()
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x, controlModel.localPosition.y, controlModel.localPosition.z - pos_increment);
        }
        private void on_MouseControlInputFieldTextChange_Pos_Z(float f)
        {
            controlModel.localPosition = new Vector3(controlModel.localPosition.x, controlModel.localPosition.y, f);
        }
        #endregion

        #region//旋转X
        private void On_Rot_X_InputField_AddBtn_Click(PointerEventData eventData)
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x+ rot_increment, controlModel.localEulerAngles.y, controlModel.localEulerAngles.z );
        }

        private void On_Rot_X_InputField_SubBtn_Click(PointerEventData eventData)
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x -rot_increment, controlModel.localEulerAngles.y, controlModel.localEulerAngles.z);
        }
        private void on_AddButton_Down_EveryFrame_Rot_X_AddButton()
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x + rot_increment, controlModel.localEulerAngles.y, controlModel.localEulerAngles.z);
        }

        private void on_AddButton_Down_EveryFrame_Rot_X_SubButton()
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x - rot_increment, controlModel.localEulerAngles.y, controlModel.localEulerAngles.z);
        }
        private void on_MouseControlInputFieldTextChange_Rot_X(float f)
        {
            controlModel.localRotation = Quaternion.Euler(f, controlModel.localEulerAngles.y, controlModel.localEulerAngles.z);
        }
        #endregion

        #region//旋转Y
        private void On_Rot_Y_InputField_AddBtn_Click(PointerEventData eventData)
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x, controlModel.localEulerAngles.y + rot_increment, controlModel.localEulerAngles.z);
        }
      
        private void On_Rot_Y_InputField_SubBtn_Click(PointerEventData eventData)
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x, controlModel.localEulerAngles.y- rot_increment, controlModel.localEulerAngles.z);
        }
        private void on_AddButton_Down_EveryFrame_Rot_Y_AddButton()
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x, controlModel.localEulerAngles.y + rot_increment, controlModel.localEulerAngles.z);
        }

        private void on_AddButton_Down_EveryFrame_Rot_Y_SubButton()
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x, controlModel.localEulerAngles.y - rot_increment, controlModel.localEulerAngles.z);
        }
        private void on_MouseControlInputFieldTextChange_Rot_Y(float f)
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x, f, controlModel.localEulerAngles.z);
        }
        #endregion

        #region//旋转Z
        private void On_Rot_Z_InputField_AddBtn_Click(PointerEventData eventData)
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x, controlModel.localEulerAngles.y, controlModel.localEulerAngles.z + rot_increment);
        }

        private void On_Rot_Z_InputField_SubBtn_Click(PointerEventData eventData)
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x, controlModel.localEulerAngles.y, controlModel.localEulerAngles.z- rot_increment);
        }
        private void on_AddButton_Down_EveryFrame_Rot_Z_AddButton()
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x, controlModel.localEulerAngles.y, controlModel.localEulerAngles.z + rot_increment);
        }

        private void on_AddButton_Down_EveryFrame_Rot_Z_SubButton()
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x, controlModel.localEulerAngles.y, controlModel.localEulerAngles.z - rot_increment);
        }
        private void on_MouseControlInputFieldTextChange_Rot_Z(float f)
        {
            controlModel.localRotation = Quaternion.Euler(controlModel.localEulerAngles.x, controlModel.localEulerAngles.y, f);
        }
        #endregion

        #region//缩放X
        private void On_Scale_X_InputField_AddBtn_Click(PointerEventData eventData)
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x+scale_increment, controlModel.localScale.y, controlModel.localScale.z );
        }

        private void On_Scale_X_InputField_SubBtn_Click(PointerEventData eventData)
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x - scale_increment, controlModel.localScale.y, controlModel.localScale.z);
        }
        private void on_AddButton_Down_EveryFrame_Scale_X_AddButton()
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x + scale_increment, controlModel.localScale.y, controlModel.localScale.z);
        }

        private void on_AddButton_Down_EveryFrame_Scale_X_SubButton()
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x - scale_increment, controlModel.localScale.y, controlModel.localScale.z);
        }
        private void on_MouseControlInputFieldTextChange_Scale_X(float f)
        {
            controlModel.localScale = new Vector3(f, controlModel.localScale.y, controlModel.localScale.z);
        }
        #endregion

        #region//缩放Y
        private void On_Scale_Y_InputField_AddBtn_Click(PointerEventData eventData)
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x, controlModel.localScale.y + scale_increment, controlModel.localScale.z);
        }

        private void On_Scale_Y_InputField_SubBtn_Click(PointerEventData eventData)
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x, controlModel.localScale.y - scale_increment, controlModel.localScale.z);
        }
        private void on_AddButton_Down_EveryFrame_Scale_Y_AddButton()
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x, controlModel.localScale.y + scale_increment, controlModel.localScale.z);
        }

        private void on_AddButton_Down_EveryFrame_Scale_Y_SubButton()
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x, controlModel.localScale.y - scale_increment, controlModel.localScale.z);
        }
        private void on_MouseControlInputFieldTextChange_Scale_Y(float f)
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x, f, controlModel.localScale.z);
        }
        #endregion

        #region//缩放Z
        private void On_Scale_Z_InputField_AddBtn_Click(PointerEventData eventData)
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x, controlModel.localScale.y, controlModel.localScale.z + scale_increment);
        }

        private void On_Scale_Z_InputField_SubBtn_Click(PointerEventData eventData)
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x, controlModel.localScale.y, controlModel.localScale.z + scale_increment);
        }
        private void on_AddButton_Down_EveryFrame_Scale_Z_AddButton()
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x, controlModel.localScale.y, controlModel.localScale.z + scale_increment);
        }

        private void on_AddButton_Down_EveryFrame_Scale_Z_SubButton()
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x, controlModel.localScale.y, controlModel.localScale.z - scale_increment);
        }
        private void on_MouseControlInputFieldTextChange_Scale_Z(float f)
        {
            controlModel.localScale = new Vector3(controlModel.localScale.x, controlModel.localScale.y, f);
        }
        #endregion
    }
}
