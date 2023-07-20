using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：测试加载卸载 AssetsBundle
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.
    /// 版本：1.0
    /// </summary>
    public class TestAssetsBundle : MonoBehaviour
    {
        AssetBundle ab;
        private void Start()
        {
            LoadRes();
        }

        private void LoadRes()
        {
            string buildPath = Application.streamingAssetsPath + "/BuildAssetBundle/";
            ab = AssetBundle.LoadFromFile(buildPath + "abobj");
            GameObject obj = ab.LoadAsset<GameObject>("cube");
            Instantiate(obj);

        }

    }
}