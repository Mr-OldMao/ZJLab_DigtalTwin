using UnityEngine;
using MFramework;
using System;
/// <summary>
/// 标题：读取配置文件
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.09.18
/// </summary>
public class ReadConfigFile
{
    public ReadConfigFile(Action actionCompleteCallback)
    {
        Read(actionCompleteCallback);
    }

    private void Read(Action actionCompleteCallback)
    {
        FileIOTxt fileIOTxt = new FileIOTxt(Application.streamingAssetsPath, "Config.json");
        fileIOTxt.ReadWebgl<string>((configJson) =>
       {
           MainData.ConfigData = JsonTool.GetInstance.JsonToObjectByLitJson<ConfigData>(configJson);
           actionCompleteCallback?.Invoke();
       });
    }
}
