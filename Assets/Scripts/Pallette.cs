using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// I tried to write as much of this on my own as possible but the general format came from this tutorial
// https://www.youtube.com/watch?v=DFijT_S6FpM&ab_channel=DilmerValecillos

public class Pallette : MonoBehaviour
{

    public OVRSkeleton skeleton;
    public OVRBone fingerTip;
    public OVRHand hand;
    static private List<OVRHand.HandFinger> fingers = new List<OVRHand.HandFinger>();
    
    public Color lineColor = Color.white;

    private bool priorPinchStatus = false;
    Pen pen;

    void Start()
    {
        pen = FindObjectOfType<Pen>();
        fingers.Add(OVRHand.HandFinger.Index);
        fingers.Add(OVRHand.HandFinger.Middle);
        fingers.Add(OVRHand.HandFinger.Ring);
        fingers.Add(OVRHand.HandFinger.Pinky);
    }


    // Update is called once per frame
    void Update()
    {
        PinchDetect();
    }

    private void PinchDetect()
    {
        bool isFingerPinching;
        OVRHand.TrackingConfidence confidence;
         

        foreach (var finger in fingers)
        {
            isFingerPinching = hand.GetFingerIsPinching(finger);
            confidence = hand.GetFingerConfidence(OVRHand.HandFinger.Index);

            if (isFingerPinching && confidence == OVRHand.TrackingConfidence.High && priorPinchStatus == false)
            {
                colorChange(finger);
                priorPinchStatus = true;
            }
            else if (isFingerPinching && confidence == OVRHand.TrackingConfidence.High && priorPinchStatus == true)
            {
                break;
            }
            else if(!isFingerPinching) 
            {
                priorPinchStatus = false;
            }   
        }
        
    }

    private void colorChange(OVRHand.HandFinger finger)
    {
        switch (finger)
        {
            case OVRHand.HandFinger.Index:
                lineColor = Color.white;
                break;
            case OVRHand.HandFinger.Middle:
                lineColor = Color.cyan;
                break;
            case OVRHand.HandFinger.Ring:
                lineColor = Color.red;
                break;
            case OVRHand.HandFinger.Pinky:
                Color randColor = UnityEngine.Random.ColorHSV();
                lineColor = randColor;
                break;
            
            default:
                lineColor = Color.white;
                break;
        }
        
        pen.ChangeColor(lineColor);
    }
}
