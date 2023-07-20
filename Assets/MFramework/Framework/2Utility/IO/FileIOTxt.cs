using System.IO;
using System.Text;
namespace MFramework
{
    /// <summary>
    /// 标题：文本文件读写
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.10.09
    /// 版本：1.0
    /// </summary>
    public class FileIOTxt : AbFileIO
    {
        public FileIOTxt(string rootPath, string fileName) : base(rootPath, fileName) { }
        public override string Read()
        {
            base.Read();
            string res = string.Empty;
            //1 通过File类读取文件
            res = File.ReadAllText(filePath, Encoding.UTF8);
            //Debug.Log("TAB1 " + res);

            ////2 文件流形式读取文档FileStream
            //using FileStream fs1 = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            //byte[] bytes1 = new byte[fs1.Length];
            //fs1.Read(bytes1, 0, bytes1.Length);
            //fs1.Close();
            //res = Encoding.UTF8.GetString(bytes1);
            //Debug.Log("TAB2 " + res);

            ////3 文件流形式读取文档 File类的OpenRead()
            //using FileStream fs = File.OpenRead(filePath);
            //byte[] bytes2 = new byte[fs.Length];
            //fs.Read(bytes2, 0, bytes2.Length);
            //fs.Close();
            //res = Encoding.UTF8.GetString(bytes2);
            //Debug.Log("TAB3 " + res);
            return res;
        }

        public override void Write(string content)
        {
            base.Write(content);
            //1.通过文件流的形式写入数据
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write);
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();

            ////2.通过流形式写入数据
            //using StreamWriter sr = new StreamWriter(filePath);
            //sr.WriteLine(content);
            //sr.Close();
            //sr.Dispose();
            //Debug.Log(content);

#if UNITY_EDITOR
            AssetDatabaseTool.Refresh();
#endif
        }
    }
}