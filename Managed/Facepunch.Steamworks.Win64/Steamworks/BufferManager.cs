using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Steamworks.Data;

namespace Steamworks;

internal static class BufferManager
{
	private sealed class ReferenceCounter
	{
		private int _count;

		public IntPtr Pointer { get; private set; }

		public int Size { get; private set; }

		public void Set(IntPtr ptr, int size, int referenceCount)
		{
			if (ptr == IntPtr.Zero)
			{
				throw new ArgumentNullException("ptr");
			}
			if (size <= 0)
			{
				throw new ArgumentOutOfRangeException("size");
			}
			if (referenceCount <= 0)
			{
				throw new ArgumentOutOfRangeException("referenceCount");
			}
			Pointer = ptr;
			Size = size;
			if (Interlocked.Exchange(ref _count, referenceCount) != 0)
			{
				SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Warning, "BufferManager set reference count when current count was not 0");
			}
		}

		public bool Decrement()
		{
			int num = Interlocked.Decrement(ref _count);
			if (num < 0)
			{
				SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Bug, "Prevented double free of BufferManager pointer");
				return false;
			}
			return num == 0;
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private unsafe delegate void FreeFn(NetMsg* msg);

	private static readonly Stack<ReferenceCounter> ReferenceCounterPool = new Stack<ReferenceCounter>(1024);

	private static readonly Dictionary<int, Stack<IntPtr>> BufferPools = new Dictionary<int, Stack<IntPtr>>();

	private static readonly Dictionary<IntPtr, ReferenceCounter> ReferenceCounters = new Dictionary<IntPtr, ReferenceCounter>(1024);

	public unsafe static readonly IntPtr FreeFunctionPointer = Marshal.GetFunctionPointerForDelegate<FreeFn>(Free);

	private const int Bucket512 = 512;

	private const int Bucket1Kb = 1024;

	private const int Bucket4Kb = 4096;

	private const int Bucket16Kb = 16384;

	private const int Bucket64Kb = 65536;

	private const int Bucket256Kb = 262144;

	public static IntPtr Get(int size, int referenceCount)
	{
		if (size < 0 || size > 16777216)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		if (referenceCount <= 0)
		{
			throw new ArgumentOutOfRangeException("referenceCount");
		}
		AllocateBuffer(size, out var ptr, out var size2);
		ReferenceCounter value = AllocateReferenceCounter(ptr, size2, referenceCount);
		SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Verbose, string.Format("{0} allocated {1:X8} (size={2}, actualSize={3}) with {4} references", "BufferManager", ptr.ToInt64(), size, size2, referenceCount));
		lock (ReferenceCounters)
		{
			ReferenceCounters.Add(ptr, value);
		}
		return ptr;
	}

	[MonoPInvokeCallback]
	private unsafe static void Free(NetMsg* msg)
	{
		IntPtr dataPtr = msg->DataPtr;
		lock (ReferenceCounters)
		{
			if (!ReferenceCounters.TryGetValue(dataPtr, out var value))
			{
				SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Bug, string.Format("Attempt to free pointer not tracked by {0}: {1:X8}", "BufferManager", dataPtr.ToInt64()));
				return;
			}
			SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Verbose, string.Format("{0} decrementing reference count of {1:X8}", "BufferManager", dataPtr.ToInt64()));
			if (value.Decrement())
			{
				SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Verbose, string.Format("{0} freeing {1:X8} as it is now unreferenced", "BufferManager", dataPtr.ToInt64()));
				if (dataPtr != value.Pointer)
				{
					SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Bug, string.Format("{0} freed pointer ({1:X8}) does not match counter pointer ({2:X8})", "BufferManager", dataPtr.ToInt64(), value.Pointer.ToInt64()));
				}
				int bucketSize = GetBucketSize(value.Size);
				if (value.Size != bucketSize)
				{
					SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Bug, string.Format("{0} freed pointer size ({1}) does not match bucket size ({2})", "BufferManager", value.Size, bucketSize));
				}
				ReferenceCounters.Remove(dataPtr);
				FreeBuffer(dataPtr, value.Size);
				FreeReferenceCounter(value);
			}
		}
	}

	private static ReferenceCounter AllocateReferenceCounter(IntPtr ptr, int size, int referenceCount)
	{
		lock (ReferenceCounterPool)
		{
			ReferenceCounter referenceCounter = ((ReferenceCounterPool.Count > 0) ? ReferenceCounterPool.Pop() : new ReferenceCounter());
			referenceCounter.Set(ptr, size, referenceCount);
			return referenceCounter;
		}
	}

	private static void FreeReferenceCounter(ReferenceCounter counter)
	{
		if (counter == null)
		{
			throw new ArgumentNullException("counter");
		}
		lock (ReferenceCounterPool)
		{
			if (ReferenceCounterPool.Count < 1024)
			{
				ReferenceCounterPool.Push(counter);
			}
		}
	}

	private static void AllocateBuffer(int minimumSize, out IntPtr ptr, out int size)
	{
		int bucketSize = GetBucketSize(minimumSize);
		if (bucketSize <= 0)
		{
			ptr = Marshal.AllocHGlobal(minimumSize);
			size = minimumSize;
			SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Verbose, string.Format("{0} allocated unpooled pointer {1:X8} (size={2})", "BufferManager", ptr.ToInt64(), size));
			return;
		}
		lock (BufferPools)
		{
			if (!BufferPools.TryGetValue(bucketSize, out var value) || value.Count == 0)
			{
				ptr = Marshal.AllocHGlobal(bucketSize);
				size = bucketSize;
				SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Verbose, string.Format("{0} allocated new poolable pointer {1:X8} (size={2})", "BufferManager", ptr.ToInt64(), size));
			}
			else
			{
				ptr = value.Pop();
				size = bucketSize;
				SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Verbose, string.Format("{0} allocated pointer from pool {1:X8} (size={2})", "BufferManager", ptr.ToInt64(), size));
			}
		}
	}

	private static void FreeBuffer(IntPtr ptr, int size)
	{
		int bucketSize = GetBucketSize(size);
		int bucketLimit = GetBucketLimit(size);
		if (bucketSize <= 0 || bucketLimit <= 0)
		{
			Marshal.FreeHGlobal(ptr);
			SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Verbose, string.Format("{0} freed unpooled pointer {1:X8} (size={2})", "BufferManager", ptr.ToInt64(), size));
			return;
		}
		lock (BufferPools)
		{
			if (!BufferPools.TryGetValue(bucketSize, out var value))
			{
				value = new Stack<IntPtr>(bucketLimit);
				BufferPools.Add(bucketSize, value);
			}
			if (value.Count >= bucketLimit)
			{
				Marshal.FreeHGlobal(ptr);
				SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Verbose, string.Format("{0} pool overflow, freed pooled pointer {1:X8} (size={2})", "BufferManager", ptr.ToInt64(), size));
			}
			else
			{
				value.Push(ptr);
				SteamNetworkingUtils.LogDebugMessage(NetDebugOutput.Verbose, string.Format("{0} returned pointer to pool {1:X8} (size={2})", "BufferManager", ptr.ToInt64(), size));
			}
		}
	}

	private static int GetBucketSize(int size)
	{
		if (size <= 512)
		{
			return 512;
		}
		if (size <= 1024)
		{
			return 1024;
		}
		if (size <= 4096)
		{
			return 4096;
		}
		if (size <= 16384)
		{
			return 16384;
		}
		if (size <= 65536)
		{
			return 65536;
		}
		if (size <= 262144)
		{
			return 262144;
		}
		return -1;
	}

	private static int GetBucketLimit(int size)
	{
		if (size <= 512)
		{
			return 1024;
		}
		if (size <= 1024)
		{
			return 512;
		}
		if (size <= 4096)
		{
			return 128;
		}
		if (size <= 16384)
		{
			return 32;
		}
		if (size <= 65536)
		{
			return 16;
		}
		if (size <= 262144)
		{
			return 8;
		}
		return -1;
	}
}
