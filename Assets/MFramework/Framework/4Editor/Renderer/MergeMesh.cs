#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 描述：合并模型网格 子对象mesh
    /// 用法：在编辑器模式下，在子模型的父对象上挂载此脚本，并右键选择合并网格即可得到合并后的网格
    /// 作者：毛俊峰
    /// 时间：2022.10.18
    /// 版本：1.0
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public  class MergeMesh : MonoBehaviour
{
        public int MaxVertex = 65535;
        public UnityEngine.Rendering.IndexFormat indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        #region 单例
        private static string singletonSceneGameName = "MFrameworkSingletonRoot";
        private static GameObject singletonRoot = null;
        public static MergeMesh m_Instance;
        public static MergeMesh GetInstance
        {
            get
            {
                if (m_Instance == null)
                {
                    if (FindObjectOfType<MergeMesh>())
                    {
                        m_Instance = FindObjectOfType<MergeMesh>();
                    }
                    else
                    {
                        if (singletonRoot == null)
                        {
                            singletonRoot = GameObject.Find(singletonSceneGameName);
                            if (singletonRoot == null)
                            {
                                singletonRoot = new GameObject(singletonSceneGameName);
                                DontDestroyOnLoad(singletonRoot);
                            }
                        }
                        GameObject singletonSubRoot = new GameObject(typeof(MergeMesh).Name);
                        singletonSubRoot.transform.SetParent(singletonRoot.transform);
                        m_Instance = singletonSubRoot.AddComponent<MergeMesh>();
                    }
                }
                return m_Instance;
            }
        } 
        #endregion


        [ContextMenu("合并网格")]
        /// <summary>
        /// 合并网格  合并当前对象以及其下所有模型
        /// </summary>
        public  void MeshCombine()
        {
            MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
            Debug.Log(filters.Length);
            CombineInstance[] combiners = new CombineInstance[filters.Length];
            for (int i = 0; i < filters.Length; i++)
            {
                combiners[i].mesh = filters[i].sharedMesh;
                combiners[i].transform = filters[i].transform.localToWorldMatrix;
                filters[i].GetComponent<MeshRenderer>().enabled = false;
            }
            Mesh finalmesh = new Mesh();
            finalmesh.indexFormat = indexFormat;
            Debug.Log("合并网格后的顶点数：" + combiners.Length + combiners.LongLength);
            finalmesh.CombineMeshes(combiners);

            GetComponent<MeshFilter>().mesh = finalmesh;
            GetComponent<MeshRenderer>().enabled = true;
#if UNITY_EDITOR
            SaveMesh(finalmesh, "newMesh", false, true);
#endif
        }

        [ContextMenu("清空网格")]
        public void ClearMesh()
        {
            GetComponent<MeshFilter>().mesh = null;
            GetComponent<MeshRenderer>().enabled = false;
            MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < filters.Length; i++)
            {
                filters[i].GetComponent<MeshRenderer>().enabled = true;
            }
        }
#if UNITY_EDITOR
        public static void SaveMesh(Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh)
        {
            string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
            if (string.IsNullOrEmpty(path)) return;

            path = FileUtil.GetProjectRelativePath(path);

            Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;

            if (optimizeMesh)
                MeshUtility.Optimize(meshToSave);

            AssetDatabase.CreateAsset(meshToSave, path);
            AssetDatabase.SaveAssets();
        }
#endif
    }
} 
#endif
