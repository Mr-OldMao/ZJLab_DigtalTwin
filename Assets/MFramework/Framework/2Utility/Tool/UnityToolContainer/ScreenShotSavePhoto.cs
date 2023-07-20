using System;
using System.IO;
using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 描述：截图保存到手机本地相册
    /// 作者：毛俊峰
    /// 时间：2022.08.16
    /// 版本：1.0
    /// </summary>
    public class ScreenShotSavePhoto : MonoBehaviour
    {
        /// <summary>
        /// 截图并保存图片
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static void ScreenshotSaveImages(Action<string> sucCallback = null)
        {
#if (UNITY_IOS) || UNITY_EDITOR_OSX
        CaptureAndSaveScreen.CallBack = sucCallback;
        CaptureAndSaveScreen.CaptureFullScrenAndSave();
        
    return;
#endif
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();

            string path = string.Empty;
#if UNITY_EDITOR
            path = Application.streamingAssetsPath;
#elif UNITY_ANDROID && !UNITY_EDITOR
        path = "/sdcard/DCIM/Camera"; //设置图片保存到设备的目录.
#elif UNITY_ANDROID && !UNITY_EDITOR
        path = Application.persistentDataPath;
#endif
            path += "/FeiCun3D";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string savePath = path + "/FeiCun3D_" + DateTime.Now.ToString("yyyy-MM-d H-mm-ss") + ".png";
            try
            {
                Application.HasUserAuthorization(UserAuthorization.Microphone);
                byte[] data = DeCompress(texture).EncodeToPNG();
                File.WriteAllBytes(savePath, data);
                OnSaveImagesPlartform(savePath, sucCallback);
            }
            catch
            {

            }
        }
        /// <summary>
        /// 刷新相册（不需要单独创建原生aar或jar）
        /// </summary>
        /// <param name="path"></param>
        private static void OnSaveImagesPlartform(string filePath, Action<string> sucCallback = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            string[] paths = new string[1] { filePath};
            using (AndroidJavaClass PlayerActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject playerActivity = PlayerActivity.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaObject Conn = new AndroidJavaObject("android.media.MediaScannerConnection", playerActivity, null))
                {
                    Conn.CallStatic("scanFile", playerActivity, paths, null, null);
                    sucCallback?.Invoke(filePath);
                }
            }
#endif

        }
        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Texture2D DeCompress(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        /// <summary>
        /// UnityTool工具类扩展-截图并保存图片
        /// </summary>
        public partial class UnityTool
        {
            /// <summary>
            /// 截图并保存图片
            /// </summary>
            public void ScreenshotSaveImages(Action<string> sucCallback = null)
            {
                ScreenShotSavePhoto.ScreenshotSaveImages(sucCallback);
            }

            /// <summary>
            /// 刷新相册（不需要单独创建原生aar或jar）
            /// </summary>
            /// <param name="path"></param>
            private void OnSaveImagesPlartform(string filePath, Action<string> sucCallback = null)
            {
                ScreenShotSavePhoto.OnSaveImagesPlartform(filePath, sucCallback);
            }
            /// <summary>
            /// 压缩图片
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            public Texture2D DeCompress(Texture2D source)
            {
                return ScreenShotSavePhoto.DeCompress(source);
            }
        }
    }
}
