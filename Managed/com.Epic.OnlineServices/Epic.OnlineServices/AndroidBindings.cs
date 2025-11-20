using System.Runtime.InteropServices;
using Epic.OnlineServices.Platform;

namespace Epic.OnlineServices;

public static class AndroidBindings
{
	[DllImport("EOSSDK-Win64-Shipping")]
	internal static extern Result EOS_Initialize(ref AndroidInitializeOptionsInternal options);
}
