using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework
{
    /// <summary>
    /// 标题：测试音频管理器
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.07.17
    /// 版本：1.0
    /// </summary>
    public class TestAudioManager : MonoBehaviour
    {
        private void Start()
        {
            Debug.LogError("演示AudioManager音频管理器，需要先导入Unity资源包后，再取消注释后续代码即可。UnityPackagePath:Assets/MFramework/Example/AssetsUnityPackage/ExampleAssetsAudioManager.unitypackage");


            Debug.Log("规则");
            Debug.Log("按下Q：背景音乐播放");
            Debug.Log("按下W：背景音乐暂停播放");
            Debug.Log("按下E：背景音乐继续播放");
            Debug.Log("按下R：背景音乐静音");
            Debug.Log("按下T：背景音乐取消静音");

            Debug.Log("按下A：音效播放(重复按下，之前音效未播放完毕，当前音效会覆盖前者音效)");
            Debug.Log("按下S：音效暂停播放");
            Debug.Log("按下D：音效继续播放");
            Debug.Log("按下F：音效静音");
            Debug.Log("按下G：音效取消静音");

            Debug.Log("按下Z：临时音效播放(重复按下，之前音效未播放完毕，当前音效不会覆盖前者音效)");
            Debug.Log("按下X：临时音效暂停播放");
            Debug.Log("按下C：临时音效继续播放");
            Debug.Log("按下V：临时音效静音");
            Debug.Log("按下B：临时音效取消静音");
        }

        float width = Screen.width;
        float height = Screen.height;
        float xMin;
        float yMin;
        private void OnGUI()
        {
            xMin = 0.5f * Screen.width - width / 2;
            yMin = 0.5f * Screen.height - height / 2;
            Rect rectCenter = new Rect(xMin, yMin, width, height);
            GUI.Box(rectCenter, "规则");

            GUI.Label(new Rect(xMin + 50, yMin + 50, width, height), 
                "按下Q：背景音乐播放" +
                "\n按下W：背景音乐暂停播放" +
                "\n按下E：背景音乐继续播放" +
                "\n按下R：背景音乐静音"+
                "\n按下T：背景音乐取消静音");
            GUI.Label(new Rect(xMin + 50, yMin + 150, width, height),
                 "按下A：音效播放(重复按下，之前音效未播放完毕，当前音效会覆盖前者音效)" +
                 "\n按下S：音效暂停播放" +
                 "\n按下D：音效继续播放" +
                 "\n按下F：音效静音" +
                 "\n按下G：音效取消静音");
            GUI.Label(new Rect(xMin + 50, yMin + 250, width, height),
                 "按下Z：临时音效播放(重复按下，之前音效未播放完毕，当前音效不会覆盖前者音效)" +
                 "\n按下X：临时音效暂停播放" +
                 "\n按下C：临时音效继续播放" +
                 "\n按下V：临时音效静音" +
                 "\n按下B：临时音效取消静音");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                AudioManager.GetInstance.Play(AudioManager.SoundType.BGM, Resources.Load<AudioClip>("TestRes/Audio/bgm1"));
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                AudioManager.GetInstance.Pause(AudioManager.SoundType.BGM);

            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                AudioManager.GetInstance.Play(AudioManager.SoundType.BGM);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                AudioManager.GetInstance.SetPlayerMute(AudioManager.SoundType.BGM, true);
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                AudioManager.GetInstance.SetPlayerMute(AudioManager.SoundType.BGM, false);
            }


            if (Input.GetKeyDown(KeyCode.A))
            {
                AudioManager.GetInstance.Play(AudioManager.SoundType.SoundEffect, Resources.Load<AudioClip>("TestRes/Audio/effJumpScene"));
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                AudioManager.GetInstance.Pause(AudioManager.SoundType.SoundEffect);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                AudioManager.GetInstance.Play(AudioManager.SoundType.SoundEffect);
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                AudioManager.GetInstance.SetPlayerMute(AudioManager.SoundType.SoundEffect, true);
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                AudioManager.GetInstance.SetPlayerMute(AudioManager.SoundType.SoundEffect, false);
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                AudioManager.GetInstance.Play(AudioManager.SoundType.SoundEffectTemp, Resources.Load<AudioClip>("TestRes/Audio/effJumpScene"));
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                AudioManager.GetInstance.Pause(AudioManager.SoundType.SoundEffectTemp);
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                AudioManager.GetInstance.Play(AudioManager.SoundType.SoundEffectTemp);
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                AudioManager.GetInstance.SetPlayerMute(AudioManager.SoundType.SoundEffectTemp, true);
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                AudioManager.GetInstance.SetPlayerMute(AudioManager.SoundType.SoundEffectTemp, false);
            }
        }
    }
}