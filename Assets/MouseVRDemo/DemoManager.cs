using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    List<int> TargetsOrder;

    List<int> RandomButtonIndices;

    List<float> TimeList;

    List<float> DistanceList;

    GameObject InteractionManagerObj;
    public InteractionManager InteractionManagerRef;

    GameObject BackgroundPanel;

    Transform CenterEyeTransform;

    Vector3 EyeForwardDirection;

    float DistanceFromEye = 0.3f;

    float StartTime = 0.0f;

    float EndTime;

    float ElapsedTime;

    int CurrentTarget;

    void Start () {
        //LinePoints = new List<Vector3>();
        //LinePoints.Add(Vector3.zero);
        //LinePoints.Add(Vector3.zero);
        //LineDebug = new VectorLine("line", LinePoints, 4.0f);
        //LineDebug.smoothWidth = true;
        ReadSettingsFromFile();
        BackgroundPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        BackgroundPanel.transform.localScale = new Vector3(2.0f, 2.0f, 0.02f);
        BackgroundPanel.GetComponent<MeshRenderer>().material.color = Color.green;
        BackgroundPanel.GetComponent<MeshRenderer>().enabled = false;
        Destroy(BackgroundPanel.GetComponent<BoxCollider>());

        VectorManager.useDraw3D = true;

        List<Vector3> testPoints = new List<Vector3>();

        TargetWidthMeters = TargetWidth / 1000.0f;
        InterTargetDistanceMeters = InterTargetDistance / 1000.0f;
        TargetRadius = (InterTargetDistanceMeters * NumberOfTargets) / (2.0f * Mathf.PI);

        //Debug.Log("TargetWidth : " + TargetWidth);
        //Debug.Log("InterTargetDistance : " + InterTargetDistance);

        //Debug.Log("Outer Circle Radius : " + TargetRadius);
        //Debug.Log(TargetWidthMeters);        
        
        ElapsedTime = 0.0f;

        InteractionManagerObj = GameObject.Find("Interaction Manager");
        if(InteractionManagerObj)
        {
            InteractionManagerRef = InteractionManagerObj.GetComponent<InteractionManager>();
        }
        
        if(CenterObj && TargetPrefab && NumberOfTargets > 0)
        {
            float CurrentAngle = 0.0f;
            float AngleDelta = 2.0f * Mathf.PI / NumberOfTargets;
            TargetsList = new List<GameObject>();
            RandomButtonIndices = new List<int>();
            TimeList = new List<float>();
            DistanceList = new List<float>();
            TargetsOrder = new List<int>();

            for (int i = 0; i < NumberOfTargets; ++i)
            {
                float x = CenterObj.transform.position.x + TargetRadius * Mathf.Cos(CurrentAngle);
                float y = CenterObj.transform.position.y + TargetRadius * Mathf.Sin(CurrentAngle);
                float z = CenterObj.transform.position.z;
                GameObject NewCircleObj = GameObject.Instantiate(TargetPrefab, new Vector3(x, y, z), Quaternion.identity);
                NewCircleObj.name = "Button" + i;
                NewCircleObj.transform.GetChild(0).gameObject.GetComponent<Rigidbody>().detectCollisions = false;
                //NewCircleObj.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().OnPress.AddListener(OnButtonPress);
                NewCircleObj.transform.GetChild(0).GetChild(0).localScale = new Vector3(TargetWidthMeters, TargetWidthMeters, 0.01f);
                NewCircleObj.transform.SetParent(BackgroundPanel.transform);
                TargetsList.Add(NewCircleObj);
                RandomButtonIndices.Add(i);
                if( i > 0)
                {
                    testPoints.Add(TargetsList[i-1].transform.position);
                    testPoints.Add(TargetsList[i].transform.position);
                }
                
                CurrentAngle += AngleDelta;
            }
            testPoints.Add(TargetsList[NumberOfTargets - 1].transform.position);
            testPoints.Add(TargetsList[0].transform.position);
        }

        //VectorLine testLine = new VectorLine("test", testPoints, 8);
        // for drawing lines between the targets
        //testLine.Draw3D();

        LeapVRCameraControl.OnValidCameraParams += OnValidCameraParams;
        InputTracking.Recenter();
        SetPanelDirectionAndPosition();
    }

    void Restart()
    {
        RandomButtonIndices.Clear();
        TimeList.Clear();
        DistanceList.Clear();
        TargetsOrder.Clear();

        for (int i = 0; i < TargetsList.Count; ++i)
        {
            TargetsList[i].transform.GetChild(0).gameObject.GetComponent<Rigidbody>().detectCollisions = false;
            TargetsList[i].transform.GetChild(0).gameObject.GetComponent<SimpleInteractionGlow>().defaultColor = Color.black;
            TargetsList[i].transform.GetChild(0).gameObject.GetComponent<SimpleInteractionGlow>().hoverColor = Color.black;
            TargetsList[i].transform.GetChild(0).gameObject.GetComponent<SimpleInteractionGlow>().primaryHoverColor = Color.black;
            RandomButtonIndices.Add(i);
        }
        ActivateRandomTarget();
    }

    private void ReadSettingsFromFile()
    {
        string file_path = "ConfigSettings.txt";
        try
        {
            StreamReader file_reader = new StreamReader(file_path);
            while(!file_reader.EndOfStream)
            {
                string input_line = file_reader.ReadLine( );
                input_line = input_line.Trim();
                string[] parameters = input_line.Split(':');
                if(parameters.Length != 2)
                    continue;
                
                parameters[0] = parameters[0].Trim();
                parameters[1] = parameters[1].Trim();

                if(parameters[0].Equals("TargetWidth", StringComparison.CurrentCultureIgnoreCase))
                {
                    TargetWidth = (float) Convert.ToDouble(parameters[1]);
                }
                if(parameters[0].Equals("InterTargetDistance", StringComparison.CurrentCultureIgnoreCase))
                {
                    InterTargetDistance = (float) Convert.ToDouble(parameters[1]);
                }
            }
            file_reader.Close( );  
        
        }
        catch(FileNotFoundException fe)
        {
            Debug.Log(fe.Message);
        }
        
    }


    public void ActivateRandomTarget()
    {
        if(RandomButtonIndices.Count > 0)
        {
            StartTime = ElapsedTime;
            int RandomIndex = UnityEngine.Random.Range(0, RandomButtonIndices.Count);
            //Debug.Log("random index : " + RandomIndex);
            int RandomValue = RandomButtonIndices[RandomIndex];
            CurrentTarget = RandomValue;
            //Debug.Log(" random value : " + RandomValue);
            GameObject ButtonInteractionRef = TargetsList[RandomValue].transform.GetChild(0).gameObject;
            ButtonInteractionRef.GetComponent<Rigidbody>().detectCollisions = true;
            ButtonInteractionRef.GetComponent<SimpleInteractionGlow>().defaultColor = Color.blue;
            ButtonInteractionRef.GetComponent<SimpleInteractionGlow>().hoverColor = Color.blue;
            ButtonInteractionRef.GetComponent<SimpleInteractionGlow>().primaryHoverColor = Color.blue;
            RandomButtonIndices.RemoveAt(RandomIndex);
            //Debug.Log("Count : " + RandomButtonIndices.Count);
        }
        
    }

    private void OnUpdateFrame(Frame frame)
    {
        //Debug.Log("OnUpdateFrame");
    
    }

    private void OnValidCameraParams(LeapVRCameraControl.CameraParams cameraParams)
    {
        CenterEyeTransform = cameraParams.CenterEyeTransform;
    }

    void SetPanelDirectionAndPosition()
    {
        SetPanelPosition();
        SetPanelDirection();
    }

    void SetPanelDirection()
    {
        if(BackgroundPanel && CenterEyeTransform)
        {
            EyeForwardDirection = CenterEyeTransform.forward;
            BackgroundPanel.transform.LookAt((CenterEyeTransform.position + (EyeForwardDirection * 100.0f)));
        }
    }

    void SetPanelPosition()
    {
        if(BackgroundPanel && CenterEyeTransform)
        {
            BackgroundPanel.transform.position = CenterEyeTransform.position + (EyeForwardDirection * DistanceFromEye);
        }
        
    }

    void FixedUpdate()
    {
        //LineDebug.Draw3D();
    }
    void Update()
    {
        ElapsedTime += Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.C))
        {
            //Debug.Log("Oculus Remote Button 1");
            SetPanelDirection();
        }

        if(Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            //Debug.Log("Oculus Remote Button 2");
        }
        
        if(Input.GetKey(KeyCode.UpArrow))
        {
            DistanceFromEye += 0.004f;
            SetPanelPosition();
        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
            DistanceFromEye -= 0.004f;
            SetPanelPosition();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Restart();
        }

        if(Input.GetKeyDown(KeyCode.D))
        {
            LogTimeAndDistance(0.0f);
        }
    }

    public void LogTimeAndDistance(float dist)
    {
        EndTime = ElapsedTime;
        TimeList.Add(EndTime - StartTime);
        DistanceList.Add(dist);
        TargetsOrder.Add(CurrentTarget);

        ActivateRandomTarget();
        if(TimeList.Count >= NumberOfTargets || DistanceList.Count >= NumberOfTargets)
        {
            string outputFile = string.Format("{0:yyyy-MM-dd_HH-mm-ss}",DateTime.Now);
            outputFile = outputFile + ".txt";
            StreamWriter swr = File.CreateText(outputFile);
            for(int i = 0; i < NumberOfTargets; ++i)
            {
                swr.WriteLine ("{0}.  Time : {1} ,  Accuracy : {2}", TargetsOrder[i], TimeList[i], DistanceList[i]);
                //Debug.Log((i+1) + ". " + " Time : " + TimeList[i] + " , Accuracy : " + DistanceList[i]);
            }
            swr.Close();
        }
    }
    private void OnDrawGizmos()
    {
        //Gizmos.DrawRay(CenterEyeAnchor.transform.position, CenterEyeAnchor.transform.forward * 5000.0f);
    }
}
