using UnityEngine;
using System.Collections;

public class PathFindTest : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	}
	
	Vector2[] coords;
	float lastRun = 0f;
	int x = 0, y = 0;
	Vector2 startpos = new Vector2(318.8f, 321.5f);
	Vector2 endpos = new Vector2(491.4f, 40.2f);
	// Update is called once per frame
	void Update ()
	{	
		//int startx=3, starty=3;

		if(Input.GetKey(KeyCode.Space) && Time.realtimeSinceStartup > lastRun + 0.25f)
		{
			TileMapScript tm = GetComponent<TileMapScript>();
			//for(int i = 0; i < 10000; i++) 
			endpos.x = Random.Range (0f,640f);
			endpos.y = Random.Range (0f,640f);
			startpos.x = Random.Range (0f,640f);
			startpos.y = Random.Range (0f,640f);
			coords = tm.GetPath(startpos, endpos); //(startx,starty,x,y);
			Debug.Log("Path: (" + startpos.ToString() + ") to (" + endpos.ToString() + "), isnull: " + (coords == null).ToString());
			//Debug.Log("Coords got to (" + x.ToString() + ", " + y.ToString() + "), isnull: " + (coords == null).ToString());
			lastRun = Time.realtimeSinceStartup;
			if(null == coords) lastRun -= 0.1f;
			x += 1;
			if (x > 9)
			{
				x = 0;
				y++;
				y%=10;
			}
		}
		
		Debug.DrawLine(startpos, endpos, Color.yellow);
		
		if(!(null == coords))
		{
			Vector3 start = new Vector3(), end = new Vector3();
			//end.x = startx  * 64 + 32;
			//end.y = starty * 64 + 32;
			end = startpos;
			Color c = Color.red;
			for(int i = 0; i <= coords.GetUpperBound(0); i++)
			{
				start.x = coords[i].x;// * 64 + 32;
				start.y = coords[i].y;// * 64 + 32;
				Debug.DrawLine(start, end, c);
				end = start;
				if(Color.red == c)
				{
					c = Color.blue;
				}
				else if (Color.blue == c)
				{
					c = Color.green;
				}
				else
				{
					c = Color.red;
				}
			}
		}
	}
}
