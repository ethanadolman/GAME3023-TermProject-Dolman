using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    public Light2D globalLight; // Reference to your 2D global light object
    private float fadeDuration = 15.0f; // Total duration for each fade (in seconds, 1 minute)
    private float minLightIntensity = 0.02f;
    private float maxLightIntensity = 1.0f;
    private bool isFadingOut = false;

    private void Start()
    {
        StartCoroutine(StartFade());
    }

    private IEnumerator StartFade()
    {
        while (true)
        {
            if (isFadingOut)
            {
                float startTime = Time.time;
                float endTime = startTime + fadeDuration;

                while (Time.time < endTime)
                {
                    float t = (Time.time - startTime) / fadeDuration;
                    float intensity = Mathf.Lerp(maxLightIntensity, minLightIntensity, t);
                    SetLightIntensity(intensity);
                    yield return null;
                }

                SetLightIntensity(minLightIntensity);
            }
            else
            {
                float startTime = Time.time;
                float endTime = startTime + fadeDuration;

                while (Time.time < endTime)
                {
                    float t = (Time.time - startTime) / fadeDuration;
                    float intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, t);
                    SetLightIntensity(intensity);
                    yield return null;
                }

                SetLightIntensity(maxLightIntensity);
            }

            isFadingOut = !isFadingOut;

            yield return new WaitForSeconds(30.0f); // Wait for 1 minute
        }
    }

    private void SetLightIntensity(float intensity)
    {
        if (globalLight != null)
        {
            globalLight.intensity = intensity;
        }
    }
}
