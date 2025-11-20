using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Sony.NP;

public class Core
{
	[StructLayout(LayoutKind.Sequential)]
	public class TerminateServiceRequest : RequestBase
	{
		internal int service;

		public ServiceTypes Service
		{
			get
			{
				return Compatibility.ConvertServiceToEnum(serviceType);
			}
			set
			{
				service = Compatibility.ConvertFromEnum(value);
			}
		}

		public TerminateServiceRequest()
			: base(ServiceTypes.Core, FunctionTypes.CoreTerminateService)
		{
		}
	}

	public enum OptionalBoolean
	{
		notSet,
		setTrue,
		setFalse
	}

	public enum PlatformType
	{
		none,
		ps3,
		psVita,
		ps4
	}

	public enum OnlineStatus
	{
		notRequested,
		online,
		standBy,
		offline
	}

	public enum ReturnCodes : uint
	{
		SUCCESS = 0u,
		DIALOG_RESULT_OK = 0u,
		DIALOG_RESULT_USER_CANCELED = 1u,
		DIALOG_RESULT_USER_PURCHASED = 2u,
		DIALOG_RESULT_ALREADY_SIGNED_IN = 3u,
		DIALOG_RESULT_NOT_SIGNED_IN = 4u,
		DIALOG_RESULT_ABORTED = 10u,
		TROPHY_PLATINUM_UNLOCKED = 1u,
		MATCHING_CREATE_SYSTEM_SESSION_FAILED = 160u,
		MATCHING_JOIN_SYSTEM_SESSION_FAILED = 176u,
		MATCHING_UPDATE_SYSTEM_SESSION_FAILED = 192u,
		MATCHING_UPDATE_EXTERNAL_NOTIFICATION_FAILED = 208u,
		ERROR_FAILED_TO_ALLOCATE = 2153065984u,
		ERROR_TOO_MANY_REQUESTS = 2153065985u,
		ERROR_LOCKED_RESPONSE = 2153065986u,
		ERROR_ALREADY_INITIALIZED = 2153065987u,
		ERROR_NOT_INITIALIZED = 2153065988u,
		ERROR_INCORRECT_ARGUMENTS = 2153065989u,
		ERROR_MODIFICATION_NOT_ALLOWED = 2153065990u,
		ERROR_MAX_USERS_REACHED = 2153065991u,
		ERROR_INVALID_IMAGE = 2153065992u,
		ERROR_MEM_POOLS_INCORRECT = 2153065993u,
		ERROR_EXT_ALLOCATOR_INCORRECT = 2153065994u,
		ERROR_MAX_NUM_CALLBACKS_REACHED = 2153065995u,
		ERROR_CALLBACK_NOT_REGISTERED = 2153065996u,
		ERROR_TROPHY_HOME_DIRECTORY_NOT_CONFIGURED = 2153066096u,
		ERROR_MATCHING_ROOM_DESTROYED = 2153066240u,
		ERROR_MATCHING_INVALID_ATTRIBUTE_SCOPE = 2153066241u,
		ERROR_MATCHING_INVALID_ATTRIBUTE_TYP = 2153066242u,
		ERROR_MATCHING_INVALID_ROOM_ATTRIBUTE_VISIBILITY = 2153066243u,
		ERROR_MATCHING_SUM_OF_MEMBER_ATTRIBUTES_SIZES_IS_MORE_THAN_64 = 2153066244u,
		ERROR_MATCHING_MORE_THAN_1_BINARY_SEARCH_ATTRIBUTE_PROVIDED = 2153066245u,
		ERROR_MATCHING_SEARCH_BINARY_ATTRIBUTE_SIZE_IS_MORE_THAN_64 = 2153066246u,
		ERROR_MATCHING_MORE_THAN_8_INTEGER_SEARCH_ATTRIBUTES_PROVIDED = 2153066247u,
		ERROR_MATCHING_SUM_OF_EXTERNAL_ROOM_ATTRIBUTES_SIZES_IS_MORE_THAN_512 = 2153066248u,
		ERROR_MATCHING_SUM_OF_INTERNAL_ROOM_ATTRIBUTES_SIZES_IS_MORE_THAN_512 = 2153066249u,
		ERROR_MATCHING_NAMES_OF_ATTRIBUTES_MUST_BE_UNIQUE = 2153066250u,
		ERROR_MATCHING_INTERNAL_ATTRIBUTES_DONT_FIT_IN_256_ARRAYS = 2153066251u,
		ERROR_MATCHING_EXTERNAL_ATTRIBUTES_DONT_FIT_IN_256_ARRAYS = 2153066252u,
		ERROR_MATCHING_BIN_ATTRIBUTE_CANNOT_BE_SIZE_0 = 2153066253u,
		ERROR_MATCHING_INIT_CONFIGURATION_ALREADY_SET = 2153066254u,
		ERROR_MATCHING_INIT_CONFIGURATION_NOT_SET = 2153066255u,
		ERROR_MATCHING_USER_IS_ALREADY_IN_A_ROOM = 2153066256u,
		ERROR_MATCHING_USER_IS_NOT_IN_A_ROOM = 2153066257u,
		ERROR_MATCHING_NO_SESSION_BOUND_TO_ROOM = 2153066258u,
		ERROR_MATCHING_INVALID_WORLD_NUMBER = 2153066259u,
		ERROR_MATCHING_ATTRIBUTE_IS_NOT_SEARCHABLE_TYPE = 2153066260u,
		ERROR_MATCHING_INVALID_ATTRIBUTE = 2153066261u,
		ERROR_MATCHING_INVALID_MEMBER_ID = 2153066262u,
		NP_ERROR_INVALID_ARGUMENT = 2153054211u,
		NP_ERROR_UNKNOWN_PLATFORM_TYPE = 2153054212u,
		NP_ERROR_OUT_OF_MEMORY = 2153054213u,
		NP_ERROR_SIGNED_OUT = 2153054214u,
		NP_ERROR_USER_NOT_FOUND = 2153054215u,
		NP_ERROR_CALLBACK_ALREADY_REGISTERED = 2153054216u,
		NP_ERROR_CALLBACK_NOT_REGISTERED = 2153054217u,
		NP_ERROR_NOT_SIGNED_UP = 2153054218u,
		NP_ERROR_AGE_RESTRICTION = 2153054219u,
		NP_ERROR_LOGOUT = 2153054220u,
		NP_ERROR_LATEST_SYSTEM_SOFTWARE_EXIST = 2153054221u,
		NP_ERROR_LATEST_SYSTEM_SOFTWARE_EXIST_FOR_TITLE = 2153054222u,
		NP_ERROR_LATEST_PATCH_PKG_EXIST = 2153054223u,
		NP_ERROR_LATEST_PATCH_PKG_DOWNLOADED = 2153054224u,
		NP_ERROR_INVALID_SIZE = 2153054225u,
		NP_ERROR_ABORTED = 2153054226u,
		NP_ERROR_REQUEST_MAX = 2153054227u,
		NP_ERROR_REQUEST_NOT_FOUND = 2153054228u,
		NP_ERROR_INVALID_ID = 2153054229u,
		NP_ERROR_PATCH_NOT_CHECKED = 2153054232u,
		NP_ERROR_TIMEOUT = 2153054234u,
		NP_UTIL_ERROR_INVALID_NP_ID = 2153055749u,
		NP_UTIL_ERROR_NOT_MATCH = 2153055753u,
		NP_WEBAPI_ERROR_LIB_CONTEXT_NOT_FOUND = 2153064708u,
		NP_TROPHY_ERROR_INVALID_ARGUMENT = 2153059844u,
		NP_TROPHY_ERROR_ALREADY_REGISTERED = 2153059856u,
		NP_TROPHY_ERROR_INVALID_GROUP_ID = 2153059851u,
		NP_TROPHY_ERROR_TROPHY_ALREADY_UNLOCKED = 2153059852u,
		NP_TROPHY_ERROR_NOT_REGISTERED = 2153059855u,
		NP_TROPHY_ERROR_TROPHY_NOT_UNLOCKED = 2153059866u,
		TOOLKIT_NP_V2_ERROR_INCORRECT_ARGUMENTS = 2153065989u,
		NET_ERROR_RESOLVER_ENODNS = 2151743969u,
		NET_CTL_ERROR_NOT_CONNECTED = 2151751944u,
		NET_CTL_ERROR_NOT_AVAIL = 2151751945u,
		NP_COMMUNITY_SERVER_ERROR_NOT_BEST_SCORE = 2153056277u,
		NP_COMMUNITY_SERVER_ERROR_INVALID_SCORE = 2153056291u,
		NP_COMMUNITY_SERVER_ERROR_GAME_DATA_ALREADY_EXISTS = 2153056300u,
		NP_COMMUNITY_SERVER_ERROR_RANKING_GAME_DATA_MASTER_NOT_FOUND = 2153056280u,
		NP_MATCHING2_ERROR_CONTEXT_NOT_STARTED = 2153057288u,
		NP_COMMUNITY_SERVER_ERROR_FORBIDDEN = 2153056262u
	}

	public struct UserServiceUserId
	{
		public const int UserIdInvalid = -1;

		internal int id;

		public int Id
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
			}
		}

		public override string ToString()
		{
			return "0x" + id.ToString("X8");
		}

		internal void Read(MemoryBuffer buffer)
		{
			id = buffer.ReadInt32();
		}

		public static implicit operator UserServiceUserId(int value)
		{
			return new UserServiceUserId
			{
				id = value
			};
		}
	}

	public struct NpAccountId
	{
		internal ulong id;

		public ulong Id
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
			}
		}

		public override string ToString()
		{
			return "0x" + id.ToString("X16");
		}

		internal void Read(MemoryBuffer buffer)
		{
			id = buffer.ReadUInt64();
		}

		public static implicit operator NpAccountId(ulong value)
		{
			return new NpAccountId
			{
				id = value
			};
		}

		public static bool operator ==(NpAccountId a, NpAccountId b)
		{
			if (object.ReferenceEquals(a, b))
			{
				return true;
			}
			return a.id == b.id;
		}

		public static bool operator !=(NpAccountId a, NpAccountId b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			return obj is NpAccountId && this == (NpAccountId)obj;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}
	}

	public struct NpId
	{
		internal OnlineID handle;

		internal byte[] opt;

		public OnlineID Handle => handle;

		public byte[] Opt => opt;

		public override string ToString()
		{
			return handle.ToString();
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SceNpIdBegin);
			handle.Read(buffer);
			buffer.ReadData(ref opt);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.SceNpIdEnd);
		}
	}

	public class OnlineID
	{
		public const int SCE_NP_ONLINEID_MAX_LENGTH = 16;

		internal byte[] data;

		internal string name = "";

		public string Name => name;

		public OnlineID()
		{
			data = new byte[16];
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NpOnlineIdBegin);
			buffer.ReadData(ref data);
			int num = 16;
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] == 0)
				{
					num = i;
					break;
				}
			}
			if (num > 0)
			{
				name = Encoding.ASCII.GetString(data, 0, num);
			}
			else
			{
				name = "";
			}
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NpOnlineIdEnd);
		}

		public override string ToString()
		{
			return name;
		}

		public static bool operator ==(OnlineID a, OnlineID b)
		{
			if (object.ReferenceEquals(a, b))
			{
				return true;
			}
			if ((object)a == null || (object)b == null)
			{
				return false;
			}
			return a.name == b.name;
		}

		public static bool operator !=(OnlineID a, OnlineID b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			return obj is OnlineID && this == (OnlineID)obj;
		}

		public override int GetHashCode()
		{
			return name.GetHashCode();
		}
	}

	public class OnlineUser
	{
		internal NpAccountId accountId;

		internal OnlineID onlineId;

		public NpAccountId AccountId => accountId;

		public OnlineID OnlineID => onlineId;

		public OnlineUser()
		{
			onlineId = new OnlineID();
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.OnlineUserBegin);
			accountId.Read(buffer);
			onlineId.Read(buffer);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.OnlineUserEnd);
		}

		public override string ToString()
		{
			return $"0x{accountId:X} : {onlineId.ToString()}\n";
		}
	}

	public class CountryCode
	{
		public const int SCE_NP_COUNTRY_CODE_LENGTH = 2;

		internal string code = "";

		public string Code
		{
			get
			{
				return code;
			}
			set
			{
				if (value.Length > 2)
				{
					throw new NpToolkitException("Country code can only be a maximum of 2 characters .");
				}
				code = value;
			}
		}

		public CountryCode()
		{
			code = "";
		}

		public CountryCode(string countryCode)
		{
			Code = countryCode;
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NpCountryCodeBegin);
			buffer.ReadString(ref code);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NpCountryCodeEnd);
		}

		public override string ToString()
		{
			return code;
		}

		public static implicit operator CountryCode(string countryCode)
		{
			CountryCode countryCode2 = new CountryCode();
			countryCode2.Code = countryCode;
			return countryCode2;
		}
	}

	public class LanguageCode
	{
		public const int SCE_NP_LANGUAGE_CODE_MAX_LEN = 5;

		internal string code;

		public string Code
		{
			get
			{
				return code;
			}
			set
			{
				if (value.Length > 5)
				{
					throw new NpToolkitException("Language code can only be a maximum of 5 characters .");
				}
				code = value;
			}
		}

		public LanguageCode()
		{
			code = "";
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NpLanguageCodeBegin);
			buffer.ReadString(ref code);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NpLanguageCodeEnd);
		}

		public override string ToString()
		{
			return code;
		}

		public static implicit operator LanguageCode(string languageCode)
		{
			LanguageCode languageCode2 = new LanguageCode();
			languageCode2.Code = languageCode;
			return languageCode2;
		}
	}

	public class TitleId
	{
		public const int SCE_NP_TITLE_ID_LEN = 12;

		internal byte[] data;

		public string Id => Encoding.ASCII.GetString(data, 0, data.Length);

		public TitleId()
		{
			data = new byte[12];
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NpTitleIdBegin);
			buffer.ReadData(ref data);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.NpTitleIdEnd);
		}

		public override string ToString()
		{
			return Encoding.ASCII.GetString(data, 0, data.Length);
		}
	}

	public class EmptyResponse : ResponseBase
	{
		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer readBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			EndReadResponseBuffer(readBuffer);
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTerminateService(TerminateServiceRequest request, out APIResult result);

	public static int TerminateService(TerminateServiceRequest request, EmptyResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxTerminateService(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}

	public static string ConvertSceErrorToString(int errorCode)
	{
		string text = "(0x" + errorCode.ToString("X8") + ")";
		ReturnCodes returnCodes = (ReturnCodes)errorCode;
		if (Enum.IsDefined(typeof(ReturnCodes), returnCodes))
		{
			return text + " (" + returnCodes.ToString() + ") ";
		}
		return text + " (UNKNOWN) ";
	}

	internal static DateTime ReadRtcTick(MemoryBuffer buffer)
	{
		ulong rtcTick = buffer.ReadUInt64();
		return RtcTicksToDateTime(rtcTick);
	}

	internal static ulong DateTimeToRtcTicks(DateTime dateTime)
	{
		ulong num = 10uL;
		ulong ticks = (ulong)dateTime.Ticks;
		return ticks / num;
	}

	internal static DateTime RtcTicksToDateTime(ulong rtcTick)
	{
		ulong num = 10uL;
		rtcTick *= num;
		return new DateTime((long)rtcTick);
	}
}
