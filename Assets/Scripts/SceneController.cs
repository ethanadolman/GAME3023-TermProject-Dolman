using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadScene(string name)
    {
        AudioManager.i.PlayClip("OptionSelected");
        StartCoroutine(SceneLoader(name));
    }

    private IEnumerator SceneLoader(string name)
    {
        
        yield return new WaitForSeconds(1f);
        AudioManager.i.StopAllClips();
        SceneManager.LoadScene(name);
    }

    public void LoadSavedGame()
    {
        AudioManager.i.PlayClip("OptionSelected");
        StartCoroutine(SavedGameLoader("World"));
    }

    private IEnumerator SavedGameLoader(string name)
    {

        yield return new WaitForSeconds(1f);
        AudioManager.i.StopAllClips();
        SceneManager.LoadScene(name);
        yield return new WaitForSeconds(0.5f);
        SaveData.i.LoadSave();
    }



}
