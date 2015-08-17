using UnityEngine;
using System.Collections;

/*
 *  Colthor's Tile Map Script for Ludum Dare
 * 
 *  Basic docs:
 * This generates a map of Squares_X by Squares_Y tiles (which are each Square_Size units to a side) with auto-tiled walls
 * (so they look like they're all joined neatly) and adds box colliders for the walls as children to the game object
 * it's attached to. Which needs a mesh renderer, mesh filter, and probably a Rigidbody2D if you want the colliders to do much.
 * 
 * The texture required is a grid of 4 by 8 tiles, and internally they're thought of as the numbers:
 * 
 *  0  1  2  3
 *  4  5  6  7
 *  8  9 10 11
 * 12 13 14 15
 * 16 17 18 19
 * 20 21 22 23
 * 24 25 26 27
 * 28 29 30 31
 * 
 * A sample texture, "32tiles.png", is included. It will probably make the following explanation clearer.
 * The level data is simply an ascii string. Characters representing single hex digits, ie. '0' - 'F', are the first sixteen tiles. These aren't given colliders.
 * The second 16 (ie. 16-31) are the auto-tiled walls, which're all represented by 'X'.
 * These start at 16 and have a value added on based on if any of the adjacent tiles are walls, based on these numbers:
 * 
 *    1
 * 8  0  2
 *    4
 * 
 * Where 0 is the tile being calculated. This means a single wall is wall tile 0 (ie. 16) an L shape is wall tile 3 (ie. 19), | is wall 5, + is  wall 15 and so forth.
 * 
 * Any characters other than 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, A, B, C, D, E, F, X  in LevelData are ignored. Letters must be upper case.
 * 
 * It runs in edit mode, which means you can see the map in the editor - but it can't automatically delete children in edit mode yet, so you might have to occasionally 
 * do that manually. As soon as you hit run they'll all be erased anyway, so that's not a problem at runtime. You can of course just prevent it executing in
 * edit mode to fix the problem entirely...
 */



[ExecuteInEditMode]
public class TileMapScript : MonoBehaviour
{
	struct MapSquare
	{
		public char RawData;
		public int TextureNum;
		public bool IsWall;
		public bool HasCollider;
	}

	MapSquare[,] MapArray;
	public int Squares_X = 10;
	public int Squares_Y = 10;
	public int Square_Size = 64;
	public bool Invert_Colliders = false; //if true doesn't generate colliders on walls, does on "empties".

	//FUDGE FACTORS. The correct value for these is 0, but Unity's anti-aliasing causes alignment artifacts.
	//Fiddling with them can reduce the artifacts. I found 0.000195 for TL to be about right, but YMMV.
	//It maybe/shouldn't even happen on other systems...
	//WARNING: Setting either of these too large will break 1:1 pixel mapping.
	public float iotaTL = 0.0f; //0.000195f; //fudge factor to prevent texture alignment artifacts, top/left.
	public float iotaBR = 0.0f; //fudge factor to prevent texture alignment artifacts, bottom/right.

	public string LevelData = "XXXXXXXXXX"
							+ "XX012345XX"
							+ "X0X6700X0X"
							+ "X0XX00XX0X"
							+ "XXXXXX0XXX"
							+ "X0X0XX0X0X"
							+ "XX0X00XX0X"
							+ "X0X0089X0X"
							+ "XXABCDEFXX"
							+ "XXXXXXXXXX";

	PathfindingImpl pathfinder;

	public Vector2[] GetPathGrid(int start_x, int start_y, int end_x, int end_y)
	{
		if(pathfinder == null) return null;

		return pathfinder.FindPath(start_x, start_y, end_x, end_y);
	}

	public Vector2[] GetPath(Vector2 start, Vector2 end)
	{
		//convert model-space coords to grid squares, and back
		int sx = (int)start.x/Square_Size, sy = (int)start.y/Square_Size;
		int ex = (int)end.x/Square_Size, ey = (int)end.y/Square_Size;

		if(sx == ex && sy == ey) //in same square already, just return end point.
		{
			if(CollideAtGrid(sx, sy)) return null;
			Vector2[] rv = new Vector2[1];
			rv[0] = end;
			return rv;
		}

		Vector2[] points = GetPathGrid(sx, sy, ex, ey);
		if(null == points) return null;
		
		Vector2 offset = start;
		offset.x %= Square_Size;
		offset.y %= Square_Size;
		Vector2 off_step = end;
		off_step.x %= Square_Size;
		off_step.y %= Square_Size;
		//offset = off_step;
		off_step = (off_step - offset)/(float) (points.GetUpperBound(0)+1);

		/*if(ex-sx != 0 && Mathf.Sign(off_step.x) != Mathf.Sign(ex-sx) ) off_step.x = 0f;
		if(ey-sy != 0 && Mathf.Sign(off_step.y) != Mathf.Sign(ey-sy)) off_step.y = 0f;*/

		for(int i = 0; i < points.GetUpperBound(0); i++)
		{
			points[i] = points[i]*Square_Size + offset + off_step * (i+1);
		}
		points[points.GetUpperBound(0)] = end;

		return points;
	}

	// Use this for initialization
	void Start ()
	{
		GenerateMap();
	}

	bool IsValidCharacter(char c)
	{
		return ('0' <= c && c <= '9') || ('A' <= c && c <= 'F') || c== 'X';
	}

	int CharacterToTileIndex(char c)
	{
		if ('0' <= c && c <= '9') 
		{
			return c - '0';
		}
		else if ('A' <= c && c <= 'F')
		{
			return 10 + (c - 'A');
		}
		else if ( 'X' == c )
		{
			return 16;
		}
		else
		{
			return 0;
		}
	}

	char GetNextValidCharacter(string lvlDat, ref int startIndex)
	{
		//Debug.Log("startIndex: " + startIndex);
		startIndex++;
		while(startIndex < lvlDat.Length && !IsValidCharacter(lvlDat[startIndex]))
		{
			startIndex+=1;
		}
		if(startIndex >= lvlDat.Length)
		{
			//Debug.Log("startIndex: " + startIndex + " greater than level data length: " + lvlDat.Length);
			return '0';
		}
		else
		{
			//Debug.Log("startIndex: " + startIndex + " result: " + lvlDat[startIndex]);
			return (lvlDat[startIndex]);
		}
		
		//Debug.Log("startIndex: " + startIndex + " char: " + lvlDat[startIndex]);
	}

	void PopulateMapArray()
	{
		int CharIndex = -1;
		
		
		for(int y = 0; y < Squares_Y; y++)
		{
			for(int x = 0; x < Squares_X; x++)
			{
				MapArray[x, y].RawData = GetNextValidCharacter(LevelData, ref CharIndex);
				MapArray[x, y].TextureNum = CharacterToTileIndex(MapArray[x, y].RawData);
				MapArray[x, y].IsWall = (16 == MapArray[x, y].TextureNum);
				MapArray[x, y].HasCollider = false;
			}
		}
	}

	void DoWallAutotiling()
	{
		for(int y = 0; y < Squares_Y; y++)
		{
			for(int x = 0; x < Squares_X; x++)
			{
				int WallTotal = 0;

				if(MapArray[x, y].IsWall)
				{
					if(y < (Squares_Y-1) && MapArray[x, y+1].IsWall) WallTotal += 4; //up
					if(x < (Squares_X-1) && MapArray[x+1, y].IsWall) WallTotal += 2; //right
					if(y > 0 && MapArray[x, y-1].IsWall) WallTotal += 1; //down
					if(x > 0 && MapArray[x-1, y].IsWall) WallTotal += 8; //left

					MapArray[x, y].TextureNum += WallTotal;
				}
			}
		}
	}

	bool CollideAtGrid(int x, int y)
	{
		if(Invert_Colliders)
		{
			return !MapArray[x,y].IsWall;
		}
		else
		{
			return MapArray[x,y].IsWall;
		}
	}

	void GenerateColliderAt(int xMin, int yMin)
	{
		int xMax = xMin;
		int yMax = yMin;

		//Find how far right the wall extends
		while(xMax < Squares_X-1 && CollideAtGrid(xMax+1,yMin) && !MapArray[xMax+1, yMin].HasCollider)
		{
			xMax++;
		}

		//find how far in +ve Y (up by default in Unity, but I may switch it to down for the map) the whole width of the wall extends
		bool TryNextRow=true;
		while(TryNextRow && yMax < Squares_Y-1)
		{
			for(int x = xMin; x <= xMax; x++)
			{
				if(!(CollideAtGrid(x,yMax+1) && !MapArray[x, yMax+1].HasCollider))
				{
					TryNextRow = false;
					x = xMax+1;
				}
			}
			if(TryNextRow) yMax++;
		}

		GameObject go = new GameObject();
		go.name = "Collider_("+xMin+","+yMin+")-("+xMax+","+yMax+")";
		int dx = xMax - xMin + 1;
		int dy = yMax - yMin + 1;
		float width = dx * Square_Size;
		float height = dy * Square_Size;
		BoxCollider2D boxcoll = go.AddComponent<BoxCollider2D>();
		boxcoll.size = new Vector2(width,height);
		boxcoll.offset= new Vector2(xMin*Square_Size + 0.5f*width, /*(Squares_Y*Square_Size) -*/ (yMin*Square_Size + 0.5f*height));
		go.transform.position = transform.position;
		go.transform.rotation = transform.rotation;
		go.transform.localScale = transform.lossyScale;
		go.transform.parent = transform;

		
		for(int y = yMin; y <= yMax; y++)
		{
			for(int x = xMin; x <= xMax; x++)
			{
				MapArray[x,y].HasCollider = true;
			}
		}
	}

	void GenerateColliders()
	{
		for(int y = 0; y < Squares_Y; y++)
		{
			for(int x = 0; x < Squares_X; x++)
			{
				if (CollideAtGrid(x, y) && !MapArray[x,y].HasCollider)
				{
					GenerateColliderAt(x,y);
				}
			}
		}

	}


	void GenerateMapArrayFromLevelData()
	{
		MapArray = new MapSquare[Squares_X,Squares_Y];
		PopulateMapArray();
		DoWallAutotiling();
	}
	
	void SetSquareTexCoords(Vector2[] uv, int squareNum, int textureNum)
	{
		const int TEX_PACK_WIDTH = 4;
		const int TEX_PACK_HEIGHT = 8;  // This is so textures can be power-of-two in both x and y;
		float squareU = 1f/(float)TEX_PACK_WIDTH;
		float squareV = 1f/(float)TEX_PACK_HEIGHT;
		int tex_x = textureNum % TEX_PACK_WIDTH;
		int tex_y = textureNum / TEX_PACK_WIDTH;
		
		uv[squareNum*4  ] = new Vector2( tex_x*squareU + iotaTL,     1f - (tex_y) * squareV - iotaTL ); // Unity's texture co-ords
		uv[squareNum*4+1] = new Vector2( (tex_x+1)*squareU - iotaBR, 1f - (tex_y) * squareV - iotaTL ); // have (0,0) in the bottom
		uv[squareNum*4+2] = new Vector2( tex_x*squareU + iotaTL,     1f - (tex_y+1)   * squareV + iotaBR ); // left. Ie. they're upside
		uv[squareNum*4+3] = new Vector2( (tex_x+1)*squareU - iotaBR, 1f - (tex_y+1)   * squareV + iotaBR ); // down.
	}

	public void GenerateMap()
	{
		foreach (Transform childTransform in transform) Destroy(childTransform.gameObject);
		GenerateMapArrayFromLevelData();

		int numSquares = Squares_X * Squares_Y;
		int numVerts = 4 * numSquares;
		int numTriPoints = 6 * numSquares;

		Vector3[] verts = new Vector3[numVerts];
		Vector3[] normals = new Vector3[numVerts];
		Vector2[] uv = new Vector2[numVerts];

		int[] tris = new int[numTriPoints];
		Vector3 towardCamera = new Vector3(0,0,-1f);

		int squareNum = 0;
		
		for(int y = 0; y < Squares_Y; y++)
		{
			for(int x = 0; x < Squares_X; x++)
			{
				verts[squareNum*4  ] = new Vector3(x*Square_Size,     /*(Squares_Y*Square_Size) -*/ y*Square_Size, 0.0f);
				verts[squareNum*4+1] = new Vector3((x+1)*Square_Size, /*(Squares_Y*Square_Size) -*/ y*Square_Size, 0.0f);
				verts[squareNum*4+2] = new Vector3(x*Square_Size,     /*(Squares_Y*Square_Size) -*/ (y+1)*Square_Size, 0.0f);
				verts[squareNum*4+3] = new Vector3((x+1)*Square_Size, /*(Squares_Y*Square_Size) -*/ (y+1)*Square_Size, 0.0f);

				tris[squareNum*6  ] = squareNum*4;
				tris[squareNum*6+1] = squareNum*4+2;
				tris[squareNum*6+2] = squareNum*4+1;
				tris[squareNum*6+3] = squareNum*4+1;
				tris[squareNum*6+4] = squareNum*4+2;
				tris[squareNum*6+5] = squareNum*4+3;

				normals[squareNum*4  ] = towardCamera;
				normals[squareNum*4+1] = towardCamera;
				normals[squareNum*4+2] = towardCamera;
				normals[squareNum*4+3] = towardCamera;

				SetSquareTexCoords(uv, squareNum, MapArray[x, y].TextureNum);

				/*if (MapArray[x, y].IsWall)
				{
					GameObject go = new GameObject();
					go.name = "Collider_("+x+","+y+")";
					BoxCollider2D boxcoll = go.AddComponent<BoxCollider2D>();
					boxcoll.size = new Vector2(Square_Size,Square_Size);
					boxcoll.center= new Vector2((x+0.5f)*Square_Size, (y+0.5f)*Square_Size);
					go.transform.position = transform.position;
					go.transform.rotation = transform.rotation;
					go.transform.localScale = transform.lossyScale;
					go.transform.parent = transform;
				}*/

				squareNum++;
			}
		}

		GenerateColliders();

		Mesh mesh = new Mesh();
		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.normals = normals;
		mesh.uv = uv;
		GetComponent<MeshFilter>().mesh = mesh;
		pathfinder = new PathfindingImpl(Squares_X, Squares_Y, CollideAtGrid);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
