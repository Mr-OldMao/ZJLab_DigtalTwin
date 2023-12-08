using UnityEngine;
using UnityEngine.UI;
using MFramework;

/// <summary>
/// 标题：UI窗体视图层(当前代码都是通过脚本自动生成的)
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.12.08
/// </summary>
public class UIFormScene2 : UIFormBase
{
    [SerializeField]
	private Button btnCamFree;
	[SerializeField]
	private Button btnCamTop;
	[SerializeField]
	private Button btnCamFixed;
	[SerializeField]
	private Transform rectBtnGroup;
	
    
    public override UILayerType GetUIFormLayer { get => UILayerType.Common; protected set => _ = UILayerType.Common; }
    public override string AssetPath { get => AssetPathRootDir + "/UIFormScene2.prefab"; protected set => _ = AssetPathRootDir + "/UIFormScene2.prefab"; }
    public static string AssetPathNew { get => AssetPathRootDir + "/UIFormScene2.prefab"; protected set => _ = AssetPathRootDir + "/UIFormScene2.prefab"; }
    public Button BtnCamFree { get => btnCamFree; set => btnCamFree = value; }
	public Button BtnCamTop { get => btnCamTop; set => btnCamTop = value; }
	public Button BtnCamFixed { get => btnCamFixed; set => btnCamFixed = value; }
	public Transform RectBtnGroup { get => rectBtnGroup; set => rectBtnGroup = value; }
	

    protected override void Awake()
    {
        base.Awake();
        InitMapField();
    }
    
    protected override void InitMapField()
	{
		btnCamFree = transform.Find<Button>("btnCamFree");
		btnCamTop = transform.Find<Button>("btnCamTop");
		btnCamFixed = transform.Find<Button>("btnCamFixed");
		rectBtnGroup = transform.Find<Transform>("rectBtnGroup");
}
	
	protected override void RegisterUIEvnet()
	{
 		btnCamFree.onClick.AddListenerCustom(() =>
		{
			Debug.Log("button click btnCamFree");
			CameraGroupScene2.GetInstance.ShowCamera(CameraGroupScene2.CameraType.Main);

		});
		btnCamTop.onClick.AddListenerCustom(() =>
		{
			Debug.Log("button click btnCamTop");
            CameraGroupScene2.GetInstance.ShowCamera(CameraGroupScene2.CameraType.Top);
        });
		btnCamFixed.onClick.AddListenerCustom(() =>
		{
			Debug.Log("button click btnCamFixed");
            CameraGroupScene2.GetInstance.ShowCamera(CameraGroupScene2.CameraType.Fixed);
        });
	}

	public void Init()
	{
        btnCamFree?.onClick?.Invoke();

    }
	
}
