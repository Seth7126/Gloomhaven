using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

public class Searching
{
	public enum SearchSortKey : uint
	{
		DirName = 0u,
		UserParam = 1u,
		Blocks = 2u,
		Time = 3u,
		FreeBlocks = 5u
	}

	public enum SearchSortOrder : uint
	{
		Ascending,
		Descending
	}

	[StructLayout(LayoutKind.Sequential)]
	public class DirNameSearchRequest : RequestBase
	{
		public const int DIR_NAME_MAXSIZE = 1024;

		internal DirName dirName;

		internal uint maxDirNameCount = 1024u;

		internal SearchSortKey key;

		internal SearchSortOrder order;

		[MarshalAs(UnmanagedType.I1)]
		internal bool includeParams;

		[MarshalAs(UnmanagedType.I1)]
		internal bool includeBlockInfo;

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

		public uint MaxDirNameCount
		{
			get
			{
				return maxDirNameCount;
			}
			set
			{
				ThrowExceptionIfLocked();
				maxDirNameCount = value;
			}
		}

		public SearchSortKey Key
		{
			get
			{
				return key;
			}
			set
			{
				ThrowExceptionIfLocked();
				key = value;
			}
		}

		public SearchSortOrder Order
		{
			get
			{
				return order;
			}
			set
			{
				ThrowExceptionIfLocked();
				order = value;
			}
		}

		public bool IncludeParams
		{
			get
			{
				return includeParams;
			}
			set
			{
				ThrowExceptionIfLocked();
				includeParams = value;
			}
		}

		public bool IncludeBlockInfo
		{
			get
			{
				return includeBlockInfo;
			}
			set
			{
				ThrowExceptionIfLocked();
				includeBlockInfo = value;
			}
		}

		public DirNameSearchRequest()
			: base(FunctionTypes.DirNameSearch)
		{
		}

		internal override void Execute(PendingRequest pendingRequest)
		{
			MemoryBufferNative data = default(MemoryBufferNative);
			PrxSaveDataDirNameSearch(this, out data, out var result);
			if (pendingRequest.response is DirNameSearchResponse dirNameSearchResponse)
			{
				dirNameSearchResponse.Populate(result, data);
			}
		}
	}

	public class SearchSaveDataItem
	{
		internal DirName dirName;

		internal SaveDataParams sdParams;

		internal SaveDataInfo sdInfo;

		public DirName DirName => dirName;

		public SaveDataParams Params => sdParams;

		public SaveDataInfo Info => sdInfo;

		internal void Read(MemoryBuffer buffer, bool hasParams, bool hasBlockInfo)
		{
			dirName.Read(buffer);
			if (hasParams)
			{
				sdParams.Read(buffer);
			}
			if (hasBlockInfo)
			{
				sdInfo.Read(buffer);
			}
		}
	}

	public class DirNameSearchResponse : ResponseBase
	{
		internal bool hasParams;

		internal bool hasBlockInfo;

		internal SearchSaveDataItem[] saveDataItems;

		public SearchSaveDataItem[] SaveDataItems
		{
			get
			{
				ThrowExceptionIfLocked();
				return saveDataItems;
			}
		}

		public bool HasParams
		{
			get
			{
				ThrowExceptionIfLocked();
				return hasParams;
			}
		}

		public bool HasInfo
		{
			get
			{
				ThrowExceptionIfLocked();
				return hasBlockInfo;
			}
		}

		internal void Populate(APIResult result, MemoryBufferNative data)
		{
			Populate(result);
			MemoryBuffer memoryBuffer = new MemoryBuffer(data);
			memoryBuffer.CheckStartMarker();
			uint num = memoryBuffer.ReadUInt32();
			hasParams = memoryBuffer.ReadBool();
			hasBlockInfo = memoryBuffer.ReadBool();
			saveDataItems = new SearchSaveDataItem[num];
			for (int i = 0; i < num; i++)
			{
				saveDataItems[i] = new SearchSaveDataItem();
				saveDataItems[i].Read(memoryBuffer, hasParams, hasBlockInfo);
			}
			memoryBuffer.CheckEndMarker();
		}
	}

	[DllImport("SaveData")]
	private static extern void PrxSaveDataDirNameSearch(DirNameSearchRequest request, out MemoryBufferNative data, out APIResult result);

	public static int DirNameSearch(DirNameSearchRequest request, DirNameSearchResponse response)
	{
		DispatchQueueThread.ThrowExceptionIfSameThread(request.async);
		PendingRequest pendingEvent = ProcessQueueThread.AddEvent(request, response);
		return ProcessQueueThread.WaitIfSyncRequest(pendingEvent);
	}
}
