using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.Party;

[Serializable]
public class CFreeLevelResetTicket : ISerializable
{
	public string Id;

	public int Order;

	public bool IsUsed;

	public CFreeLevelResetTicket()
	{
	}

	public CFreeLevelResetTicket(CFreeLevelResetTicket state, ReferenceDictionary references)
	{
		Id = state.Id;
		Order = state.Order;
		IsUsed = state.IsUsed;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Id", Id);
		info.AddValue("Order", Order);
		info.AddValue("IsUsed", IsUsed);
	}

	protected CFreeLevelResetTicket(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Id":
					Id = info.GetString("Id");
					break;
				case "IsUsed":
					IsUsed = info.GetBoolean("IsUsed");
					break;
				case "Order":
					Order = info.GetInt32("Order");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CFreeLevelResetTicket entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CFreeLevelResetTicket(string id, int order)
	{
		Id = id;
		Order = order;
		IsUsed = false;
	}
}
