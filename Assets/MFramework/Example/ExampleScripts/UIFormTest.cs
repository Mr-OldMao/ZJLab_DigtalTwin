using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework;
public class UIFormTest : UIFormBase
{
    public override UILayerType GetUIFormLayer
    {
        get => UILayerType.Common;
        protected set => _ = UILayerType.Common;
    }
    public override string AssetPath
    {
        get => AssetPathRootDir + "/Main/UIFormTest.prefab";
        protected set => _ = AssetPathRootDir + "/Main/UIFormTest.prefab";
    }

    public void Test()
    {
        Debug.Log("UIFormTest Test()");
    }

    public override void Show()
    {
        base.Show();
        Debug.Log("UIFormTest Show()");
    }
    public override void InitMapField()
    {
        
    }
    protected override void RegisterUIEvnet()
    {
        
    }
}
