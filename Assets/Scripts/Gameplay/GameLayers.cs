using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask encountersLayer;

    public static GameLayers i { get; set; }

    private void Awake()
    {
        i = this;
    }

    public LayerMask SolidLayer => solidObjectsLayer;
    public LayerMask InteractableLayer => interactableLayer;
    public LayerMask EncountersLayer => encountersLayer;
}
