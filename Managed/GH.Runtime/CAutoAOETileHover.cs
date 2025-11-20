using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SM.Utils;
using ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("PlacementTile: {PlacementTile}")]
public class CAutoAOETileHover : CAuto, ISerializable
{
	public TileIndex PlacementTile;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PlacementTile", PlacementTile);
	}

	protected CAutoAOETileHover(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "PlacementTile")
				{
					PlacementTile = (TileIndex)info.GetValue("PlacementTile", typeof(TileIndex));
				}
			}
			catch (Exception ex)
			{
				LogUtils.LogError("Exception while trying to deserialize CAutoAOETileHover entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CAutoAOETileHover(int id, TileIndex placementTile)
		: base(EAutoType.AOETileHover, id)
	{
		PlacementTile = placementTile;
	}
}
