using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private List<AudioSource> sfxPlayer;

    public static AudioManager i { get; private set; }

    void Awake()
    {
        i = this;
    }
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();
    }

    public void PlaySfx(AudioClip clip, bool loop = false)
    {
        if (clip == null) return;

        foreach (var channel in sfxPlayer)
        {
            if (!channel.isPlaying)
            {
                channel.clip = clip;
                channel.loop = loop;
                channel.Play();
                break;
            }
        }
    }

    public void ClearSfx()
    {
        foreach (var channel in sfxPlayer)
        {
            if (!channel.isPlaying)
            {
                channel.Stop();
            }
        }
    }
}
