using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] private AudioClip optionSelected;
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
}
