using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.Locations;
using StateCodeGenerator;

namespace MapRuleLibrary.MapState;

[Serializable]
public class CStoreLocationState : CLocationState, ISerializable
{
	public CStoreLocation StoreLocation => MapRuleLibraryClient.MRLYML.StoreLocations.SingleOrDefault((CStoreLocation s) => s.ID == base.ID);

	public CStoreLocationState()
	{
	}

	public CStoreLocationState(CStoreLocationState state, ReferenceDictionary references)
		: base(state, references)
	{
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CStoreLocationState(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (base.Location != null)
		{
			if (base.UnlockConditionState == null && base.Location.UnlockCondition != null)
			{
				base.UnlockConditionState = new CUnlockConditionState(base.Location.UnlockCondition);
			}
			if (base.UnlockConditionState != null && base.Location.UnlockCondition != null)
			{
				base.UnlockConditionState.CacheUnlockCondition(base.Location.UnlockCondition);
			}
		}
	}

	public CStoreLocationState(CStoreLocation storeLocation)
		: base(storeLocation)
	{
		base.Mesh = storeLocation.Mesh;
	}
}
