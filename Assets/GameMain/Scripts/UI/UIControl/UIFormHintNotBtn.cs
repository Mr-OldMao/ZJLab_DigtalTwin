using UnityEngine;
using UnityEngine.UI;
using MFramework;

/// <summary>
/// 标题：提示框，无按钮版、UI窗体视图层 
/// 功能：自定义提示的标题、内容，允许全屏遮罩
/// 作者：毛俊峰
/// 时间：2023.11.15
/// </summary>
public class UIFormHintNotBtn : UIFormBase
{
    [SerializeField]
    private Text txtHintContext;
    [SerializeField]
    private Text txtTopContext;
    [SerializeField]
    private Image imgBg;
    [SerializeField]
    private Image imgFullMask;


    public override UILayerType GetUIFormLayer { get => UILayerType.Common; protected set => _ = UILayerType.Common; }
    public override string AssetPath { get => AssetPathRootDir + "/UIFormHintNotBtn.prefab"; protected set => _ = AssetPathRootDir + "/UIFormHintNotBtn.prefab"; }
    public static string AssetPathNew { get => AssetPathRootDir + "/UIFormHintNotBtn.prefab"; protected set => _ = AssetPathRootDir + "/UIFormHintNotBtn.prefab"; }
    public Text TxtHintContext { get => txtHintContext; set => txtHintContext = value; }
    public Text TxtTopContext { get => txtTopContext; set => txtTopContext = value; }
    public Image ImgBg { get => imgBg; set => imgBg = value; }
    public Image ImgFullMask { get => imgFullMask; set => imgFullMask = value; }


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
    }

    public class ShowParams
    {
        public string txtHintContent;
        public string txtTopContent = "";
        public bool isFullMask = true;
        /// <summary>
        /// 自动关闭窗口的延时时间，小于等于0则不自动关闭窗口
        /// </summary>
        public float delayCloseUIFormTime = 2f;
        public Color colorHintContent = Color.white;
        public Color colorTopContent = Color.white;
    }

    public void Show(string txtHintContent)
    {
        Show(new ShowParams { txtHintContent = txtHintContent });
    }

    public void Show(ShowParams showParams)
    {
        TxtHintContext.text = showParams.txtHintContent;
        TxtHintContext.color = showParams.colorHintContent;
        TxtTopContext.text = showParams.txtTopContent;
        TxtTopContext.color = showParams.colorTopContent;

        ImgFullMask?.gameObject.SetActive(showParams.isFullMask);
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
