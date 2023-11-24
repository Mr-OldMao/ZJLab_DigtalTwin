using System;
using UnityEngine;
using UnityEngine.UI;
namespace MouseControlObj
{

    /// <summary>
    /// 可移动物体控制UI
    /// </summary>
    public class MouseControlUI : MonoBehaviour
    {
        public Action On_CloseButton_ClickAction;

     
        /// <summary>
        /// 控制的模型Transform
        /// </summary>
        private Transform controlModel;
        private MouseControlTransformPanel transformPanel;
        /// <summary>
        /// transform控制面板
        /// </summary>
        private MouseControlTransformPanel TransformPanel
        {
            get
            {
                if (transformPanel == null)
                {
                    transformPanel = transform.FindChildForName("TransformPanel").GetComponent<MouseControlTransformPanel>();
                }
                return transformPanel;
            }
        }
        private Button closeButton;
        /// <summary>
        /// 关闭按钮
        /// </summary>
        private Button CloseButton
        {
            get
            {
                if (closeButton == null)
                {
                    closeButton = transform.FindChildForName("CloseButton").GetComponent<Button>();
                }
                return closeButton;
            }
        }
        private Button deleteButton;
        /// <summary>
        /// 删除按钮
        /// </summary>
        private Button DeleteButton
        {
            get
            {
                if (deleteButton == null)
                {
                    deleteButton = transform.FindChildForName("DeleteButton").GetComponent<Button>();
                }
                return deleteButton;
            }
        }
        private Button refreshButton;
        /// <summary>
        /// 刷新按钮
        /// </summary>
        private Button RefreshButton
        {
            get
            {
                if (refreshButton == null)
                {
                    refreshButton = transform.FindChildForName("RefreshButton").GetComponent<Button>();
                }
                return refreshButton;
            }
        }

      
        private Transform createdModel;
        /// <summary>
        /// 当前的创建模型
        /// </summary>
        private Transform CreatedModel
        {
            get
            {
                return controlModel.FindChildForName("Models").GetChild(0);
            }
        }
        private void OnEnable()
        {

        }
        // Start is called before the first frame update
        void Start()
        {

        }


       
       
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="transformPanelControlModel"></param>
        public void Init(Transform transformPanelControlModel,string currentModelName=null)
        {
            CloseButton.onClick.RemoveAllListeners();
            DeleteButton.onClick.RemoveAllListeners();
            RefreshButton.onClick.RemoveAllListeners();
            TransformPanel.Set_controlModel(transformPanelControlModel);
            TransformPanel.Init();
            controlModel = transformPanelControlModel;
            CloseButton.onClick.AddListener(delegate { On_CloseButton_Click(); });
            DeleteButton.onClick.AddListener(delegate { On_DeleteButton_Click(); });
            RefreshButton.onClick.AddListener(delegate { On_RefreshButton_Click(); });
        }
        private void On_CloseButton_Click()
        {
            gameObject.SetActive(false);
            if (On_CloseButton_ClickAction != null)
            {
                On_CloseButton_ClickAction();
            }
        }

        private void On_DeleteButton_Click()
        {
            Destroy(controlModel.gameObject);
            gameObject.SetActive(false);

        }

        private void On_RefreshButton_Click()
        {
            controlModel.transform.localPosition = Vector3.zero;
             controlModel.transform.localRotation =Quaternion.Euler( Vector3.zero);
            controlModel.transform.localScale = Vector3.one;
        }
   
      

    }
}