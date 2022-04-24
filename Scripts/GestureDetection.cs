using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> fingerData;
    public UnityEvent onRecognized;
}

public class GestureDetection : MonoBehaviour
{

    public OVRSkeleton skeleton;
    public List<Gesture> gestures;
    private List<OVRBone> fingerBones = null;
    public float threshold = 0.05f;
    private Gesture previousGesture;
    public bool debugMode = true;
    public bool boneInit = false;

    Pen pen;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelayBoneInit(3.0f, Initialize));
        previousGesture = new Gesture();
        pen = FindObjectOfType<Pen>();

    }

    public IEnumerator DelayBoneInit(float delayTime, Action nextAction)
    {
        yield return new WaitForSeconds(delayTime);
        nextAction.Invoke();;
    }

    public void Initialize()
    {
        SetBones();
        boneInit = true;
    }

    private void SetBones()
    {
        try{
            fingerBones = new List<OVRBone>(skeleton.Bones);
        }
        catch{}
    }

    // Update is called once per frame
    void Update()
    {
        if (fingerBones == null)
        {
            SetBones();
            return;
        }
        if (debugMode && Input.GetKeyDown(KeyCode.Space))
        {
            Save();
        }
        Gesture currentGesture = Recognize();

        if (currentGesture.name == "Bird")
        {
            pen.Erase();
        }
        else if (currentGesture.name == "RockOut")
        {
            pen.Rave();
        }


        bool hasRecognized = !currentGesture.Equals(new Gesture());
        if (hasRecognized && !currentGesture.Equals(previousGesture))
        {
            previousGesture = currentGesture;
            currentGesture.onRecognized.Invoke();
        }
    }
    
    
    // Credit to Valem for general gesture saving system
    // https://www.youtube.com/watch?v=lBzwUKQ3tbw&ab_channel=Valem
    void Save()
    {
        Gesture g = new Gesture();
        g.name = "New Gesture";
        List<Vector3> data = new List<Vector3>();
        foreach (var bone in fingerBones)
        {
            //Finger position relative to root posit
            data.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
        }

        g.fingerData = data;
        gestures.Add(g);
    }

    Gesture Recognize()
    {
        Gesture currentGesture = new Gesture();
        float currentMin = Mathf.Infinity;

        foreach (var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            //Cycle through all finger bone positions and check distance from gesture's root positions
            for (int i = 0; i < fingerBones.Count; i++)
            {
                Vector3 currentPosition = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);
                float distance = Vector3.Distance(currentPosition, gesture.fingerData[i]);
                if (distance>threshold)
                {
                    isDiscarded = true;
                    break;
                }
                sumDistance += distance;
            }
            //If gesture isn't discarded and the sum of distances from the root is within threshold, most likely gesture detected
            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }
        }
        Debug.Log(currentGesture.name);
        return currentGesture;
    }
}
