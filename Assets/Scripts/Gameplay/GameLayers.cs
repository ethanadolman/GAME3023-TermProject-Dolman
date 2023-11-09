using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask encountersLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fovLayer;

    public static GameLayers i { get; set; }

    private void Awake()
    {
        i = this;
    }

    public LayerMask SolidLayer => solidObjectsLayer;
    public LayerMask InteractableLayer => interactableLayer;
    public LayerMask EncountersLayer => encountersLayer;
    public LayerMask PlayerLayer => playerLayer;
    public LayerMask FovLayer => fovLayer;
}
