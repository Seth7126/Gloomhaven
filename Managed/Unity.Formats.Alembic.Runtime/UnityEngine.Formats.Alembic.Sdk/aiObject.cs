using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiObject
{
	internal IntPtr self;

	public aiContext context => NativeMethods.aiObjectGetContext(self);

	public string name => Marshal.PtrToStringAnsi(NativeMethods.aiObjectGetName(self));

	public string fullname => Marshal.PtrToStringAnsi(NativeMethods.aiObjectGetFullName(self));

	public aiObject parent => NativeMethods.aiObjectGetParent(self);

	public int childCount => NativeMethods.aiObjectGetNumChildren(self);

	public static implicit operator bool(aiObject v)
	{
		return v.self != IntPtr.Zero;
	}

	public void SetEnabled(bool value)
	{
		NativeMethods.aiObjectSetEnabled(self, value);
	}

	public aiObject GetChild(int i)
	{
		return NativeMethods.aiObjectGetChild(self, i);
	}

	internal aiXform AsXform()
	{
		return NativeMethods.aiObjectAsXform(self);
	}

	internal aiCamera AsCamera()
	{
		return NativeMethods.aiObjectAsCamera(self);
	}

	internal aiPoints AsPoints()
	{
		return NativeMethods.aiObjectAsPoints(self);
	}

	internal aiCurves AsCurves()
	{
		return NativeMethods.aiObjectAsCurves(self);
	}

	internal aiPolyMesh AsPolyMesh()
	{
		return NativeMethods.aiObjectAsPolyMesh(self);
	}

	internal aiSubD AsSubD()
	{
		return NativeMethods.aiObjectAsSubD(self);
	}

	public void EachChild(Action<aiObject> act)
	{
		if (act != null)
		{
			int num = childCount;
			for (int i = 0; i < num; i++)
			{
				act(GetChild(i));
			}
		}
	}
}
