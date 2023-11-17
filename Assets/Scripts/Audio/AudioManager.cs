using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public class Audio
{
    public string name;
    public AudioClip clip;
}
public class AudioManager : MonoBehaviour
{
    private List<AudioSource> clipPlayer = new List<AudioSource>();
    public int clipTracks;
    private AudioSource MusicPlayer = new AudioSource();

    [SerializeField] public List<Audio> Sounds;
    private Dictionary<string, AudioClip> AudioDatabase = new Dictionary<string, AudioClip>();

    public static AudioManager i { get; private set; }

    void Start()
    {
        i = this;

        for (int j = 0; j < clipTracks; j++)
        {
            GameObject clipSourceObject = new GameObject("ClipTrack_" + j);
            clipSourceObject.transform.parent = transform;
            AudioSource newSource = clipSourceObject.AddComponent<AudioSource>();
            clipPlayer.Add(newSource);
        }
        GameObject musicSourceObject = new GameObject("MusicTrack");
        musicSourceObject.transform.parent = transform;
        MusicPlayer = musicSourceObject.AddComponent<AudioSource>();
        

    }

    private void Awake()
    {
        foreach (var Track in Sounds)
        {
            AudioDatabase.Add(Track.name, Track.clip);
        }
    }


    public void PlayClip(string clip)
    {

        if (AudioDatabase[clip] == null) return;

        foreach (var channel in clipPlayer)
        {
            if (!channel.isPlaying)
            {
                channel.time = 0f;
                channel.clip = AudioDatabase[clip];
                channel.loop = false;
                channel.Play();
                break;
            }
        }
    }
    public void PlayClip(string clip, bool loop = false, float startPos = 0f)
    {
        
        if (AudioDatabase[clip] == null) return;
        if (startPos > AudioDatabase[clip].length) return;

        foreach (var channel in clipPlayer)
        {
            if (!channel.isPlaying)
            {
                channel.time = startPos;
                channel.clip = AudioDatabase[clip];
                channel.loop = loop;
                channel.Play();
                break;
            }
        }
    }

    public void StopClip(string clip)
    {
        foreach (var channel in clipPlayer)
        {
            if (channel.isPlaying && channel.clip == AudioDatabase[clip])
            {
                channel.Stop();
            }
        }
    }

    public void StopAllClips()
    {
        foreach (var channel in clipPlayer)
        {
            if (channel.isPlaying)
            {
                channel.Stop();
            }
        }
    }
    public void PlayMusic(string song, bool loop = true, float startPos = 0f)
    {
        if (AudioDatabase[song] == null) return;
        if (startPos > AudioDatabase[song].length) return;

        MusicPlayer.Stop();
        MusicPlayer.time = startPos;
        MusicPlayer.clip = AudioDatabase[song];
        MusicPlayer.loop = loop;
        MusicPlayer.Play();
    }

    public void StopMusic()
    {
        MusicPlayer.Stop();
    }





}
