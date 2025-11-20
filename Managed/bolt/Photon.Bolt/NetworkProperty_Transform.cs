#define DEBUG
using Photon.Bolt.Collections;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

internal class NetworkProperty_Transform : NetworkProperty
{
	private const int POSITION = 0;

	private const int ROTATION = 1;

	private const int VELOCITY = 2;

	private int PositionMask;

	private int RotationMask;

	private bool PositionEnabled = false;

	private bool RotationEnabled = false;

	private TransformSpaces Space;

	private PropertyExtrapolationSettings Extrapolation;

	internal PropertyQuaternionCompression RotationCompression;

	internal PropertyVectorCompressionSettings PositionCompression;

	public override bool AllowCallbacks => false;

	public override bool WantsOnRender => true;

	public override bool WantsOnSimulateAfter => true;

	public override bool WantsOnSimulateBefore => true;

	public void Settings_Space(TransformSpaces space)
	{
		Space = space;
	}

	public void Settings_Vector(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z, bool strict)
	{
		PositionEnabled = true;
		PositionCompression = PropertyVectorCompressionSettings.Create(x, y, z, strict);
		if (PositionCompression.X.BitsRequired > 0)
		{
			PositionMask |= 1;
		}
		if (PositionCompression.Y.BitsRequired > 0)
		{
			PositionMask |= 2;
		}
		if (PositionCompression.Z.BitsRequired > 0)
		{
			PositionMask |= 4;
		}
	}

	public void Settings_Quaternion(PropertyFloatCompressionSettings compression, bool strict)
	{
		RotationEnabled = true;
		RotationCompression = PropertyQuaternionCompression.Create(compression, strict);
	}

	public void Settings_QuaternionEuler(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z, bool strict)
	{
		RotationEnabled = true;
		RotationCompression = PropertyQuaternionCompression.Create(PropertyVectorCompressionSettings.Create(x, y, z, strict));
		if (RotationCompression.Euler.X.BitsRequired > 0)
		{
			RotationMask |= 1;
		}
		if (RotationCompression.Euler.Y.BitsRequired > 0)
		{
			RotationMask |= 2;
		}
		if (RotationCompression.Euler.Z.BitsRequired > 0)
		{
			RotationMask |= 4;
		}
	}

	public void Settings_Extrapolation(PropertyExtrapolationSettings extrapolation)
	{
		Extrapolation = extrapolation;
	}

	private Vector3 GetPosition(Transform t)
	{
		if (PositionMask == 7)
		{
			if (Space == TransformSpaces.World)
			{
				return t.position;
			}
			return t.localPosition;
		}
		Vector3 result = ((Space == TransformSpaces.World) ? t.position : t.localPosition);
		switch (PositionMask)
		{
		case 6:
			result.x = 0f;
			break;
		case 5:
			result.y = 0f;
			break;
		case 4:
			result.x = 0f;
			result.y = 0f;
			break;
		case 3:
			result.z = 0f;
			break;
		case 2:
			result.x = 0f;
			result.z = 0f;
			break;
		case 1:
			result.y = 0f;
			result.z = 0f;
			break;
		}
		return result;
	}

	private void SetPosition(Transform t, Vector3 p)
	{
		if (PositionMask == 7)
		{
			if (Space == TransformSpaces.World)
			{
				t.position = p;
			}
			else
			{
				t.localPosition = p;
			}
		}
		else if (Space == TransformSpaces.World)
		{
			Vector3 position = t.position;
			switch (PositionMask)
			{
			case 6:
				t.position = new Vector3(position.x, p.y, p.z);
				break;
			case 5:
				t.position = new Vector3(p.x, position.y, p.z);
				break;
			case 4:
				t.position = new Vector3(position.x, position.y, p.z);
				break;
			case 3:
				t.position = new Vector3(p.x, p.y, position.z);
				break;
			case 2:
				t.position = new Vector3(position.x, p.y, position.z);
				break;
			case 1:
				t.position = new Vector3(p.x, position.y, position.z);
				break;
			}
		}
		else
		{
			Vector3 localPosition = t.localPosition;
			switch (PositionMask)
			{
			case 6:
				t.localPosition = new Vector3(localPosition.x, p.y, p.z);
				break;
			case 5:
				t.localPosition = new Vector3(p.x, localPosition.y, p.z);
				break;
			case 4:
				t.localPosition = new Vector3(localPosition.x, localPosition.y, p.z);
				break;
			case 3:
				t.localPosition = new Vector3(p.x, p.y, localPosition.z);
				break;
			case 2:
				t.localPosition = new Vector3(localPosition.x, p.y, localPosition.z);
				break;
			case 1:
				t.localPosition = new Vector3(p.x, localPosition.y, localPosition.z);
				break;
			}
		}
	}

	private void SetRotation(Transform t, Quaternion q)
	{
		if (RotationMask == 0 || RotationMask == 7)
		{
			if (Space == TransformSpaces.World)
			{
				t.rotation = q;
			}
			else
			{
				t.localRotation = q;
			}
			return;
		}
		Vector3 eulerAngles = q.eulerAngles;
		Vector3 euler = ((Space == TransformSpaces.World) ? t.rotation.eulerAngles : t.localRotation.eulerAngles);
		switch (RotationMask)
		{
		case 6:
			euler.y = eulerAngles.y;
			euler.z = eulerAngles.z;
			break;
		case 5:
			euler.x = eulerAngles.x;
			euler.z = eulerAngles.z;
			break;
		case 4:
			euler.z = eulerAngles.z;
			break;
		case 3:
			euler.x = eulerAngles.x;
			euler.y = eulerAngles.y;
			break;
		case 2:
			euler.y = eulerAngles.y;
			break;
		case 1:
			euler.x = eulerAngles.x;
			break;
		}
		if (Space == TransformSpaces.World)
		{
			t.rotation = Quaternion.Euler(euler);
		}
		else
		{
			t.localRotation = Quaternion.Euler(euler);
		}
	}

	private Quaternion GetRotation(Transform t)
	{
		if (RotationMask == 0 || RotationMask == 7)
		{
			return (Space == TransformSpaces.World) ? t.rotation : t.localRotation;
		}
		Vector3 euler = ((Space == TransformSpaces.World) ? t.rotation.eulerAngles : t.localRotation.eulerAngles);
		switch (RotationMask)
		{
		case 6:
			euler.x = 0f;
			break;
		case 5:
			euler.y = 0f;
			break;
		case 4:
			euler.x = 0f;
			euler.y = 0f;
			break;
		case 3:
			euler.z = 0f;
			break;
		case 2:
			euler.x = 0f;
			euler.z = 0f;
			break;
		case 1:
			euler.y = 0f;
			euler.z = 0f;
			break;
		}
		return Quaternion.Euler(euler);
	}

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].Transform;
	}

	public override int BitCount(NetworkObj obj)
	{
		if (Extrapolation.Enabled)
		{
			return PositionCompression.BitsRequired * 2 + RotationCompression.BitsRequired;
		}
		return PositionCompression.BitsRequired + RotationCompression.BitsRequired;
	}

	public override void OnInit(NetworkObj obj)
	{
		obj.Storage.Values[obj[this]].Transform = new NetworkTransform(Space);
		obj.Storage.Values[obj[this]].Transform.PropertyIndex = obj[this];
		obj.Storage.Values[obj[this] + 1].Quaternion = Quaternion.identity;
	}

	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		NetworkTransform transform = obj.Storage.Values[obj[this]].Transform;
		if (transform != null && (bool)transform.Simulate)
		{
			Vector3 vector = obj.Storage.Values[obj[this]].Vector3;
			Quaternion quaternion = obj.Storage.Values[obj[this] + 1].Quaternion;
			string arg = string.Format("X:{0} Y:{1} Z:{2}", vector.x.ToString("F2"), vector.y.ToString("F2"), vector.z.ToString("F2"));
			string arg2 = string.Format("X:{0} Y:{1} Z:{2}", quaternion.x.ToString("F2"), quaternion.y.ToString("F2"), quaternion.z.ToString("F2"));
			string arg3 = "";
			if ((bool)transform.Render)
			{
				arg3 = $"(R: {transform.Render.gameObject.name})";
			}
			return $"{arg} / {arg2}{arg3}";
		}
		return "NOT ASSIGNED";
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		if (obj.RootState.Entity.HasParent)
		{
			if (!connection._entityChannel.ExistsOnRemote(obj.RootState.Entity.Parent))
			{
				return false;
			}
			packet.WriteEntity(obj.RootState.Entity.Parent);
		}
		else
		{
			packet.WriteEntity(null);
		}
		packet.WriteBool(storage.Values[obj[this]].Transform.Teleport);
		storage.Values[obj[this]].Transform.SetTeleportInternal(teleport: false);
		if (PositionEnabled)
		{
			PositionCompression.Pack(packet, storage.Values[obj[this]].Vector3);
			if (Extrapolation.Enabled)
			{
				PositionCompression.Pack(packet, storage.Values[obj[this] + 2].Vector3);
			}
		}
		if (RotationEnabled)
		{
			RotationCompression.Pack(packet, storage.Values[obj[this] + 1].Quaternion);
		}
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		obj.RootState.Entity.SetParentInternal(packet.ReadEntity());
		if (packet.ReadBool())
		{
			BoltIterator<NetworkStorage> iterator = obj.RootState.Frames.GetIterator();
			Vector3 vector = Vector3.zero;
			if (PositionEnabled)
			{
				vector = PositionCompression.Read(packet);
				if (Extrapolation.Enabled)
				{
					PositionCompression.Read(packet);
				}
			}
			Quaternion quaternion = Quaternion.identity;
			if (RotationEnabled)
			{
				quaternion = RotationCompression.Read(packet);
			}
			while (iterator.Next())
			{
				iterator.val.Values[obj[this]].Vector3 = vector;
				iterator.val.Values[obj[this] + 1].Quaternion = quaternion;
			}
			return;
		}
		if (PositionEnabled)
		{
			storage.Values[obj[this]].Vector3 = PositionCompression.Read(packet);
			if (Extrapolation.Enabled)
			{
				storage.Values[obj[this] + 2].Vector3 = PositionCompression.Read(packet);
			}
		}
		if (RotationEnabled)
		{
			storage.Values[obj[this] + 1].Quaternion = RotationCompression.Read(packet);
		}
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		if (other.Values[obj[this]].Transform.Teleport)
		{
			BoltIterator<NetworkStorage> iterator = obj.RootState.Frames.GetIterator();
			Vector3 vector = Vector3.zero;
			if (PositionEnabled)
			{
				vector = other.Values[obj[this]].Vector3;
			}
			Quaternion quaternion = Quaternion.identity;
			if (RotationEnabled)
			{
				quaternion = other.Values[obj[this] + 1].Quaternion;
			}
			while (iterator.Next())
			{
				iterator.val.Values[obj[this]].Vector3 = vector;
				iterator.val.Values[obj[this] + 1].Quaternion = quaternion;
			}
			return;
		}
		if (PositionEnabled)
		{
			storage.Values[obj[this]].Vector3 = other.Values[obj[this]].Vector3;
			if (Extrapolation.Enabled)
			{
				storage.Values[obj[this] + 2].Vector3 = other.Values[obj[this] + 2].Vector3;
			}
		}
		if (RotationEnabled)
		{
			storage.Values[obj[this] + 1].Quaternion = other.Values[obj[this] + 1].Quaternion;
		}
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		if (storage1.Values[obj[this]].Transform.Teleport != storage2.Values[obj[this]].Transform.Teleport)
		{
			return false;
		}
		if (PositionEnabled)
		{
			if (storage1.Values[obj[this]].Vector3 != storage2.Values[obj[this]].Vector3)
			{
				return false;
			}
			if (Extrapolation.Enabled && storage1.Values[obj[this] + 2].Vector3 != storage2.Values[obj[this] + 2].Vector3)
			{
				return false;
			}
		}
		if (RotationEnabled && storage1.Values[obj[this] + 1].Quaternion != storage2.Values[obj[this] + 1].Quaternion)
		{
			return false;
		}
		return true;
	}

	public override bool SupportsDeltaCompression()
	{
		return true;
	}

	public override void OnRender(NetworkObj obj)
	{
		if (obj.RootState.Entity.IsOwner)
		{
			return;
		}
		NetworkTransform transform = obj.Storage.Values[obj[this]].Transform;
		if (transform != null && (bool)transform.Render)
		{
			if (PositionEnabled)
			{
				Vector3 previous = transform.RenderDoubleBufferPosition.Previous;
				Vector3 current = transform.RenderDoubleBufferPosition.Current;
				transform.Render.position = Vector3.Lerp(previous, current, BoltCore.frameAlpha);
			}
			if (RotationEnabled)
			{
				Quaternion previous2 = transform.RenderDoubleBufferRotation.Previous;
				Quaternion current2 = transform.RenderDoubleBufferRotation.Current;
				transform.Render.rotation = Quaternion.Slerp(previous2, current2, BoltCore.frameAlpha);
			}
		}
	}

	public override void OnSimulateAfter(NetworkObj obj)
	{
		NetworkTransform transform = obj.Storage.Values[obj[this]].Transform;
		if (transform == null || !transform.Simulate)
		{
			return;
		}
		if (obj.RootState.Entity.IsOwner)
		{
			Vector3 vector = obj.Storage.Values[obj[this]].Vector3;
			Vector3 vector2 = obj.Storage.Values[obj[this] + 2].Vector3;
			Quaternion quaternion = obj.Storage.Values[obj[this] + 1].Quaternion;
			obj.Storage.Values[obj[this]].Vector3 = GetPosition(transform.Simulate);
			obj.Storage.Values[obj[this] + 2].Vector3 = CalculateVelocity(transform, vector);
			obj.Storage.Values[obj[this] + 1].Quaternion = GetRotation(transform.Simulate);
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (PositionCompression.StrictComparison)
			{
				flag = NetworkValue.Diff_Strict(vector, obj.Storage.Values[obj[this]].Vector3);
				if (Extrapolation.Enabled)
				{
					flag2 = NetworkValue.Diff_Strict(vector2, obj.Storage.Values[obj[this] + 2].Vector3);
				}
			}
			else
			{
				flag = NetworkValue.Diff(vector, obj.Storage.Values[obj[this]].Vector3);
				if (Extrapolation.Enabled)
				{
					flag2 = NetworkValue.Diff(vector2, obj.Storage.Values[obj[this] + 2].Vector3);
				}
				if (flag && (vector - obj.Storage.Values[obj[this]].Vector3).magnitude < 0.001f)
				{
					flag = false;
				}
				if (flag2 && (vector2 - obj.Storage.Values[obj[this] + 2].Vector3).magnitude < 0.001f)
				{
					flag2 = false;
				}
			}
			if (RotationCompression.StrictComparison)
			{
				flag3 = NetworkValue.Diff_Strict(quaternion, obj.Storage.Values[obj[this] + 1].Quaternion);
			}
			else
			{
				flag3 = NetworkValue.Diff(quaternion, obj.Storage.Values[obj[this] + 1].Quaternion);
				if (flag3)
				{
					Quaternion quaternion2 = obj.Storage.Values[obj[this] + 1].Quaternion;
					Vector4 vector3 = new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
					Vector4 vector4 = new Vector4(quaternion2.x, quaternion2.y, quaternion2.z, quaternion2.w);
					if ((vector3 - vector4).magnitude < 0.001f)
					{
						flag3 = false;
					}
				}
			}
			if (flag || flag2 || flag3)
			{
				obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
			}
		}
		transform.RenderDoubleBufferPosition = transform.RenderDoubleBufferPosition.Shift(transform.Simulate.position);
		transform.RenderDoubleBufferRotation = transform.RenderDoubleBufferRotation.Shift(transform.Simulate.rotation);
	}

	public override void OnSimulateBefore(NetworkObj obj)
	{
		NetworkState networkState = (NetworkState)obj.Root;
		if (networkState.Entity.IsOwner || (networkState.Entity.HasControl && !ToController))
		{
			return;
		}
		NetworkTransform transform = obj.Storage.Values[obj[this]].Transform;
		if (transform == null || !transform.Simulate)
		{
			return;
		}
		bool snapped = false;
		Vector3 p = Vector3.zero;
		Quaternion q = Quaternion.identity;
		if (Extrapolation.Enabled)
		{
			if (PositionEnabled)
			{
				p = Math.ExtrapolateVector(GetPosition(transform.Simulate), obj.Storage.Values[obj[this]].Vector3, obj.Storage.Values[obj[this] + 2].Vector3, obj.RootState.Frames.first.Frame, obj.RootState.Entity.Frame, Extrapolation, ref snapped);
				p = transform.Clamper(obj.RootState.Entity.UnityObject, p);
			}
			if (RotationEnabled)
			{
				q = Math.ExtrapolateQuaternion(GetRotation(transform.Simulate), obj.Storage.Values[obj[this] + 1].Quaternion, obj.RootState.Frames.first.Frame, obj.RootState.Entity.Frame, Extrapolation);
			}
		}
		else if (Interpolation.Enabled)
		{
			if (PositionEnabled)
			{
				p = Math.InterpolateVector(obj.RootState.Frames, obj[this], obj.RootState.Entity.Frame, Interpolation.SnapMagnitude, ref snapped);
			}
			if (RotationEnabled)
			{
				q = Math.InterpolateQuaternion(obj.RootState.Frames, obj[this] + 1, obj.RootState.Entity.Frame);
			}
		}
		else
		{
			snapped = true;
			if (PositionEnabled)
			{
				p = obj.Storage.Values[obj[this]].Vector3;
			}
			if (RotationEnabled)
			{
				q = obj.Storage.Values[obj[this] + 1].Quaternion;
			}
		}
		if (PositionEnabled)
		{
			SetPosition(transform.Simulate, p);
			if (snapped)
			{
				transform.RenderDoubleBufferPosition = transform.RenderDoubleBufferPosition.Shift(transform.Simulate.position).Shift(transform.Simulate.position);
			}
		}
		if (RotationEnabled)
		{
			SetRotation(transform.Simulate, q);
		}
	}

	public override void OnParentChanged(NetworkObj obj, Entity newParent, Entity oldParent)
	{
		NetworkTransform transform = obj.Storage.Values[obj[this]].Transform;
		if (transform != null && (bool)transform.Simulate)
		{
			transform.Simulate.transform.SetParent(newParent?.UnityObject.transform, worldPositionStays: true);
			Matrix4x4 l2w = oldParent?.UnityObject.transform.localToWorldMatrix ?? Matrix4x4.identity;
			Matrix4x4 w2l = newParent?.UnityObject.transform.worldToLocalMatrix ?? Matrix4x4.identity;
			if (transform.space == TransformSpaces.Local)
			{
				UpdateTransformValues(obj, l2w, w2l);
			}
		}
		if (obj.RootState.Entity.IsOwner)
		{
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	private Vector3 CalculateVelocity(NetworkTransform nt, Vector3 position)
	{
		switch (Extrapolation.VelocityMode)
		{
		case ExtrapolationVelocityModes.CalculateFromPosition:
			return (GetPosition(nt.Simulate) - position) * BoltCore._config.framesPerSecond;
		case ExtrapolationVelocityModes.CopyFromRigidbody:
			return nt.Simulate.GetComponent<Rigidbody>().velocity;
		case ExtrapolationVelocityModes.CopyFromRigidbody2D:
			return nt.Simulate.GetComponent<Rigidbody2D>().velocity;
		case ExtrapolationVelocityModes.CopyFromCharacterController:
			return nt.Simulate.GetComponent<CharacterController>().velocity;
		default:
			BoltLog.Error("Unknown velocity extrapolation mode {0}", Extrapolation.VelocityMode);
			return (GetPosition(nt.Simulate) - position) * BoltCore._config.framesPerSecond;
		}
	}

	private void UpdateTransformValues(NetworkObj obj, Matrix4x4 l2w, Matrix4x4 w2l)
	{
		BoltIterator<NetworkStorage> iterator = obj.RootState.Frames.GetIterator();
		while (iterator.Next())
		{
			NetworkStorage val = iterator.val;
			Vector3 vector = val.Values[obj[this]].Vector3;
			Quaternion quaternion = val.Values[obj[this] + 1].Quaternion;
			quaternion.ToAngleAxis(out var angle, out var axis);
			vector = l2w.MultiplyPoint(vector);
			vector = w2l.MultiplyPoint(vector);
			axis = l2w.MultiplyVector(axis);
			axis = w2l.MultiplyVector(axis);
			quaternion = Quaternion.AngleAxis(angle, axis);
			val.Values[obj[this]].Vector3 = vector;
			val.Values[obj[this] + 1].Quaternion = quaternion;
		}
	}
}
