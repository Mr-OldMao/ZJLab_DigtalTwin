using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework;
public class UIFormTest : UIFormBase
{
    public void Test()
    {
        Debug.Log("UIFormTest Test()");
    }

    public override void Show()
    {
        base.Show();
        Debug.Log("UIFormTest Show()");
    }
}
