using UnityEngine;
using System.Collections;
using Priority_Queue;


public class PathfindingImpl
{
	public delegate bool IsPassable(int x, int y);
	
	int m_Width;
	int m_Height;
	IsPassable m_Unpassable;
	MapNode[,] m_Map;

	public struct coord
	{
		public int x, y;
	}

	class MapNode:PriorityQueueNode
	{
		public coord pos;
		public bool passable;
		public int f, g, h;
		public int count;
		public coord previous;
		public bool closed;
	}

	public PathfindingImpl(int mapWidth, int mapHeight, IsPassable unpassabilityFunc)
	{
		m_Width = mapWidth;
		m_Height = mapHeight;
		m_Map = new MapNode[mapWidth,mapHeight];
		m_Unpassable = unpassabilityFunc;
		InitMap();
	}

	private void InitMap()
	{		
		for(int x = 0; x < m_Width; x++)
		{
			for(int y = 0; y < m_Height; y++)
			{
				m_Map[x,y] = new MapNode();
				m_Map[x,y].pos.x = x;
				m_Map[x,y].pos.y = y;
			}
		}
	}

	private void ResetMap()
	{
		
		for(int x = 0; x < m_Width; x++)
		{
			for(int y = 0; y < m_Height; y++)
			{
				m_Map[x,y].passable = !m_Unpassable(x,y); //*perhaps* unnecessary to do every time.
				m_Map[x,y].f = int.MaxValue;
				m_Map[x,y].g = int.MaxValue;
				m_Map[x,y].h = int.MaxValue;
				m_Map[x,y].count = 0;
				m_Map[x,y].previous.x = -1;
				m_Map[x,y].previous.y = -1;
				m_Map[x,y].closed = false;
			}
		}
	}

	int Heuristic(int x, int y, int dest_x, int dest_y)
	{
		return System.Math.Abs(dest_x-x) + System.Math.Abs(dest_y - y);
	}

	bool IsAccessible(int x, int y)
	{
		if(x < 0 || x >= m_Width) return false;
		if(y < 0 || y >= m_Height) return false;

		return(m_Map[x,y].passable);
	}

	void SetVals(HeapPriorityQueue<MapNode> openQ, int x, int y, int g, int g_inc, int count, coord prev, int end_x, int end_y)
	{
		if(!m_Map[x,y].closed && m_Map[x,y].g > g+g_inc)
		{
			m_Map[x,y].g = g+g_inc;
			m_Map[x,y].h = Heuristic(x, y, end_x, end_y);
			m_Map[x,y].f = m_Map[x,y].g + m_Map[x,y].h;
			m_Map[x,y].count = count+1;
			m_Map[x,y].previous = prev;

			/*if(Heuristic(prev.x, prev.y, x, y) > 1)
			{
				Debug.Log("Err: H: " + Heuristic(prev.x, prev.y, x, y).ToString() + ": prev (" + prev.x.ToString() +", " + prev.y.ToString() + ") to current (" + x.ToString() + ", " + y.ToString() + ")");
			}*/

			if(openQ.Contains(m_Map[x,y]))
			{
				openQ.UpdatePriority(m_Map[x,y], m_Map[x,y].f);
			}
			else
			{
				openQ.Enqueue(m_Map[x,y], m_Map[x,y].f);
			}
		}
	}

	void ProcessNode(HeapPriorityQueue<MapNode> openQ, MapNode m, int end_x, int end_y)
	{
		bool L = IsAccessible(m.pos.x+1, m.pos.y);
		bool R = IsAccessible(m.pos.x-1, m.pos.y);
		bool T = IsAccessible(m.pos.x, m.pos.y+1);
		bool B = IsAccessible(m.pos.x, m.pos.y-1);
		bool TL = T && L && IsAccessible(m.pos.x+1, m.pos.y+1);
		bool TR = T && R && IsAccessible(m.pos.x-1, m.pos.y+1);
		bool BL = B && L && IsAccessible(m.pos.x+1, m.pos.y-1);
		bool BR = B && R && IsAccessible(m.pos.x-1, m.pos.y-1);
		if(L) SetVals(openQ, m.pos.x+1, m.pos.y, m.g, 10, m.count, m.pos, end_x, end_y);
		if(R) SetVals(openQ, m.pos.x-1, m.pos.y, m.g, 10, m.count, m.pos, end_x, end_y);
		if(T) SetVals(openQ, m.pos.x, m.pos.y+1, m.g, 10, m.count, m.pos, end_x, end_y);
		if(B) SetVals(openQ, m.pos.x, m.pos.y-1, m.g, 10, m.count, m.pos, end_x, end_y);

		if(TL) SetVals(openQ, m.pos.x+1, m.pos.y+1, m.g, 14, m.count, m.pos, end_x, end_y);
		if(TR) SetVals(openQ, m.pos.x-1, m.pos.y+1, m.g, 14, m.count, m.pos, end_x, end_y);
		if(BL) SetVals(openQ, m.pos.x+1, m.pos.y-1, m.g, 14, m.count, m.pos, end_x, end_y);
		if(BR) SetVals(openQ, m.pos.x-1, m.pos.y-1, m.g, 14, m.count, m.pos, end_x, end_y);
	}

	public Vector2[] FindPath(int start_x, int start_y, int end_x, int end_y)
	{
		if(start_x < 0 || start_x >= m_Width) return null;
		if(start_y < 0 || start_y >= m_Height) return null;
		if(start_x == end_x && start_y == end_y) return null;

		ResetMap();
		HeapPriorityQueue<MapNode> openQ = new HeapPriorityQueue<MapNode>(m_Height * m_Width); //Probably way beyond worst-case size...
		//int x = start_x, y = start_y;
		bool found = false;
		if (!m_Map[start_x, start_y].passable || !m_Map[end_x, end_y].passable) return null;
		m_Map[start_x, start_y].g = 0;
		m_Map[start_x, start_y].count = 0;
		m_Map[start_x, start_y].h = Heuristic(start_x, start_y, end_x, end_y);
		m_Map[start_x, start_y].f = m_Map[start_x, start_y].h;
		openQ.Enqueue(m_Map[start_x, start_y], 0);

		do
		{
			MapNode m = openQ.Dequeue();
			m.closed = true;
			if(m.pos.x == end_x && m.pos.y == end_y)
			{
				found = true;
			}
			else
			{
				ProcessNode(openQ, m, end_x, end_y);
			}


		} while(!found && openQ.Count > 0);

		if(found)
		{
			Vector2[] rv = new Vector2[m_Map[end_x, end_y].count]; //would be g+1 if we needed to return the first node, but we don't obv.
			int x = end_x, y = end_y;
			for(int i = m_Map[end_x, end_y].count-1; i >=0; i--)
			{
				MapNode m = m_Map[x,y];
				rv[i].x = m.pos.x;
				rv[i].y = m.pos.y;
				x = m.previous.x;
				y = m.previous.y;
			}
			return rv;
		}
		else
		{
			return null;
		}
	}

}
