using UnityEngine;
using UnityEngine.UI;
using MFramework;

/// <summary>
/// 以下代码都是通过脚本自动生成的
/// 时间:2023.09.01
/// </summary>
public class UIFormMain : UIFormBase
{
    public override UILayerType GetUIFormLayer { get => UILayerType.Common; protected set => _ = UILayerType.Common; }
    public override string AssetPath { get => AssetPathRootDir + "/UIFormMain.prefab"; protected set => _ = AssetPathRootDir + "/UIFormMain.prefab"; }

    [SerializeField]
    private Button btnStart;
    [SerializeField]
    private Button btnStop;
    [SerializeField]
    private Button btnPause;
    [SerializeField]
    private Button btnResume;
    [SerializeField]
    private Image imgBg;
    [SerializeField]
    private Image rectProgramStateGroup;
    [SerializeField]
    private Button btnCameraFree;
    [SerializeField]
    private Text txtCameraFree;
    [SerializeField]
    private Button btnRobotRelocation;
    [SerializeField]
    private Button btnRegenerateScene;

    public Button BtnStart { get => btnStart; set => btnStart = value; }
    public Button BtnStop { get => btnStop; set => btnStop = value; }
    public Button BtnPause { get => btnPause; set => btnPause = value; }
    public Button BtnResume { get => btnResume; set => btnResume = value; }
    public Image ImgBg { get => imgBg; set => imgBg = value; }
    public Image RectProgramStateGroup { get => rectProgramStateGroup; set => rectProgramStateGroup = value; }
    public Button BtnCameraFree { get => btnCameraFree; set => btnCameraFree = value; }
    public Text TxtCameraFree { get => txtCameraFree; set => txtCameraFree = value; }
    public Button BtnRobotRelocation { get => btnRobotRelocation; set => btnRobotRelocation = value; }
    public Button BtnRegenerateScene { get => btnRegenerateScene; set => btnRegenerateScene = value; }


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        txtCameraFree.text = "切换自由视角";
    }

    protected override void InitMapField()
    {
        btnStart = transform.Find<Button>("btnStart");
        btnStop = transform.Find<Button>("btnStop");
        btnPause = transform.Find<Button>("btnPause");
        btnResume = transform.Find<Button>("btnResume");
        imgBg = transform.Find<Image>("imgBg");
        rectProgramStateGroup = transform.Find<Image>("rectProgramStateGroup");
        btnCameraFree = transform.Find<Button>("btnCameraFree");
        txtCameraFree = transform.Find<Text>("txtCameraFree");
        btnRobotRelocation = transform.Find<Button>("btnRobotRelocation");
        btnRegenerateScene = transform.Find<Button>("btnRegenerateScene");
    }

    protected override void RegisterUIEvnet()
    {
        BtnStart.onClick.AddListenerCustom(() =>
        {
            InterfaceDataCenter.GetInstance.ChangeProgramState(MainData.ID, ProgramState.start);
        });
        btnStop.onClick.AddListenerCustom(() =>
        {
            InterfaceDataCenter.GetInstance.ChangeProgramState(MainData.ID, ProgramState.stop);
        });
        btnPause.onClick.AddListenerCustom(() =>
        {
            InterfaceDataCenter.GetInstance.ChangeProgramState(MainData.ID, ProgramState.pause);
            Time.timeScale = 0;
        });
        btnResume.onClick.AddListenerCustom(() =>
        {
            InterfaceDataCenter.GetInstance.ChangeProgramState(MainData.ID, ProgramState.resume);
            Time.timeScale = 1;
        });
        btnCameraFree.onClick.AddListenerCustom(() =>
        {
            CameraControl.GetInstance.ClickCameraFree();
        });
        btnRobotRelocation.onClick.AddListenerCustom(() =>
        {
            GameLogic.GetInstance.GenerateRobot();
        });
        btnRegenerateScene.onClick.AddListenerCustom(() =>
        {
            GameLogic.GetInstance.GenerateScene();
        });
    }
}
