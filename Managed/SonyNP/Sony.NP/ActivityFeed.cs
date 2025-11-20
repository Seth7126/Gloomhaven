using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public class ActivityFeed
{
	public enum FeedType
	{
		Invalid,
		UserFeed,
		UserNews,
		TitleFeed,
		TitleNews
	}

	public enum StoryType
	{
		InGamePost,
		PlayedWith,
		VideoUpload,
		Broadcasting
	}

	public enum ActionType
	{
		Invalid,
		Url,
		Store,
		StartGame
	}

	public struct StoryId
	{
		public const int STORY_ID_LEN = 64;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
		internal string id;

		public string Id
		{
			get
			{
				return id;
			}
			set
			{
				if (value.Length > 64)
				{
					throw new NpToolkitException("The size of the string is more than " + 64 + " characters.");
				}
				id = value;
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref id);
		}
	}

	public struct Caption
	{
		public const int CAPTION_MAX_LEN = 511;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
		internal string languageCode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
		internal string caption;

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

		public string CaptionText
		{
			get
			{
				return caption;
			}
			set
			{
				if (value.Length > 511)
				{
					throw new NpToolkitException("The size of the string is more than " + 511 + " characters.");
				}
				caption = value;
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref languageCode);
			buffer.ReadString(ref caption);
		}
	}

	public struct Media
	{
		public const int URL_MAX_LEN = 255;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		internal string largeImageUrl;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		internal string videoUrl;

		public string LargeImageUrl
		{
			get
			{
				return largeImageUrl;
			}
			set
			{
				if (value.Length > 255)
				{
					throw new NpToolkitException("The size of the string is more than " + 255 + " characters.");
				}
				largeImageUrl = value;
			}
		}

		public string VideoUrl
		{
			get
			{
				return videoUrl;
			}
			set
			{
				if (value.Length > 255)
				{
					throw new NpToolkitException("The size of the string is more than " + 255 + " characters.");
				}
				videoUrl = value;
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref largeImageUrl);
			buffer.ReadString(ref videoUrl);
		}
	}

	public struct ButtonCaption
	{
		public const int TEXT_MAX_LEN = 20;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
		internal string languageCode;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
		internal string text;

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

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				if (value.Length > 20)
				{
					throw new NpToolkitException("The size of the string is more than " + 20 + " characters.");
				}
				text = value;
			}
		}
	}

	public struct Action
	{
		public const int MAX_BUTTON_CAPTIONS = 32;

		public const int URL_MAX_LEN = 255;

		public const int STORE_LABEL_MAX_LEN = 31;

		public const int START_GAME_ARGS_MAX = 2083;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		internal string imageUrl;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		internal string uri;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		internal string storeLabel;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2084)]
		internal string startGameArguments;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		internal ButtonCaption[] buttonCaptions;

		internal uint storeServiceLabel;

		internal ActionType type;

		public string ImageUrl
		{
			get
			{
				return imageUrl;
			}
			set
			{
				if (value.Length > 255)
				{
					throw new NpToolkitException("The size of the string is more than " + 255 + " characters.");
				}
				imageUrl = value;
			}
		}

		public string Uri
		{
			get
			{
				return uri;
			}
			set
			{
				if (value.Length > 255)
				{
					throw new NpToolkitException("The size of the string is more than " + 255 + " characters.");
				}
				uri = value;
			}
		}

		public string StoreLabel
		{
			get
			{
				return storeLabel;
			}
			set
			{
				if (value.Length > 31)
				{
					throw new NpToolkitException("The size of the string is more than " + 31 + " characters.");
				}
				storeLabel = value;
			}
		}

		public string StartGameArguments
		{
			get
			{
				return startGameArguments;
			}
			set
			{
				if (value.Length > 2083)
				{
					throw new NpToolkitException("The size of the string is more than " + 2083 + " characters.");
				}
				startGameArguments = value;
			}
		}

		public ButtonCaption[] ButtonCaptions
		{
			get
			{
				ButtonCaption[] array = new ButtonCaption[32];
				Array.Copy(buttonCaptions, array, 32);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 32)
					{
						throw new NpToolkitException("The size of the button captions array is more than " + 32);
					}
					buttonCaptions = new ButtonCaption[32];
					value.CopyTo(buttonCaptions, 0);
				}
			}
		}

		public uint StoreServiceLabel
		{
			get
			{
				return storeServiceLabel;
			}
			set
			{
				storeServiceLabel = value;
			}
		}

		public ActionType ActionType
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
			}
		}
	}

	public class StoryUser
	{
		public const int URL_MAX_LEN = 255;

		internal Core.OnlineUser user = new Core.OnlineUser();

		internal string avatarUrl;

		internal Profiles.RealName realName = new Profiles.RealName();

		public Core.OnlineUser User => user;

		public string AvatarUrl => avatarUrl;

		public Profiles.RealName RealName => realName;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.StoryUserBegin);
			user.Read(buffer);
			buffer.ReadString(ref avatarUrl);
			realName.Read(buffer);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.StoryUserEnd);
		}
	}

	public class Story
	{
		internal string creationDate;

		internal string userComment;

		internal Media media;

		internal StoryId storyId;

		internal Caption caption;

		internal StoryType storyType;

		internal int subType;

		internal StoryUser postCreator = new StoryUser();

		internal uint numLikes;

		internal uint numComments;

		internal bool isReshareable;

		internal bool isLiked;

		public string CreationDate => creationDate;

		public string UserComment => userComment;

		public Media Media => media;

		public StoryId StoryId => storyId;

		public Caption Caption => caption;

		public StoryType StoryType => storyType;

		public int SubType => subType;

		public StoryUser PostCreator => postCreator;

		public uint NumLikes => numLikes;

		public uint NumComments => numComments;

		public bool IsReshareable => isReshareable;

		public bool IsLiked => isLiked;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.StoryBegin);
			buffer.ReadString(ref creationDate);
			buffer.ReadString(ref userComment);
			media.Read(buffer);
			storyId.Read(buffer);
			caption.Read(buffer);
			storyType = (StoryType)buffer.ReadUInt32();
			subType = buffer.ReadInt32();
			postCreator.Read(buffer);
			numLikes = buffer.ReadUInt32();
			numComments = buffer.ReadUInt32();
			isReshareable = buffer.ReadBool();
			isLiked = buffer.ReadBool();
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.StoryEnd);
		}
	}

	public class PlayedWithStory
	{
		internal StoryId storyId;

		internal StoryUser[] users;

		internal string titleName;

		internal string date;

		internal Core.TitleId titleId = new Core.TitleId();

		internal string playedWithDescription;

		public StoryId StoryId => storyId;

		public StoryUser[] Users => users;

		public string TitleName => titleName;

		public string Date => date;

		public Core.TitleId TitleId => titleId;

		public string PlayedWithDescription => playedWithDescription;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.StoryBegin);
			storyId.Read(buffer);
			uint num = buffer.ReadUInt32();
			users = new StoryUser[num];
			for (int i = 0; i < num; i++)
			{
				users[i] = new StoryUser();
				users[i].Read(buffer);
			}
			buffer.ReadString(ref titleName);
			buffer.ReadString(ref date);
			titleId.Read(buffer);
			buffer.ReadString(ref playedWithDescription);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.StoryEnd);
		}
	}

	public class SharedVideo
	{
		internal StoryId storyId;

		internal Caption caption;

		internal StoryUser sourceCreator = new StoryUser();

		internal string snType;

		internal string videoId;

		internal string creationDate;

		internal string videoDuration;

		internal string comment;

		public StoryId StoryId => storyId;

		public Caption Caption => caption;

		public StoryUser SourceCreator => sourceCreator;

		public string SNType => snType;

		public string VideoId => videoId;

		public string CreationDate => creationDate;

		public string VideoDuration => videoDuration;

		public string Comment => comment;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SharedVideoBegin);
			storyId.Read(buffer);
			caption.Read(buffer);
			sourceCreator.Read(buffer);
			buffer.ReadString(ref snType);
			buffer.ReadString(ref videoId);
			buffer.ReadString(ref creationDate);
			buffer.ReadString(ref videoDuration);
			buffer.ReadString(ref comment);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SharedVideoEnd);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetFeedRequest : RequestBase
	{
		public const int MAX_PAGE_SIZE = 100;

		public const int DEFAULT_PAGE_SIZE = 20;

		internal Core.NpAccountId user;

		internal FeedType feedType;

		internal uint offset;

		internal uint pageSize;

		public Core.NpAccountId User
		{
			get
			{
				return user;
			}
			set
			{
				user = value;
			}
		}

		public FeedType FeedType
		{
			get
			{
				return feedType;
			}
			set
			{
				feedType = value;
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
			}
		}

		public GetFeedRequest()
			: base(ServiceTypes.ActivityFeed, FunctionTypes.ActivityFeedGetFeed)
		{
			pageSize = 20u;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class PostInGameStoryRequest : RequestBase
	{
		public const int USER_COMMENT_MAX_LEN = 1000;

		public const int MAX_ACTIONS = 3;

		public const int MAX_CAPTIONS = 1024;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		internal Action[] actions;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
		internal Caption[] captions;

		private uint numCaptions;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
		internal Caption[] condensedCaptions;

		private uint numCondensedCaptions;

		internal Media media;

		internal int subType;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1001)]
		internal string userComment;

		public Action[] Actions
		{
			get
			{
				Action[] array = new Action[3];
				Array.Copy(actions, array, 3);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 3)
					{
						throw new NpToolkitException("The size of the actions array is more than " + 3);
					}
					actions = new Action[3];
					value.CopyTo(actions, 0);
				}
			}
		}

		public Caption[] Captions
		{
			get
			{
				Caption[] array = new Caption[numCaptions];
				Array.Copy(captions, array, numCaptions);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 1024)
					{
						throw new NpToolkitException("The size of the captions array is more than " + 1024);
					}
					numCaptions = (uint)value.Length;
					captions = new Caption[1024];
					value.CopyTo(captions, 0);
				}
			}
		}

		public Caption[] CondensedCaptions
		{
			get
			{
				Caption[] array = new Caption[numCondensedCaptions];
				Array.Copy(condensedCaptions, array, numCondensedCaptions);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 1024)
					{
						throw new NpToolkitException("The size of the condensed captions array is more than " + 1024);
					}
					numCondensedCaptions = (uint)value.Length;
					condensedCaptions = new Caption[1024];
					value.CopyTo(condensedCaptions, 0);
				}
			}
		}

		public Media Media
		{
			get
			{
				return media;
			}
			set
			{
				media = value;
			}
		}

		public int SubType
		{
			get
			{
				return subType;
			}
			set
			{
				subType = value;
			}
		}

		public string UserComment
		{
			get
			{
				return userComment;
			}
			set
			{
				if (value.Length > 1000)
				{
					throw new NpToolkitException("The size of the string is more than " + 1000 + " characters.");
				}
				userComment = value;
			}
		}

		public PostInGameStoryRequest()
			: base(ServiceTypes.ActivityFeed, FunctionTypes.ActivityFeedPostInGameStory)
		{
			actions = new Action[3];
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SetLikedRequest : RequestBase
	{
		internal StoryId storyId;

		[MarshalAs(UnmanagedType.I1)]
		internal bool isLiked;

		public StoryId StoryId
		{
			get
			{
				return storyId;
			}
			set
			{
				storyId = value;
			}
		}

		public bool IsLiked
		{
			get
			{
				return isLiked;
			}
			set
			{
				isLiked = value;
			}
		}

		public SetLikedRequest()
			: base(ServiceTypes.ActivityFeed, FunctionTypes.ActivityFeedSetLiked)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetWhoLikedRequest : RequestBase
	{
		public const int MAX_PAGE_SIZE = 100;

		public const int DEFAULT_PAGE_SIZE = 20;

		internal Core.NpAccountId lastUserRetrieved;

		internal StoryId storyId;

		internal uint pageSize;

		public Core.NpAccountId LastUserRetrieved
		{
			get
			{
				return lastUserRetrieved;
			}
			set
			{
				lastUserRetrieved = value;
			}
		}

		public StoryId StoryId
		{
			get
			{
				return storyId;
			}
			set
			{
				storyId = value;
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
			}
		}

		public GetWhoLikedRequest()
			: base(ServiceTypes.ActivityFeed, FunctionTypes.ActivityFeedGetWhoLiked)
		{
			pageSize = 20u;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class PostPlayedWithRequest : RequestBase
	{
		public const int DESCRIPTION_MAX_LEN = 2083;

		public const int MAX_USERS = 19;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
		internal Core.NpAccountId[] userIds;

		internal uint numUsers;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2084)]
		internal string playedWithDescription;

		public Core.NpAccountId[] UserIds
		{
			get
			{
				Core.NpAccountId[] array = new Core.NpAccountId[numUsers];
				Array.Copy(userIds, array, numUsers);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 19)
					{
						throw new NpToolkitException("The size of the users array is more than " + 19);
					}
					numUsers = (uint)value.Length;
					userIds = new Core.NpAccountId[19];
					value.CopyTo(userIds, 0);
				}
				else
				{
					numUsers = 0u;
				}
			}
		}

		public string PlayedWithDescription
		{
			get
			{
				return playedWithDescription;
			}
			set
			{
				if (value.Length > 2083)
				{
					throw new NpToolkitException("The size of the string is more than " + 2083 + " characters.");
				}
				playedWithDescription = value;
			}
		}

		public PostPlayedWithRequest()
			: base(ServiceTypes.ActivityFeed, FunctionTypes.ActivityFeedPostPlayedWith)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetPlayedWithRequest : RequestBase
	{
		public GetPlayedWithRequest()
			: base(ServiceTypes.ActivityFeed, FunctionTypes.ActivityFeedGetPlayedWith)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetSharedVideosRequest : RequestBase
	{
		internal Core.NpAccountId user;

		public Core.NpAccountId User
		{
			get
			{
				return user;
			}
			set
			{
				user = value;
			}
		}

		public GetSharedVideosRequest()
			: base(ServiceTypes.ActivityFeed, FunctionTypes.ActivityFeedGetSharedVideos)
		{
		}
	}

	public class FeedResponse : ResponseBase
	{
		internal Story[] stories;

		public Story[] Stories => stories;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GetFeedBegin);
			uint num = memoryBuffer.ReadUInt32();
			stories = new Story[num];
			for (int i = 0; i < num; i++)
			{
				stories[i] = new Story();
				stories[i].Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.GetFeedEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class UsersWhoLikedResponse : ResponseBase
	{
		internal StoryUser[] users;

		public StoryUser[] Users => users;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.UsersWhoLikedBegin);
			uint num = memoryBuffer.ReadUInt32();
			users = new StoryUser[num];
			for (int i = 0; i < num; i++)
			{
				users[i] = new StoryUser();
				users[i].Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.UsersWhoLikedEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class PlayedWithFeedResponse : ResponseBase
	{
		internal PlayedWithStory[] stories;

		public PlayedWithStory[] Stories => stories;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PlayedWithFeedBegin);
			uint num = memoryBuffer.ReadUInt32();
			stories = new PlayedWithStory[num];
			for (int i = 0; i < num; i++)
			{
				stories[i] = new PlayedWithStory();
				stories[i].Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PlayedWithFeedEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	public class SharedVideosResponse : ResponseBase
	{
		internal SharedVideo[] videos;

		public SharedVideo[] Videos => videos;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SharedVideosBegin);
			uint num = memoryBuffer.ReadUInt32();
			videos = new SharedVideo[num];
			for (int i = 0; i < num; i++)
			{
				videos[i] = new SharedVideo();
				videos[i].Read(memoryBuffer);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SharedVideosEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetFeed(GetFeedRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxPostInGameStory(PostInGameStoryRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxSetLiked(SetLikedRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetWhoLiked(GetWhoLikedRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxPostPlayedWith(PostPlayedWithRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetPlayedWith(GetPlayedWithRequest request, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetSharedVideos(GetSharedVideosRequest request, out APIResult result);

	public static int PostInGameStory(PostInGameStoryRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxPostInGameStory(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetFeed(GetFeedRequest request, FeedResponse response)
	{
		if (Main.initResult.sceSDKVersion > 122683392 && (request.FeedType == FeedType.UserFeed || request.FeedType == FeedType.UserNews))
		{
			throw new NpToolkitException("This request is not available in SDK " + Main.initResult.SceSDKVersion.ToString() + ". Only available in SDK 7.0 or less.");
		}
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetFeed(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int SetLiked(SetLikedRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxSetLiked(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetWhoLiked(GetWhoLikedRequest request, UsersWhoLikedResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetWhoLiked(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int PostPlayedWith(PostPlayedWithRequest request, Core.EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxPostPlayedWith(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetPlayedWith(GetPlayedWithRequest request, PlayedWithFeedResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetPlayedWith(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static int GetSharedVideos(GetSharedVideosRequest request, SharedVideosResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxGetSharedVideos(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
