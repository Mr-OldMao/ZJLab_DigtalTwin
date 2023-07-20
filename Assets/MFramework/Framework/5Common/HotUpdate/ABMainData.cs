using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// AB包信息数据结构 用于序列化Ab包数据信息（MD5、版本号）
    /// </summary>
    [System.Serializable]
    public class ABMainData 
    {
        [XmlElement("Version")]
        public string version { get; set; }
        [XmlElement("ABInfoList")]
        public List<ABInfo> abInfoList { get; set; }
    }

    [System.Serializable]
    public class ABInfo
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlAttribute("Md5")]
        public string Md5 { get; set; }
        [XmlAttribute("Size")]
        public double Size { get; set; }
    }
}
