using UnityEngine;
using UnityEngine.UI;
using MFramework;

/// <summary>
/// 以下代码都是通过脚本自动生成的
/// 时间:2023.09.01
/// </summary>
public class UIFormMain : UIFormBase
{
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
	
    
    public Button BtnStart { get => btnStart; set => btnStart = value; }
	public Button BtnStop { get => btnStop; set => btnStop = value; }
	public Button BtnPause { get => btnPause; set => btnPause = value; }
	public Button BtnResume { get => btnResume; set => btnResume = value; }
	public Image ImgBg { get => imgBg; set => imgBg = value; }
	public Image RectProgramStateGroup { get => rectProgramStateGroup; set => rectProgramStateGroup = value; }
	

    protected override void Awake()
    {
        base.Awake();
        InitMapField();
    }

    
    public void InitMapField()
	{
		btnStart = transform.Find<Button>("btnStart");
		btnStop = transform.Find<Button>("btnStop");
		btnPause = transform.Find<Button>("btnPause");
		btnResume = transform.Find<Button>("btnResume");
		imgBg = transform.Find<Image>("imgBg");
		rectProgramStateGroup = transform.Find<Image>("rectProgramStateGroup");
	}
}
