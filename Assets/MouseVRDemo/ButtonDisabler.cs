using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Interaction;

public class ButtonDisabler : MonoBehaviour {

    DemoManager DemoManagerRef;

	// Use this for initialization
	void Start () {
        GetComponent<InteractionButton>().OnPress.AddListener(OnButtonPress);
        GameObject DemoManagerObj = GameObject.Find("DemoManager");
        DemoManagerRef = DemoManagerObj.GetComponent<DemoManager>();
        //Debug.Log(transform.parent.position.x + " , " + transform.parent.position.y + " , " + transform.parent.position.z);
    }
	
    void OnButtonPress()
    {
        //Debug.Log("OnButtonPress : " + gameObject.transform.parent.name);
        GetComponent<Rigidbody>().detectCollisions = false;
        GetComponent<SimpleInteractionGlow>().defaultColor = Color.black;
        GetComponent<SimpleInteractionGlow>().hoverColor = Color.black;
        GetComponent<SimpleInteractionGlow>().primaryHoverColor = Color.black;
        //GetComponent<InteractionButton>().enabled = false;

        foreach (InteractionController ic in DemoManagerRef.InteractionManagerRef.interactionControllers)
        {
            InteractionHand ih = (InteractionHand)ic;
            if (ih && ih.isTracked && ih.contactingObjects.Count > 0)
            {
                Vector TipPosition = ih.leapHand.Fingers[(int)Finger.FingerType.TYPE_INDEX].TipPosition;
                Vector2 TipPosition2D = new Vector2(TipPosition.x, TipPosition.y);
                Vector2 objPos2D = new Vector2(transform.parent.position.x, transform.parent.position.y);
                float dist2D = Vector2.Distance(TipPosition2D, objPos2D);
                float floatdist2D_mm = dist2D * 1000.0f;
                DemoManagerRef.LogTimeAndDistance(floatdist2D_mm);
                //DebugCube.transform.position = ih.leapHand.Fingers[(int)Finger.FingerType.TYPE_INDEX].TipPosition.ToVector3();
            }
        }

        
    }

	// Update is called once per frame
	void Update () {
	}
}
