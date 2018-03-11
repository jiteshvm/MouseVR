using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine.VR;

public class DemoManager2 : MonoBehaviour {


    GameObject BackgroundPanel;
	float ElapsedTime;

	public GameObject TestPanel;

	public HandModel RightHandModel;
	//Hand RightLeapHand;
	Vector3 IndexFingerPos;
    //Vector3 IndexBoneDir;
    //Transform IndexBoneTransform;
    Texture2D TextureRef;

    public float RaycastDistance = 0.04f;

    int layerMask = 1 << 8;
    void Start () {

		// if(RightHandModel) {
		// 	RightLeapHand = RightHandModel.GetLeapHand();
		// }

        BackgroundPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
		BackgroundPanel.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
        BackgroundPanel.transform.localScale = new Vector3(2.0f, 2.0f, 0.02f);
        BackgroundPanel.GetComponent<MeshRenderer>().material.color = Color.magenta;
        BackgroundPanel.GetComponent<MeshRenderer>().enabled = false;
        Destroy(BackgroundPanel.GetComponent<BoxCollider>());
        InputTracking.Recenter();
    }

    void Restart()
    {

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
                    
                }
                if(parameters[0].Equals("InterTargetDistance", StringComparison.CurrentCultureIgnoreCase))
                {
                   
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
                Renderer rend = hit.transform.GetComponent<Renderer>();
                MeshCollider meshCollider = hit.collider as MeshCollider;

                if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                    return;

                TextureRef = rend.material.mainTexture as Texture2D;
                Vector2 pixelUV = hit.textureCoord;
                pixelUV.x *= TextureRef.width;
                pixelUV.y *= TextureRef.height;
                Circle(TextureRef, (int)pixelUV.x, (int)pixelUV.y, 64, Color.black);
            }
			
		} 
    }

    void Update()
    {
        ElapsedTime += Time.deltaTime;
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

        }

        if(Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            //Debug.Log("Oculus Remote Button 2");
        }
        
        if(Input.GetKey(KeyCode.UpArrow))
        {

        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
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
    }
    private void OnDrawGizmos()
    {
        //Gizmos.DrawRay(CenterEyeAnchor.transform.position, CenterEyeAnchor.transform.forward * 5000.0f);
    }

    public void Circle(Texture2D tex, int cx, int cy, int r, Color col)
    {
        int x, y, px, nx, py, ny, d;
        Color32[] tempArray = tex.GetPixels32();
        Debug.Log("temp arary length : " + tempArray.Length);
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
                if (indx1 >= 0 && indx1 < tempArray.Length)
                    tempArray[indx1] = col;

                int indx2 = py * tex.width + nx;
                if (indx2 >= 0 && indx2 < tempArray.Length)
                    tempArray[indx2] = col;

                int indx3 = ny * tex.width + px;
                if (indx3 >= 0 && indx3 < tempArray.Length)
                    tempArray[indx3] = col;

                int indx4 = ny * tex.width + nx;
                if (indx4 >= 0 && indx4 < tempArray.Length)
                    tempArray[indx4] = col;

                //IsProgressUpdated = false;
            }
        }

        tex.SetPixels32(tempArray);
        tex.Apply();
    }
}
