using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
[RequireComponent (typeof (Camera))]
public class CameraSizer : MonoBehaviour
{

	private int PrevWidth;
	private int PrevHeight;
	private float PrevPPU;
	public float PixelsPerUnit = 100f;

	/*Lighting objects*/
	public bool Lighting = true;
	public float LightTextureScale = 1f;
	public Color AmbientLightColour = Color.gray;
	private GameObject LightQuad = null;
	private Camera LightCamera = null;
	private GameObject LightCamOb = null;

	void LightingResize()
	{
		if(!Lighting) return;
		float aspect = (float)PrevWidth/(float)PrevHeight;
		LightCamera.orthographicSize = this.GetComponent<Camera>().orthographicSize;
		Vector3 ls = new Vector3(aspect * 2f*LightCamera.orthographicSize, 2f*LightCamera.orthographicSize, 1f);
		LightQuad.transform.localScale = ls;

		RenderTexture rt = new RenderTexture((int)(PrevWidth*LightTextureScale), (int)(PrevHeight*LightTextureScale), 0);
		LightQuad.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = rt;
		LightCamera.backgroundColor = AmbientLightColour;
		RenderTexture ot = LightCamera.targetTexture;
		LightCamera.targetTexture = rt;
		if (ot != null)
		{
			ot.Release();
			DestroyImmediate(ot);
		}
	}

	void InitLighting()
	{
		if(!Lighting) return;
		Vector3 pos = this.transform.position + new Vector3(0f, 0f, 1f);
		LightQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		LightQuad.GetComponent<MeshRenderer>().sharedMaterial.shader = Shader.Find("Particles/Multiply");

		LightCamOb = new GameObject("Light Camera");
		LightCamera = LightCamOb.AddComponent<Camera>();
		LightCamera.orthographic = true;
		LightCamera.cullingMask = 1 << LayerMask.NameToLayer("Lights");
		this.GetComponent<Camera>().cullingMask -=  LightCamera.cullingMask;

		LightQuad.transform.parent = this.transform;
		LightCamOb.transform.parent = this.transform;
		LightQuad.transform.position = pos;
		LightCamOb.transform.position = pos;
		//LightingInit = true;
	}


	// Use this for initialization
	void Start ()
	{
		InitLighting();
		ResizeCamera();
	}

	void ResizeCamera()
	{
		PrevWidth = Screen.width;
		PrevHeight = Screen.height;
		PrevPPU = PixelsPerUnit;
		GetComponent<Camera>().orthographicSize = PrevHeight / (2.0f * PixelsPerUnit);
		LightingResize();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
		if(!(PrevWidth == Screen.width && PrevHeight == Screen.height && PixelsPerUnit == PrevPPU))
		{
			ResizeCamera();
		}
	}
}
