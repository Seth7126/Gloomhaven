using System.IO;
using System.Runtime.InteropServices;
using Apparance.Net;

public static class ArrayStreamingExtensions
{
	private const int copyBufferSize = 8192;

	private static byte[] copyBuffer = new byte[8192];

	private unsafe static void CopyBytes(BinaryReader stream, object array_object, int byte_count)
	{
		UnmanagedMemoryStream unmanagedMemoryStream = (UnmanagedMemoryStream)stream.BaseStream;
		UnmanagedMemoryStream unmanagedMemoryStream2 = new UnmanagedMemoryStream(unmanagedMemoryStream.PositionPointer, byte_count);
		GCHandle gCHandle = GCHandle.Alloc(array_object, GCHandleType.Pinned);
		UnmanagedMemoryStream unmanagedMemoryStream3 = new UnmanagedMemoryStream((byte*)gCHandle.AddrOfPinnedObject().ToPointer(), byte_count, byte_count, FileAccess.Write);
		CopyToNoAlloc(unmanagedMemoryStream2, unmanagedMemoryStream3);
		unmanagedMemoryStream.Position += byte_count;
		gCHandle.Free();
		unmanagedMemoryStream2.Dispose();
		unmanagedMemoryStream3.Dispose();
	}

	private static void CopyToNoAlloc(Stream from, Stream to)
	{
		int num = 0;
		do
		{
			num = from.Read(copyBuffer, 0, 8192);
			if (num > 0)
			{
				to.Write(copyBuffer, 0, num);
			}
		}
		while (num == 8192);
	}

	public static void Stream(this Vector2[] array, BinaryReader stream)
	{
		array.Stream(stream, array.Length);
	}

	public unsafe static void Stream(this Vector2[] array, BinaryReader stream, int num_elements)
	{
		CopyBytes(stream, array, num_elements * sizeof(Vector2));
	}

	public static void Stream(this Vector3[] array, BinaryReader stream)
	{
		array.Stream(stream, array.Length);
	}

	public unsafe static void Stream(this Vector3[] array, BinaryReader stream, int num_elements)
	{
		CopyBytes(stream, array, num_elements * sizeof(Vector3));
	}

	public static void Stream(this int[] array, BinaryReader stream)
	{
		array.Stream(stream, array.Length);
	}

	public static void Stream(this int[] array, BinaryReader stream, int num_elements)
	{
		CopyBytes(stream, array, num_elements * 4);
	}

	public static void Stream(this uint[] array, BinaryReader stream)
	{
		array.Stream(stream, array.Length);
	}

	public static void Stream(this uint[] array, BinaryReader stream, int num_elements)
	{
		CopyBytes(stream, array, num_elements * 4);
	}
}
