﻿using System;
using System.Collections;
using System.Collections.Generic;
using Tobii.XR;
using UnityEngine;
using UnityEngine.SceneManagement;
using ViveSR.anipal.Eye;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class EyetrackingDataRecorder : MonoBehaviour
{
    // Start is called before the first frame update
    private float _sampleRate;
    private List<EyeTrackingDataFrame> _recordedEyeTrackingData;
    private EyetrackingManager _eyetrackingManager;
    private Transform _hmdTransform;
    private bool recordingEnded;
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        _recordedEyeTrackingData= new List<EyeTrackingDataFrame>();
        
        _eyetrackingManager= EyetrackingManager.Instance;
        
        _sampleRate = _eyetrackingManager.GetSampleRate();
        _hmdTransform = _eyetrackingManager.GetHmdTransform();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void  OnSceneLoaded(Scene scene, LoadSceneMode mode)  // generally I am not proud of this call, but seems necessary for the moment.
    {
        _hmdTransform = _eyetrackingManager.GetHmdTransform();        //refresh the HMD transform after sceneload;
        //Debug.Log("hello new World");
    }

    
    public void StartRecording()
    {
        recordingEnded = false;
        StartCoroutine(RecordEyeTrackingData());
        
    }

    public void StopRecording()
    {
        recordingEnded = true;
    }
    
    private IEnumerator RecordEyeTrackingData()
    {
        int frameCounter = new int();
        Debug.Log("starting recording...");
        while (!recordingEnded)
        {
            EyeTrackingDataFrame dataFrame = new EyeTrackingDataFrame();
            var eyeTrackingDataWorld = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
            var eyeTrackingDataLocal = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);

            if (eyeTrackingDataWorld.GazeRay.IsValid)
            {
                Vector3 gazeRayOrigin = eyeTrackingDataWorld.GazeRay.Origin;
                Vector3 gazeRayDirection = eyeTrackingDataWorld.GazeRay.Direction;
                dataFrame.TobiiTimeStamp = eyeTrackingDataWorld.Timestamp;
                dataFrame.EyePosWorldCombined = gazeRayOrigin;
                dataFrame.EyeDirWorldCombined = gazeRayDirection;

                dataFrame.LeftEyeIsBlinkingWorld = eyeTrackingDataWorld.IsLeftEyeBlinking;
                dataFrame.RightEyeIsBlinkingWorld = eyeTrackingDataWorld.IsRightEyeBlinking;

                dataFrame.hitObjects = GetHitObjectsFromGaze(gazeRayOrigin, gazeRayDirection);
            }

            if (eyeTrackingDataLocal.GazeRay.IsValid)
            {
                dataFrame.EyePosLocalCombined = eyeTrackingDataLocal.GazeRay.Origin;
                dataFrame.EyeDirLocalCombined = eyeTrackingDataLocal.GazeRay.Direction;
                dataFrame.LeftEyeIsBlinkingLocal = eyeTrackingDataLocal.IsLeftEyeBlinking;
                dataFrame.RightEyeIsBlinkingLocal = eyeTrackingDataLocal.IsRightEyeBlinking;
            }

            dataFrame.UnixTimeStamp = TimeManager.Instance.GetCurrentUnixTimeStamp();

            dataFrame.TimeStamp = eyeTrackingDataWorld.Timestamp;
            
            dataFrame.HmdPosition = EyetrackingManager.Instance.GetHmdTransform().position;

            dataFrame.NoseVector =  EyetrackingManager.Instance.GetHmdTransform().forward;
            
            _recordedEyeTrackingData.Add(dataFrame);
            frameCounter++;
            
            yield return new WaitForSeconds(_sampleRate);
            
        }
        
    }


    private List<HitObjectInfo> GetHitObjectsFromGaze(Vector3 gazeOrigin, Vector3 gazeDirection)
    {
        RaycastHit[] hitColliders = Physics.RaycastAll(gazeOrigin, gazeDirection);
        
        List<HitObjectInfo> hitObjectInfoList= new List<HitObjectInfo>();
        
        foreach (var colliderhit in hitColliders)
        {
                    
            HitObjectInfo hitInfo = new HitObjectInfo();
            hitInfo.ObjectName = colliderhit.collider.gameObject.name;
            hitInfo.HitObjectPosition = colliderhit.collider.transform.position;
            hitInfo.HitPointOnObject = colliderhit.point;
            hitObjectInfoList.Add(hitInfo);
        }

        return hitObjectInfoList;
    }



    public List<EyeTrackingDataFrame> GetDataFrames()
    {
        if (recordingEnded)
        {
            return _recordedEyeTrackingData;
        }
        else
        {
            throw new Exception("Eyetracking Data Recording has not been finished");
        }
        
    }
}
