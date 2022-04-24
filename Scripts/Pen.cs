using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Credit to Dilmer on some of the general line renderer concepts
// https://www.youtube.com/watch?v=DFijT_S6FpM&ab_channel=DilmerValecillos

public class Pen : MonoBehaviour
{

    public OVRSkeleton skeleton;
    public OVRBone fingerTip;
    public OVRHand hand;
    private GameObject fingerTrack;
    
    public List<LineRenderer> lineList = new List<LineRenderer>();
    public float lineWidth = 0.005f;
    [SerializeField] float bigBrush = 0.05f;
    [SerializeField] float mediumBrush = 0.01f;
    [SerializeField] float smallBrush = 0.005f;
    public Color lineColor = Color.white;
    public Material lineMat;
    private GameObject renderHouse;
    private LineRenderer currentLR;
    private Vector3 lastPointLocation = Vector3.zero;
    private int positionNum = 0;
    private int rhNum = 0;

    private bool priorPinchStatus = false;
    public bool boneInit = false;

    void Start()
    {
        StartCoroutine(DelayBoneInit(4.0f, SetHandStruct)); //Hand tracking issues if this script starts before the hands have loaded in
    }

    public IEnumerator DelayBoneInit(float delayTime, Action nextAction)
    {
        yield return new WaitForSeconds(delayTime);
        nextAction.Invoke();;
    }

    private void SetHandStruct()
    {
        foreach (var bone in skeleton.Bones)
        {
            if(bone.Id == OVRSkeleton.BoneId.Hand_IndexTip){
                fingerTip = bone;
            }
        }
        try
        {
            fingerTrack = fingerTip.Transform.gameObject;
        }
        catch
        {
        }

        NewLineRenderer();
    }


    // Update is called once per frame
    void Update()
    {
        if (fingerTrack == null)
        {
            SetHandStruct();
            return;
        }
        else
        {
            fingerTrack = fingerTip.Transform.gameObject;
        }
        PinchDetect();
    }

    private void PinchDetect()
    {
        //Detects pinches with high confidence and either creates or updates line renderer
        bool isIndexFingerPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        OVRHand.TrackingConfidence confidence = hand.GetFingerConfidence(OVRHand.HandFinger.Index);

        if (isIndexFingerPinching && confidence == OVRHand.TrackingConfidence.High && priorPinchStatus == false)
        {
            priorPinchStatus = true;
            NewLineRenderer();
        }
        else if (isIndexFingerPinching && confidence == OVRHand.TrackingConfidence.High && priorPinchStatus == true)
        {
            UpdateLineRenderer();
        }
        else if(!isIndexFingerPinching) 
        {
            priorPinchStatus = false;
            lastPointLocation = Vector3.zero;
            positionNum = 0;
        }   

        // Large Brush
        bool isMiddleFingerPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Middle);
        OVRHand.TrackingConfidence middleConfidence = hand.GetFingerConfidence(OVRHand.HandFinger.Middle);
        if (isMiddleFingerPinching && middleConfidence == OVRHand.TrackingConfidence.High)
        {
            lineWidth = bigBrush;
        }

        // Medium brush
        bool isRingFingerPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Ring);
        OVRHand.TrackingConfidence ringConfidence = hand.GetFingerConfidence(OVRHand.HandFinger.Ring);
        if (isRingFingerPinching && ringConfidence == OVRHand.TrackingConfidence.High)
        {
            lineWidth = mediumBrush;
        }
 
        // Small brush
        bool isPinkyFingerPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Pinky);
        OVRHand.TrackingConfidence pinkyConfidence = hand.GetFingerConfidence(OVRHand.HandFinger.Pinky);
        if (isPinkyFingerPinching && pinkyConfidence == OVRHand.TrackingConfidence.High)
        {
            lineWidth = smallBrush;
        }



    }

    private void UpdateLineRenderer()
    {
        if (lastPointLocation == Vector3.zero)
        {
            lastPointLocation = fingerTrack.transform.position;
        }
        else if (Mathf.Abs(Vector3.Distance(lastPointLocation, fingerTrack.transform.position)) >= 0.01f)
        {
            //Need to frequently start new lines for smooth drawing
            lastPointLocation = fingerTrack.transform.position;
            currentLR.SetPosition(positionNum, lastPointLocation);
            positionNum++;
            currentLR.positionCount = positionNum + 1;
            currentLR.SetPosition(positionNum, lastPointLocation);
        }

    }

    private void NewLineRenderer()
    {
        GameObject renderHouse = new GameObject("RenderHouse"+rhNum);
        renderHouse.tag = "RenderHouse";
        rhNum++;
        // Avoid exceptions if hand tracking temp lost. 
        try{
            renderHouse.transform.position = fingerTrack.transform.position;
        }
        catch{
            return;
        }

        LineRenderer newLine = renderHouse.AddComponent<LineRenderer>();
        newLine.endWidth = lineWidth;
        newLine.startWidth = lineWidth;
        newLine.material.color = lineColor;
        newLine.material.EnableKeyword("_EMISSION");
        newLine.material.SetColor("_EmissionColor", lineColor);
        newLine.useWorldSpace = true;
        newLine.positionCount = 1;

        currentLR = newLine;
        lineList.Add(newLine);
    }

    public void ChangeColor(Color color){
        lineColor = color;
        currentLR.material.color = lineColor;
        currentLR.material.EnableKeyword("_EMISSION");
        currentLR.material.SetColor("_EmissionColor", lineColor);
    }

    public Color GetColor(){
        return lineColor;
    }

    public void Erase()
    {
        lineList = new List<LineRenderer>();
        GameObject[] allLineRenderers =  GameObject.FindGameObjectsWithTag("RenderHouse");
        if (allLineRenderers.Length > 0)
        {
            foreach (var line in allLineRenderers)
            {
                Destroy(line);
            }
        }
    }

    public void Rave()
    {
        //Strobe lines, not fully implemented yet
        // foreach (var line in lineList)
        // {
        //     Color randColor = new Color(UnityEngine.Random.Range(0,1), UnityEngine.Random.Range(0,1), UnityEngine.Random.Range(0,1));
        //     line.material.color = randColor;
        //     line.material.EnableKeyword("_EMISSION");
        //     line.material.SetColor("_EmissionColor", randColor);
        // }
        
        LineRenderer[] allLineRenderers =  FindObjectsOfType<LineRenderer>();
        if (allLineRenderers.Length > 0)
        {
            foreach (var line in allLineRenderers)
            {
                Color randColor = UnityEngine.Random.ColorHSV();
                line.material.color = randColor;
                line.material.EnableKeyword("_EMISSION");
                line.material.SetColor("_EmissionColor", randColor);
            }
        }
    }
}
