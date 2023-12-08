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
    [SerializeField]
    private Button btnSave;
    [SerializeField]
    private Toggle tgeLive;
    [SerializeField]
    private Toggle tgeEdit;
    [SerializeField]
    private ToggleGroup rectTgeEditTypeGroup;
    [SerializeField]
    private Toggle tgePos;
    [SerializeField]
    private Toggle tgeRot;
    [SerializeField]
    private Toggle tgeScale;
    [SerializeField]
    private Button btnEditReset;

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
    public Button BtnSave { get => btnSave; set => btnSave = value; }
    public Toggle TgeLive { get => tgeLive; set => tgeLive = value; }
    public Toggle TgeEdit { get => tgeEdit; set => tgeEdit = value; }
    public ToggleGroup RectTgeEditTypeGroup { get => rectTgeEditTypeGroup; set => rectTgeEditTypeGroup = value; }
    public Toggle TgePos { get => tgePos; set => tgePos = value; }
    public Toggle TgeRot { get => tgeRot; set => tgeRot = value; }
    public Toggle TgeScale { get => tgeScale; set => tgeScale = value; }
    public Button BtnEditReset { get => btnEditReset; set => btnEditReset = value; }



    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        txtCameraFree.text = "开启自由视角";
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
        btnSave = transform.Find<Button>("btnSave");
        tgeLive = transform.Find<Toggle>("tgeLive");
        tgeEdit = transform.Find<Toggle>("tgeEdit");
        rectTgeEditTypeGroup = transform.Find<ToggleGroup>("rectTgeEditTypeGroup");
        tgePos = transform.Find<Toggle>("tgePos");
        tgeRot = transform.Find<Toggle>("tgeRot");
        tgeScale = transform.Find<Toggle>("tgeScale");
        btnEditReset = transform.Find<Button>("btnEditReset");

        tgeLive.isOn = false;
        tgeEdit.isOn = false;
        rectTgeEditTypeGroup.gameObject.SetActive(false);
        tgeEdit.gameObject.SetActive(false);
    }

    protected override void RegisterUIEvnet()
    {
        BtnStart.onClick.AddListenerCustom(() =>
        {
            OnClickStateBtn(ProgramState.start);
        });
        btnStop.onClick.AddListenerCustom(() =>
        {
            OnClickStateBtn(ProgramState.stop);
        });
        btnPause.onClick.AddListenerCustom(() =>
        {
            OnClickStateBtn(ProgramState.pause);
        });
        btnResume.onClick.AddListenerCustom(() =>
        {
            OnClickStateBtn(ProgramState.resume);
        });
        btnCameraFree.onClick.AddListenerCustom(() =>
        {
            CameraControl.GetInstance.ClickCameraFree();
            bool freeCamState = CameraControl.GetInstance.GetCameraEntity(CameraControl.CameraType.Free).gameObject.activeSelf;
            tgeEdit.gameObject.SetActive(freeCamState);
            if (!freeCamState)
            {
                SelectObjByMouse.GetInstance.CanSelect = false;
            }
            if (freeCamState && tgeEdit.isOn)
            {
                SelectObjByMouse.GetInstance.CanSelect = true;
            }
        });
        btnRobotRelocation.onClick.AddListenerCustom(() =>
        {
            GameLogic.GetInstance.GenerateRobot();
        });
        btnRegenerateScene.onClick.AddListenerCustom(() =>
        {
            UIManager.GetInstance.GetUIFormLogicScript<UIFormHintNotBtn>().Show(new UIFormHintNotBtn.ShowParams
            {
                txtHintContent = "正在生成场景，请稍等...",
                delayCloseUIFormTime = -1
            });
            GameLogic.GetInstance.GenerateScene();
        });
        btnSave.onClick.AddListenerCustom(() =>
        {
            DataSave.GetInstance.Save(() =>
            {
                //UIManager.GetInstance.GetUIFormLogicScript<UIFormHintNotBtn>().Show(new UIFormHintNotBtn.ShowParams
                //{
                //    txtHintContent = "存档成功!"
                //});
                Time.timeScale = 0;
                UIManager.GetInstance.GetUIFormLogicScript<UIFormHintOneBtn>().Show(new UIFormHintOneBtn.ShowParams
                {
                    txtHintContent = "存档成功！即将刷新页面",
                    btnConfirmContent = "确认",
                    isFullMask = true,
                }, () =>
                {
                    Debugger.Log("尝试刷新页面");
#if UNITY_WEBGL && !UNITY_EDITOR
                        MqttWebglCenter.GetInstance.RefreshWeb();
#endif
                });
            });
        });
        tgeLive.onValueChanged.AddListenerCustom((ison) =>
        {
            LiveStreaming.GetInstance.IsBeginLiveStreaming = ison;
        });
        tgeEdit.onValueChanged.AddListenerCustom((ison) =>
        {
            rectTgeEditTypeGroup.gameObject.SetActive(ison);
            SelectObjByMouse.GetInstance.CanSelect = tgeEdit.isOn;
        });
        tgePos.onValueChanged.AddListenerCustom((ison) =>
        {
            if (ison)
            {
                SelectObjByMouse.GetInstance.SetPos();
            }
        });
        TgeRot.onValueChanged.AddListenerCustom((ison) =>
        {
            if (ison)
            {
                SelectObjByMouse.GetInstance.SetRot();
            }
        });
        tgeScale.onValueChanged.AddListenerCustom((ison) =>
        {
            if (ison)
            {
                SelectObjByMouse.GetInstance.SetScale();
            }
        });
        btnEditReset.onClick.AddListenerCustom(() =>
        {
            SelectObjByMouse.GetInstance.ResetPos();
        });
    }

    public void OnClickStateBtn(ProgramState programState, string id = "")
    {
        if (string.IsNullOrEmpty(id))
        {
            id = MainData.SceneID;
        }
        InterfaceDataCenter.GetInstance.ChangeProgramState(id, programState);
        switch (programState)
        {
            case ProgramState.start:
                TaskCenter.GetInstance.CanExecuteTask = true;
                break;
            case ProgramState.pause:
                Time.timeScale = 0;
                break;
            case ProgramState.resume:
                Time.timeScale = 1;
                break;
            case ProgramState.stop:
                MsgEvent.SendMsg(MsgEventName.RobotMoveEnd);
                TaskCenter.GetInstance.CanExecuteTask = false;
                TaskCenter.GetInstance.StopTask();
                //FindObjectOfType<AIRobotMove>().
                break;
        }
    }
}
