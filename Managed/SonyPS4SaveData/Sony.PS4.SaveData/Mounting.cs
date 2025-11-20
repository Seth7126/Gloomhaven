using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

public class Mounting
{
	public struct MountPointName
	{
		public const int MOUNT_POINT_DATA_MAXSIZE = 15;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
		internal string data;

		public string Data => data;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.ReadString(ref data);
		}

		public override string ToString()
		{
			return data;
		}
	}

	[Flags]
	public enum MountModeFlags : uint
	{
		Invalid = 0u,
		ReadOnly = 1u,
		ReadWrite = 2u,
		Create = 4u,
		DestructOff = 8u,
		CopyIcon = 0x10u,
		Create2 = 0x20u
	}

	public class MountPoint
	{
		internal MountPointName name;

		internal DateTime openTime;

		internal int userId;

		internal MountModeFlags mountMode;

		internal DirName dirName;

		internal bool isMounted;

		public MountPointName PathName => name;

		public DateTime OpenTime => openTime;

		public double TimeMountedEstimate => (DateTime.UtcNow - openTime).TotalSeconds;

		public int UserId => userId;

		public MountModeFlags MountMode => mountMode;

		public DirName DirName => dirName;

		public bool IsMounted => isMounted;

		public override string ToString()
		{
			return name.Data;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class MountRequest : RequestBase
	{
		public const int BLOCK_SIZE = 32768;

		public const int BLOCKS_MIN = 96;

		public const int BLOCKS_MAX = 32768;

		internal DirName dirName;

		internal ulong blocks;

		internal MountModeFlags mountMode;

		public DirName DirName
		{
			get
			{
				return dirName;
			}
			set
			{
				ThrowExceptionIfLocked();
				dirName = value;
			}
		}

		public ulong Blocks
		{
			get
			{
				return blocks;
			}
			set
			{
				ThrowExceptionIfLocked();
				if (value < 96)
				{
					throw new SaveDataException("The block size can't be less than " + 96 + " blocks (BLOCKS_MIN)");
				}
				if (value > 32768)
				{
					throw new SaveDataException("The block size can't be greater than " + 32768 + " blocks (BLOCKS_MAX)");
				}
				blocks = value;
			}
		}

		public MountModeFlags MountMode
		{
			get
			{
				return mountMode;
			}
			set
			{
				ThrowExceptionIfLocked();
				mountMode = value;
			}
		}

		public MountRequest()
			: base(FunctionTypes.Mount)
		{
			blocks = 96uL;
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			MemoryBufferNative data = default(MemoryBufferNative);
			PrxSaveDataMount(this, out data, out var result);
			if (pendingRequest.response is MountResponse mountResponse)
			{
				mountResponse.Populate(result, data);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class UnmountRequest : RequestBase
	{
		internal MountPointName mountPointName;

		[MarshalAs(UnmanagedType.I1)]
		internal bool backup;

		public MountPointName MountPointName
		{
			get
			{
				return mountPointName;
			}
			set
			{
				ThrowExceptionIfLocked();
				mountPointName = value;
			}
		}

		public bool Backup
		{
			get
			{
				return backup;
			}
			set
			{
				ThrowExceptionIfLocked();
				backup = value;
			}
		}

		public UnmountRequest()
			: base(FunctionTypes.Unmount)
		{
			backup = false;
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			PrxSaveDataUnmount(this, out var result);
			UnmountRequest unmountRequest = pendingRequest.request as UnmountRequest;
			if (unmountRequest.backup)
			{
				Notifications.ExpectingNotification();
			}
			if (pendingRequest.response is EmptyResponse emptyResponse)
			{
				emptyResponse.Populate(result);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetMountInfoRequest : RequestBase
	{
		internal MountPointName mountPointName;

		public MountPointName MountPointName
		{
			get
			{
				return mountPointName;
			}
			set
			{
				ThrowExceptionIfLocked();
				mountPointName = value;
			}
		}

		public GetMountInfoRequest()
			: base(FunctionTypes.GetMountInfo)
		{
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			MemoryBufferNative data = default(MemoryBufferNative);
			PrxSaveDataGetMountInfo(this, out data, out var result);
			if (pendingRequest.response is MountInfoResponse mountInfoResponse)
			{
				mountInfoResponse.Populate(result, data);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class GetMountParamsRequest : RequestBase
	{
		internal MountPointName mountPointName;

		public MountPointName MountPointName
		{
			get
			{
				return mountPointName;
			}
			set
			{
				ThrowExceptionIfLocked();
				mountPointName = value;
			}
		}

		public GetMountParamsRequest()
			: base(FunctionTypes.GetMountParams)
		{
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			MemoryBufferNative data = default(MemoryBufferNative);
			PrxSaveDataGetMountParams(this, out data, out var result);
			if (pendingRequest.response is MountParamsResponse mountParamsResponse)
			{
				mountParamsResponse.Populate(result, data);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SetMountParamsRequest : RequestBase
	{
		internal MountPointName mountPointName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		internal string title;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		internal string subTitle;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		internal string detail;

		internal uint userParam;

		public MountPointName MountPointName
		{
			get
			{
				return mountPointName;
			}
			set
			{
				ThrowExceptionIfLocked();
				mountPointName = value;
			}
		}

		public SaveDataParams Params
		{
			get
			{
				return new SaveDataParams
				{
					title = title,
					subTitle = subTitle,
					detail = detail,
					userParam = userParam
				};
			}
			set
			{
				ThrowExceptionIfLocked();
				title = value.title;
				subTitle = value.subTitle;
				detail = value.detail;
				userParam = value.userParam;
			}
		}

		public SetMountParamsRequest()
			: base(FunctionTypes.SetMountParams)
		{
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			PrxSaveDataSetMountParams(this, out var result);
			if (pendingRequest.response is EmptyResponse emptyResponse)
			{
				emptyResponse.Populate(result);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SaveIconRequest : RequestBase
	{
		public const int FILEPATH_LENGTH = 1023;

		public const int ICON_FILE_MAXSIZE = 116736;

		public const int DATA_ICON_WIDTH = 228;

		public const int DATA_ICON_HEIGHT = 128;

		internal MountPointName mountPointName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		internal string iconPath;

		[MarshalAs(UnmanagedType.LPArray)]
		internal byte[] rawPNG;

		internal ulong pngDataSize;

		public MountPointName MountPointName
		{
			get
			{
				return mountPointName;
			}
			set
			{
				ThrowExceptionIfLocked();
				mountPointName = value;
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
				ThrowExceptionIfLocked();
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
				ThrowExceptionIfLocked();
				rawPNG = value;
				pngDataSize = (ulong)((value != null) ? value.Length : 0);
			}
		}

		public SaveIconRequest()
			: base(FunctionTypes.SaveIcon)
		{
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			if (pngDataSize == 0)
			{
				rawPNG = File.ReadAllBytes(iconPath);
				pngDataSize = (ulong)rawPNG.Length;
			}
			PrxSaveDataSaveIcon(this, out var result);
			if (pendingRequest.response is EmptyResponse emptyResponse)
			{
				emptyResponse.Populate(result);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class LoadIconRequest : RequestBase
	{
		internal MountPointName mountPointName;

		public MountPointName MountPointName
		{
			get
			{
				return mountPointName;
			}
			set
			{
				ThrowExceptionIfLocked();
				mountPointName = value;
			}
		}

		public LoadIconRequest()
			: base(FunctionTypes.LoadIcon)
		{
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			MemoryBufferNative data = default(MemoryBufferNative);
			PrxSaveDataLoadIcon(this, out data, out var result);
			if (pendingRequest.response is LoadIconResponse loadIconResponse)
			{
				loadIconResponse.Populate(result, data);
			}
		}
	}

	public class MountResponse : ResponseBase
	{
		internal MountPoint mountPoint = new MountPoint();

		internal ulong requiredBlocks;

		internal bool wasCreated;

		public MountPoint MountPoint
		{
			get
			{
				ThrowExceptionIfLocked();
				return mountPoint;
			}
		}

		public ulong RequiredBlocks
		{
			get
			{
				ThrowExceptionIfLocked();
				return requiredBlocks;
			}
		}

		public bool WasCreated
		{
			get
			{
				ThrowExceptionIfLocked();
				return wasCreated;
			}
		}

		internal void Populate(APIResult result, MemoryBufferNative data)
		{
			Populate(result);
			MemoryBuffer memoryBuffer = new MemoryBuffer(data);
			memoryBuffer.CheckStartMarker();
			mountPoint.name.Read(memoryBuffer);
			requiredBlocks = memoryBuffer.ReadUInt64();
			uint num = memoryBuffer.ReadUInt32();
			wasCreated = false;
			if (num == 1)
			{
				wasCreated = true;
			}
			memoryBuffer.CheckEndMarker();
			mountPoint.openTime = DateTime.UtcNow;
			mountPoint.isMounted = true;
		}
	}

	public class MountInfoResponse : ResponseBase
	{
		internal SaveDataInfo sdInfo;

		public SaveDataInfo Info
		{
			get
			{
				ThrowExceptionIfLocked();
				return sdInfo;
			}
		}

		internal void Populate(APIResult result, MemoryBufferNative data)
		{
			Populate(result);
			sdInfo = default(SaveDataInfo);
			MemoryBuffer memoryBuffer = new MemoryBuffer(data);
			memoryBuffer.CheckStartMarker();
			sdInfo.Read(memoryBuffer);
			memoryBuffer.CheckEndMarker();
		}
	}

	public class MountParamsResponse : ResponseBase
	{
		internal SaveDataParams sdParams;

		public SaveDataParams Params
		{
			get
			{
				ThrowExceptionIfLocked();
				return sdParams;
			}
		}

		internal void Populate(APIResult result, MemoryBufferNative data)
		{
			Populate(result);
			sdParams = default(SaveDataParams);
			MemoryBuffer memoryBuffer = new MemoryBuffer(data);
			memoryBuffer.CheckStartMarker();
			sdParams.Read(memoryBuffer);
			memoryBuffer.CheckEndMarker();
		}
	}

	public class LoadIconResponse : ResponseBase
	{
		internal Icon icon = null;

		public Icon Icon
		{
			get
			{
				ThrowExceptionIfLocked();
				return icon;
			}
		}

		internal void Populate(APIResult result, MemoryBufferNative data)
		{
			Populate(result);
			MemoryBuffer memoryBuffer = new MemoryBuffer(data);
			memoryBuffer.CheckStartMarker();
			if (memoryBuffer.ReadBool())
			{
				icon = Icon.ReadAndCreate(memoryBuffer);
			}
			else
			{
				icon = null;
			}
			memoryBuffer.CheckEndMarker();
		}
	}

	internal static List<MountPoint> activeMountPoints = new List<MountPoint>();

	internal static object activeMPSync = new object();

	public static List<MountPoint> ActiveMountPoints
	{
		get
		{
			lock (activeMPSync)
			{
				return new List<MountPoint>(activeMountPoints);
			}
		}
	}

	[DllImport("SaveData")]
	private static extern void PrxSaveDataMount(MountRequest request, out MemoryBufferNative data, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataUnmount(UnmountRequest request, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataGetMountInfo(GetMountInfoRequest request, out MemoryBufferNative data, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataGetMountParams(GetMountParamsRequest request, out MemoryBufferNative data, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataSetMountParams(SetMountParamsRequest request, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataSaveIcon(SaveIconRequest request, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataLoadIcon(LoadIconRequest request, out MemoryBufferNative data, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataGetIconSize(byte[] pngData, out int width, out int height);

	internal static void AddMountPoint(MountPoint mountPoint)
	{
		lock (activeMPSync)
		{
			activeMountPoints.Add(mountPoint);
		}
	}

	internal static bool RemoveMountPoint(MountPoint mountPoint)
	{
		lock (activeMPSync)
		{
			for (int i = 0; i < activeMountPoints.Count; i++)
			{
				if (activeMountPoints[i] == mountPoint)
				{
					activeMountPoints.RemoveAt(i);
					return true;
				}
			}
		}
		return false;
	}

	internal static bool RemoveMountPoint(MountPointName mountPointName)
	{
		lock (activeMPSync)
		{
			for (int i = 0; i < activeMountPoints.Count; i++)
			{
				if (activeMountPoints[i].name.data == mountPointName.data)
				{
					activeMountPoints.RemoveAt(i);
					return true;
				}
			}
		}
		return false;
	}

	internal static MountPoint FindMountPoint(MountPointName mountPointName)
	{
		lock (activeMPSync)
		{
			for (int i = 0; i < activeMountPoints.Count; i++)
			{
				if (activeMountPoints[i].name.data == mountPointName.data)
				{
					return activeMountPoints[i];
				}
			}
		}
		return null;
	}

	public static int Mount(MountRequest request, MountResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		response.mountPoint.mountMode = request.mountMode;
		response.mountPoint.userId = request.userId;
		response.mountPoint.dirName = request.dirName;
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}

	public static int Unmount(UnmountRequest request, EmptyResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		MountPoint mountPoint = FindMountPoint(request.mountPointName);
		if (mountPoint == null)
		{
			throw new SaveDataException("The mount point name provided isn't a currently active mount point. " + request.mountPointName);
		}
		if (!mountPoint.isMounted)
		{
			throw new SaveDataException("The mount point name provided is already unmounted. " + request.mountPointName);
		}
		mountPoint.isMounted = false;
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		int result = ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
		if (!request.async && response.ReturnCode != ReturnCodes.SUCCESS)
		{
			mountPoint.isMounted = true;
		}
		return result;
	}

	public static int GetMountInfo(GetMountInfoRequest request, MountInfoResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}

	public static int GetMountParams(GetMountParamsRequest request, MountParamsResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}

	public static int SetMountParams(SetMountParamsRequest request, EmptyResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}

	public static int SaveIcon(SaveIconRequest request, EmptyResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		if (request.pngDataSize != 0)
		{
			if (request.pngDataSize > 116736)
			{
				throw new SaveDataException("The number of bytes in the PNG icon is " + request.pngDataSize + " which is greater than the maximum" + 116736 + " allowed.\nSee SaveIconRequest.ICON_FILE_MAXSIZE.");
			}
			int width = 0;
			int height = 0;
			PrxSaveDataGetIconSize(request.rawPNG, out width, out height);
			if (width != 228 || height != 128)
			{
				throw new SaveDataException("The PNG icon size is incorrect. The current size is " + width + " x " + height + ".\nThe size must be " + 228 + " x " + 128 + ". See SaveIconRequest.DATA_ICON_WIDTH and SaveIconRequest.DATA_ICON_HEIGHT.");
			}
		}
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}

	public static int LoadIcon(LoadIconRequest request, LoadIconResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}
}
