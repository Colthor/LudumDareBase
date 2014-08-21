using UnityEngine;
using System.Collections;



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
	public string LevelData = "8888888888"
							+ "8801234588"
							+ "8086700808"
							+ "8008008808"
							+ "8800880888"
							+ "8000880808"
							+ "8008008808"
							+ "8080GH0808"
							+ "88ABCDEF88"
							+ "8888888888";

	// Use this for initialization
	void Start ()
	{
		GenerateMap();
	}

	bool IsValidCharacter(char c)
	{
		return ('0' <= c && c <= '8') || ('A' <= c && c <= 'H');
	}

	int CharacterToTileIndex(char c)
	{
		if ('0' <= c && c <= '8') 
		{
			return c - '0';
		}
		else
		{
			return 24 + (c - 'A');
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
				MapArray[x, y].IsWall = (8 == MapArray[x, y].TextureNum);
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

	void GenerateColliderAt(int xMin, int yMin)
	{
		int xMax = xMin;
		int yMax = yMin;

		//Find how far right the wall extends
		while(xMax < Squares_X-1 && MapArray[xMax+1,yMin].IsWall && !MapArray[xMax+1, yMin].HasCollider)
		{
			xMax++;
		}

		//find how far in +ve Y (up by default in Unity, but I may switch it to down for the map) the whole width of the wall extends
		bool TryNextRow=true;
		while(TryNextRow && yMax < Squares_Y-1)
		{
			for(int x = xMin; x <= xMax; x++)
			{
				if(!(MapArray[x,yMax+1].IsWall && !MapArray[x, yMax+1].HasCollider))
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
		boxcoll.center= new Vector2(xMin*Square_Size + 0.5f*width, (Squares_Y*Square_Size) - (yMin*Square_Size + 0.5f*height));
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
				if (MapArray[x, y].IsWall && !MapArray[x,y].HasCollider)
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
										// bottom two rows of squares currently unused.
		float squareU = 1f/(float)TEX_PACK_WIDTH;
		float squareV = 1f/(float)TEX_PACK_HEIGHT;
		int tex_x = textureNum % TEX_PACK_WIDTH;
		int tex_y = textureNum / TEX_PACK_WIDTH;
		
		uv[squareNum*4  ] = new Vector2( tex_x*squareU,     1f - (tex_y) * squareV ); // Unity's texture co-ords
		uv[squareNum*4+1] = new Vector2( (tex_x+1)*squareU, 1f - (tex_y) * squareV ); // have (0,0) in the bottom
		uv[squareNum*4+2] = new Vector2( tex_x*squareU,     1f - (tex_y+1)   * squareV ); // left. Ie. they're upside
		uv[squareNum*4+3] = new Vector2( (tex_x+1)*squareU, 1f - (tex_y+1)   * squareV ); // down.
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
				verts[squareNum*4  ] = new Vector3(x*Square_Size,     (Squares_Y*Square_Size) - y*Square_Size, 0.0f);
				verts[squareNum*4+1] = new Vector3((x+1)*Square_Size, (Squares_Y*Square_Size) - y*Square_Size, 0.0f);
				verts[squareNum*4+2] = new Vector3(x*Square_Size,     (Squares_Y*Square_Size) - (y+1)*Square_Size, 0.0f);
				verts[squareNum*4+3] = new Vector3((x+1)*Square_Size, (Squares_Y*Square_Size) - (y+1)*Square_Size, 0.0f);

				tris[squareNum*6  ] = squareNum*4;
				tris[squareNum*6+1] = squareNum*4+1;
				tris[squareNum*6+2] = squareNum*4+2;
				tris[squareNum*6+3] = squareNum*4+1;
				tris[squareNum*6+4] = squareNum*4+3;
				tris[squareNum*6+5] = squareNum*4+2;

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
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
