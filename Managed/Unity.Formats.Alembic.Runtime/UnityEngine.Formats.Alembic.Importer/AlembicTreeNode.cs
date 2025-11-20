using System;
using System.Collections.Generic;

namespace UnityEngine.Formats.Alembic.Importer;

internal sealed class AlembicTreeNode : IDisposable
{
	private List<AlembicTreeNode> children = new List<AlembicTreeNode>();

	public AlembicStream stream { get; set; }

	public GameObject gameObject { get; set; }

	internal AlembicElement abcObject { get; set; }

	public List<AlembicTreeNode> Children => children;

	public void Dispose()
	{
		ResetTree();
		gameObject = null;
	}

	public void ResetTree()
	{
		foreach (AlembicTreeNode child in Children)
		{
			child.Dispose();
		}
		Children.Clear();
		if (abcObject != null)
		{
			abcObject.Dispose();
			abcObject = null;
		}
	}

	internal T GetOrAddAlembicObj<T>() where T : AlembicElement, new()
	{
		T val = abcObject as T;
		if (val == null)
		{
			val = (T)(abcObject = new T
			{
				abcTreeNode = this
			});
		}
		return val;
	}

	internal T GetAlembicObj<T>() where T : AlembicElement, new()
	{
		return abcObject as T;
	}

	internal void RemoveAlembicObject(AlembicElement obj)
	{
		if (obj != null && obj == abcObject)
		{
			abcObject = null;
		}
	}

	public AlembicTreeNode FindNode(GameObject go)
	{
		if (go == gameObject)
		{
			return this;
		}
		foreach (AlembicTreeNode child in Children)
		{
			AlembicTreeNode alembicTreeNode = child.FindNode(go);
			if (alembicTreeNode != null)
			{
				return alembicTreeNode;
			}
		}
		return null;
	}

	public void VisitRecursively(Action<AlembicElement> cb)
	{
		cb(abcObject);
		foreach (AlembicTreeNode child in Children)
		{
			child.VisitRecursively(cb);
		}
	}
}
