using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine.VR;

public struct LogData
{
    public float Time;
    public int Index;
}
public class DemoManager2 : MonoBehaviour {

    // all the targets are parented to this object so it can be moved around easily
    GameObject BackgroundPanel;
	float ElapsedTime;
    public int Rows;
    public int Columns;
    public Vector2 PanelSize;
    public GameObject TestPanel;
	public HandModel RightHandModel;
	Vector3 IndexFingerPos;
    //Vector3 IndexBoneDir;
    //Transform IndexBoneTransform;
    GameObject CurrentPanel;
    Texture2D TextureRef;
    public float RaycastDistance = 0.04f;
    public Color32[] Colors;
    int CurrentTarget;
    List<GameObject> TargetsList;
    List<LogData> LogDataList;
    List<int> RandomIndices;
    bool IsProgressUpdated = false;
    Int32 count;
    int layerMask = 1 << 8;
    Transform CenterEyeTransform;
    Vector3 EyeForwardDirection;
    float DistanceFromEye = 0.3f;
    float StartTime = 0.0f;

    public int FingerRadius = 98;
    void Start () {

        ReadSettingsFromFile();

        TargetsList = new List<GameObject>();
        RandomIndices = new List<int>();
        LogDataList = new List<LogData>();

        CurrentTarget = 0;
        BackgroundPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
		BackgroundPanel.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
        //BackgroundPanel.transform.localScale = new Vector3(2.0f, 2.0f, 0.02f);
        BackgroundPanel.GetComponent<MeshRenderer>().material.color = Color.magenta;
        BackgroundPanel.GetComponent<MeshRenderer>().enabled = false;
        BackgroundPanel.name = "BackgroundPanel";
        Destroy(BackgroundPanel.GetComponent<BoxCollider>());

        Vector3 PanelPosition = new Vector3(BackgroundPanel.transform.position.x, BackgroundPanel.transform.position.y, BackgroundPanel.transform.position.z);
        int colorindx = 0;
        int panelindx = 0;
        for(int i = 0; i < Columns; ++i)
        {
            for( int j = 0; j < Rows; ++j)
            {
                GameObject NewPanel = GameObject.Instantiate(TestPanel, PanelPosition, TestPanel.transform.rotation);
                NewPanel.name = "Panel_" + (i+1) + "" + (j+1);
                NewPanel.transform.SetParent(BackgroundPanel.transform);
                NewPanel.transform.localScale = new Vector3(PanelSize.x, 0.1f, PanelSize.y);
                ResetTexture ResetScript = NewPanel.GetComponent<ResetTexture>();
                ResetScript.TextureColor = Colors[colorindx];
                PanelPosition.x = PanelPosition.x + PanelSize.x * 10.0f;
                TargetsList.Add(NewPanel);
                
                RandomIndices.Add(panelindx);
                panelindx++;

                colorindx++;
                if(colorindx >= Colors.Length)
                    colorindx = 0;
            }
            PanelPosition.y = PanelPosition.y - PanelSize.y * 10.0f;
            PanelPosition.x = BackgroundPanel.transform.position.x;
        }
        //Debug.Log("RandomIndices Count : " +  RandomIndices.Count);
        LeapVRCameraControl.OnValidCameraParams += OnValidCameraParams;
        InputTracking.Recenter();
        SetPanelDirectionAndPosition();
        //StartCoroutine(LateStart(1.0f));
    }

    IEnumerator LateStart(float waitTime)
     {
         yield return new WaitForSeconds(waitTime);
         Restart();
         TargetsList[0].GetComponent<ResetTexture>().Enable();
     }
    void Restart()
    {
        RandomIndices.Clear();
        for (int i = 0; i < TargetsList.Count; ++i)
        {
            TargetsList[i].GetComponent<ResetTexture>().Disable();
            RandomIndices.Add(i);
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

                if(parameters[0].Equals("Rows", StringComparison.CurrentCultureIgnoreCase))
                {
                    Rows = (int) Convert.ToUInt16(parameters[1]);
                }
                if(parameters[0].Equals("Columns", StringComparison.CurrentCultureIgnoreCase))
                {
                   Columns = (int) Convert.ToUInt16(parameters[1]);
                }
                if(parameters[0].Equals("FingerRadius", StringComparison.CurrentCultureIgnoreCase))
                {
                   FingerRadius = (int) Convert.ToUInt16(parameters[1]);
                }
                if(parameters[0].Equals("PanelSizeX", StringComparison.CurrentCultureIgnoreCase))
                {
                   PanelSize.x = (float) Convert.ToDouble(parameters[1]);
                }
                if(parameters[0].Equals("PanelSizeY", StringComparison.CurrentCultureIgnoreCase))
                {
                   PanelSize.y = (float) Convert.ToDouble(parameters[1]);
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
        if(RandomIndices.Count > 0)
        {
            StartTime = ElapsedTime;
            
            // random order
            int RandomIndex = UnityEngine.Random.Range(0, RandomIndices.Count);
            int RandomValue = RandomIndices[RandomIndex];
            CurrentTarget = RandomValue;
            //Debug.Log(" random value : " + RandomValue);
            TargetsList[RandomValue].GetComponent<ResetTexture>().Enable();
            RandomIndices.RemoveAt(RandomIndex);

            //linear order
            //TargetsList[CurrentTarget].GetComponent<ResetTexture>().Enable();
            //RandomIndices.RemoveAt(0);
            //CurrentTarget++;
            //Debug.Log("CurrentTarget : " + CurrentTarget);
        }
    }



    void FixedUpdate()
    {
        //LineDebug.Draw3D();
		 RaycastHit hit;
		if(TestPanel) {
            
            //Vector3 Dir = TestPanel.transform.position - IndexFingerPos;
            Vector3 Dir = -TestPanel.transform.up;
            //Vector3 Dir = -IndexBoneTransform.up;
            Debug.DrawLine(IndexFingerPos, IndexFingerPos + (Dir * RaycastDistance), Color.green);
            //Debug.DrawRay(IndexFingerPos, Dir, Color.blue); 
			if (Physics.Raycast(IndexFingerPos, Dir, out hit, RaycastDistance, layerMask)) {
				//print("Found an object - distance: " + hit.collider.name);
                CurrentPanel = hit.transform.gameObject;
                Renderer rend = hit.transform.GetComponent<Renderer>();
                MeshCollider meshCollider = hit.collider as MeshCollider;

                if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                    return;

                TextureRef = rend.material.mainTexture as Texture2D;
                Vector2 pixelUV = hit.textureCoord;
                pixelUV.x *= TextureRef.width;
                pixelUV.y *= TextureRef.height;
                
                //Circle(TextureRef, (int)pixelUV.x, (int)pixelUV.y, 64, Color.black);
                CircleOld(TextureRef, (int)pixelUV.x, (int)pixelUV.y, FingerRadius, Color.black);
            }
			
		} 
    }

    void Update()
    {
        ElapsedTime += Time.deltaTime;
        UpdateProgress();
		for (int i = 0; i < HandModel.NUM_FINGERS;i++)
		{
			FingerModel finger = RightHandModel.fingers[i];
			if(finger.fingerType == Finger.FingerType.TYPE_INDEX) {
                //IndexBoneTransform = finger.bones[(int)Bone.BoneType.TYPE_DISTAL];
				IndexFingerPos = finger.GetTipPosition();
				//IndexBoneDir = finger.GetBoneDirection((int)Bone.BoneType.TYPE_DISTAL);
				// draw ray from finger tips (enable Gizmos in Game window to see)
				//Debug.DrawRay(finger.GetTipPosition(), finger.GetRay().direction, Color.red); 
			
			}
				
			
		}
        if(Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.C))
        {
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
    }

    public void LogOutput()
    {
        LogData OutputData = new LogData();
        OutputData.Index = CurrentTarget+1;
        OutputData.Time = ElapsedTime - StartTime;
        LogDataList.Add(OutputData);

        if(RandomIndices.Count > 0)
        {
            ActivateRandomTarget();
        }
        else
        {
            string outputFile = string.Format("{0:yyyy-MM-dd_HH-mm-ss}",DateTime.Now);
            outputFile = outputFile + ".txt";
            StreamWriter swr = File.CreateText(outputFile);
            for(int i = 0; i < LogDataList.Count; ++i)
            {
                swr.WriteLine ("{0}.  Time : {1}", LogDataList[i].Index, LogDataList[i].Time);
            }
            swr.Close();
        }
    }
    private void OnDrawGizmos()
    {
        //Gizmos.DrawRay(CenterEyeAnchor.transform.position, CenterEyeAnchor.transform.forward * 5000.0f);
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
    public void Circle(Texture2D tex, int cx, int cy, int r, Color col)
    {
        int x, y, px, nx, py, ny, d;
        Color32[] tempArray = tex.GetPixels32();
        //Debug.Log("temp arary length : " + tempArray.Length);
        for (x = 0; x <= r; x++)
        {
            d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
            for (y = 0; y <= d; y++)
            {
                px = cx + x;
                nx = cx - x;
                py = cy + y;
                ny = cy - y;

                int indx1 = py * tex.width + px;
                if (indx1 >= 0 && indx1 < tempArray.Length) {
                    tempArray[indx1] = col;   
                }
                    

                int indx2 = py * tex.width + nx;
                if (indx2 >= 0 && indx2 < tempArray.Length) {
                    tempArray[indx2] = col;
                }
                    

                int indx3 = ny * tex.width + px;
                if (indx3 >= 0 && indx3 < tempArray.Length) {
                    tempArray[indx3] = col;
                }
                    

                int indx4 = ny * tex.width + nx;
                if (indx4 >= 0 && indx4 < tempArray.Length) {
                    tempArray[indx4] = col;
                }

                IsProgressUpdated = false;
            }
        }

        tex.SetPixels32(tempArray);
        tex.Apply();
    }

    public void CircleOld(Texture2D tex, int cx, int cy, int r, Color col)
     {
         int x, y, px, nx, py, ny, d;
         
         for (x = 0; x <= r; x++)
         {
             d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
             for (y = 0; y <= d; y++)
             {
                 px = cx + x;
                 nx = cx - x;
                 py = cy + y;
                 ny = cy - y;

                 if(tex.GetPixel(px,py) != col)
                    count++;
                
                if(tex.GetPixel(nx,py) != col)
                    count++;
                
                if(tex.GetPixel(px,ny) != col)
                    count++;
                
                if(tex.GetPixel(nx,ny) != col)
                    count++;

                 tex.SetPixel(px, py, col);
                 tex.SetPixel(nx, py, col);
  
                 tex.SetPixel(px, ny, col);
                 tex.SetPixel(nx, ny, col);

                IsProgressUpdated = false;
             }
         }
         tex.Apply();    
     }

    public void UpdateProgress()
    {
        if(!IsProgressUpdated)
        {
            //Color32[] tempArray = TextureRef.GetPixels32();
            float progress = (count / 262144.0f) * 100.0f;
            //Debug.Log("progress : " + progress);
            //Debug.Log("TextureRef count : " + tempArray.Length);
            IsProgressUpdated = true;
            if(progress > 100.0f && CurrentPanel) {
                CurrentPanel.GetComponent<ResetTexture>().Disable();
                count = 0;
                LogOutput();
            }
            //TextureRef = null;
        }
    }

    private void OnValidCameraParams(LeapVRCameraControl.CameraParams cameraParams)
    {
        CenterEyeTransform = cameraParams.CenterEyeTransform;
    }
}
