using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTexture : MonoBehaviour {


	// Use this for initialization
	void Start () {
		Renderer rend = GetComponent<Renderer>();

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null)
            return;

		Texture2D tex = Instantiate(rend.material.mainTexture) as Texture2D;
		rend.material.mainTexture = tex;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
