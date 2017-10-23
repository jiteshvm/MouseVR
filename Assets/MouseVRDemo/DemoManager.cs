using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Vectrosity;

public class DemoManager : MonoBehaviour {

    public GameObject CenterEyeAnchor;

    VectorLine line;

    List<Vector3> points;

    Camera cam;
    
    void Start () {

        LeapVRCameraControl.OnValidCameraParams += OnValidCameraParams;
        VectorManager.useDraw3D = true;
        points = new List<Vector3>();
        points.Add(Vector3.zero);
        points.Add(Vector3.zero);
        line = new VectorLine("line", points, 4.0f);
        line.smoothWidth = true;
        cam = CenterEyeAnchor.GetComponent<Camera>();
        VectorLine.SetCamera3D(cam);
        //line.drawTransform = cam.transform;
        line.drawTransform = CenterEyeAnchor.transform;
    }

    private void OnValidCameraParams(LeapVRCameraControl.CameraParams cameraParams)
    {
        //line.drawTransform = cameraParams.CenterEyeTransform;
    }
    
    void Update()
    {
        if (CenterEyeAnchor)
        {
            
            line.points3[0] = CenterEyeAnchor.transform.position + CenterEyeAnchor.transform.forward.normalized * 0.5f;
            line.points3[1] = CenterEyeAnchor.transform.position + CenterEyeAnchor.transform.forward.normalized * 100.0f;
            line.Draw3D();
            //Debug.DrawRay(CenterEyeAnchor.transform.position, CenterEyeAnchor.transform.forward * 5000.0f);
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawRay(CenterEyeAnchor.transform.position, CenterEyeAnchor.transform.forward * 5000.0f);
    }
}
