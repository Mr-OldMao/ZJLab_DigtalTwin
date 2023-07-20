#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
public class AssetDatabaseTool : MonoBehaviour
{
    public static void Refresh()
    {
        AssetDatabase.Refresh();
    }

    public static string[] GetAllAssetBundleNames()
    {
        return AssetDatabase.GetAllAssetBundleNames();
    }

    public static T LoadAssetAtPath<T>(string assetPath) where T : UnityEngine.Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(assetPath);
    }
}
#endif
