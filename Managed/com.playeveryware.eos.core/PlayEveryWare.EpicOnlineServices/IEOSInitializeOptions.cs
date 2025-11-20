using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Platform;

namespace PlayEveryWare.EpicOnlineServices;

public interface IEOSInitializeOptions
{
	IntPtr AllocateMemoryFunction { get; set; }

	IntPtr ReallocateMemoryFunction { get; set; }

	IntPtr ReleaseMemoryFunction { get; set; }

	Utf8String ProductName { get; set; }

	Utf8String ProductVersion { get; set; }

	InitializeThreadAffinity? OverrideThreadAffinity { get; set; }
}
