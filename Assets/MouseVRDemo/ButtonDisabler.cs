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
        gameObject.GetComponent<InteractionButton>().OnPress.AddListener(OnButtonPress);
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
                Vector tmp = ih.leapHand.Fingers[(int)Finger.FingerType.TYPE_INDEX].TipPosition;
                //Vector3 fingerPos3 = tmp.ToVector3();
                Vector2 fingerPos = new Vector2(tmp.x, tmp.y);
                Vector2 objPos = new Vector2(transform.parent.position.x, transform.parent.position.y);
                float dist = Vector2.Distance(fingerPos, objPos);
                //float dist = Vector3.Distance(transform.parent.position, fingerPos3);
                Debug.Log(dist * 1000.0f);
                //DebugCube.transform.position = ih.leapHand.Fingers[(int)Finger.FingerType.TYPE_INDEX].TipPosition.ToVector3();
            }
        }

        DemoManagerRef.ActivateRandomTarget();
    }

	// Update is called once per frame
	void Update () {
		
	}
}
