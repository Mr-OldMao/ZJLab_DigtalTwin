using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：数据读写接口
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.10.09
    /// 版本：1.0
    /// </summary>
    public abstract class AbFileIO
    {
        public string rootPath;
        public string fileName;
        public string filePath;
        public AbFileIO(string rootPath, string fileName)
        {
            this.rootPath = rootPath;
            this.fileName = fileName;
            this.filePath = rootPath + "/" + fileName;
        }
        public virtual void Write(string content)
        {
            JudgeFilePathExist(true);
        }
        public virtual string Read()
        {
            JudgeFilePathExist();
            return default;
        }

        /// <summary>
        /// 判定文件所在路径是否存在
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="fileName"></param>
        /// <param name="filePathNoExistAutoCreate">不存在则自动生成</param>
        /// <returns></returns>
        private bool JudgeFilePathExist(bool filePathNoExistAutoCreate = false)
        {
            bool isExist = false;
            if (Directory.Exists(rootPath))
            {
                if (File.Exists(filePath))
                {
                    isExist = true;
                }
                else
                {
                    if (filePathNoExistAutoCreate)
                    {
                        using FileStream fileStream = File.Create(filePath);
                        Debug.Log("filePath is Null Auto Create，rootPath：" + filePath);
                    }
                    else
                    {
                        Debug.LogError("filePath is Null，filePath：" + filePath);
                    }
                }
            }
            else
            {
                if (filePathNoExistAutoCreate)
                {
                    Directory.CreateDirectory(rootPath);
                    using FileStream fileStream = File.Create(filePath);
                    Debug.Log("filePath is Null Auto Create，rootPath：" + filePath);
                }
                else
                {
                    Debug.LogError("rootPath is Null，rootPath：" + rootPath);
                }
            }
            return isExist;
        }
    }
}