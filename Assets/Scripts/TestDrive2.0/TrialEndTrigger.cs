﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialEndTrigger : MonoBehaviour
{
    [SerializeField] private TestEventManager testEventManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ManualController>())
        {
            testEventManager.TrialEndTrigger();
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
