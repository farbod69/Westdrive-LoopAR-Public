﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// name is at the moment a bit miss leading. It is a overall manager class, which is aware of all events in that scene, the Participant and the AI Cars.
/// It is aware of starting events and ending events globally.
/// It is intended to handle the reaction of AI cars in case of events of AI cars to avoid them of interfering into the event
/// </summary>

[DisallowMultipleComponent]
public class PersistentTrafficEventManager : MonoBehaviour
{
    public static PersistentTrafficEventManager Instance { get; private set; }
    
    [SerializeField] private GameObject participantsCar; //needs a functionality to find the participants Car
    [SerializeField] private float eventSpeed = 5f;
    
    
    private List<EventBehavior> _eventBehaviorListeners;
    private ControlSwitch _participantsControlSwitch;
    private List<GameObject> _eventObjects;
    private GameObject _eventObject;

    
    private void Awake()
    {
        _eventBehaviorListeners = new List<EventBehavior>();
        
        //singleton pattern a la Unity
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);    //the Traffic Manager should be persistent by changing the scenes maybe change it on the the fly
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        _participantsControlSwitch = participantsCar.GetComponent<ControlSwitch>();
    }

    public void RegisterTrafficListeners(EventBehavior listener)
    {
        _eventBehaviorListeners.Add(listener);
    }


    public void InitiateEvent(List<GameObject> eventObjects)
    {
        foreach (var eventListener in _eventBehaviorListeners)
        {
            eventListener.AvoidInterference(10f);
        }

        _eventObjects = eventObjects;
        
        _participantsControlSwitch.SwitchControl(true);
        _participantsControlSwitch.GetComponentInChildren<WindscreenHUD>().DriverAlert();
        _participantsControlSwitch.GetComponentInChildren<HUDLite>().ActivateHUD(_eventObjects);
        ExperimentManager.Instance.SetEventActivationState(true);
    }

    public void FinalizeEvent()
    {
        foreach (var eventListener in _eventBehaviorListeners)
        {
            Debug.Log("setting back to normal");
            eventListener.ReestablishNormalBehavior();
        }
        
        _participantsControlSwitch.SwitchControl(false);
        _participantsControlSwitch.GetComponentInChildren<HUDLite>().DeactivateHUD();
        _participantsControlSwitch.GetComponentInChildren<WindscreenHUD>().DeactivateHUD();
        ExperimentManager.Instance.SetEventActivationState(false);
    }

    public GameObject GetParticipantsCar()
    {
        return participantsCar;
    }

    public float GetEventSpeed()
    {
        return eventSpeed;
    }

    public void SetEventObject(List<GameObject> objects)
    {
        _eventObjects = objects;
    }
    
    public void SetEventObject(GameObject objects)
    {
        _eventObject = objects;
    }
}
