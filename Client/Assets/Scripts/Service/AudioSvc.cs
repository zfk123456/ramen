/****************************************************
    文件：AudioSvc.cs
	功能：声音播放服务
*****************************************************/

using UnityEngine;

public class AudioSvc : MonoBehaviour {
    public static AudioSvc Instance = null;
    public AudioSource bgAudio;
    public AudioSource uiAudio;

    public void InitSvc() {
        Instance = this;
        PECommon.Log("Init AudioSvc...");
    }

    public void StopBGMusic() {
        if (bgAudio != null) {
            bgAudio.Stop();
        }
    }


    public void PlayBGMusic(string name, bool isLoop = true) {
        AudioClip audio = ResSvc.Instance.LoadAudio("ResAudio/" + name, true);
        if (bgAudio.clip == null || bgAudio.clip.name != audio.name) {
            bgAudio.clip = audio;
            bgAudio.loop = isLoop;
            bgAudio.Play();
        }
    }

    public void PlayUIAudio(string name) {
        AudioClip audio = ResSvc.Instance.LoadAudio("ResAudio/" + name, true);
        uiAudio.clip = audio;
        uiAudio.Play();
    }

    public void PlayCharAudio(string name, AudioSource audioSrc) {
        AudioClip audio = ResSvc.Instance.LoadAudio("ResAudio/" + name, true);
        audioSrc.clip = audio;
        audioSrc.Play();
    }
}