using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：音频管理器
    /// 功能：播放、暂停、继续播放、设置静音、循环、音量大小   播放器：一个背景音乐播放器、一个音效播放器、若干临时音效播放器（使用对象池动态创建、回收临时音效播放器）
    /// 作者：毛俊峰
    /// 时间：2022.07.16
    /// 版本：1.0
    /// </summary>
    public class AudioManager : SingletonByMono<AudioManager>
    {
        /// <summary>
        /// 背景音乐播放器
        /// </summary>
        private AudioSource m_AudioSourceBGM;
        /// <summary>
        /// 音效播放器
        /// </summary>
        private AudioSource m_AudioSourceEffect;
        /// <summary>
        /// 临时音效播放器对象池
        /// </summary>
        private Pool<AudioSource> m_PoolAudioSourceEffectTemp;

        /// <summary>
        /// 缓存临时音效 播放器设置参数
        /// </summary>
        private AudioSourceInfo AudioSourceEffectTempSetting { get; set; }

        /// <summary>
        /// 音频类型
        /// </summary>
        public enum SoundType
        {
            /// <summary>
            /// 背景音乐
            /// </summary>
            BGM,
            /// <summary>
            /// 常驻音效（唯一）
            /// </summary>
            SoundEffect,
            /// <summary>
            /// 临时音效（多个，可并发播放） 
            /// </summary>
            SoundEffectTemp
        }
        /// <summary>
        /// 播放器参数信息
        /// </summary>
        public class AudioSourceInfo
        {
            /// <summary>
            /// 音量
            /// </summary>
            public float volume;
            /// <summary>
            /// 循环
            /// </summary>
            public bool isLoop;
            /// <summary>
            /// 静音
            /// </summary>
            public bool isMute;
        }

        private void Awake()
        {
            InitAudioSource();
        }
        /// <summary>
        /// 初始化播放器
        /// </summary>
        private void InitAudioSource()
        {
            //BGM
            m_AudioSourceBGM = new GameObject("audioBGM").AddComponent<AudioSource>();
            m_AudioSourceBGM.loop = true;
            m_AudioSourceBGM.transform.SetParent(transform);
            //Eff
            Transform audioSourceEffectTypeGroup = new GameObject("audioSourceEffectTypeGroup").transform;
            audioSourceEffectTypeGroup.SetParent(transform);
            m_AudioSourceEffect = new GameObject("audioSourceEffect").AddComponent<AudioSource>();
            m_AudioSourceEffect.loop = false;
            m_AudioSourceEffect.transform.SetParent(audioSourceEffectTypeGroup);
            AudioSource audioSourceEffectType = new GameObject("audioSourceEffectType").AddComponent<AudioSource>();
            audioSourceEffectType.loop = false;
            audioSourceEffectType.transform.SetParent(audioSourceEffectTypeGroup);
            //EffTemp
            m_PoolAudioSourceEffectTemp = new Pool<AudioSource>(() =>
            {
                //回调 获取对象-创建新对象后回调
                AudioSource newObj = UnityEngine.Object.Instantiate<AudioSource>(audioSourceEffectType);
                newObj.transform.SetParent(audioSourceEffectTypeGroup.transform);
                newObj.gameObject.SetActive(false);
                return newObj;
            }, (AudioSource audioSource) =>
            {
                //获取对象后回调
                audioSource.gameObject.SetActive(true);
                audioSource.volume = AudioSourceEffectTempSetting.volume;
                audioSource.loop = AudioSourceEffectTempSetting.isLoop;
                audioSource.mute = AudioSourceEffectTempSetting.isMute;

            }, (AudioSource audioSource) =>
            {
                //回收对象时回调
                audioSource.gameObject.SetActive(false);
            }, 0);
            m_PoolAudioSourceEffectTemp.Recycle(audioSourceEffectType);

            //初始化临时音效播放器缓存 音量、循环、静音
            AudioSourceEffectTempSetting = new AudioSourceInfo { volume = 1, isLoop = false, isMute = false };
        }

        #region 核心功能
        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="soundType">音频类型</param>
        /// <param name="clip">音频实例</param>
        /// <param name="action">回调 (回调时机：音频正常播放完毕、或者中途暂停播放)</param>
        public void Play(SoundType soundType, AudioClip clip, Action action = null)
        {
            AudioSource audioSource = GetAudioSoucreBySoundType(soundType);
            if (audioSource)
            {
                audioSource.clip = clip;
                audioSource.Play();
                //播放完毕延时调用
                //有bug 不能用audioSource.clip.length 判断音效播放完毕，要考虑播放器暂停情况 待解决

                UnityTool.GetInstance.DelayCoroutineWaitReturnFalse(() =>
                {
                    Debug.Log("测试 " + audioSource.isPlaying);
                    return audioSource.isPlaying;
                }, () =>
                {
                    audioSource.clip = null;
                    //临时音效 播放完毕自动回收
                    if (soundType == SoundType.SoundEffectTemp)
                    {
                        m_PoolAudioSourceEffectTemp.Recycle(audioSource);
                    }
                    action?.Invoke();
                });
            }
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        /// <param name="soundType"></param>
        public void Pause(SoundType soundType)
        {
            switch (soundType)
            {
                case SoundType.BGM:
                case SoundType.SoundEffect:
                    GetAudioSoucreBySoundType(soundType)?.Pause();
                    break;
                case SoundType.SoundEffectTemp:
                    m_PoolAudioSourceEffectTemp.GetUsingObjs.ForEach((audioSource) => audioSource.Pause());
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 继续播放
        /// </summary>
        /// <param name="soundType"></param>
        public void Play(SoundType soundType)
        {
            switch (soundType)
            {
                case SoundType.BGM:
                case SoundType.SoundEffect:
                    GetAudioSoucreBySoundType(soundType)?.Play();
                    break;
                case SoundType.SoundEffectTemp:
                    m_PoolAudioSourceEffectTemp.GetUsingObjs.ForEach((audioSource) => audioSource.Play());
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region 播放器参数设置

        /// <summary>
        /// 设置播放器音量大小
        /// </summary>
        /// <param name="soundType"></param>
        /// <param name="volume"></param>
        public void SetPlayerVolume(SoundType soundType, float volume)
        {
            switch (soundType)
            {
                case SoundType.BGM:
                case SoundType.SoundEffect:
                    AudioSource audioSource = GetAudioSoucreBySoundType(soundType);
                    if (audioSource)
                    {
                        audioSource.volume = volume;
                    }
                    break;
                case SoundType.SoundEffectTemp:
                    AudioSourceEffectTempSetting.volume = volume;
                    List<AudioSource> soundEffectTempArr = m_PoolAudioSourceEffectTemp.GetUsingObjs;
                    foreach (AudioSource item in soundEffectTempArr)
                    {
                        item.volume = volume;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置播放器循环
        /// </summary>
        /// <param name="soundType"></param>
        /// <param name="volume"></param>
        public void SetPlayerLoop(SoundType soundType, bool isLoop)
        {
            switch (soundType)
            {
                case SoundType.BGM:
                case SoundType.SoundEffect:
                    AudioSource audioSource = GetAudioSoucreBySoundType(soundType);
                    if (audioSource)
                    {
                        audioSource.loop = isLoop;
                    }
                    break;
                case SoundType.SoundEffectTemp:
                    AudioSourceEffectTempSetting.isLoop = isLoop;
                    List<AudioSource> soundEffectTempArr = m_PoolAudioSourceEffectTemp.GetUsingObjs;
                    foreach (AudioSource item in soundEffectTempArr)
                    {
                        item.loop = isLoop;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置播放器静音
        /// </summary>
        /// <param name="soundType"></param>
        /// <param name="volume"></param>
        public void SetPlayerMute(SoundType soundType, bool isMute)
        {
            switch (soundType)
            {
                case SoundType.BGM:
                case SoundType.SoundEffect:
                    AudioSource audioSource = GetAudioSoucreBySoundType(soundType);
                    if (audioSource)
                    {
                        audioSource.mute = isMute;
                    }
                    break;
                case SoundType.SoundEffectTemp:
                    AudioSourceEffectTempSetting.isMute = isMute;
                    List<AudioSource> soundEffectTempArr = m_PoolAudioSourceEffectTemp.GetUsingObjs;
                    foreach (AudioSource item in soundEffectTempArr)
                    {
                        item.mute = isMute;
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion


        /// <summary>
        /// 获取播放器 根据音效类型
        /// </summary>
        /// <param name="soundType"></param>
        /// <returns></returns>
        private AudioSource GetAudioSoucreBySoundType(SoundType soundType)
        {
            AudioSource audioSource = null;
            switch (soundType)
            {
                case SoundType.BGM:
                    audioSource = m_AudioSourceBGM;
                    break;
                case SoundType.SoundEffect:
                    audioSource = m_AudioSourceEffect;
                    break;
                case SoundType.SoundEffectTemp:
                    audioSource = m_PoolAudioSourceEffectTemp.Allocate();
                    break;
                default:
                    break;
            }
            return audioSource;
        }
    }


}