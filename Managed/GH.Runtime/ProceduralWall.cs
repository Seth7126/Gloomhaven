using System;
using System.Collections.Generic;
using Apparance.Net;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(ApparanceEntity))]
[RequireComponent(typeof(ProceduralStyle))]
public class ProceduralWall : ProceduralTileObserver
{
	public struct CornerInfo
	{
		public bool IsOwner;

		public UnityEngine.Vector3 Position;

		public float InsideAngle;

		public float AdjacentDepth;

		public int Flags;
	}

	[NonSerialized]
	public bool IsOwner = true;

	private static List<ProceduralWall> m_WallCache = new List<ProceduralWall>();

	public CornerInfo LeftCorner { get; set; }

	public CornerInfo RightCorner { get; set; }

	public float Length { get; set; }

	public float perimeterOffset { get; set; }

	public float perimiterLength { get; set; }

	private void Start()
	{
	}

	public override void WriteExtraParameters(ParameterCollection parameters, int next_id)
	{
		parameters.WriteBool(IsOwner, next_id++);
		ParameterCollection parameterCollection = parameters.WriteListBegin(next_id++);
		parameterCollection.WriteBool(LeftCorner.IsOwner);
		parameterCollection.WriteVector3(Conversion.AVfromUV(LeftCorner.Position));
		parameterCollection.WriteFloat(LeftCorner.InsideAngle);
		parameterCollection.WriteFloat(LeftCorner.AdjacentDepth);
		parameterCollection.WriteInteger(LeftCorner.Flags);
		parameters.WriteListEnd();
		ParameterCollection parameterCollection2 = parameters.WriteListBegin(next_id++);
		parameterCollection2.WriteBool(RightCorner.IsOwner);
		parameterCollection2.WriteVector3(Conversion.AVfromUV(RightCorner.Position));
		parameterCollection2.WriteFloat(RightCorner.InsideAngle);
		parameterCollection2.WriteFloat(RightCorner.AdjacentDepth);
		parameterCollection2.WriteInteger(RightCorner.Flags);
		parameters.WriteListEnd();
		ProceduralStyle proceduralStyle = GloomUtility.EnsureComponent<ProceduralStyle>(base.gameObject);
		ParameterCollection style_struct = parameters.WriteListBegin(next_id++);
		proceduralStyle.WriteStyleParameters(style_struct, GetScenarioSeed(), GetRoomSeed(), proceduralStyle.Seed);
		parameters.WriteListEnd();
		ParameterCollection parameterCollection3 = parameters.WriteListBegin(next_id++);
		parameterCollection3.WriteFloat(Length);
		parameterCollection3.WriteFloat(perimeterOffset);
		parameterCollection3.WriteFloat(perimiterLength);
		parameters.WriteListEnd();
		ApparancePlatformSettingData apparenceSettingByCurrentLevel = PlatformLayer.Setting.GetApparenceSettingByCurrentLevel();
		int integer_value = ((PlatformLayer.Setting != null) ? apparenceSettingByCurrentLevel._qualityLevel : 0);
		parameters.WriteInteger(integer_value, next_id++);
		bool bool_value = PlatformLayer.Setting != null && apparenceSettingByCurrentLevel._showUnderground;
		parameters.WriteBool(bool_value, next_id++);
	}

	[UsedImplicitly]
	protected override void Awake()
	{
		base.Awake();
		m_WallCache.Add(this);
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		m_WallCache.Remove(this);
	}

	public static IEnumerable<ProceduralWall> FindWallsNear(UnityEngine.Vector3 position, float tolerance = 1f)
	{
		IEnumerable<ProceduralWall> wallCache = m_WallCache;
		foreach (ProceduralWall item in wallCache)
		{
			Bounds bounds = default(Bounds);
			CollectBounds(item.gameObject, ref bounds);
			bounds.Expand(tolerance);
			if (bounds.Contains(position))
			{
				yield return item;
			}
		}
	}

	public static void CollectBounds(GameObject obj, ref Bounds bounds)
	{
		if (!(obj != null))
		{
			return;
		}
		BoxCollider component = obj.GetComponent<BoxCollider>();
		if (component != null)
		{
			if (bounds.size.x < 0.001f && bounds.size.y < 0.001f && bounds.size.z < 0.001f)
			{
				bounds = component.bounds;
			}
			else
			{
				bounds.Encapsulate(component.bounds);
			}
		}
		else
		{
			for (int i = 0; i < obj.transform.childCount; i++)
			{
				CollectBounds(obj.transform.GetChild(i).gameObject, ref bounds);
			}
		}
	}
}
