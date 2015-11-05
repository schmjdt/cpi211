using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(AudioSource))]
public class SoundControl : MonoBehaviour {
    public static SoundControl instance;
    
    public List<Audio> audio = new List<Audio>();

    AudioSource source;

    void Awake()
    {
        instance = this;

        source = GetComponent<AudioSource>();
    }

    public bool setAudio(string nameA, string nameS)
    {
        foreach (Audio a in audio)
        {
            if (a.name == nameA)
            {
                source.clip = a.getClip(nameS);
                return true;
            }
        }
        return false;
    }

    public void playAudio(string nameA, string nameS)
    {
        if (setAudio(nameA, nameS))
            source.Play();
    }

    public void playAudio()
    {
        source.Play();
    }
}

[System.Serializable]
public class Audio
{
    public string name;
    public AudioClipInfo[] audioClips;

    public AudioClip getClip(int i) { return audioClips[i].audioClip;  }
    public AudioClip getClip(string s)
    {
        foreach (AudioClipInfo item in audioClips)
        {
            if (item.name == s)
                return item.audioClip;
        }
        return null;
    }

}

[System.Serializable]
public class AudioClipInfo
{
    public string name;
    public AudioClip audioClip;
}