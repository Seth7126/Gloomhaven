using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.State;
using SM.Utils;
using UnityEngine;

[Serializable]
public class SaveOwner : ISerializable
{
	[Serializable]
	public class SerializedAvatar : ISerializable
	{
		private Sprite m_Sprite;

		public int X { get; private set; }

		public int Y { get; private set; }

		public byte[] Bytes { get; private set; }

		public Sprite Avatar
		{
			get
			{
				if (m_Sprite == null && Bytes != null)
				{
					Texture2D texture2D = new Texture2D(X, Y);
					texture2D.LoadImage(Bytes);
					m_Sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
				}
				return m_Sprite;
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("X", X);
			info.AddValue("Y", Y);
			info.AddValue("Bytes", Bytes);
		}

		public SerializedAvatar(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SerializationEntry current = enumerator.Current;
				try
				{
					switch (current.Name)
					{
					case "X":
						X = info.GetInt32("X");
						break;
					case "Y":
						Y = info.GetInt32("Y");
						break;
					case "Bytes":
						Bytes = (byte[])info.GetValue("Bytes", typeof(byte[]));
						break;
					}
				}
				catch (Exception ex)
				{
					LogUtils.LogError("Exception while trying to deserialize SerializedAvatar entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public SerializedAvatar(Texture2D tex)
		{
			X = tex.width;
			Y = tex.height;
			Bytes = tex.EncodeToPNG();
		}
	}

	private Texture2D m_StadiaImage;

	public string PlatformPlayerID { get; private set; }

	public string PlatformAccountID { get; private set; }

	public string PlatformNetworkAccountID { get; private set; }

	public string PlatformName { get; private set; }

	public string Username { get; set; }

	public SerializedAvatar Avatar { get; private set; }

	public string ActualAvatarURL { get; set; }

	public bool AvatarSet { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("PlatformPlayerID", PlatformPlayerID);
		info.AddValue("PlatformAccountID", PlatformAccountID);
		info.AddValue("PlatformNetworkAccountID", PlatformNetworkAccountID);
		info.AddValue("PlatformName", PlatformName);
		info.AddValue("Username", Username);
		info.AddValue("Avatar", Avatar);
		info.AddValue("AvatarSet", AvatarSet);
	}

	public SaveOwner(SerializationInfo info, StreamingContext context)
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
				case "PlatformName":
					PlatformName = info.GetString("PlatformName");
					break;
				case "Username":
					Username = info.GetString("Username");
					break;
				case "Avatar":
					Avatar = (SerializedAvatar)info.GetValue("Avatar", typeof(SerializedAvatar));
					break;
				case "AvatarSet":
					AvatarSet = info.GetBoolean("AvatarSet");
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
				LogUtils.LogError("Exception while trying to deserialize SaveOwner entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SaveOwner()
	{
		PlatformPlayerID = PlatformLayer.UserData.PlatformPlayerID;
		PlatformAccountID = PlatformLayer.UserData.PlatformAccountID;
		PlatformNetworkAccountID = PlatformLayer.UserData.PlatformNetworkAccountPlayerID;
		PlatformName = PlatformLayer.Instance.PlatformID;
		Username = PlatformLayer.UserData.UserName;
		Avatar = null;
		PlatformLayer.UserData.GetAvatarForSaveOwner(this);
	}

	public SaveOwner(string platformUserID, string platformAccountID, string platformNetworkAccountID, string username, string platformName)
	{
		PlatformPlayerID = platformUserID;
		PlatformAccountID = platformAccountID;
		PlatformNetworkAccountID = platformNetworkAccountID;
		PlatformName = platformName;
		Username = username;
		Avatar = null;
		PlatformLayer.UserData.GetAvatarForSaveOwner(this);
	}

	public SaveOwner(MapStateSaveOwner saveOwner)
	{
		PlatformPlayerID = saveOwner.PlatformPlayerID;
		PlatformAccountID = saveOwner.PlatformAccountID;
		PlatformNetworkAccountID = saveOwner.PlatformNetworkAccountID;
		Username = saveOwner.Username;
		PlatformName = saveOwner.PlatformName;
		Avatar = null;
		PlatformLayer.UserData.GetAvatarForSaveOwner(this);
	}

	public void MaskBadWordsInUsername(Action onCallback)
	{
		Username.GetCensoredStringAsync(delegate(string censoredUsername)
		{
			Username = censoredUsername;
			onCallback?.Invoke();
		});
	}

	public MapStateSaveOwner ConvertToMapStateSaveOwner()
	{
		return new MapStateSaveOwner(PlatformPlayerID, PlatformAccountID, PlatformNetworkAccountID, Username, PlatformName);
	}

	public void SetAvatar(Texture2D avatarTexture)
	{
		Avatar = new SerializedAvatar(avatarTexture);
		AvatarSet = true;
	}
}
