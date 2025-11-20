using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SM.Utils;

[Serializable]
public class FreeLevelResetTicket : ISerializable
{
	public string Id;

	public int Order;

	public bool IsUsed;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Id", Id);
		info.AddValue("Order", Order);
		info.AddValue("IsUsed", IsUsed);
	}

	protected FreeLevelResetTicket(SerializationInfo info, StreamingContext context)
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
				LogUtils.LogError("Exception while trying to deserialize FreeLevelResetTicket entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public FreeLevelResetTicket(string id, int order)
	{
		Id = id;
		Order = order;
		IsUsed = false;
	}
}
