#define DEBUG
using System;
using Photon.Bolt.Collections;
using Photon.Bolt.Exceptions;
using UnityEngine;

namespace Photon.Bolt;

[Documentation]
public class NetworkTransform
{
	internal int PropertyIndex;

	internal Transform Render;

	internal Transform Simulate;

	internal bool SkipInterpolation;

	internal Func<BoltEntity, Vector3, Vector3> Clamper = (BoltEntity entity, Vector3 position) => position;

	internal DoubleBuffer<Vector3> RenderDoubleBufferPosition;

	internal DoubleBuffer<Quaternion> RenderDoubleBufferRotation;

	internal TransformSpaces space;

	public Transform Transform => Simulate;

	public bool Teleport => SkipInterpolation;

	public Vector3 Position => RenderDoubleBufferPosition.Current;

	public Quaternion Rotation => RenderDoubleBufferRotation.Current;

	internal NetworkTransform()
		: this(TransformSpaces.World)
	{
	}

	internal NetworkTransform(TransformSpaces space)
	{
		this.space = space;
	}

	public void SetExtrapolationClamper(Func<BoltEntity, Vector3, Vector3> clamper)
	{
		Assert.NotNull(clamper);
		Clamper = clamper;
	}

	public void ChangeTransforms(Transform simulate)
	{
		ChangeTransforms(simulate, null);
	}

	public void ChangeTransforms(Transform simulate, Transform render)
	{
		if ((bool)render)
		{
			Render = render;
			if (space == TransformSpaces.World)
			{
				RenderDoubleBufferPosition = DoubleBuffer<Vector3>.InitBuffer(simulate.position);
			}
			else if (space == TransformSpaces.Local)
			{
				RenderDoubleBufferPosition = DoubleBuffer<Vector3>.InitBuffer(simulate.localPosition);
			}
			RenderDoubleBufferRotation = DoubleBuffer<Quaternion>.InitBuffer(simulate.rotation);
		}
		else
		{
			Render = null;
			RenderDoubleBufferPosition = DoubleBuffer<Vector3>.InitBuffer(Vector3.zero);
			RenderDoubleBufferRotation = DoubleBuffer<Quaternion>.InitBuffer(Quaternion.identity);
		}
		Simulate = simulate;
	}

	internal void SetTeleportInternal(bool teleport)
	{
		SkipInterpolation = teleport;
	}

	internal void SetTransformsInternal(Transform simulate, Transform render)
	{
		if (simulate != null)
		{
			Render = render;
			Simulate = simulate;
			if (space == TransformSpaces.World)
			{
				RenderDoubleBufferPosition = DoubleBuffer<Vector3>.InitBuffer(simulate.position);
			}
			else if (space == TransformSpaces.Local)
			{
				RenderDoubleBufferPosition = DoubleBuffer<Vector3>.InitBuffer(simulate.localPosition);
			}
			RenderDoubleBufferRotation = DoubleBuffer<Quaternion>.InitBuffer(simulate.rotation);
		}
		else
		{
			Simulate = null;
			Render = null;
			RenderDoubleBufferPosition = DoubleBuffer<Vector3>.InitBuffer(Vector3.zero);
			RenderDoubleBufferRotation = DoubleBuffer<Quaternion>.InitBuffer(Quaternion.identity);
		}
	}
}
