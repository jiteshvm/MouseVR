using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTexture : MonoBehaviour {


	// Use this for initialization

    public Color TextureColor;
    Texture2D TexRef;

	void Start () {
		Renderer rend = GetComponent<Renderer>();

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null)
            return;

		TexRef = Instantiate(rend.material.mainTexture) as Texture2D;
        rend.material.mainTexture = TexRef;
        Disable();
    }
	
    public void Enable()
    {
        MeshCollider collider = GetComponent<MeshCollider>();
        collider.enabled = true;

        Color32[] clearPixels = new Color32[TexRef.width * TexRef.height];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = TextureColor;
        }

        TexRef.SetPixels32(clearPixels);
        TexRef.Apply();
    }

    public void Disable()
    {
        MeshCollider collider = GetComponent<MeshCollider>();
        collider.enabled = false;

        Color32[] clearPixels = new Color32[TexRef.width * TexRef.height];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = Color.black;
        }

        TexRef.SetPixels32(clearPixels);
        TexRef.Apply();
    }
	// Update is called once per frame
	void Update () {
		
	}
}
