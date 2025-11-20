using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.State;

[Serializable]
public class MapStateSaveOwner : ISerializable
{
	public string PlatformPlayerID { get; private set; }

	public string PlatformAccountID { get; private set; }

	public string PlatformNetworkAccountID { get; private set; }

	public string Username { get; private set; }

	public string PlatformName { get; private set; }

	public MapStateSaveOwner()
	{
	}

	public MapStateSaveOwner(MapStateSaveOwner state, ReferenceDictionary references)
	{
		PlatformPlayerID = state.PlatformPlayerID;
		PlatformAccountID = state.PlatformAccountID;
		PlatformNetworkAccountID = state.PlatformNetworkAccountID;
		Username = state.Username;
		PlatformName = state.PlatformName;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("PlatformPlayerID", PlatformPlayerID);
		info.AddValue("PlatformAccountID", PlatformAccountID);
		info.AddValue("PlatformNetworkAccountID", PlatformNetworkAccountID);
		info.AddValue("Username", Username);
		info.AddValue("PlatformName", PlatformName);
	}

	public MapStateSaveOwner(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "PlatformPlayerID":
					PlatformPlayerID = info.GetString("PlatformPlayerID");
					break;
				case "PlatformAccountID":
					PlatformAccountID = info.GetString("PlatformAccountID");
					break;
				case "PlatformNetworkAccountID":
					PlatformNetworkAccountID = info.GetString("PlatformNetworkAccountID");
					break;
				case "Username":
					Username = info.GetString("Username");
					break;
				case "PlatformName":
					PlatformName = info.GetString("PlatformName");
					break;
				case "SteamID":
					PlatformPlayerID = ((ulong)info.GetValue("SteamID", typeof(ulong))).ToString();
					break;
				case "AccountID":
					PlatformAccountID = info.GetUInt32("AccountID").ToString();
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize MapStateSaveOwner entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public MapStateSaveOwner(string platformUserID, string platformAccountID, string platformNetworkAccountID, string username, string platformName)
	{
		PlatformPlayerID = platformUserID;
		PlatformAccountID = platformAccountID;
		PlatformNetworkAccountID = platformNetworkAccountID;
		Username = username;
		PlatformName = platformName;
	}
}
