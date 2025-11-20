using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PlayEveryWare.EpicOnlineServices;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct EOSSteamInternal : IDisposable
{
	public int ApiVersion;

	public IntPtr m_OverrideLibraryPath;

	public string OverrideLibraryPath
	{
		set
		{
			if (m_OverrideLibraryPath != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(m_OverrideLibraryPath);
			}
			if (value == null)
			{
				m_OverrideLibraryPath = IntPtr.Zero;
				return;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			int num = bytes.Length;
			m_OverrideLibraryPath = Marshal.AllocCoTaskMem(num + 1);
			Marshal.Copy(bytes, 0, m_OverrideLibraryPath, num);
			Marshal.WriteByte(m_OverrideLibraryPath, num, 0);
		}
	}

	public void Dispose()
	{
		if (m_OverrideLibraryPath != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(m_OverrideLibraryPath);
		}
	}
}
