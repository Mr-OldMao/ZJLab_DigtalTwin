using UnityEngine;
using MFramework;
/// <summary>
/// 标题：读取配置文件
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.09.18
/// </summary>
public class ReadConfigFile
{
    public ReadConfigFile()
    {
        Read();
    }

    private void Read()
    {
        FileIOTxt fileIOTxt = new FileIOTxt(Application.streamingAssetsPath, "Config.json");
        string configJson = fileIOTxt.Read();
        MainData.ConfigData = JsonTool.GetInstance.JsonToObjectByLitJson<ConfigData>(configJson);
    }
}
