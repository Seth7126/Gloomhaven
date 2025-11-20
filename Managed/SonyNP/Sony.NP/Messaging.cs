using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public class Messaging
{
	[StructLayout(LayoutKind.Sequential)]
	public class SendInGameMessageRequest : RequestBase
	{
		public const int NP_IN_GAME_MESSAGE_DATA_SIZE_MAX = 512;

		internal ulong messageSize;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
		internal byte[] message = new byte[512];

		internal Core.NpAccountId recipientId;

		internal Core.PlatformType recipientPlatformType;

		public byte[] Message
		{
			get
			{
				if (messageSize == 0)
				{
					return null;
				}
				byte[] array = new byte[messageSize];
				Array.Copy(message, array, (int)messageSize);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 512)
					{
						throw new NpToolkitException("The size of the array is larger than " + 512);
					}
					value.CopyTo(message, 0);
					messageSize = (ulong)value.Length;
				}
				else
				{
					messageSize = 0uL;
				}
			}
		}

		public Core.NpAccountId RecipientId
		{
			get
			{
				return recipientId;
			}
			set
			{
				recipientId = value;
			}
		}

		public Core.PlatformType RecipientPlatformType
		{
			get
			{
				return recipientPlatformType;
			}
			set
			{
				recipientPlatformType = value;
			}
		}

		public SendInGameMessageRequest()
			: base(ServiceTypes.Messaging, FunctionTypes.MessagingSendInGameMessage)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DisplayReceivedGameDataMessagesDialogRequest : RequestBase
	{
		public DisplayReceivedGameDataMessagesDialogRequest()
			: base(ServiceTypes.Messaging, FunctionTypes.MessagingDisplayReceivedGameDataMessagesDialog)
		{
		}
	}

	public enum GameCustomDataTypes
	{
		Invalid,
		Url,
		Attachment
	}

	public struct GameDataMessageImage
	{
		public const int IMAGE_PATH_MAX_LEN = 255;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		internal string imgPath;

		public string ImgPath
		{
			get
			{
				return imgPath;
			}
			set
			{
				if (value.Length > 255)
				{
					throw new NpToolkitException("The size of the image path string is more than " + 255 + " characters.");
				}
				imgPath = value;
			}
		}

		internal bool IsValid()
		{
			if (imgPath == null || imgPath.Length == 0)
			{
				return false;
			}
			return true;
		}
	}

	public struct LocalizedMetadata
	{
		public const int MAX_SIZE_DATA_NAME = 127;

		public const int MAX_SIZE_DATA_DESCRIPTION = 511;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
		internal string languageCode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		internal string name;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
		internal string description;

		public Core.LanguageCode LanguageCode
		{
			get
			{
				Core.LanguageCode languageCode = new Core.LanguageCode();
				languageCode.code = this.languageCode;
				return languageCode;
			}
			set
			{
				languageCode = value.code;
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (value.Length > 127)
				{
					throw new NpToolkitException("The size of the string is more than " + 127 + " characters.");
				}
				name = value;
			}
		}

		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				if (value.Length > 511)
				{
					throw new NpToolkitException("The size of the string is more than " + 511 + " characters.");
				}
				description = value;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SendGameDataMessageRequest : RequestBase
	{
		public const int MAX_SIZE_TEXT_MESSAGE = 511;

		public const int MAX_SIZE_DATA_NAME = 127;

		public const int MAX_SIZE_DATA_DESCRIPTION = 511;

		public const int MAX_NUM_RECIPIENTS = 16;

		public const int MAX_SIZE_ATTACHMENT = 1048576;

		public const int MAX_URL_SIZE = 1023;

		public const int MAX_LOCALIZED_METADATA = 50;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
		internal string textMessage;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		internal string dataName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
		internal string dataDescription;

		internal uint numRecipients;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		internal Core.NpAccountId[] recipients = new Core.NpAccountId[16];

		internal GameCustomDataTypes dataType;

		internal uint expireMinutes;

		[MarshalAs(UnmanagedType.LPArray)]
		internal byte[] attachment;

		internal ulong attachmentSize;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		internal string url;

		internal ulong numDataLocalized;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
		internal LocalizedMetadata[] localizedMetaData = new LocalizedMetadata[50];

		internal GameDataMessageImage thumbnail;

		internal uint maxNumberRecipientsToAdd;

		[MarshalAs(UnmanagedType.I1)]
		private bool enableDialog;

		[MarshalAs(UnmanagedType.I1)]
		private bool senderCanEditRecipients;

		[MarshalAs(UnmanagedType.I1)]
		private bool isPS4Available;

		[MarshalAs(UnmanagedType.I1)]
		private bool isPSVitaAvailable;

		[MarshalAs(UnmanagedType.I1)]
		private bool addGameDataMsgIdToUrl;

		public string TextMessage
		{
			get
			{
				return textMessage;
			}
			set
			{
				if (value.Length > 511)
				{
					throw new NpToolkitException("The size of the string is more than " + 511 + " characters.");
				}
				textMessage = value;
			}
		}

		public string DataName
		{
			get
			{
				return dataName;
			}
			set
			{
				if (value.Length > 127)
				{
					throw new NpToolkitException("The size of the string is more than " + 127 + " characters.");
				}
				dataName = value;
			}
		}

		public string DataDescription
		{
			get
			{
				return dataDescription;
			}
			set
			{
				if (value.Length > 511)
				{
					throw new NpToolkitException("The size of the string is more than " + 511 + " characters.");
				}
				dataDescription = value;
			}
		}

		public Core.NpAccountId[] Recipients
		{
			get
			{
				if (numRecipients == 0)
				{
					return null;
				}
				Core.NpAccountId[] array = new Core.NpAccountId[numRecipients];
				Array.Copy(recipients, array, (int)numRecipients);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 16)
					{
						throw new NpToolkitException("The size of the array is larger than " + 16);
					}
					value.CopyTo(recipients, 0);
					numRecipients = (uint)value.Length;
				}
				else
				{
					numRecipients = 0u;
				}
			}
		}

		public GameCustomDataTypes DataType => dataType;

		public byte[] Attachment
		{
			get
			{
				return attachment;
			}
			set
			{
				if (value.Length > 1048576)
				{
					throw new NpToolkitException("The size of the array is larger than " + 1048576);
				}
				attachment = value;
				attachmentSize = (ulong)((value != null) ? value.Length : 0);
				dataType = GameCustomDataTypes.Attachment;
			}
		}

		public uint ExpireMinutes
		{
			get
			{
				return expireMinutes;
			}
			set
			{
				expireMinutes = value;
			}
		}

		public string Url
		{
			get
			{
				return url;
			}
			set
			{
				if (value.Length > 1023)
				{
					throw new NpToolkitException("The size of the string is more than " + 1023 + " characters.");
				}
				url = value;
				dataType = GameCustomDataTypes.Url;
			}
		}

		public LocalizedMetadata[] LocalizedMetaData
		{
			get
			{
				if (numDataLocalized == 0)
				{
					return null;
				}
				LocalizedMetadata[] array = new LocalizedMetadata[numDataLocalized];
				Array.Copy(localizedMetaData, array, (int)numDataLocalized);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 50)
					{
						throw new NpToolkitException("The size of the localized game metadata array is more than " + 50);
					}
					localizedMetaData = value;
					numDataLocalized = (ulong)value.Length;
				}
				else
				{
					numDataLocalized = 0uL;
				}
			}
		}

		public GameDataMessageImage Thumbnail
		{
			get
			{
				return thumbnail;
			}
			set
			{
				thumbnail = value;
			}
		}

		public uint MaxNumberRecipientsToAdd
		{
			get
			{
				return maxNumberRecipientsToAdd;
			}
			set
			{
				maxNumberRecipientsToAdd = value;
			}
		}

		public bool EnableDialog
		{
			get
			{
				return enableDialog;
			}
			set
			{
				enableDialog = value;
			}
		}

		public bool SenderCanEditRecipients
		{
			get
			{
				return senderCanEditRecipients;
			}
			set
			{
				senderCanEditRecipients = value;
			}
		}

		public bool IsPS4Available
		{
			get
			{
				return isPS4Available;
			}
			set
			{
				isPS4Available = value;
			}
		}

		public bool IsPSVitaAvailable
		{
			get
			{
				return isPSVitaAvailable;
			}
			set
			{
				isPSVitaAvailable = value;
			}
		}

		public bool AddGameDataMsgIdToUrl
		{
			get
			{
				return addGameDataMsgIdToUrl;
			}
			set
			{
				addGameDataMsgIdToUrl = value;
			}
		}

		public SendGameDataMessageRequest()
			: base(ServiceTypes.Messaging, FunctionTypes.MessagingSendGameDataMessage)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class ConsumeGameDataMessageRequest : RequestBase
	{
		private ulong gameDataMsgId;

		public ulong GameDataMsgId
		{
			get
			{
				return gameDataMsgId;
			}
			set
			{
				gameDataMsgId = value;
			}
		}

		public ConsumeGameDataMessageRequest()
			: base(ServiceTypes.Messaging, FunctionTypes.MessagingConsumeGameDataMessage)
		{
		}
	}

	public enum GameDataMessagesToRetrieve
	{
		FromGameDataMsgIds,
		All
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetReceivedGameDataMessagesRequest : RequestBase
	{
		public const int MAX_NUM_GAME_DATA_MSG_IDS = 20;

		public const int MAX_PAGE_SIZE = 100;

		internal ulong numGameDataMsgIds;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
		internal ulong[] gameDataMsgIds = new ulong[20];

		internal uint pageSize;

		internal uint offset;

		internal GameDataMessagesToRetrieve retrieveType;

		public ulong[] GameDataMsgIds
		{
			get
			{
				if (numGameDataMsgIds == 0)
				{
					return null;
				}
				ulong[] array = new ulong[numGameDataMsgIds];
				Array.Copy(gameDataMsgIds, array, (int)numGameDataMsgIds);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 20)
					{
						throw new NpToolkitException("The size of the array is more than " + 20);
					}
					gameDataMsgIds = value;
					numGameDataMsgIds = (ulong)value.Length;
				}
				else
				{
					numGameDataMsgIds = 0uL;
				}
				if (numGameDataMsgIds != 0)
				{
					retrieveType = GameDataMessagesToRetrieve.FromGameDataMsgIds;
				}
				else
				{
					retrieveType = GameDataMessagesToRetrieve.All;
				}
			}
		}

		public uint PageSize
		{
			get
			{
				return pageSize;
			}
			set
			{
				pageSize = value;
				retrieveType = GameDataMessagesToRetrieve.All;
			}
		}

		public uint Offset
		{
			get
			{
				return offset;
			}
			set
			{
				offset = value;
				retrieveType = GameDataMessagesToRetrieve.All;
			}
		}

		public GameDataMessagesToRetrieve RetrieveType => retrieveType;

		public GetReceivedGameDataMessagesRequest()
			: base(ServiceTypes.Messaging, FunctionTypes.MessagingGetReceivedGameDataMessages)
		{
			pageSize = 100u;
			offset = 0u;
			retrieveType = GameDataMessagesToRetrieve.All;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetGameDataMessageThumbnailRequest : RequestBase
	{
		private ulong gameDataMsgId;

		public ulong GameDataMsgId
		{
			get
			{
				return gameDataMsgId;
			}
			set
			{
				gameDataMsgId = value;
			}
		}

		public GetGameDataMessageThumbnailRequest()
			: base(ServiceTypes.Messaging, FunctionTypes.MessagingGetGameDataMessageThumbnail)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetGameDataMessageAttachmentRequest : RequestBase
	{
		private ulong gameDataMsgId;

		public ulong GameDataMsgId
		{
			get
			{
				return gameDataMsgId;
			}
			set
			{
				gameDataMsgId = value;
			}
		}

		public GetGameDataMessageAttachmentRequest()
			: base(ServiceTypes.Messaging, FunctionTypes.MessagingGetGameDataMessageAttachment)
		{
		}
	}

	public class GameDataMessageDetails
	{
		internal string dataName;

		internal string dataDescription;

		internal string textMessage;

		public string DataName => dataName;

		public string DataDescription => dataDescription;

		public string TextMessage => textMessage;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GameDataMessageDetailsBegin);
			buffer.ReadString(ref dataName);
			buffer.ReadString(ref dataDescription);
			buffer.ReadString(ref textMessage);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GameDataMessageDetailsEnd);
		}
	}

	public class GameDataMessage
	{
		internal ulong gameDataMsgId;

		internal Core.OnlineUser fromUser = new Core.OnlineUser();

		internal string receivedDate;

		internal string expiredDate;

		internal bool isPS4Available;

		internal bool isPSVitaAvailable;

		internal GameCustomDataTypes dataType;

		internal string url;

		internal GameDataMessageDetails details;

		internal bool hasDetails;

		internal bool isUsed;

		public ulong GameDataMsgId => gameDataMsgId;

		public Core.OnlineUser FromUser => fromUser;

		public string ReceivedDate => receivedDate;

		public string ExpiredDate => expiredDate;

		public bool IsPS4Available => isPS4Available;

		public bool IsPSVitaAvailable => isPSVitaAvailable;

		public GameCustomDataTypes DataType => dataType;

		public string Url => url;

		public GameDataMessageDetails Details => details;

		public bool HasDetails => hasDetails;

		public bool IsUsed => isUsed;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GameDataMessageBegin);
			gameDataMsgId = buffer.ReadUInt64();
			fromUser.Read(buffer);
			buffer.ReadString(ref receivedDate);
			buffer.ReadString(ref expiredDate);
			isPS4Available = buffer.ReadBool();
			isPSVitaAvailable = buffer.ReadBool();
			dataType = (GameCustomDataTypes)buffer.ReadUInt32();
			buffer.ReadString(ref url);
			hasDetails = buffer.ReadBool();
			if (hasDetails)
			{
				details = new GameDataMessageDetails();
				details.Read(buffer);
			}
			isUsed = buffer.ReadBool();
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GameDataMessageEnd);
		}
	}

	public class GameDataMessagesResponse : ResponseBase
	{
		internal GameDataMessage[] gameDataMessages;

		public GameDataMessage[] GameDataMessages => gameDataMessages;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GameDataMessagesBegin);
			ulong num = memoryBuffer.ReadUInt64();
			gameDataMessages = new GameDataMessage[num];
			for (ulong num2 = 0uL; num2 < num; num2++)
			{
				gameDataMessages[num2] = new GameDataMessage();
				gameDataMessages[num2].Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GameDataMessagesEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class GameDataMessageThumbnailResponse : ResponseBase
	{
		internal ulong gameDataMsgId;

		internal byte[] thumbnail = null;

		public ulong GameDataMsgId => gameDataMsgId;

		public byte[] Thumbnail => thumbnail;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GameDataMessageThumbnailBegin);
			gameDataMsgId = memoryBuffer.ReadUInt64();
			memoryBuffer.ReadData(ref thumbnail);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GameDataMessageThumbnailEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class GameDataMessageAttachmentResponse : ResponseBase
	{
		internal ulong gameDataMsgId;

		internal byte[] attachment;

		public ulong GameDataMsgId => gameDataMsgId;

		public byte[] Attachment => attachment;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GameDataMessageAttachmentBegin);
			gameDataMsgId = memoryBuffer.ReadUInt64();
			memoryBuffer.ReadData(ref attachment);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GameDataMessageAttachmentEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class NewInGameMessageResponse : ResponseBase
	{
		internal byte[] message;

		internal Core.OnlineUser sender = new Core.OnlineUser();

		internal Core.OnlineUser recipient = new Core.OnlineUser();

		internal Core.PlatformType senderPlatformType;

		internal Core.PlatformType recipientPlatformType;

		public byte[] Message => message;

		public Core.OnlineUser Sender => sender;

		public Core.OnlineUser Recipient => recipient;

		public Core.PlatformType SenderPlatformType => senderPlatformType;

		public Core.PlatformType RecipientPlatformType => recipientPlatformType;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NewInGameMessageBegin);
			memoryBuffer.ReadData(ref message);
			sender.Read(memoryBuffer);
			recipient.Read(memoryBuffer);
			senderPlatformType = (Core.PlatformType)memoryBuffer.ReadUInt32();
			recipientPlatformType = (Core.PlatformType)memoryBuffer.ReadUInt32();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NewInGameMessageEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class NewGameDataMessageResponse : ResponseBase
	{
		internal Core.OnlineUser to = new Core.OnlineUser();

		internal Core.OnlineUser from = new Core.OnlineUser();

		public Core.OnlineUser To => to;

		public Core.OnlineUser From => from;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NewGameDataMessageBegin);
			to.Read(memoryBuffer);
			from.Read(memoryBuffer);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NewGameDataMessageEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class GameCustomDataEventResponse : ResponseBase
	{
		internal ulong itemId;

		internal Core.OnlineID onlineId = new Core.OnlineID();

		internal Core.UserServiceUserId userId;

		public ulong ItemId => itemId;

		public Core.OnlineID OnlineId => onlineId;

		public Core.UserServiceUserId UserId => userId;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GameCustomDataEventBegin);
			itemId = memoryBuffer.ReadUInt64();
			onlineId.Read(memoryBuffer);
			userId.Read(memoryBuffer);
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GameCustomDataEventEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSendInGameMessage(SendInGameMessageRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxDisplayReceivedGameDataMessagesDialog(DisplayReceivedGameDataMessagesDialogRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSendGameDataMessage(SendGameDataMessageRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxConsumeGameDataMessage(ConsumeGameDataMessageRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetReceivedGameDataMessages(GetReceivedGameDataMessagesRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetGameDataMessageThumbnail(GetGameDataMessageThumbnailRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetGameDataMessageAttachment(GetGameDataMessageAttachmentRequest request, out APIResult result);

	public static int SendInGameMessage(SendInGameMessageRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSendInGameMessage(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int DisplayReceivedGameDataMessagesDialog(DisplayReceivedGameDataMessagesDialogRequest request, Core.EmptyResponse response)
	{
		if (Main.initResult.sceSDKVersion > 122683392)
		{
			throw new NpToolkitException("This request is not available in SDK " + Main.initResult.SceSDKVersion.ToString() + ". Only available in SDK 7.0 or less.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxDisplayReceivedGameDataMessagesDialog(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int SendGameDataMessage(SendGameDataMessageRequest request, Core.EmptyResponse response)
	{
		if (Main.initResult.sceSDKVersion > 122683392)
		{
			throw new NpToolkitException("This request is not available in SDK " + Main.initResult.SceSDKVersion.ToString() + ". Only available in SDK 7.0 or less.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		if (!request.thumbnail.IsValid())
		{
			throw new NpToolkitException("Request thumbnail image hasn't be defined. A message can't be created without an image.");
		}
		APIResult result;
		int num = PrxSendGameDataMessage(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int ConsumeGameDataMessage(ConsumeGameDataMessageRequest request, Core.EmptyResponse response)
	{
		if (Main.initResult.sceSDKVersion > 122683392)
		{
			throw new NpToolkitException("This request is not available in SDK " + Main.initResult.SceSDKVersion.ToString() + ". Only available in SDK 7.0 or less.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxConsumeGameDataMessage(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetReceivedGameDataMessages(GetReceivedGameDataMessagesRequest request, GameDataMessagesResponse response)
	{
		if (Main.initResult.sceSDKVersion > 122683392)
		{
			throw new NpToolkitException("This request is not available in SDK " + Main.initResult.SceSDKVersion.ToString() + ". Only available in SDK 7.0 or less.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetReceivedGameDataMessages(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetGameDataMessageThumbnail(GetGameDataMessageThumbnailRequest request, GameDataMessageThumbnailResponse response)
	{
		if (Main.initResult.sceSDKVersion > 122683392)
		{
			throw new NpToolkitException("This request is not available in SDK " + Main.initResult.SceSDKVersion.ToString() + ". Only available in SDK 7.0 or less.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetGameDataMessageThumbnail(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetGameDataMessageAttachment(GetGameDataMessageAttachmentRequest request, GameDataMessageAttachmentResponse response)
	{
		if (Main.initResult.sceSDKVersion > 122683392)
		{
			throw new NpToolkitException("This request is not available in SDK " + Main.initResult.SceSDKVersion.ToString() + ". Only available in SDK 7.0 or less.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetGameDataMessageAttachment(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
