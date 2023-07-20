using System.Collections.Generic;
using System.Xml.Serialization;

namespace MFramework
{
    /// <summary>
    /// 标题：热更服务器信息配置表
    /// 作者：毛俊峰
    /// 时间：2023.05.26
    /// 版本：1.0
    /// </summary>
    [System.Serializable]
    public class ServserInfo
    {
        [XmlElement("GameVersion")]
        public VersionInfo[] versionInfos;
    }

    /// <summary>
    /// 当前游戏版本对应的所有补丁
    /// </summary>
    [System.Serializable]
    public class VersionInfo
    {
        /// <summary>
        /// 热更包版本号
        /// </summary>
        [XmlAttribute]
        public string vsersion;
        [XmlElement]
        public Patches[] patches;

    }

    /// <summary>
    /// 一个总热更包
    /// </summary>
    [System.Serializable]
    public class Patches
    {
        /// <summary>
        /// 热更包版本号
        /// </summary>
        [XmlElement]
        public string version;
        /// <summary>
        /// 热更更新内容描述
        /// </summary>
        [XmlElement]
        public string des;
        /// <summary>
        /// 当前版本所有热更包信息容器
        /// </summary>
        [XmlElement]
        public List<Patch> files;
    }

    /// <summary>
    /// 单个补丁包信息
    /// </summary>
    [System.Serializable]
    public class Patch
    {
        [XmlAttribute]
        public string abName;
        [XmlAttribute] 
        public string url;
        [XmlAttribute] 
        public string platform;
        [XmlAttribute] 
        public string md5;
        [XmlAttribute] 
        public double size;
    }
}
