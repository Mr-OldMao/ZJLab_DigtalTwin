using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：测试四类资源的同步加载、异步加载、卸载自动回收
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2023.04.21
    /// 版本：1.0
    /// </summary>
    public class TestResManager : MonoBehaviour
    {
        private string pathCube1 = "Assets/GameMain/AB/TestResLoader/Prefab/Cube1.prefab";//不关心大小写，但资源必须要有后缀
        private string pathCube2 = "Assets/GameMain/AB/TestResLoader/Prefab/Cube2.prefab";
        private string pathCube3 = "Assets/GameMain/AB/TestResLoader/Prefab/Cube3.prefab";

        private string pathResouces = "TestResManager/Cube4";  //不允许加后缀
        private void Start()
        {
            Debug.LogError("演示UIResLoader资源的加载、卸载、释放 案例，需要先导入Unity资源包后，再取消注释后续代码即可。UnityPackagePath:Assets/MFramework/Example/AssetsUnityPackage/ExampleAssetsResManager.unitypackage");
            Debug.Log("Q：同步加载编辑器资源，" +
                "A：异步加载编辑器资源，" +
                "Z：回收编辑器资源，" +
                "W：同步加载AB包，" +
                "S：异步加载AB包，" +
                "X：回收AB包资源，" +
                "E：同步加载AB包中具体资源，" +
                "D：异步加载AB包中具体资源，" +
                "C：回收AB包中具体资源" +
                "R：同步加载Reoueces资源，" +
                "F：异步加载Reoueces资源，" +
                "V：回收Reoueces资源");
        }
        private void Update()
        {
            #region Editor
            if (Input.GetKeyDown(KeyCode.Q))
            {
                GameObject go = ResManager.LoadSync<GameObject>(pathCube1, LoadMode.ResEditor);
                go.transform.position = Random.insideUnitSphere;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                ResManager.LoadAsync<GameObject>(pathCube1, (go) =>
                {
                    Instantiate(go, Random.insideUnitSphere, Quaternion.identity);
                }, LoadMode.ResEditor);
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                ResManager.UnLoadAssets(pathCube1);
            }
            #endregion

            #region AssetBundlePack
            if (Input.GetKeyDown(KeyCode.W))
            {
                ResManager.LoadSync<AssetBundle>(pathCube2, LoadMode.ResAssetBundlePack);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                //异步加载AB包
                ResManager.LoadAsync<AssetBundle>(pathCube2, (go) => Instantiate(go, Random.insideUnitSphere, Quaternion.identity), LoadMode.ResAssetBundlePack);
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                //卸载AB包 
                string abPath = LoadResource.ParseAssetPath(pathCube2);
                ResManager.UnLoadAssets(abPath);
            }
            #endregion

            #region AssetBundleAsset
            if (Input.GetKeyDown(KeyCode.E))
            {
                //同步加载AB包中具体资源
                //GameObject go = ResManager.LoadSync<GameObject>(pathCube3, ResType.ResAssetBundleAsset);

                //注意 AssetBundleAsset 资源 可省略不写ResType类型，默认Default根据工程模式来觉得加载方式
                GameObject go = ResManager.LoadSync<GameObject>(pathCube3);

                go.transform.position = Random.insideUnitSphere;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                //异步加载AB包中具体资源
                //ResManager.LoadAsync<GameObject>(pathCube3, (go) => Instantiate(go), ResType.ResAssetBundleAsset);

                //注意 AssetBundleAsset 资源 可省略不写ResType类型，默认Default根据工程模式来觉得加载方式
                ResManager.LoadAsync<GameObject>(pathCube3, (go) => Instantiate(go, Random.insideUnitSphere, Quaternion.identity));
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                string abPath = LoadResource.ParseAssetPath(pathCube3);
                ResManager.UnLoadAssets(abPath);
            }
            #endregion

            #region Resources
            if (Input.GetKeyDown(KeyCode.R))
            {
                GameObject go = ResManager.LoadSync<GameObject>(pathResouces, LoadMode.ResResources);
                go.transform.position = Random.insideUnitSphere;
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                ResManager.LoadAsync<GameObject>(pathResouces, (go) => Instantiate(go), LoadMode.ResResources);
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                ResManager.UnLoadAssets(pathResouces);
            }
            #endregion

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ResLoader.ShowResLogInfo();
                Resources.UnloadUnusedAssets();
            }
        }
    }
}