using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Xml.Serialization;
using UnityEngine;
/// <summary>
/// 标题：热更工具类，为打热更包服务提供api
/// 功能：AB包核心数据写入：为AB文件添加版本号信息、MD5码信息等，生成二进制、xml、json文件，自动写入到本地
///       AB包核心数据读取：读取指定文件信息
///       MD5文件对比：AB文件的MD5信息对比，返回差异列表即：热更列表
///       生成服务器配置表
///       文件序列化写入
/// </summary>
namespace MFramework
{
    public class HotUpdateTool
    {
        /// <summary>
        /// 写入文件格式
        /// </summary>
        public enum WriteFileType
        {
            /// <summary>
            /// 二进制
            /// </summary>
            Binary,
            XML,
            Json
        }
        #region 读写MD5



        /// <summary>
        /// MD5信息写入：根据AssetsBundle文件生成MD5码，生成二进制、xml、json文件，自动写入到本地
        /// </summary>
        /// <param name="writeMD5Type">MD5文件格式</param>
        /// <param name="abFileFullPath">AB包存放路径</param>
        /// <param name="exportDirectory">MD5文件导出位置</param>
        public static void WriteMD5(WriteFileType writeMD5Type, string abFileFullPath, string exportDirectory)
        {
            //版本号
            string version = HotUpdateSetting.GetProjectVersion;
            ABMainData abMainData = GetABMainInfo(abFileFullPath);
            if (abMainData == null)
            {
                Debug.LogError("abMainData is null");
                return;
            }

            if (!Directory.Exists(exportDirectory))
            {
                Directory.CreateDirectory(exportDirectory);
            }

            //TODO 后期命名需要考虑各个平台 各个渠道 各个版本
            string targetFileFullPath = exportDirectory + "/" + HotUpdateSetting.fileName_ABMD5 + "_" + version;
            SerializerWriteFile(writeMD5Type, abMainData, targetFileFullPath);
            //打开MD5文件夹
            WindowsTool.OpenDirectory(exportDirectory);
        }

        ///// <summary>
        ///// MD5信息读取：
        ///// </summary>
        ///// <param name="writeMD5Type">MD5文件格式</param>
        //public static ABMainData ReadMD5(WriteFileType writeMD5Type)
        //{
        //    string md5FileFullPath = "";
        //    switch (writeMD5Type)
        //    {
        //        case WriteFileType.Binary:
        //            md5FileFullPath = RootDirABMD5 + m_AbMD5Name + ".bytes";
        //            break;
        //        case WriteFileType.XML:
        //            md5FileFullPath = RootDirABMD5 + m_AbMD5Name + ".xml";
        //            break;
        //        case WriteFileType.Json:
        //            md5FileFullPath = RootDirABMD5 + m_AbMD5Name + ".json";
        //            break;
        //        default:
        //            break;
        //    }
        //    return ReadMD5(md5FileFullPath);
        //}


        /// <summary>
        /// MD5信息读取：
        /// </summary>
        /// <param name="md5FileFullPath">md5文件位置全路径</param>
        public static ABMainData ReadMD5(string md5FileFullPath)
        {
            ABMainData abMainData = null;
            if (File.Exists(md5FileFullPath))
            {
                string content = string.Empty;
                using (StreamReader sr = new StreamReader(md5FileFullPath))
                {
                    content = sr.ReadToEnd();
                }
                if (!string.IsNullOrEmpty(content))
                {
                    //识别MD5文件类型
                    if (md5FileFullPath.EndsWith(".json"))
                    {
                        abMainData = JsonTool.GetInstance.JsonToObjectByLitJson<ABMainData>(content);
                    }
                    else if (md5FileFullPath.EndsWith(".bytes"))
                    {
                        using (FileStream fs = File.OpenRead(md5FileFullPath))
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            abMainData = bf.Deserialize(fs) as ABMainData;
                        }
                    }
                    else if (md5FileFullPath.EndsWith(".xml"))
                    {
                        using (FileStream fs = File.OpenRead(md5FileFullPath))
                        {
                            XmlSerializer xml = new XmlSerializer(typeof(ABMainData));
                            abMainData = xml.Deserialize(fs) as ABMainData;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("md5FileFullPath:" + md5FileFullPath);
            }
            Debug.Log("读取AB包资源信息, version:" + abMainData.version + "，assetsCount:" + abMainData.abInfoList?.Count);
            return abMainData;
        }

        /// <summary>
        /// AB文件的MD5信息对比，返回差异列表即：热更列表
        /// </summary>
        /// <param name="abMD5InfoList">历史AB包的MD5信息</param>
        /// <param name="abBundlePath">AB包存放路径</param>
        /// <returns></returns>
        public static List<ABInfo> MD5Comprae(List<ABInfo> abMD5InfoList, string abBundlePath = "")
        {
            List<ABInfo> res = new List<ABInfo>();

            if (string.IsNullOrEmpty(abBundlePath))
            {
                abBundlePath = Application.streamingAssetsPath;
            }
            //读取各个AB包的MD5
            DirectoryInfo directoryInfo = new DirectoryInfo(abBundlePath);
            FileInfo[] fileInfoArr = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            string curMD5Code = "";
            for (int i = 0; i < fileInfoArr.Length; i++)
            {
                if (!fileInfoArr[i].Name.EndsWith(".meta") && !fileInfoArr[i].Name.EndsWith(".manifest") && !fileInfoArr[i].Name.EndsWith(".json"))
                {
                    //缓存资源信息
                    ABInfo aBMD5Base = new ABInfo();
                    aBMD5Base.Name = fileInfoArr[i].Name;
                    aBMD5Base.Md5 = HotUpdateTool.BuildFileMd5(fileInfoArr[i].FullName);
                    aBMD5Base.Size = fileInfoArr[i].Length / 1024f;


                    //检索同名包名对比差异
                    ABInfo aBInfo = abMD5InfoList.Find((p) => { return fileInfoArr[i].Name == p.Name; });
                    if (aBInfo != null)
                    {
                        curMD5Code = HotUpdateTool.BuildFileMd5(fileInfoArr[i].FullName);
                        if (aBInfo.Md5 != curMD5Code)
                        {
                            res.Add(aBMD5Base);
                        }
                    }
                    else
                    {
                        res.Add(aBMD5Base);
                    }
                }
            }
            Debug.Log("资源MD5码比对 差异包体数量:" + res?.Count);
            foreach (var item in res)
            {
                Debug.Log("bundleName:" + item.Name + "，MD5:" + item.Md5);
            }
            return res;
        }


        /// <summary>
        /// 生成MD5码根据具体文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string BuildFileMd5(string filePath)
        {
            string fileMD5 = null;
            try
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    MD5 md5 = MD5.Create();
                    byte[] fileMD5Bytes = md5.ComputeHash(fileStream);//计算指定Stream对象哈希值
                    fileMD5 = FormatMD5(fileMD5Bytes);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("BuildFileMd5 is file ,e:" + e);
            }
            return fileMD5;
        }

        /// <summary>
        /// 将byte[]转换成字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string FormatMD5(byte[] data)
        {
            return System.BitConverter.ToString(data).Replace("-", "").ToLower();
        }
        #endregion

        #region 写入服务器配置表文件
        /// <summary>
        /// 写入服务器配置表文
        /// <param name="writeMD5Type">MD5文件格式</param>
        /// <param name="abBundlePath">AB包存放路径</param>
        /// <param name="exportDirectory">文件导出位置</param>
        /// </summary>
        public static void WriteServerConfigFile(WriteFileType writeMD5Type, string abBundlePath, string exportDirectory)
        {
            ABMainData abMainData = GetABMainInfo(abBundlePath);
            if (abMainData == null)
            {
                Debug.LogError("abMainData is null");
                return;
            }
            Patches patches = new Patches
            {
                version = "1",//热更版本号
                des = "这个当前热更内容的描述",
                files = new List<Patch>()
            };
            for (int i = 0; i < abMainData.abInfoList.Count; i++)
            {
                Patch patch = new Patch
                {
                    abName = abMainData.abInfoList[i].Name,
                    md5 = abMainData.abInfoList[i].Md5,
                    platform = "PC",//TODO
                    size = abMainData.abInfoList[i].Size,
                    url = @"\\192.168.18.188\HotUpdate\" + patches.version + @"\" + abMainData.abInfoList[i].Name   //TODO
                };
                patches.files.Add(patch);
            }
            //写入文件
            string targetFileFullPath = exportDirectory + "/" + HotUpdateSetting.fileName_HotUpdateConfig + "_" + HotUpdateSetting.GetProjectVersion;
            SerializerWriteFile(writeMD5Type, patches, targetFileFullPath);
        }


        /// <summary>
        /// 获取AB包信息
        /// </summary>
        /// <param name="abFileFullPath">ab包文件全路径</param>
        private static ABMainData GetABMainInfo(string abFileFullPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(abFileFullPath);
            FileInfo[] fileInfoArr = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            ABMainData abMainData = new ABMainData();
            abMainData.version = HotUpdateSetting.GetProjectVersion;
            abMainData.abInfoList = new List<ABInfo>();
            for (int i = 0; i < fileInfoArr.Length; i++)
            {
                if (fileInfoArr[i].Name.EndsWith(".meta") || fileInfoArr[i].Name.EndsWith(".manifest") || fileInfoArr[i].Name.EndsWith(".json") || fileInfoArr[i].Name.EndsWith(".xml"))
                {
                    continue;
                }
                ABInfo aBMD5Base = new ABInfo();
                aBMD5Base.Name = fileInfoArr[i].Name;
                aBMD5Base.Md5 = HotUpdateTool.BuildFileMd5(fileInfoArr[i].FullName);
                aBMD5Base.Size = fileInfoArr[i].Length / 1024f;
                abMainData.abInfoList.Add(aBMD5Base);
            }
            return abMainData;
        }

        /// <summary>
        /// 序列化文件
        /// </summary>
        /// <param name="writeFileType">写入的文件类型</param>
        /// <param name="data">被序列化的数据对象</param>
        /// <param name="targetFileFullPath">文件全路径 无需后缀名</param>
        private static void SerializerWriteFile(WriteFileType writeFileType, object data, string targetFileFullPath)
        {
            if (data == null)
            {
                Debug.LogError("Create File File, data is null");
                return;
            }
            if (string.IsNullOrEmpty(targetFileFullPath))
            {
                Debug.LogError("Create File File, targetFileFullPath is null");
                return;
            }
            Debug.Log("Create File Complete，FilePath：" + targetFileFullPath + ",writeFileType：" + writeFileType);

            //序列化文件信息
            switch (writeFileType)
            {
                case WriteFileType.Binary:
                    //二进制存储  //原文链接：https://blog.csdn.net/weixin_43673589/article/details/117996732
                    BinaryFormatter bf = new BinaryFormatter();
                    targetFileFullPath += ".bytes";
                    using (FileStream fs1 = File.Create(targetFileFullPath))
                    {
                        bf.Serialize(fs1, data);
                    }
                    break;
                case WriteFileType.XML:
                    //XML格式存储
                    targetFileFullPath += ".xml";
                    using (FileStream fs2 = new FileStream(targetFileFullPath, FileMode.OpenOrCreate))
                    {
                        XmlSerializer xml = new XmlSerializer(data.GetType());
                        xml.Serialize(fs2, data);
                    }
                    break;
                case WriteFileType.Json:
                    //JSON格式存储
                    targetFileFullPath += ".json";
                    using (StreamWriter sw = new StreamWriter(targetFileFullPath))
                    {
                        string json = MFramework.JsonTool.GetInstance.ObjectToJsonStringByLitJson(data);
                        sw.Write(json);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion
    }


}
