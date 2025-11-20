#define DEBUG
using System;
using Photon.Bolt.Collections;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;
using UnityEngine;

namespace Photon.Bolt.Utils;

[Documentation]
internal static class Math
{
	internal static float InterpolateFloat(BoltDoubleList<NetworkStorage> frames, int offset, int frame, bool angle)
	{
		NetworkStorage first = frames.first;
		float @float = first.Values[offset].Float1;
		if (frames.count == 1 || first.Frame >= frame)
		{
			return @float;
		}
		NetworkStorage networkStorage = frames.Next(first);
		float float2 = networkStorage.Values[offset].Float1;
		Assert.True(networkStorage.Frame > first.Frame);
		Assert.True(networkStorage.Frame > frame);
		int num = first.Frame;
		if (num < networkStorage.Frame - BoltCore.remoteSendRate * 2)
		{
			num = networkStorage.Frame - BoltCore.remoteSendRate * 2;
		}
		float num2 = networkStorage.Frame - num;
		float num3 = frame - num;
		return angle ? Mathf.LerpAngle(@float, float2, num3 / num2) : Mathf.Lerp(@float, float2, num3 / num2);
	}

	internal static Vector3 InterpolateVector(BoltDoubleList<NetworkStorage> frames, int offset, int frame, float snapLimit)
	{
		bool snapped = false;
		return InterpolateVector(frames, offset, frame, snapLimit, ref snapped);
	}

	internal static Vector3 InterpolateVector(BoltDoubleList<NetworkStorage> frames, int offset, int frame, float snapLimit, ref bool snapped)
	{
		NetworkStorage first = frames.first;
		Vector3 vector = first.Values[offset].Vector3;
		if (frames.count == 1 || first.Frame >= frame)
		{
			return vector;
		}
		NetworkStorage networkStorage = frames.Next(first);
		Vector3 vector2 = networkStorage.Values[offset].Vector3;
		Assert.True(networkStorage.Frame > first.Frame);
		Assert.True(networkStorage.Frame > frame);
		if ((vector - vector2).sqrMagnitude > snapLimit * snapLimit)
		{
			snapped = true;
			return vector2;
		}
		int num = first.Frame;
		if (num < networkStorage.Frame - BoltCore.remoteSendRate * 2)
		{
			num = networkStorage.Frame - BoltCore.remoteSendRate * 2;
		}
		float num2 = networkStorage.Frame - num;
		float num3 = frame - num;
		return Vector3.Lerp(vector, vector2, num3 / num2);
	}

	internal static Quaternion InterpolateQuaternion(BoltDoubleList<NetworkStorage> frames, int offset, int frame)
	{
		NetworkStorage first = frames.first;
		Quaternion quaternion = first.Values[offset].Quaternion;
		if (frames.count == 1 || first.Frame >= frame)
		{
			return quaternion;
		}
		NetworkStorage networkStorage = frames.Next(first);
		Quaternion quaternion2 = networkStorage.Values[offset].Quaternion;
		Assert.True(networkStorage.Frame > first.Frame);
		Assert.True(networkStorage.Frame > frame);
		int num = first.Frame;
		if (num < networkStorage.Frame - BoltCore.remoteSendRate * 2)
		{
			num = networkStorage.Frame - BoltCore.remoteSendRate * 2;
		}
		float num2 = networkStorage.Frame - num;
		float num3 = frame - num;
		return Quaternion.Slerp(quaternion, quaternion2, num3 / num2);
	}

	internal static Vector3 ExtrapolateVector(Vector3 cpos, Vector3 rpos, Vector3 rvel, int recievedFrame, int entityFrame, PropertyExtrapolationSettings settings, ref bool snapped)
	{
		rvel *= BoltNetwork.FrameDeltaTime;
		float num = System.Math.Min(settings.MaxFrames, entityFrame + 1 - recievedFrame);
		float t = num / (float)System.Math.Max(1, settings.MaxFrames);
		Vector3 vector = cpos + rvel;
		Vector3 vector2 = rpos + rvel * num;
		float sqrMagnitude = (vector2 - vector).sqrMagnitude;
		if (settings.SnapMagnitude > 0f && sqrMagnitude > settings.SnapMagnitude * settings.SnapMagnitude)
		{
			snapped = true;
			return vector2;
		}
		return Vector3.Lerp(vector, vector2, t);
	}

	internal static Quaternion ExtrapolateQuaternion(Quaternion cquat, Quaternion rquat, int recievedFrame, int entityFrame, PropertyExtrapolationSettings settings)
	{
		Quaternion quaternion = rquat * Quaternion.Inverse(cquat);
		float num = System.Math.Min(settings.MaxFrames, entityFrame + 1 - recievedFrame);
		float num2 = num / (float)System.Math.Max(1, settings.MaxFrames);
		quaternion.ToAngleAxis(out var angle, out var axis);
		if (float.IsInfinity(axis.x) || float.IsNaN(axis.x))
		{
			angle = 0f;
			axis = Vector3.right;
		}
		else if (angle > 180f)
		{
			angle -= 360f;
		}
		return Quaternion.AngleAxis(angle * num2 % 360f, axis) * cquat;
	}

	internal static int SequenceDistance(uint from, uint to, int shift)
	{
		from <<= shift;
		to <<= shift;
		return (int)(from - to) >> shift;
	}

	public static int HighBit(uint v)
	{
		int num = 0;
		while (v != 0)
		{
			num++;
			v >>= 1;
		}
		return num;
	}

	public static int BytesRequired(int bits)
	{
		return bits + 7 >> 3;
	}

	public static int BitsRequired(int number)
	{
		if (number < 0)
		{
			return 32;
		}
		if (number == 0)
		{
			return 1;
		}
		for (int num = 31; num >= 0; num--)
		{
			int num2 = 1 << num;
			if ((number & num2) == num2)
			{
				return num + 1;
			}
		}
		throw new Exception();
	}

	public static int PopCount(ulong value)
	{
		int num = 0;
		for (int i = 0; i < 32; i++)
		{
			if ((value & (ulong)(1L << i)) != 0)
			{
				num++;
			}
		}
		return num;
	}
}
