using UnityEngine;
using UnityEngine.UI;
using MFramework;
using System;
using UnityEngine.Events;

/// <summary>
/// 标题：提示框，无按钮版、UI窗体视图层 
/// 功能：自定义提示的标题、内容，允许全屏遮罩
/// 作者：毛俊峰
/// 时间：2023.11.28
/// </summary>
public class UIFormHintOneBtn : UIFormBase
{
    [SerializeField]
    private Text txtHintContext;
    [SerializeField]
    private Text txtTopContext;
    [SerializeField]
    private Image imgBg;
    [SerializeField]
    private Image imgFullMask;
    [SerializeField]
    private Button btnConfirm;


    public override UILayerType GetUIFormLayer { get => UILayerType.Common; protected set => _ = UILayerType.Common; }
    public override string AssetPath { get => AssetPathRootDir + "/UIFormHintOneBtn.prefab"; protected set => _ = AssetPathRootDir + "/UIFormHintOneBtn.prefab"; }
    public static string AssetPathNew { get => AssetPathRootDir + "/UIFormHintOneBtn.prefab"; protected set => _ = AssetPathRootDir + "/UIFormHintOneBtn.prefab"; }
    public Text TxtHintContext { get => txtHintContext; set => txtHintContext = value; }
    public Text TxtTopContext { get => txtTopContext; set => txtTopContext = value; }
    public Image ImgBg { get => imgBg; set => imgBg = value; }
    public Image ImgFullMask { get => imgFullMask; set => imgFullMask = value; }
    public Button BtnConfirm { get => btnConfirm; set => btnConfirm = value; }


    private Coroutine m_DelayCloseUiFormCor = null;
    protected override void Awake()
    {
        base.Awake();
        InitMapField();
    }

    protected override void InitMapField()
    {
        txtHintContext = transform.Find<Text>("txtHintContext");
        txtTopContext = transform.Find<Text>("txtTopContext");
        imgBg = transform.Find<Image>("imgBg");
        imgFullMask = transform.Find<Image>("imgFullMask");
        btnConfirm = transform.Find<Button>("btnConfirm");
    }

    public class ShowParams
    {
        public string txtHintContent;
        public string txtTopContent = "";
        public string btnConfirmContent = "确认";
        public bool isFullMask = false;
        /// <summary>
        /// 自动关闭窗口的延时时间，小于等于0则不自动关闭窗口
        /// </summary>
        public float delayCloseUIFormTime = 0;
        public Color colorHintContent = Color.white;
        public Color colorTopContent = Color.white;
    }

    public void Show(string txtHintContent, UnityAction btnAction)
    {
        Show(new ShowParams { txtHintContent = txtHintContent }, btnAction);
    }

    public void Show(ShowParams showParams , UnityAction btnAction)
    {
        TxtHintContext.text = showParams.txtHintContent;
        TxtHintContext.color = showParams.colorHintContent;
        TxtTopContext.text = showParams.txtTopContent;
        TxtTopContext.color = showParams.colorTopContent;
        ImgFullMask?.gameObject.SetActive(showParams.isFullMask);
        BtnConfirm.GetComponentInChildren<Text>().text = showParams.btnConfirmContent;
        if (BtnConfirm.onClick != null)
        {
            BtnConfirm.onClick.RemoveAllListeners();
        }
        BtnConfirm.onClick.AddListener(() =>
        {
            btnAction?.Invoke();
            Hide();
        });
        base.Show();
        if (m_DelayCloseUiFormCor != null)
        {
            StopCoroutine(m_DelayCloseUiFormCor);
            m_DelayCloseUiFormCor = null;
        }
        if (showParams.delayCloseUIFormTime > 0)
        {
            m_DelayCloseUiFormCor = UnityTool.GetInstance.DelayCoroutine(showParams.delayCloseUIFormTime, () => base.Hide());
        }
    }



    protected override void RegisterUIEvnet()
    {
    }

}
