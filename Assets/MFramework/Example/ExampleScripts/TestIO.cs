using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：测试IO流，文件写入与读取
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.
    /// 版本：1.0
    /// </summary>
    public class TestIO : MonoBehaviour
    {
        private void Start()
        {
            string path = Application.streamingAssetsPath+"/TestIO";
            AbFileIO io1 = new FileIOTxt(path,"fileTxt.txt");
            io1.Write("txt111111");
            io1.Read();

            AbFileIO io2 = new FileIOTxt(path, "file.json");
            io2.Write("json222222");
            io2.Read();
        }
    }
}