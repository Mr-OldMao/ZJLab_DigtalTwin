using UnityEngine;
using UnityEngine.UI;
using MFramework;

/// <summary>
/// 标题：UI窗体视图层(当前代码都是通过脚本自动生成的)
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.09.15
/// </summary>
public class UIFormCameraHint : UIFormBase
{
    [SerializeField]
    private Text txtHintCameraMain;
    [SerializeField]
    private Text txtHintCameraRightUp;
    [SerializeField]
    private Text txtHintCameraRightDown;
    [SerializeField]
    private Transform rectPanel;


    public override UILayerType GetUIFormLayer { get => UILayerType.Common; protected set => _ = UILayerType.Top; }
    public override string AssetPath { get => AssetPathRootDir + "/UIFormCameraHint.prefab"; protected set => _ = AssetPathRootDir + "/UIFormCameraHint.prefab"; }
    public Text TxtHintCameraMain { get => txtHintCameraMain; set => txtHintCameraMain = value; }
    public Text TxtHintCameraRightUp { get => txtHintCameraRightUp; set => txtHintCameraRightUp = value; }
    public Text TxtHintCameraRightDown { get => txtHintCameraRightDown; set => txtHintCameraRightDown = value; }
    public Transform RectPanel { get => rectPanel; set => rectPanel = value; }


    protected override void Awake()
    {
        base.Awake();
        InitMapField();
    }

    protected override void Start()
    {
        base.Start();
    }
    protected override void InitMapField()
    {
        txtHintCameraMain = transform.Find<Text>("txtHintCameraMain");
        txtHintCameraRightUp = transform.Find<Text>("txtHintCameraRightUp");
        txtHintCameraRightDown = transform.Find<Text>("txtHintCameraRightDown");
        rectPanel = transform.Find<Transform>("rectPanel");
    }

    protected override void RegisterUIEvnet()
    {
        MsgEvent.RegisterMsgEvent(MsgEventName.ChangeCamera, () =>
        {
            UpdateUITextContent();
        });
    }

    private void UpdateUITextContent()
    {
        SetUITextContent(CameraControl.CameraType.Top, CameraControl.GetInstance.GetCameraLocation(CameraControl.CameraType.Top));
        SetUITextContent(CameraControl.CameraType.Three, CameraControl.GetInstance.GetCameraLocation(CameraControl.CameraType.Three));
        SetUITextContent(CameraControl.CameraType.First, CameraControl.GetInstance.GetCameraLocation(CameraControl.CameraType.First));
        SetUITextContent(CameraControl.CameraType.Free, CameraControl.GetInstance.GetCameraLocation(CameraControl.CameraType.Free));
    }

    /// <summary>
    /// 设置UI文本位置
    /// </summary>
    /// <param name="cameraType"></param>
    /// <param name="index">0不在屏幕上，1-主屏幕，2-右上角，3-右下角</param>
    private void SetUITextContent(CameraControl.CameraType cameraType, int index)
    {
        if (index == 0)
        {
            return;
        }
        string des = string.Empty;
        switch (cameraType)
        {
            case CameraControl.CameraType.Top:
                des = "上帝视角";
                break;
            case CameraControl.CameraType.Three:
                des = "第三人称视角";
                break;
            case CameraControl.CameraType.First:
                des = "第一人称视角";
                break;
            case CameraControl.CameraType.Free:
                des = "自由视角";
                break;

        }
        if (index == 1)
        {
            txtHintCameraMain.text = des;
        }
        else if (index == 2)
        {
            txtHintCameraRightUp.text = des;
        }
        else if (index == 3)
        {
            txtHintCameraRightDown.text = des; ;
        }
    }


    public override void Show()
    {
        base.Show();
    }

}
