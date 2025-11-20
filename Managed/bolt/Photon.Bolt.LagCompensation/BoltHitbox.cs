using System;
using UnityEngine;

namespace Photon.Bolt.LagCompensation;

[Documentation]
public class BoltHitbox : MonoBehaviour
{
	[SerializeField]
	internal BoltHitboxShape _shape = BoltHitboxShape.Box;

	[SerializeField]
	internal BoltHitboxType _type = BoltHitboxType.Unknown;

	[SerializeField]
	internal Vector3 _center = Vector3.zero;

	[SerializeField]
	internal Vector3 _boxSize = new Vector3(0.25f, 0.25f, 0.25f);

	[SerializeField]
	internal float _sphereRadius = 0.25f;

	public BoltHitboxShape hitboxShape
	{
		get
		{
			return _shape;
		}
		set
		{
			_shape = value;
		}
	}

	public BoltHitboxType hitboxType
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	public Vector3 hitboxCenter
	{
		get
		{
			return _center;
		}
		set
		{
			_center = value;
		}
	}

	public Vector3 hitboxBoxSize
	{
		get
		{
			return _boxSize;
		}
		set
		{
			_boxSize = value;
		}
	}

	public float hitboxSphereRadius
	{
		get
		{
			return _sphereRadius;
		}
		set
		{
			_sphereRadius = value;
		}
	}

	private void OnDrawGizmos()
	{
		Draw(base.transform.localToWorldMatrix);
	}

	internal bool OverlapSphere(ref Matrix4x4 matrix, Vector3 center, float radius)
	{
		center = matrix.MultiplyPoint(center);
		return _shape switch
		{
			BoltHitboxShape.Box => OverlapSphereOnBox(center, radius), 
			BoltHitboxShape.Sphere => OverlapSphereOnSphere(center, radius), 
			_ => false, 
		};
	}

	internal bool Raycast(ref Matrix4x4 matrix, ref Vector3 scale, Vector3 origin, Vector3 direction, out float distance)
	{
		origin = matrix.MultiplyPoint(origin);
		direction = matrix.MultiplyVector(direction);
		switch (_shape)
		{
		case BoltHitboxShape.Box:
			return new Bounds(_center, Vector3.Scale(_boxSize, scale)).IntersectRay(new Ray(origin, direction), out distance);
		case BoltHitboxShape.Sphere:
			return RaycastSphere(origin, direction, scale, out distance);
		default:
			distance = 0f;
			return false;
		}
	}

	private bool OverlapSphereOnSphere(Vector3 center, float radius)
	{
		return Vector3.Distance(_center, center) <= _sphereRadius + radius;
	}

	private bool OverlapSphereOnBox(Vector3 center, float radius)
	{
		Bounds bounds = new Bounds(_center, _boxSize);
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		ClampVector(ref center, ref min, ref max, out var result);
		return Vector3.Distance(center, result) <= radius;
	}

	private bool RaycastSphere(Vector3 origin, Vector3 direction, Vector3 scale, out float distance)
	{
		float num = Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z)) * _sphereRadius;
		Vector3 vector = origin - _center;
		float num2 = Vector3.Dot(vector, direction);
		float num3 = Vector3.Dot(vector, vector) - num * num;
		if (num3 > 0f && num2 > 0f)
		{
			distance = 0f;
			return false;
		}
		float num4 = num2 * num2 - num3;
		if (num4 < 0f)
		{
			distance = 0f;
			return false;
		}
		distance = 0f - num2 - (float)Math.Sqrt(num4);
		if (distance < 0f)
		{
			distance = 0f;
		}
		return true;
	}

	private static void ClampVector(ref Vector3 value, ref Vector3 min, ref Vector3 max, out Vector3 result)
	{
		float x = value.x;
		x = ((x > max.x) ? max.x : x);
		x = ((x < min.x) ? min.x : x);
		float y = value.y;
		y = ((y > max.y) ? max.y : y);
		y = ((y < min.y) ? min.y : y);
		float z = value.z;
		z = ((z > max.z) ? max.z : z);
		z = ((z < min.z) ? min.z : z);
		result = new Vector3(x, y, z);
	}

	internal void Draw(Matrix4x4 matrix)
	{
		Color color = Gizmos.color;
		Gizmos.color = new Color(1f, 0.5019608f, 13f / 85f);
		Gizmos.matrix = matrix;
		switch (_shape)
		{
		case BoltHitboxShape.Box:
			Gizmos.DrawWireCube(_center, _boxSize);
			break;
		case BoltHitboxShape.Sphere:
			Gizmos.DrawWireSphere(_center, _sphereRadius);
			break;
		}
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = color;
	}
}
