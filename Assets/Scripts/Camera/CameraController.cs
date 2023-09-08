using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static CameraController;

public class CameraController : MonoBehaviour
{
    public enum CameraOptions
    {
        Follow,
        SmoothFollow,
        Stationary
    }
    [SerializeField] private CameraOptions cameraOption = CameraOptions.SmoothFollow;

    [SerializeField] private Transform targetPos;
    [SerializeField] private Vector3 cameraOffset;
    [SerializeField] private float lerpSpeed = 4;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (cameraOption)
        {
            case CameraOptions.Follow:
                if (targetPos != null)
                {
                    transform.position = targetPos.position + cameraOffset;
                }
                break;
            case CameraOptions.SmoothFollow:
                if (targetPos != null)
                {
                    transform.position = Vector3.Lerp(transform.position, targetPos.position + cameraOffset,
                        lerpSpeed * Time.deltaTime);
                }
                break;
            case CameraOptions.Stationary:
                break;
        }

    }



}
