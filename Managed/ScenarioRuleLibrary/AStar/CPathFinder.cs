using System;
using System.Collections.Generic;
using System.Threading;
using ScenarioRuleLibrary;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;

namespace AStar;

public class CPathFinder
{
	private class CInternalPath
	{
		public List<Point> m_Points;

		public int m_MovementCost;

		public CInternalPath(List<Point> points, int movementCost)
		{
			m_Points = points;
			m_MovementCost = movementCost;
		}
	}

	public enum EAdjacentPosition
	{
		ELeft,
		ETopLeft,
		ETopRight,
		ERight,
		EBottomLeft,
		EBottomRight
	}

	private static object s_LockObject = new object();

	private static Thread s_LockingThread;

	private int m_Width;

	private int m_Height;

	private CNode[,] m_Nodes;

	private CNode m_StartNode;

	private CNode m_EndNode;

	private CNode m_PreferredAdjacentStartNode;

	private List<List<Point>> m_QueuedTransientBlockedLists;

	private List<List<Point>> m_QueuedTransientSuperBlockedLists;

	private static bool s_OddEvenCheck;

	private static Point[] _adjacentLocations6;

	private static Point[] _adjacentLocations8;

	public static bool OddEvenCheck => s_OddEvenCheck;

	public static bool Locked => s_LockingThread != null;

	public CNode[,] Nodes => m_Nodes;

	public List<List<Point>> QueuedTransientBlockedLists => m_QueuedTransientBlockedLists;

	public List<List<Point>> QueuedTransientSuperBlockedLists => m_QueuedTransientSuperBlockedLists;

	public CPathFinder(int width, int height, bool oddEvenCheck)
	{
		s_OddEvenCheck = oddEvenCheck;
		CreateNodes(width, height);
		_adjacentLocations6 = new Point[6];
		_adjacentLocations8 = new Point[8];
		m_QueuedTransientBlockedLists = new List<List<Point>>();
		m_QueuedTransientSuperBlockedLists = new List<List<Point>>();
	}

	private List<Point> FindPathInternal(Point startLocation, Point endLocation, bool ignoreBlocked, out bool foundPath, bool ignoreBridges = false, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false)
	{
		m_StartNode = m_Nodes[startLocation.X, startLocation.Y];
		m_EndNode = m_Nodes[endLocation.X, endLocation.Y];
		ReinitialiseNodes(endLocation, ignoreBlocked || ignoreMoveCost, ignoreDifficultTerrain);
		m_StartNode.State = NodeState.Open;
		List<Point> list = new List<Point>();
		bool flag = Search(m_StartNode, ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain);
		if (flag)
		{
			CNode cNode = m_EndNode;
			while (cNode.ParentNode != null)
			{
				list.Add(cNode.Location);
				cNode = cNode.ParentNode;
			}
			list.Reverse();
		}
		if (!ignoreBridges)
		{
			CNode cNode2 = null;
			foreach (Point item in list)
			{
				if (!m_Nodes[item.X, item.Y].IsBridge)
				{
					continue;
				}
				if (cNode2 != null)
				{
					if (!cNode2.IsBridgeOpen && !cNode2.HasHealth)
					{
						list.Clear();
						flag = false;
						break;
					}
					cNode2 = m_Nodes[item.X, item.Y];
				}
				else
				{
					cNode2 = m_Nodes[item.X, item.Y];
				}
			}
			if (list.Count > 0 && !m_Nodes[list[list.Count - 1].X, list[list.Count - 1].Y].IsBridge && cNode2 != null && !cNode2.IsBridgeOpen)
			{
				list.Clear();
				flag = false;
			}
		}
		foundPath = flag;
		return list;
	}

	public List<Point> FindPath(Point startLocation, Point endLocation, bool ignoreBlocked, bool ignoreMoveCost, out bool foundPath, bool ignoreBridges = false, bool ignoreDifficultTerrain = false, bool logFailure = false)
	{
		try
		{
			Lock();
			List<CInternalPath> list = new List<CInternalPath>();
			m_StartNode = m_Nodes[startLocation.X, startLocation.Y];
			m_EndNode = m_Nodes[endLocation.X, endLocation.Y];
			ReinitialiseNodes(endLocation, ignoreBlocked || ignoreMoveCost, ignoreDifficultTerrain);
			m_Nodes[startLocation.X, startLocation.Y].State = NodeState.Closed;
			foreach (CNode adjacentNode in GetAdjacentNodes(m_Nodes[startLocation.X, startLocation.Y], ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain))
			{
				m_PreferredAdjacentStartNode = adjacentNode;
				bool foundPath2;
				List<Point> list2 = FindPathInternal(startLocation, endLocation, ignoreBlocked, out foundPath2, ignoreBridges, ignoreMoveCost, ignoreDifficultTerrain);
				int movementCost = CAbilityMove.CalculateMoveCost(list2, !ignoreBlocked, nojump: true, ignoreMoveCost, ignoreDifficultTerrain);
				if (foundPath2 && (!logFailure || (logFailure && list2.Count > 0)))
				{
					list.Add(new CInternalPath(list2, movementCost));
				}
			}
			if (ignoreMoveCost)
			{
				list.Sort((CInternalPath cInternalPath, CInternalPath cInternalPath2) => cInternalPath.m_Points.Count.CompareTo(cInternalPath2.m_Points.Count));
			}
			else
			{
				list.Sort((CInternalPath cInternalPath, CInternalPath cInternalPath2) => cInternalPath.m_MovementCost.CompareTo(cInternalPath2.m_MovementCost));
			}
			foundPath = list.Count > 0;
			if (logFailure && (!foundPath || list[0].m_Points.Count == 0))
			{
				SimpleLog.AddToSimpleLog("[FindPath] - Failed to find path from: [" + startLocation.X + ", " + startLocation.Y + "], to:[" + endLocation.X + ", " + endLocation.Y + "], IgnoreBlocked=" + ignoreBlocked + ", ignoreBridges=" + ignoreBridges + ", ignoreMoveCost=" + ignoreMoveCost + ", ignoreDifficultTerrain=" + ignoreDifficultTerrain + ", internalPathCount=" + list.Count);
				for (int num = 0; num < m_Height; num++)
				{
					for (int num2 = 0; num2 < m_Width; num2++)
					{
						if (m_Nodes[num2, num] == null)
						{
							continue;
						}
						if (m_Nodes[num2, num].State != NodeState.Untested)
						{
							_ = m_Nodes[num2, num].Location;
							int x = m_Nodes[num2, num].Location.X;
							_ = m_Nodes[num2, num].Location;
							int y = m_Nodes[num2, num].Location.Y;
							SimpleLog.AddToSimpleLog("[FindPath] - Node[" + num2 + "," + num + "]: Loc:[" + x + "," + y + "], State=" + m_Nodes[num2, num].State.ToString() + ", Walkable=" + m_Nodes[num2, num].Walkable + ", Blocked=" + m_Nodes[num2, num].Blocked + ", SuperBlocked=" + m_Nodes[num2, num].SuperBlocked + ", TransientBlocked=" + m_Nodes[num2, num].TransientBlocked + ", IsBridge=" + m_Nodes[num2, num].IsBridge + ", IsBridgeOpen=" + m_Nodes[num2, num].IsBridgeOpen + ", F (total cost G+H)=" + m_Nodes[num2, num].F + ", G (cost from start)=" + m_Nodes[num2, num].G + ", H (cost to end)=" + m_Nodes[num2, num].H);
						}
						if (m_Nodes[num2, num].TransientBlocked)
						{
							CActor cActor = ScenarioManager.Scenario.FindActorAt(m_Nodes[num2, num].Location);
							if (cActor != null)
							{
								SimpleLog.AddToSimpleLog("[FindPath] - Node[" + num2 + "," + num + "]: Blocked by actor:" + cActor.Class?.ID);
							}
							else
							{
								SimpleLog.AddToSimpleLog("[FindPath] - Node[" + num2 + "," + num + "]: Blocked by actor: Unknown");
							}
						}
						if (!m_Nodes[num2, num].Blocked)
						{
							continue;
						}
						foreach (CObjectProp prop in ScenarioManager.Tiles[num2, num].m_Props)
						{
							if (prop != null)
							{
								SimpleLog.AddToSimpleLog("[FindPath] - Node[" + num2 + "," + num + "]: Blocked by Prop:" + prop.ObjectType);
							}
						}
					}
				}
			}
			else if (logFailure)
			{
				SimpleLog.AddToSimpleLog("[FindPath] - Did not log failed path output: logFailure:" + logFailure + " foundPath:" + foundPath + " internalPathCount:" + list.Count);
			}
			Unlock();
			return foundPath ? list[0].m_Points : new List<Point>();
		}
		catch (Exception ex)
		{
			SimpleLog.AddToSimpleLog("[FindPath] - Caught exception during FindPath, logging it and ignoring it: \n" + ex.Message + "\n" + ex.StackTrace);
			foundPath = false;
			Unlock();
			return new List<Point>();
		}
	}

	public List<Point> FindStraightPath(Point startLocation, Point endLocation, bool ignoreBlocked, out bool foundPath, bool ignoreBridges = false)
	{
		try
		{
			Lock();
			m_StartNode = m_Nodes[startLocation.X, startLocation.Y];
			m_EndNode = m_Nodes[endLocation.X, endLocation.Y];
			ReinitialiseNodes(endLocation);
			m_Nodes[startLocation.X, startLocation.Y].State = NodeState.Closed;
			for (EAdjacentPosition eAdjacentPosition = EAdjacentPosition.ELeft; eAdjacentPosition <= EAdjacentPosition.EBottomRight; eAdjacentPosition++)
			{
				CNode specificAdjacentNode = GetSpecificAdjacentNode(m_Nodes[startLocation.X, startLocation.Y], ignoreBlocked, eAdjacentPosition);
				m_PreferredAdjacentStartNode = specificAdjacentNode;
				bool foundPath2;
				List<Point> result = FindStraightPathInternal(startLocation, endLocation, eAdjacentPosition, ignoreBlocked, out foundPath2, ignoreBridges);
				if (foundPath2)
				{
					foundPath = true;
					Unlock();
					return result;
				}
			}
			foundPath = false;
			Unlock();
			return new List<Point>();
		}
		catch
		{
			foundPath = false;
			Unlock();
			return new List<Point>();
		}
	}

	private List<Point> FindStraightPathInternal(Point startLocation, Point endLocation, EAdjacentPosition adjacentPosition, bool ignoreBlocked, out bool foundPath, bool ignoreBridges = false)
	{
		m_StartNode = m_Nodes[startLocation.X, startLocation.Y];
		m_EndNode = m_Nodes[endLocation.X, endLocation.Y];
		ReinitialiseNodes(endLocation);
		m_StartNode.State = NodeState.Open;
		List<Point> list = new List<Point>();
		bool flag = SearchSpecificAdjacentPosition(m_StartNode, ignoreBlocked, adjacentPosition);
		if (flag)
		{
			CNode cNode = m_EndNode;
			while (cNode.ParentNode != null)
			{
				list.Add(cNode.Location);
				cNode = cNode.ParentNode;
			}
			list.Reverse();
		}
		if (!ignoreBridges)
		{
			CNode cNode2 = null;
			foreach (Point item in list)
			{
				if (!m_Nodes[item.X, item.Y].IsBridge)
				{
					continue;
				}
				if (cNode2 != null)
				{
					if (!cNode2.IsBridgeOpen)
					{
						list.Clear();
						flag = false;
						break;
					}
					cNode2 = m_Nodes[item.X, item.Y];
				}
				else
				{
					cNode2 = m_Nodes[item.X, item.Y];
				}
			}
			if (list.Count > 0 && !m_Nodes[list[list.Count - 1].X, list[list.Count - 1].Y].IsBridge && cNode2 != null && !cNode2.IsBridgeOpen)
			{
				list.Clear();
				flag = false;
			}
		}
		foundPath = flag;
		return list;
	}

	private void CreateNodes(int width, int height)
	{
		m_Width = width;
		m_Height = height;
		m_Nodes = new CNode[m_Width, m_Height];
		for (int i = 0; i < m_Height; i++)
		{
			for (int j = 0; j < m_Width; j++)
			{
				m_Nodes[j, i] = new CNode(j, i);
			}
		}
	}

	public void Lock()
	{
		try
		{
			if (s_LockingThread == Thread.CurrentThread)
			{
				DLLDebug.LogError("[ASTAR] AStar state already locked by this thread!");
				return;
			}
			Monitor.Enter(s_LockObject);
			s_LockingThread = Thread.CurrentThread;
			List<List<Point>> queuedTransientBlockedLists = QueuedTransientBlockedLists;
			if (queuedTransientBlockedLists != null && queuedTransientBlockedLists.Count > 0)
			{
				if (QueuedTransientBlockedLists[0] != null)
				{
					foreach (Point item in QueuedTransientBlockedLists[0])
					{
						if (Nodes[item.X, item.Y] != null)
						{
							Nodes[item.X, item.Y].TransientBlocked = true;
							continue;
						}
						string[] obj = new string[5] { "[FindPath-Lock] - Blocked Node at x:", null, null, null, null };
						int x = item.X;
						obj[1] = x.ToString();
						obj[2] = ",y:";
						x = item.Y;
						obj[3] = x.ToString();
						obj[4] = " is null, skipping";
						SimpleLog.AddToSimpleLog(string.Concat(obj));
					}
				}
				else
				{
					SimpleLog.AddToSimpleLog("[FindPath-Lock] - Block list is null, skipping");
				}
				if (QueuedTransientBlockedLists != null)
				{
					QueuedTransientBlockedLists.RemoveAt(0);
				}
				else
				{
					m_QueuedTransientBlockedLists = new List<List<Point>>();
				}
			}
			else
			{
				m_QueuedTransientBlockedLists = new List<List<Point>>();
			}
			List<List<Point>> queuedTransientSuperBlockedLists = QueuedTransientSuperBlockedLists;
			if (queuedTransientSuperBlockedLists != null && queuedTransientSuperBlockedLists.Count > 0)
			{
				if (QueuedTransientSuperBlockedLists[0] != null)
				{
					foreach (Point item2 in QueuedTransientSuperBlockedLists[0])
					{
						if (Nodes[item2.X, item2.Y] != null)
						{
							Nodes[item2.X, item2.Y].SuperBlocked = true;
							continue;
						}
						string[] obj2 = new string[5] { "[FindPath-Lock] - SuperBlocked Node at x:", null, null, null, null };
						int x = item2.X;
						obj2[1] = x.ToString();
						obj2[2] = ",y:";
						x = item2.Y;
						obj2[3] = x.ToString();
						obj2[4] = " is null, skipping";
						SimpleLog.AddToSimpleLog(string.Concat(obj2));
					}
				}
				else
				{
					SimpleLog.AddToSimpleLog("[FindPath-Lock] - SuperBlock list is null, skipping");
				}
				if (QueuedTransientSuperBlockedLists != null)
				{
					QueuedTransientSuperBlockedLists.RemoveAt(0);
				}
				else
				{
					m_QueuedTransientSuperBlockedLists = new List<List<Point>>();
				}
			}
			else
			{
				m_QueuedTransientSuperBlockedLists = new List<List<Point>>();
			}
		}
		catch (Exception ex)
		{
			SimpleLog.AddToSimpleLog("[FindPath-Lock] - Caught exception during FindPath-Lock, logging it and ignoring it: \n" + ex.Message + "\n" + ex.StackTrace);
			m_QueuedTransientBlockedLists = new List<List<Point>>();
			m_QueuedTransientSuperBlockedLists = new List<List<Point>>();
		}
	}

	public void Unlock()
	{
		if (s_LockingThread == null)
		{
			DLLDebug.LogError("[ASTAR] AStar state not locked!");
			return;
		}
		for (int i = 0; i < m_Height; i++)
		{
			for (int j = 0; j < m_Width; j++)
			{
				m_Nodes[j, i].TransientBlocked = false;
			}
		}
		if (s_LockingThread != null)
		{
			s_LockingThread = null;
			Monitor.Exit(s_LockObject);
		}
	}

	private void ReinitialiseNodes(Point endLocation, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false)
	{
		for (int i = 0; i < m_Height; i++)
		{
			for (int j = 0; j < m_Width; j++)
			{
				m_Nodes[j, i].Reinitialise(j, i, endLocation, ignoreMoveCost, ignoreDifficultTerrain);
			}
		}
	}

	private bool Search(CNode currentNode, bool ignoreBlocked, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false)
	{
		currentNode.State = NodeState.Closed;
		List<CNode> adjacentNodes = GetAdjacentNodes(currentNode, ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain);
		adjacentNodes.Sort((CNode node1, CNode node2) => node1.F.CompareTo(node2.F));
		for (int num = 0; num < adjacentNodes.Count; num++)
		{
			CNode cNode = adjacentNodes[num];
			if (m_PreferredAdjacentStartNode == null || cNode == m_PreferredAdjacentStartNode)
			{
				m_PreferredAdjacentStartNode = null;
				if (cNode.Location == m_EndNode.Location)
				{
					return true;
				}
				if (Search(cNode, ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain))
				{
					return true;
				}
			}
		}
		return false;
	}

	private List<CNode> GetAdjacentNodes(CNode fromNode, bool ignoreBlocked, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false)
	{
		List<CNode> list = new List<CNode>();
		Point[] adjacentLocations = GetAdjacentLocations(fromNode.Location);
		for (int i = 0; i < adjacentLocations.Length; i++)
		{
			Point point = adjacentLocations[i];
			int x = point.X;
			int y = point.Y;
			if (x < 0 || x >= m_Width || y < 0 || y >= m_Height)
			{
				continue;
			}
			CNode cNode = m_Nodes[x, y];
			if (cNode.State == NodeState.Closed || !fromNode.NavTo(cNode, ignoreBlocked))
			{
				continue;
			}
			if (cNode.State == NodeState.Open)
			{
				float traversalCost = CNode.GetTraversalCost(cNode.Location, cNode.ParentNode.Location, ignoreBlocked, jump: false, ignoreMoveCost, ignoreDifficultTerrain);
				if (fromNode.G + traversalCost < cNode.G)
				{
					cNode.ParentNode = fromNode;
					cNode.G = cNode.ParentNode.G + CNode.GetTraversalCost(cNode.Location, cNode.ParentNode.Location, ignoreBlocked, jump: false, ignoreMoveCost, ignoreDifficultTerrain);
					list.Add(cNode);
				}
			}
			else
			{
				cNode.ParentNode = fromNode;
				cNode.G = cNode.ParentNode.G + CNode.GetTraversalCost(cNode.Location, cNode.ParentNode.Location, ignoreBlocked, jump: false, ignoreMoveCost, ignoreDifficultTerrain);
				cNode.State = NodeState.Open;
				list.Add(cNode);
			}
		}
		return list;
	}

	private static Point[] GetAdjacentLocations(Point fromLocation)
	{
		if (s_OddEvenCheck)
		{
			if ((fromLocation.Y & 1) == 0)
			{
				_adjacentLocations6[0] = new Point(fromLocation.X - 1, fromLocation.Y - 1);
				_adjacentLocations6[1] = new Point(fromLocation.X - 1, fromLocation.Y);
				_adjacentLocations6[2] = new Point(fromLocation.X - 1, fromLocation.Y + 1);
				_adjacentLocations6[3] = new Point(fromLocation.X, fromLocation.Y + 1);
				_adjacentLocations6[4] = new Point(fromLocation.X + 1, fromLocation.Y);
				_adjacentLocations6[5] = new Point(fromLocation.X, fromLocation.Y - 1);
				return _adjacentLocations6;
			}
			_adjacentLocations6[0] = new Point(fromLocation.X - 1, fromLocation.Y);
			_adjacentLocations6[1] = new Point(fromLocation.X, fromLocation.Y + 1);
			_adjacentLocations6[2] = new Point(fromLocation.X + 1, fromLocation.Y + 1);
			_adjacentLocations6[3] = new Point(fromLocation.X + 1, fromLocation.Y);
			_adjacentLocations6[4] = new Point(fromLocation.X + 1, fromLocation.Y - 1);
			_adjacentLocations6[5] = new Point(fromLocation.X, fromLocation.Y - 1);
			return _adjacentLocations6;
		}
		_adjacentLocations8[0] = new Point(fromLocation.X - 1, fromLocation.Y - 1);
		_adjacentLocations8[1] = new Point(fromLocation.X - 1, fromLocation.Y);
		_adjacentLocations8[2] = new Point(fromLocation.X - 1, fromLocation.Y + 1);
		_adjacentLocations8[3] = new Point(fromLocation.X, fromLocation.Y + 1);
		_adjacentLocations8[4] = new Point(fromLocation.X + 1, fromLocation.Y + 1);
		_adjacentLocations8[5] = new Point(fromLocation.X + 1, fromLocation.Y);
		_adjacentLocations8[6] = new Point(fromLocation.X + 1, fromLocation.Y - 1);
		_adjacentLocations8[7] = new Point(fromLocation.X, fromLocation.Y - 1);
		return _adjacentLocations8;
	}

	private bool SearchSpecificAdjacentPosition(CNode currentNode, bool ignoreBlocked, EAdjacentPosition adjacentPosition)
	{
		currentNode.State = NodeState.Closed;
		CNode specificAdjacentNode = GetSpecificAdjacentNode(currentNode, ignoreBlocked, adjacentPosition);
		if (specificAdjacentNode != null && (m_PreferredAdjacentStartNode == null || specificAdjacentNode == m_PreferredAdjacentStartNode))
		{
			m_PreferredAdjacentStartNode = null;
			if (specificAdjacentNode.Location == m_EndNode.Location)
			{
				return true;
			}
			if (SearchSpecificAdjacentPosition(specificAdjacentNode, ignoreBlocked, adjacentPosition))
			{
				return true;
			}
		}
		return false;
	}

	private CNode GetSpecificAdjacentNode(CNode fromNode, bool ignoreBlocked, EAdjacentPosition adjacentPosition)
	{
		CNode result = null;
		Point specificAdjacentLocation = GetSpecificAdjacentLocation(fromNode.Location, adjacentPosition);
		int x = specificAdjacentLocation.X;
		int y = specificAdjacentLocation.Y;
		if (x < 0 || x >= m_Width || y < 0 || y >= m_Height)
		{
			return result;
		}
		CNode cNode = m_Nodes[x, y];
		if (cNode.State == NodeState.Closed)
		{
			return result;
		}
		if (!fromNode.NavTo(cNode, ignoreBlocked))
		{
			return result;
		}
		if (cNode.State == NodeState.Open)
		{
			float traversalCost = CNode.GetTraversalCost(cNode.Location, cNode.ParentNode.Location);
			if (fromNode.G + traversalCost < cNode.G)
			{
				cNode.ParentNode = fromNode;
				result = cNode;
			}
		}
		else
		{
			cNode.ParentNode = fromNode;
			cNode.State = NodeState.Open;
			result = cNode;
		}
		return result;
	}

	private static Point GetSpecificAdjacentLocation(Point fromLocation, EAdjacentPosition adjacentPosition)
	{
		if (s_OddEvenCheck)
		{
			if ((fromLocation.Y & 1) == 0)
			{
				switch (adjacentPosition)
				{
				case EAdjacentPosition.ELeft:
					return new Point(fromLocation.X - 1, fromLocation.Y);
				case EAdjacentPosition.ERight:
					return new Point(fromLocation.X + 1, fromLocation.Y);
				case EAdjacentPosition.ETopLeft:
					return new Point(fromLocation.X - 1, fromLocation.Y - 1);
				case EAdjacentPosition.ETopRight:
					return new Point(fromLocation.X, fromLocation.Y - 1);
				case EAdjacentPosition.EBottomLeft:
					return new Point(fromLocation.X - 1, fromLocation.Y + 1);
				case EAdjacentPosition.EBottomRight:
					return new Point(fromLocation.X, fromLocation.Y + 1);
				}
			}
			else
			{
				switch (adjacentPosition)
				{
				case EAdjacentPosition.ELeft:
					return new Point(fromLocation.X - 1, fromLocation.Y);
				case EAdjacentPosition.ERight:
					return new Point(fromLocation.X + 1, fromLocation.Y);
				case EAdjacentPosition.ETopLeft:
					return new Point(fromLocation.X, fromLocation.Y - 1);
				case EAdjacentPosition.ETopRight:
					return new Point(fromLocation.X + 1, fromLocation.Y - 1);
				case EAdjacentPosition.EBottomLeft:
					return new Point(fromLocation.X, fromLocation.Y + 1);
				case EAdjacentPosition.EBottomRight:
					return new Point(fromLocation.X + 1, fromLocation.Y + 1);
				}
			}
		}
		return new Point(0, 0);
	}
}
