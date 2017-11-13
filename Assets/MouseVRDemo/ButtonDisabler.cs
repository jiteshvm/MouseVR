using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

public class ButtonDisabler : MonoBehaviour {

    DemoManager DemoManagerRef;

	// Use this for initialization
	void Start () {
        gameObject.GetComponent<InteractionButton>().OnPress.AddListener(OnButtonPress);
        GameObject DemoManagerObj = GameObject.Find("DemoManager");
        DemoManagerRef = DemoManagerObj.GetComponent<DemoManager>();
    }
	
    void OnButtonPress()
    {
        //Debug.Log("OnButtonPress : " + gameObject.transform.parent.name);
        //gameObject.GetComponent<InteractionButton>().enabled = false;
        //gameObject.SetActive(false);
        GetComponent<Rigidbody>().isKinematic = true;
        DemoManagerRef.ActivateRandomTarget();
    }

	// Update is called once per frame
	void Update () {
		
	}
}
