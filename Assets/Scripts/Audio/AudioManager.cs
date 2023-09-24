using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private List<AudioSource> sfxPlayer;

    public static AudioManager i { get; private set; }

    void Start()
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

    public void PlaySfx(AudioClip clip, bool loop = false, float startPos = 0f)
    {
        if (clip == null) return;
        if (startPos > clip.length) return;

        foreach (var channel in sfxPlayer)
        {
            if (!channel.isPlaying)
            {
                channel.time = startPos;
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
            if (channel.isPlaying)
            {
                channel.Stop();
            }
        }
    }
}
