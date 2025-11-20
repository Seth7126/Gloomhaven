using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace System.Drawing;

internal sealed class ComIStreamWrapper : IStream
{
	private const int STG_E_INVALIDFUNCTION = -2147287039;

	private readonly Stream baseStream;

	private long position = -1L;

	internal ComIStreamWrapper(Stream stream)
	{
		baseStream = stream;
	}

	private void SetSizeToPosition()
	{
		if (position != -1)
		{
			if (position > baseStream.Length)
			{
				baseStream.SetLength(position);
			}
			baseStream.Position = position;
			position = -1L;
		}
	}

	public void Read(byte[] pv, int cb, IntPtr pcbRead)
	{
		int val = 0;
		if (cb != 0)
		{
			SetSizeToPosition();
			val = baseStream.Read(pv, 0, cb);
		}
		if (pcbRead != IntPtr.Zero)
		{
			Marshal.WriteInt32(pcbRead, val);
		}
	}

	public void Write(byte[] pv, int cb, IntPtr pcbWritten)
	{
		if (cb != 0)
		{
			SetSizeToPosition();
			baseStream.Write(pv, 0, cb);
		}
		if (pcbWritten != IntPtr.Zero)
		{
			Marshal.WriteInt32(pcbWritten, cb);
		}
	}

	public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
	{
		long length = baseStream.Length;
		long num = (SeekOrigin)dwOrigin switch
		{
			SeekOrigin.Begin => dlibMove, 
			SeekOrigin.Current => (position != -1) ? (position + dlibMove) : (baseStream.Position + dlibMove), 
			SeekOrigin.End => length + dlibMove, 
			_ => throw new ExternalException(null, -2147287039), 
		};
		if (num > length)
		{
			position = num;
		}
		else
		{
			baseStream.Position = num;
			position = -1L;
		}
		if (plibNewPosition != IntPtr.Zero)
		{
			Marshal.WriteInt64(plibNewPosition, num);
		}
	}

	public void SetSize(long libNewSize)
	{
		baseStream.SetLength(libNewSize);
	}

	public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
	{
		long num = 0L;
		if (cb != 0L)
		{
			int num2 = (int)((cb >= 4096) ? 4096 : cb);
			byte[] array = new byte[num2];
			SetSizeToPosition();
			int num3;
			while ((num3 = baseStream.Read(array, 0, num2)) != 0)
			{
				pstm.Write(array, num3, IntPtr.Zero);
				num += num3;
				if (num >= cb)
				{
					break;
				}
				if (cb - num < 4096)
				{
					num2 = (int)(cb - num);
				}
			}
		}
		if (pcbRead != IntPtr.Zero)
		{
			Marshal.WriteInt64(pcbRead, num);
		}
		if (pcbWritten != IntPtr.Zero)
		{
			Marshal.WriteInt64(pcbWritten, num);
		}
	}

	public void Commit(int grfCommitFlags)
	{
		baseStream.Flush();
		SetSizeToPosition();
	}

	public void Revert()
	{
		throw new ExternalException(null, -2147287039);
	}

	public void LockRegion(long libOffset, long cb, int dwLockType)
	{
		throw new ExternalException(null, -2147287039);
	}

	public void UnlockRegion(long libOffset, long cb, int dwLockType)
	{
		throw new ExternalException(null, -2147287039);
	}

	public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
	{
		pstatstg = default(System.Runtime.InteropServices.ComTypes.STATSTG);
		pstatstg.cbSize = baseStream.Length;
	}

	public void Clone(out IStream ppstm)
	{
		ppstm = null;
		throw new ExternalException(null, -2147287039);
	}
}
