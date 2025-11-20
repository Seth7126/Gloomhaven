using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

public class Dialogs
{
	[StructLayout(LayoutKind.Sequential)]
	internal class OpenDialogSettings
	{
		internal DialogMode mode;

		internal DialogType dispType;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class UserMessageParam
	{
		public const int MESSAGE_MAXSIZE = 255;

		internal DialogButtonTypes buttonType;

		internal UserMessageType msgType;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		internal string msg;

		public DialogButtonTypes ButtonType
		{
			get
			{
				return buttonType;
			}
			set
			{
				buttonType = value;
			}
		}

		public UserMessageType MsgType
		{
			get
			{
				return msgType;
			}
			set
			{
				msgType = value;
			}
		}

		public string Message
		{
			get
			{
				return msg;
			}
			set
			{
				if (value != null && value.Length > 255)
				{
					throw new SaveDataException("The length of the message string is more than " + 255 + " characters (MESSAGE_MAXSIZE)");
				}
				msg = value;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SystemMessageParam
	{
		internal SystemMessageType sysMsgType;

		internal ulong value;

		public SystemMessageType SysMsgType
		{
			get
			{
				return sysMsgType;
			}
			set
			{
				sysMsgType = value;
			}
		}

		public ulong Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class AnimationParam
	{
		internal Animation userOK;

		internal Animation userCancel;

		public Animation UserOK
		{
			get
			{
				return userOK;
			}
			set
			{
				userOK = value;
			}
		}

		public Animation UserCancel
		{
			get
			{
				return userCancel;
			}
			set
			{
				userCancel = value;
			}
		}

		public AnimationParam(Animation ok, Animation cancel)
		{
			userOK = ok;
			userCancel = cancel;
		}

		public AnimationParam()
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class ProgressBarParam
	{
		public const int MESSAGE_MAXSIZE = 255;

		internal ProgressBarType barType;

		internal ProgressSystemMessageType sysMsgType;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		internal string msg;

		public ProgressBarType BarType
		{
			get
			{
				return barType;
			}
			set
			{
				barType = value;
			}
		}

		public ProgressSystemMessageType SysMsgType
		{
			get
			{
				return sysMsgType;
			}
			set
			{
				sysMsgType = value;
			}
		}

		public string Message
		{
			get
			{
				return msg;
			}
			set
			{
				if (value != null && value.Length > 255)
				{
					throw new SaveDataException("The length of the message string is more than " + 255 + " characters (MESSAGE_MAXSIZE)");
				}
				msg = value;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class ErrorCodeParam
	{
		internal int errorCode;

		public int ErrorCode
		{
			get
			{
				return errorCode;
			}
			set
			{
				errorCode = value;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class Items
	{
		public const int DIR_NAME_MAXSIZE = 1024;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
		internal DirName[] dirNames = new DirName[1024];

		internal uint dirNameNum;

		internal FocusPos focusPos;

		internal DirName focusPosDirName;

		internal ItemStyle itemStyle;

		public DirName[] DirNames
		{
			get
			{
				if (dirNameNum == 0)
				{
					return null;
				}
				DirName[] array = new DirName[dirNameNum];
				Array.Copy(dirNames, array, (int)dirNameNum);
				return array;
			}
			set
			{
				if (value != null)
				{
					if (value.Length > 1024)
					{
						throw new SaveDataException("The size of the array is more than " + 1024);
					}
					value.CopyTo(dirNames, 0);
					dirNameNum = (uint)value.Length;
				}
				else
				{
					dirNameNum = 0u;
				}
			}
		}

		public uint DirNameCount => dirNameNum;

		public FocusPos FocusPos
		{
			get
			{
				return focusPos;
			}
			set
			{
				focusPos = value;
			}
		}

		public DirName FocusPosDirName
		{
			get
			{
				return focusPosDirName;
			}
			set
			{
				if (focusPos != FocusPos.DirName)
				{
					throw new SaveDataException("Can't set a focus DirName unless FocusPos is set to FocusPos.DirName value.");
				}
				focusPosDirName = value;
			}
		}

		public ItemStyle ItemStyle
		{
			get
			{
				return itemStyle;
			}
			set
			{
				itemStyle = value;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class NewItem
	{
		public const int TITLE_MAXSIZE = 127;

		public const int FILEPATH_LENGTH = 1023;

		public const int ICON_FILE_MAXSIZE = 116736;

		public const int DATA_ICON_WIDTH = 228;

		public const int DATA_ICON_HEIGHT = 128;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		internal string iconPath;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		internal string title;

		[MarshalAs(UnmanagedType.LPArray)]
		internal byte[] rawPNG;

		internal ulong pngDataSize;

		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				if (value != null && value.Length > 127)
				{
					throw new SaveDataException("The length of the title string is more than " + 127 + " characters (TITLE_MAXSIZE)");
				}
				title = value;
			}
		}

		public string IconPath
		{
			get
			{
				return iconPath;
			}
			set
			{
				iconPath = value;
			}
		}

		public byte[] RawPNG
		{
			get
			{
				return rawPNG;
			}
			set
			{
				rawPNG = value;
				pngDataSize = (ulong)((value != null) ? value.Length : 0);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class OptionParam
	{
		internal OptionBack back;

		public OptionBack Back
		{
			get
			{
				return back;
			}
			set
			{
				back = value;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class CloseParam
	{
		internal Animation anim;

		public Animation Anim
		{
			get
			{
				return anim;
			}
			set
			{
				anim = value;
			}
		}
	}

	public class OpenDialogRequest : RequestBase
	{
		internal OpenDialogSettings basicSettings = new OpenDialogSettings();

		internal UserMessageParam userMessage;

		internal SystemMessageParam systemMessage;

		internal ErrorCodeParam errorCode;

		internal ProgressBarParam progressBar;

		internal OptionParam option;

		internal Items items = new Items();

		internal NewItem newItem;

		internal AnimationParam animations;

		internal static bool dialogIsOpen = false;

		internal static CloseParam closeParam;

		public DialogMode Mode
		{
			get
			{
				return basicSettings.mode;
			}
			set
			{
				ThrowExceptionIfLocked();
				basicSettings.mode = value;
			}
		}

		public DialogType DispType
		{
			get
			{
				return basicSettings.dispType;
			}
			set
			{
				ThrowExceptionIfLocked();
				basicSettings.dispType = value;
			}
		}

		public UserMessageParam UserMessage
		{
			get
			{
				return userMessage;
			}
			set
			{
				ThrowExceptionIfLocked();
				userMessage = value;
			}
		}

		public SystemMessageParam SystemMessage
		{
			get
			{
				return systemMessage;
			}
			set
			{
				ThrowExceptionIfLocked();
				systemMessage = value;
			}
		}

		public ErrorCodeParam ErrorCode
		{
			get
			{
				return errorCode;
			}
			set
			{
				ThrowExceptionIfLocked();
				errorCode = value;
			}
		}

		public ProgressBarParam ProgressBar
		{
			get
			{
				return progressBar;
			}
			set
			{
				ThrowExceptionIfLocked();
				progressBar = value;
			}
		}

		public OptionParam Option
		{
			get
			{
				return option;
			}
			set
			{
				ThrowExceptionIfLocked();
				option = value;
			}
		}

		public Items Items
		{
			get
			{
				return items;
			}
			set
			{
				ThrowExceptionIfLocked();
				items = value;
			}
		}

		public NewItem NewItem
		{
			get
			{
				return newItem;
			}
			set
			{
				ThrowExceptionIfLocked();
				newItem = value;
			}
		}

		public AnimationParam Animations
		{
			get
			{
				return animations;
			}
			set
			{
				ThrowExceptionIfLocked();
				animations = value;
			}
		}

		internal override bool IsDeferred => true;

		public OpenDialogRequest()
			: base(FunctionTypes.OpenDialog)
		{
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			closeParam = null;
			if (newItem != null && newItem.pngDataSize == 0)
			{
				newItem.rawPNG = File.ReadAllBytes(newItem.iconPath);
				newItem.pngDataSize = (ulong)newItem.rawPNG.Length;
			}
			OpenDialogResponse openDialogResponse = pendingRequest.response as OpenDialogResponse;
			bool flag = false;
			APIResult result;
			if (!dialogIsOpen)
			{
				PrxSaveDataInitializeDialog(out result);
				if (result.sceErrorCode < 0)
				{
					openDialogResponse.Populate(result);
					return;
				}
				flag = true;
			}
			PrxSaveDataOpenDialog(userId, basicSettings, items, userMessage, systemMessage, errorCode, progressBar, newItem, option, animations, out var result2);
			if (openDialogResponse == null)
			{
				return;
			}
			openDialogResponse.Populate(result2);
			if (!openDialogResponse.IsErrorCodeWithoutLockCheck)
			{
				DialogUpdateStatus();
				if (!dialogIsOpen)
				{
					SendDialogOpenedNotification(pendingRequest);
					dialogIsOpen = true;
				}
			}
			else if (flag)
			{
				PrxSaveDataTerminateDialog(out result);
			}
		}

		internal override bool ExecutePolling(PendingRequest pendingRequest)
		{
			DialogStatus dialogStatus = DialogUpdateStatus();
			if (dialogStatus == DialogStatus.Finished)
			{
				DialogResult dialogResult = GetDialogResult();
				bool flag = true;
				if (HasDialogBeenCanceled(dialogResult))
				{
					if (animations != null && animations.userCancel == Animation.Off)
					{
						flag = false;
					}
				}
				else if (closeParam != null)
				{
					if (closeParam.anim == Animation.Off)
					{
						flag = false;
					}
				}
				else if (animations != null && animations.userOK == Animation.Off)
				{
					flag = false;
				}
				if (flag)
				{
					PrxSaveDataTerminateDialog(out var _);
					SendDialogClosedNotification(pendingRequest);
					dialogIsOpen = false;
					closeParam = null;
				}
				if (pendingRequest.response is OpenDialogResponse openDialogResponse)
				{
					openDialogResponse.Populate(dialogResult);
				}
				return false;
			}
			return true;
		}

		internal bool HasDialogBeenCanceled(DialogResult result)
		{
			if (result.callResult == DialogCallResults.UserCanceled)
			{
				return true;
			}
			if (result.buttonId == DialogButtonIds.No)
			{
				return true;
			}
			return false;
		}

		internal void SendDialogOpenedNotification(PendingRequest pendingRequest)
		{
			EmptyResponse emptyResponse = new EmptyResponse();
			emptyResponse.returnCode = 0;
			int num = -1;
			if (pendingRequest.request != null)
			{
				num = pendingRequest.request.userId;
			}
			DispatchQueueThread.AddNotificationRequest(emptyResponse, FunctionTypes.NotificationDialogOpened, num, pendingRequest.requestId, pendingRequest.request);
		}

		internal void SendDialogClosedNotification(PendingRequest pendingRequest)
		{
			EmptyResponse emptyResponse = new EmptyResponse();
			emptyResponse.returnCode = 0;
			int num = -1;
			if (pendingRequest.request != null)
			{
				num = pendingRequest.request.userId;
			}
			DispatchQueueThread.AddNotificationRequest(emptyResponse, FunctionTypes.NotificationDialogClosed, num, pendingRequest.requestId, pendingRequest.request);
		}
	}

	public class OpenDialogResponse : ResponseBase
	{
		internal DialogResult result;

		public DialogResult Result
		{
			get
			{
				ThrowExceptionIfLocked();
				return result;
			}
		}

		internal void Populate(DialogResult result)
		{
			returnCode = result.sceErrorCode;
			this.result = result;
		}
	}

	public enum DialogStatus
	{
		None,
		Initialized,
		Running,
		Finished
	}

	public enum DialogMode
	{
		Invalid,
		List,
		UserMsg,
		SystemMsg,
		ErrorCode,
		ProgressBar
	}

	public enum DialogType
	{
		Invalid,
		Save,
		Load,
		Delete
	}

	public enum UserMessageType
	{
		Normal,
		Error
	}

	public enum DialogCallResults
	{
		OK,
		UserCanceled
	}

	public enum DialogButtonTypes
	{
		OK,
		YesNo,
		None,
		OKCancel
	}

	public enum DialogButtonIds
	{
		Invalid = 0,
		OK = 1,
		Yes = 1,
		No = 2
	}

	public enum Animation
	{
		On,
		Off
	}

	public enum SystemMessageType
	{
		Invalid = 0,
		NoData = 1,
		Confirm = 2,
		Overwrite = 3,
		NoSpace = 4,
		Progress = 5,
		Corrupted = 6,
		Finished = 7,
		NoSpaceContinuable = 8,
		CorruptedAndDelete = 10,
		CorruptedAndCreate = 11,
		CurruptedAndRestore = 13,
		TotalSizeExceeded = 14
	}

	public enum ProgressBarType
	{
		Percentage
	}

	public enum ProgressSystemMessageType
	{
		Invalid,
		Progress,
		Restore
	}

	public enum OptionBack
	{
		Enable,
		Disable
	}

	public enum FocusPos
	{
		ListHead,
		ListTail,
		DataHead,
		DataTail,
		DataLatest,
		DataOldest,
		DirName
	}

	public enum ItemStyle
	{
		DateSizeSubtitle,
		SubtitleDataSize,
		DataSize
	}

	public class DialogResult
	{
		internal int sceErrorCode;

		internal DialogMode mode;

		internal DialogCallResults callResult;

		internal DialogButtonIds buttonId;

		internal DirName dirName;

		internal SaveDataParams param;

		public DialogMode Mode => mode;

		public DialogCallResults CallResult => callResult;

		public DialogButtonIds ButtonId => buttonId;

		public DirName DirName => dirName;

		public SaveDataParams Param => param;

		public int ErrorCode => sceErrorCode;
	}

	[DllImport("SaveData")]
	private static extern void PrxSaveDataOpenDialog(int userId, OpenDialogSettings basicSettings, Items itemsSettings, UserMessageParam userMessage, SystemMessageParam systemMessage, ErrorCodeParam errorCode, ProgressBarParam progressBar, NewItem newItem, OptionParam optionSettings, AnimationParam animations, out APIResult result);

	[DllImport("SaveData")]
	private static extern int PrxSaveDataDialogUpdateStatus();

	[DllImport("SaveData")]
	private static extern int PrxSaveDataDialogGetStatus();

	[DllImport("SaveData")]
	private static extern int PrxSaveDataDialogIsReadyToDisplay(out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataDialogGetResult(out MemoryBufferNative data, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataDialogProgressBarInc(uint delta, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataDialogProgressBarSetValue(uint rate, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataDialogClose(CloseParam closeParam, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataInitializeDialog(out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataTerminateDialog(out APIResult result);

	public static int OpenDialog(OpenDialogRequest request, OpenDialogResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}

	internal static DialogStatus DialogUpdateStatus()
	{
		return (DialogStatus)PrxSaveDataDialogUpdateStatus();
	}

	public static DialogStatus DialogGetStatus()
	{
		return (DialogStatus)PrxSaveDataDialogGetStatus();
	}

	public static bool DialogIsReadyToDisplay()
	{
		APIResult result;
		int num = PrxSaveDataDialogIsReadyToDisplay(out result);
		if (result.RaiseException)
		{
			throw new SaveDataException(result);
		}
		if (num == 0)
		{
			return false;
		}
		return true;
	}

	internal static DialogResult GetDialogResult()
	{
		DialogResult dialogResult = new DialogResult();
		MemoryBufferNative data = default(MemoryBufferNative);
		PrxSaveDataDialogGetResult(out data, out var result);
		if (result.sceErrorCode < 0)
		{
			dialogResult.sceErrorCode = result.sceErrorCode;
			dialogResult.mode = DialogMode.Invalid;
		}
		else
		{
			dialogResult.sceErrorCode = 0;
			MemoryBuffer memoryBuffer = new MemoryBuffer(data);
			memoryBuffer.CheckStartMarker();
			dialogResult.mode = (DialogMode)memoryBuffer.ReadInt32();
			dialogResult.callResult = (DialogCallResults)memoryBuffer.ReadInt32();
			dialogResult.buttonId = (DialogButtonIds)memoryBuffer.ReadInt32();
			dialogResult.dirName.Read(memoryBuffer);
			dialogResult.param.Read(memoryBuffer);
			memoryBuffer.CheckEndMarker();
		}
		return dialogResult;
	}

	public static void ProgressBarInc(uint delta)
	{
		PrxSaveDataDialogProgressBarInc(delta, out var result);
		if (result.RaiseException)
		{
			throw new SaveDataException(result);
		}
	}

	public static void ProgressBarSetValue(uint rate)
	{
		PrxSaveDataDialogProgressBarSetValue(rate, out var result);
		if (result.RaiseException)
		{
			throw new SaveDataException(result);
		}
	}

	public static void Close(CloseParam closeParam)
	{
		OpenDialogRequest.closeParam = closeParam;
		PrxSaveDataDialogClose(closeParam, out var result);
		if (result.RaiseException)
		{
			OpenDialogRequest.closeParam = null;
			throw new SaveDataException(result);
		}
	}
}
