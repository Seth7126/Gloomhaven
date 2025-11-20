using System.Threading;

namespace System.Internal;

internal sealed class HandleCollector
{
	private class HandleType
	{
		internal readonly string name;

		private int _initialThreshHold;

		private int _threshHold;

		private int _handleCount;

		private readonly int _deltaPercent;

		internal HandleType(string name, int expense, int initialThreshHold)
		{
			this.name = name;
			_initialThreshHold = initialThreshHold;
			_threshHold = initialThreshHold;
			_deltaPercent = 100 - expense;
		}

		internal void Add(IntPtr handle)
		{
			if (!(handle == IntPtr.Zero))
			{
				bool flag = false;
				int currentHandleCount = 0;
				lock (this)
				{
					_handleCount++;
					flag = NeedCollection();
					currentHandleCount = _handleCount;
				}
				lock (s_internalSyncObject)
				{
					HandleCollector.HandleAdded?.Invoke(name, handle, currentHandleCount);
				}
				if (flag && flag)
				{
					GC.Collect();
					Thread.Sleep((100 - _deltaPercent) / 4);
				}
			}
		}

		internal bool NeedCollection()
		{
			if (_handleCount > _threshHold)
			{
				_threshHold = _handleCount + _handleCount * _deltaPercent / 100;
				return true;
			}
			int num = 100 * _threshHold / (100 + _deltaPercent);
			if (num >= _initialThreshHold && _handleCount < (int)((float)num * 0.9f))
			{
				_threshHold = num;
			}
			return false;
		}

		internal IntPtr Remove(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				return handle;
			}
			int currentHandleCount = 0;
			lock (this)
			{
				_handleCount--;
				if (_handleCount < 0)
				{
					_handleCount = 0;
				}
				currentHandleCount = _handleCount;
			}
			lock (s_internalSyncObject)
			{
				HandleCollector.HandleRemoved?.Invoke(name, handle, currentHandleCount);
			}
			return handle;
		}
	}

	private static HandleType[] s_handleTypes;

	private static int s_handleTypeCount;

	private static object s_internalSyncObject = new object();

	internal static event HandleChangeEventHandler HandleAdded;

	internal static event HandleChangeEventHandler HandleRemoved;

	internal static IntPtr Add(IntPtr handle, int type)
	{
		s_handleTypes[type - 1].Add(handle);
		return handle;
	}

	internal static int RegisterType(string typeName, int expense, int initialThreshold)
	{
		lock (s_internalSyncObject)
		{
			if (s_handleTypeCount == 0 || s_handleTypeCount == s_handleTypes.Length)
			{
				HandleType[] destinationArray = new HandleType[s_handleTypeCount + 10];
				if (s_handleTypes != null)
				{
					Array.Copy(s_handleTypes, 0, destinationArray, 0, s_handleTypeCount);
				}
				s_handleTypes = destinationArray;
			}
			s_handleTypes[s_handleTypeCount++] = new HandleType(typeName, expense, initialThreshold);
			return s_handleTypeCount;
		}
	}

	internal static IntPtr Remove(IntPtr handle, int type)
	{
		return s_handleTypes[type - 1].Remove(handle);
	}
}
