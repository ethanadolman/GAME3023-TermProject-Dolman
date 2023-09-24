using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroll : MonoBehaviour
{
    [SerializeField] private RectTransform Background;
    [SerializeField] private RectTransform Midground;
    [SerializeField] private RectTransform Foreground;
    [SerializeField] private int scrollSpeed;
    [SerializeField] private AudioClip titleTrack;

    private void Start()
    {
        AudioManager.i.PlayMusic(titleTrack);
    }

    private void Update()
    {
        int speedBoost = Input.GetKey(KeyCode.LeftShift) ? 6 : 1;
        if (Foreground.anchoredPosition.y < 25)
        {
            Background.anchoredPosition += Vector2.up * (scrollSpeed / 1) * Time.deltaTime * speedBoost;
            Midground.anchoredPosition += Vector2.up * (scrollSpeed / 1.5f) * Time.deltaTime * speedBoost;
            Foreground.anchoredPosition += Vector2.up * (scrollSpeed / 2) * Time.deltaTime * speedBoost;
        }
    }
}
