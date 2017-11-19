using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Interaction;
using Vectrosity;
using UnityEngine.VR;

public class DemoManager : MonoBehaviour {

    public GameObject TargetPrefab;

    public GameObject CenterObj;

    public int NumberOfTargets;

    float TargetRadius;

    //center to center inter target distance in mm
    public float InterTargetDistance = 142;
    float InterTargetDistanceMeters;

    //width of each target in mm
    public float TargetWidth = 12;
    float TargetWidthMeters;

    VectorLine LineDebug;

    List<Vector3> LinePoints;

    List<GameObject> TargetsList;

    List<int> RandomButtonIndices;

    GameObject InteractionManagerObj;
    public InteractionManager InteractionManagerRef;

    GameObject DebugCube;

    void Start () {

        //DebugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //DebugCube.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        //DebugCube.GetComponent<MeshRenderer>().material.color = Color.green;

        VectorManager.useDraw3D = true;
        LinePoints = new List<Vector3>();
        LineDebug = new VectorLine("line", LinePoints, 4.0f);
        LineDebug.smoothWidth = true;

        TargetWidthMeters = TargetWidth / 1000.0f;
        InterTargetDistanceMeters = InterTargetDistance / 1000.0f;
        TargetRadius = (InterTargetDistanceMeters * NumberOfTargets) / (2.0f * Mathf.PI);
        //Debug.Log("Outer Circle Radius : " + TargetRadius);
        //Debug.Log(TargetWidthMeters);        
        
        InteractionManagerObj = GameObject.Find("Interaction Manager");
        if(InteractionManagerObj)
        {
            InteractionManagerRef = InteractionManagerObj.GetComponent<InteractionManager>();
            /*
            if(InteractionManagerRef)
            {
                foreach(InteractionController ic in InteractionManagerRef.interactionControllers)
                {
                    InteractionHand ih = (InteractionHand)ic;
                    if(ih && ih.isRight) 
                    {
                        //Debug.Log(ih.leapHand.Fingers[Finger.FingerType.TYPE_INDEX].TipPosition);
                        //ih.leapProvider.OnUpdateFrame += OnUpdateFrame;
                    }
                }
            }
            */
        }
        
        if(CenterObj && TargetPrefab && NumberOfTargets > 0)
        {
            float CurrentAngle = 0.0f;
            float AngleDelta = 2.0f * Mathf.PI / NumberOfTargets;
            TargetsList = new List<GameObject>();
            RandomButtonIndices = new List<int>();

            for (int i = 0; i < NumberOfTargets; ++i)
            {
                float x = CenterObj.transform.position.x + TargetRadius * Mathf.Cos(CurrentAngle);
                float y = CenterObj.transform.position.y + TargetRadius * Mathf.Sin(CurrentAngle);
                float z = CenterObj.transform.position.z;
                GameObject NewCircleObj = GameObject.Instantiate(TargetPrefab, new Vector3(x, y, z), Quaternion.identity);
                NewCircleObj.name = "Button" + i;
                NewCircleObj.transform.GetChild(0).gameObject.GetComponent<Rigidbody>().detectCollisions = false;
                NewCircleObj.transform.GetChild(0).GetChild(0).localScale = new Vector3(TargetWidthMeters, TargetWidthMeters, 0.01f);
                TargetsList.Add(NewCircleObj);
                RandomButtonIndices.Add(i);
                CurrentAngle += AngleDelta;
            }
        }

        //float dist = Vector3.Distance(TargetsList[1].transform.position, TargetsList[2].transform.position);
        //Debug.Log(dist);

        List<Vector3> testPoints = new List<Vector3>();
        testPoints.Add(TargetsList[0].transform.position);
        testPoints.Add(TargetsList[1].transform.position);
        VectorLine testLine = new VectorLine("test", testPoints, 8);
        testLine.Draw3D();


        ActivateRandomTarget();
        LeapVRCameraControl.OnValidCameraParams += OnValidCameraParams;
        InputTracking.Recenter();
    }

    public void ActivateRandomTarget()
    {
        if(RandomButtonIndices.Count > 0)
        {
            int RandomIndex = Random.Range(0, RandomButtonIndices.Count);
            //Debug.Log("random index : " + RandomIndex);
            int RandomValue = RandomButtonIndices[RandomIndex];
            //Debug.Log(" random value : " + RandomValue);
            GameObject ButtonInteractionRef = TargetsList[RandomValue].transform.GetChild(0).gameObject;
            ButtonInteractionRef.GetComponent<Rigidbody>().detectCollisions = true;
            ButtonInteractionRef.GetComponent<SimpleInteractionGlow>().defaultColor = Color.blue;
            ButtonInteractionRef.GetComponent<SimpleInteractionGlow>().hoverColor = Color.blue;
            ButtonInteractionRef.GetComponent<SimpleInteractionGlow>().primaryHoverColor = Color.blue;
            RandomButtonIndices.Remove(RandomValue);
            //Debug.Log("Count : " + RandomButtonIndices.Count);
        }
        
    }

    private void OnUpdateFrame(Frame frame)
    {
        Debug.Log("test");
    
    }

    private void OnValidCameraParams(LeapVRCameraControl.CameraParams cameraParams)
    {
        //LineDebug.drawTransform = cameraParams.CenterEyeTransform;
    }

    void Update()
    {
        /*
        if (InteractionManagerRef)
        {
            foreach (InteractionController ic in InteractionManagerRef.interactionControllers)
            {
                InteractionHand ih = (InteractionHand)ic;
                if (ih && ih.isTracked && ih.contactingObjects.Count > 0)
                {
                    Debug.Log(ih.leapHand.Fingers[(int)Finger.FingerType.TYPE_INDEX].TipPosition);
                    DebugCube.transform.position = ih.leapHand.Fingers[(int)Finger.FingerType.TYPE_INDEX].TipPosition.ToVector3();
                }
            }
        }
        */
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawRay(CenterEyeAnchor.transform.position, CenterEyeAnchor.transform.forward * 5000.0f);
    }
}
