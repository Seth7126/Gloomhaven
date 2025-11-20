using System.IO;
using System.Internal;
using System.Runtime.InteropServices;

namespace System.Drawing;

internal class UnsafeNativeMethods
{
	[ComImport]
	[Guid("0000000C-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStream
	{
		int Read([In] IntPtr buf, [In] int len);

		int Write([In] IntPtr buf, [In] int len);

		[return: MarshalAs(UnmanagedType.I8)]
		long Seek([In][MarshalAs(UnmanagedType.I8)] long dlibMove, [In] int dwOrigin);

		void SetSize([In][MarshalAs(UnmanagedType.I8)] long libNewSize);

		[return: MarshalAs(UnmanagedType.I8)]
		long CopyTo([In][MarshalAs(UnmanagedType.Interface)] IStream pstm, [In][MarshalAs(UnmanagedType.I8)] long cb, [Out][MarshalAs(UnmanagedType.LPArray)] long[] pcbRead);

		void Commit([In] int grfCommitFlags);

		void Revert();

		void LockRegion([In][MarshalAs(UnmanagedType.I8)] long libOffset, [In][MarshalAs(UnmanagedType.I8)] long cb, [In] int dwLockType);

		void UnlockRegion([In][MarshalAs(UnmanagedType.I8)] long libOffset, [In][MarshalAs(UnmanagedType.I8)] long cb, [In] int dwLockType);

		void Stat([In] IntPtr pStatstg, [In] int grfStatFlag);

		[return: MarshalAs(UnmanagedType.Interface)]
		IStream Clone();
	}

	internal class ComStreamFromDataStream : IStream
	{
		protected Stream dataStream;

		private long _virtualPosition = -1L;

		internal ComStreamFromDataStream(Stream dataStream)
		{
			this.dataStream = dataStream ?? throw new ArgumentNullException("dataStream");
		}

		private void ActualizeVirtualPosition()
		{
			if (_virtualPosition != -1)
			{
				if (_virtualPosition > dataStream.Length)
				{
					dataStream.SetLength(_virtualPosition);
				}
				dataStream.Position = _virtualPosition;
				_virtualPosition = -1L;
			}
		}

		public virtual IStream Clone()
		{
			NotImplemented();
			return null;
		}

		public virtual void Commit(int grfCommitFlags)
		{
			dataStream.Flush();
			ActualizeVirtualPosition();
		}

		public virtual long CopyTo(IStream pstm, long cb, long[] pcbRead)
		{
			int num = 4096;
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException();
			}
			long num2 = 0L;
			try
			{
				int num4;
				for (; num2 < cb; num2 += num4)
				{
					int num3 = num;
					if (num2 + num3 > cb)
					{
						num3 = (int)(cb - num2);
					}
					num4 = Read(intPtr, num3);
					if (num4 == 0)
					{
						break;
					}
					if (pstm.Write(intPtr, num4) != num4)
					{
						throw EFail("Wrote an incorrect number of bytes");
					}
				}
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
			if (pcbRead != null && pcbRead.Length != 0)
			{
				pcbRead[0] = num2;
			}
			return num2;
		}

		public virtual void LockRegion(long libOffset, long cb, int dwLockType)
		{
		}

		protected static ExternalException EFail(string msg)
		{
			throw new ExternalException(msg, -2147467259);
		}

		protected static void NotImplemented()
		{
			throw new ExternalException(global::SR.Format("Not implemented."), -2147467263);
		}

		public virtual int Read(IntPtr buf, int length)
		{
			byte[] array = new byte[length];
			int result = Read(array, length);
			Marshal.Copy(array, 0, buf, length);
			return result;
		}

		public virtual int Read(byte[] buffer, int length)
		{
			ActualizeVirtualPosition();
			return dataStream.Read(buffer, 0, length);
		}

		public virtual void Revert()
		{
			NotImplemented();
		}

		public virtual long Seek(long offset, int origin)
		{
			long num = _virtualPosition;
			if (_virtualPosition == -1)
			{
				num = dataStream.Position;
			}
			long length = dataStream.Length;
			switch (origin)
			{
			case 0:
				if (offset <= length)
				{
					dataStream.Position = offset;
					_virtualPosition = -1L;
				}
				else
				{
					_virtualPosition = offset;
				}
				break;
			case 2:
				if (offset <= 0)
				{
					dataStream.Position = length + offset;
					_virtualPosition = -1L;
				}
				else
				{
					_virtualPosition = length + offset;
				}
				break;
			case 1:
				if (offset + num <= length)
				{
					dataStream.Position = num + offset;
					_virtualPosition = -1L;
				}
				else
				{
					_virtualPosition = offset + num;
				}
				break;
			}
			if (_virtualPosition != -1)
			{
				return _virtualPosition;
			}
			return dataStream.Position;
		}

		public virtual void SetSize(long value)
		{
			dataStream.SetLength(value);
		}

		public virtual void Stat(IntPtr pstatstg, int grfStatFlag)
		{
			NotImplemented();
		}

		public virtual void UnlockRegion(long libOffset, long cb, int dwLockType)
		{
		}

		public virtual int Write(IntPtr buf, int length)
		{
			byte[] array = new byte[length];
			Marshal.Copy(buf, array, 0, length);
			return Write(array, length);
		}

		public virtual int Write(byte[] buffer, int length)
		{
			ActualizeVirtualPosition();
			dataStream.Write(buffer, 0, length);
			return length;
		}
	}

	[DllImport("kernel32", CharSet = CharSet.Auto, EntryPoint = "RtlMoveMemory", ExactSpelling = true, SetLastError = true)]
	public static extern void CopyMemory(HandleRef destData, HandleRef srcData, int size);

	[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "GetDC", ExactSpelling = true, SetLastError = true)]
	private static extern IntPtr IntGetDC(HandleRef hWnd);

	public static IntPtr GetDC(HandleRef hWnd)
	{
		return System.Internal.HandleCollector.Add(IntGetDC(hWnd), SafeNativeMethods.CommonHandles.HDC);
	}

	[DllImport("gdi32", CharSet = CharSet.Auto, EntryPoint = "DeleteDC", ExactSpelling = true, SetLastError = true)]
	private static extern bool IntDeleteDC(HandleRef hDC);

	public static bool DeleteDC(HandleRef hDC)
	{
		System.Internal.HandleCollector.Remove((IntPtr)hDC, SafeNativeMethods.CommonHandles.GDI);
		return IntDeleteDC(hDC);
	}

	[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "ReleaseDC", ExactSpelling = true, SetLastError = true)]
	private static extern int IntReleaseDC(HandleRef hWnd, HandleRef hDC);

	public static int ReleaseDC(HandleRef hWnd, HandleRef hDC)
	{
		System.Internal.HandleCollector.Remove((IntPtr)hDC, SafeNativeMethods.CommonHandles.HDC);
		return IntReleaseDC(hWnd, hDC);
	}

	[DllImport("gdi32", CharSet = CharSet.Auto, EntryPoint = "CreateCompatibleDC", ExactSpelling = true, SetLastError = true)]
	private static extern IntPtr IntCreateCompatibleDC(HandleRef hDC);

	public static IntPtr CreateCompatibleDC(HandleRef hDC)
	{
		return System.Internal.HandleCollector.Add(IntCreateCompatibleDC(hDC), SafeNativeMethods.CommonHandles.GDI);
	}

	[DllImport("gdi32", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	public static extern IntPtr GetStockObject(int nIndex);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int GetSystemDefaultLCID();

	[DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	public static extern int GetSystemMetrics(int nIndex);

	[DllImport("user32", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool SystemParametersInfo(int uiAction, int uiParam, [In][Out] NativeMethods.NONCLIENTMETRICS pvParam, int fWinIni);

	[DllImport("user32", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool SystemParametersInfo(int uiAction, int uiParam, [In][Out] SafeNativeMethods.LOGFONT pvParam, int fWinIni);

	[DllImport("gdi32", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	public static extern int GetDeviceCaps(HandleRef hDC, int nIndex);

	[DllImport("gdi32", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	public static extern int GetObjectType(HandleRef hObject);
}
