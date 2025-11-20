using System;
using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Importer;

internal abstract class AlembicElement : IDisposable
{
	private aiObject m_abcObj;

	public bool disposed { get; protected set; }

	public AlembicTreeNode abcTreeNode { get; set; }

	public aiObject abcObject => m_abcObj;

	internal abstract aiSchema abcSchema { get; }

	public abstract bool visibility { get; }

	public Camera GetOrAddCamera()
	{
		Camera camera = abcTreeNode.gameObject.GetComponent<Camera>();
		if (camera == null)
		{
			camera = abcTreeNode.gameObject.AddComponent<Camera>();
			camera.usePhysicalProperties = true;
		}
		return camera;
	}

	protected virtual void Dispose(bool v)
	{
		if (abcTreeNode != null)
		{
			abcTreeNode.RemoveAlembicObject(this);
		}
	}

	public void Dispose()
	{
		if (!disposed)
		{
			Dispose(v: true);
			GC.SuppressFinalize(this);
		}
		disposed = true;
	}

	internal virtual void AbcSetup(aiObject abcObj, aiSchema abcSchema)
	{
		m_abcObj = abcObj;
	}

	public virtual void AbcPrepareSample()
	{
	}

	public virtual void AbcSyncDataBegin()
	{
	}

	public virtual void AbcSyncDataEnd()
	{
	}
}
