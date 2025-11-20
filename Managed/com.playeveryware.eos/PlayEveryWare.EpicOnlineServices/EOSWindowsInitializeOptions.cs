using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Platform;

namespace PlayEveryWare.EpicOnlineServices;

public class EOSWindowsInitializeOptions : IEOSInitializeOptions
{
	public InitializeOptions options;

	public IntPtr AllocateMemoryFunction
	{
		get
		{
			return options.AllocateMemoryFunction;
		}
		set
		{
			options.AllocateMemoryFunction = value;
		}
	}

	public IntPtr ReallocateMemoryFunction
	{
		get
		{
			return options.ReallocateMemoryFunction;
		}
		set
		{
			options.ReallocateMemoryFunction = value;
		}
	}

	public IntPtr ReleaseMemoryFunction
	{
		get
		{
			return options.ReleaseMemoryFunction;
		}
		set
		{
			options.ReleaseMemoryFunction = value;
		}
	}

	public Utf8String ProductName
	{
		get
		{
			return options.ProductName;
		}
		set
		{
			options.ProductName = value;
		}
	}

	public Utf8String ProductVersion
	{
		get
		{
			return options.ProductVersion;
		}
		set
		{
			options.ProductVersion = value;
		}
	}

	public InitializeThreadAffinity? OverrideThreadAffinity
	{
		get
		{
			return options.OverrideThreadAffinity;
		}
		set
		{
			options.OverrideThreadAffinity = value;
		}
	}
}
