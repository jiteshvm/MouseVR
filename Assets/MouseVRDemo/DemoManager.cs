using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Vectrosity;
using UnityEngine.VR;

public class DemoManager : MonoBehaviour {

    public GameObject CenterEyeAnchor;

    public GameObject ButtonPrefab;

    public GameObject CenterCircleObject;

    public int NumberOfCircles;

    public float CircleRadius;

    VectorLine LineDebug;

    List<Vector3> LinePoints;

    Camera CameraRef;

    List<GameObject> ButtonsList;

    void Start () {

        VectorManager.useDraw3D = true;
        LinePoints = new List<Vector3>();
        LinePoints.Add(Vector3.zero);
        LinePoints.Add(Vector3.zero);
        LineDebug = new VectorLine("line", LinePoints, 4.0f);
        LineDebug.smoothWidth = true;

        if(CenterEyeAnchor)
        {
            CameraRef = CenterEyeAnchor.GetComponent<Camera>();
            VectorLine.SetCamera3D(CameraRef);
            LineDebug.drawTransform = CenterEyeAnchor.transform;
            //LineDebug.drawTransform = cam.transform;
        }

        if(CenterCircleObject && ButtonPrefab && NumberOfCircles > 0)
        {
            float CurrentAngle = 0.0f;
            float AngleDelta = 2.0f * Mathf.PI / NumberOfCircles;
            ButtonsList = new List<GameObject>();

            for (int i = 0; i < NumberOfCircles; ++i)
            {
                float x = CenterCircleObject.transform.position.x + CircleRadius * Mathf.Cos(CurrentAngle);
                float y = CenterCircleObject.transform.position.y + CircleRadius * Mathf.Sin(CurrentAngle);
                float z = CenterCircleObject.transform.position.z;
                GameObject NewCircleObj = GameObject.Instantiate(ButtonPrefab, new Vector3(x, y, z), Quaternion.identity);
                ButtonsList.Add(NewCircleObj);
                CurrentAngle += AngleDelta;
            }
        }
        

        LeapVRCameraControl.OnValidCameraParams += OnValidCameraParams;
        InputTracking.Recenter();
    }

    private void OnValidCameraParams(LeapVRCameraControl.CameraParams cameraParams)
    {
        //LineDebug.drawTransform = cameraParams.CenterEyeTransform;
    }

    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawRay(CenterEyeAnchor.transform.position, CenterEyeAnchor.transform.forward * 5000.0f);
    }
}
