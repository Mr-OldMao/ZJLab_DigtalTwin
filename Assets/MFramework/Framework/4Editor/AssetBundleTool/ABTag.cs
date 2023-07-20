using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：AB资源标签一键标记，自动对资源进行AB标记
    /// 功能：对指定目录下的资源自动标记相应的AssetBundleName
    /// 作者：毛俊峰
    /// 时间：2022.09.28
    /// 版本：1.0
    /// </summary>
    public class ABTag : MonoBehaviour
    {
#if UNITY_EDITOR
        /// <summary>
        /// 自动对资源进行AB标记
        /// </summary>
        public static void AutoTagAB()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            // 标记AB，本地根路径
            string assetsDataPathRoot = Application.dataPath + "/" + ABSetting.assetsSubPath;
            DirectoryInfo dir = new DirectoryInfo(assetsDataPathRoot);
            DirectoryInfo[] DirSons = dir.GetDirectories();            
            foreach (FileSystemInfo fileSystemInfo in DirSons)
            {
                string SonsPath = assetsDataPathRoot + "/" + fileSystemInfo.Name;
                DirectoryInfo Dinfo = new DirectoryInfo(SonsPath);
                if (Dinfo != null)
                {
                    AssetBundleFilesystemInfo(Dinfo, SonsPath);
                }
            }
            Debug.Log("ABName自动标记成功");
        }

        /// <summary>
        /// 迭代读取子文件夹
        /// </summary>
        /// <param name="filesysteminfo">文件系统</param>
        /// <param name="AbPathName">ab包的路径名称</param>
        public static void AssetBundleFilesystemInfo(FileSystemInfo filesysteminfo, string AbPathName)
        {
            //判断文件系统是否为空
            if (!filesysteminfo.Exists)
            {
                Debug.LogError("该文件夹为空" + filesysteminfo.FullName + "is null");
                return;
            }
            //标记文件夹abname
            //SetAssetBundleName(filesysteminfo);
            //Debug.Log("AbPathName：" + AbPathName + ",fileinfo.FullName:" + filesysteminfo.FullName);

            //如果继续则是找到了文件系统
            //获取该文件系统（实例化，这是二级文件夹下的文件系统）
            DirectoryInfo Dire_02 = new DirectoryInfo(AbPathName);
            FileSystemInfo[] fileinfo = Dire_02.GetFileSystemInfos();                          //这里是二级文件夹
            FileInfo file;
            string patheNAME = AbPathName;
            //遍历二级文件夹 下所有的文件系统
            foreach (FileSystemInfo sysinfo_03 in fileinfo)
            {
                DirectoryInfo dirinfo = sysinfo_03 as DirectoryInfo;
                file = sysinfo_03 as FileInfo;
                if (file == null)
                {
                    patheNAME = AbPathName + "/" + dirinfo.Name;
                    AssetBundleFilesystemInfo(dirinfo, patheNAME);
                    continue;
                }
                SetAssetBundleName(file, AbPathName);
            }
        }

        /// <summary>
        /// 标记文件夹的ABName
        /// </summary>
        /// <param name="fileSystemInfo"></param>
        public static void SetAssetBundleName(FileSystemInfo fileSystemInfo)
        {
            int index = fileSystemInfo.FullName.IndexOf("Assets");
            string folderPath = fileSystemInfo.FullName.Substring(index);
            AssetImporter ai = AssetImporter.GetAtPath(folderPath);
            ai.assetBundleName = folderPath;
            //Debug.Log("folderPath：" + folderPath + ",path_last:" + folderPath);
        }

        /// <summary>
        /// 标记文件ABName
        /// </summary>
        /// <param name="fileinfo"></param>
        /// <param name="AbPathName"></param>
        public static void SetAssetBundleName(FileInfo fileinfo, string AbPathName)
        {
            if (fileinfo.Extension == ".meta")//这个.meta文件是属于系统自动生成的备份文件，我们不需要，所以写个方法排除掉
            {
                return;
            }
            //拼接包名
            int indexs = AbPathName.IndexOf(ABSetting.assetsSubPath);
            AbPathName = AbPathName.Substring(indexs);//去除前缀
            int nameIndex = fileinfo.Name.IndexOf(".");
            string name = fileinfo.Name.Substring(0, nameIndex);//去除后缀
            AbPathName += "/" + name;
            //string[] AbPathNameSplit = AbPathName.Split(".");
            //AbPathName= AbPathName.Replace(AbPathNameSplit[AbPathNameSplit.Length - 1], "");


            //获取到了包名之后，我们就需要开始将文件资源进行设置包名
            //获取文件资源，此时我们只需要Asset后面的路径
            int index = fileinfo.FullName.IndexOf("Assets");
            string path_last = fileinfo.FullName.Substring(index);
            AssetImporter assetImporter = AssetImporter.GetAtPath(path_last);//通过该API获取路径资源(该路径是unity项目内部的路径)
            //设置包名
            assetImporter.assetBundleName = AbPathName;
            //Debug.Log("assetBundleName：" + AbPathName + "，path："+ path_last);



            ////设置后缀
            //if (fileinfo.Extension == ".unity")
            //{
            //    assetImporter.assetBundleVariant = "u3d";
            //}
            //else
            //{
            //    assetImporter.assetBundleVariant = "assetbundle";
            //}
            assetImporter.assetBundleVariant = string.Empty;
        }
#endif
    }
}