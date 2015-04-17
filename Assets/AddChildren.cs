using UnityEngine;
using System.Collections;

public class AddChildren : MonoBehaviour {

	// Use this for initialization
	void Start () {
		for(float x = -31.5f; x<32.5f; x+=1.0f)
		{
			for(float y = -31.5f; y<32.5f; y+=1.0f)
			{
				GameObject go = new GameObject();
				go.name = "Collider_("+x+","+y+")";
				BoxCollider2D boxcoll = go.AddComponent<BoxCollider2D>();
				boxcoll.size = new Vector2(1.0f,1.0f);
				boxcoll.offset= new Vector2(x, y);
				go.transform.position = transform.position;
				go.transform.parent = transform;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
