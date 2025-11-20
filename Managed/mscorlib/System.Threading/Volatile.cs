using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace System.Threading;

/// <summary>Contains methods for performing volatile memory operations.</summary>
public static class Volatile
{
	private struct VolatileBoolean
	{
		public volatile bool Value;
	}

	private struct VolatileByte
	{
		public volatile byte Value;
	}

	private struct VolatileInt16
	{
		public volatile short Value;
	}

	private struct VolatileInt32
	{
		public volatile int Value;
	}

	private struct VolatileIntPtr
	{
		public volatile IntPtr Value;
	}

	private struct VolatileSByte
	{
		public volatile sbyte Value;
	}

	private struct VolatileSingle
	{
		public volatile float Value;
	}

	private struct VolatileUInt16
	{
		public volatile ushort Value;
	}

	private struct VolatileUInt32
	{
		public volatile uint Value;
	}

	private struct VolatileUIntPtr
	{
		public volatile UIntPtr Value;
	}

	private struct VolatileObject
	{
		public volatile object Value;
	}

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method. </summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[Intrinsic]
	public static bool Read(ref bool location)
	{
		return Unsafe.As<bool, VolatileBoolean>(ref location).Value;
	}

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer. </param>
	[Intrinsic]
	public static void Write(ref bool location, bool value)
	{
		Unsafe.As<bool, VolatileBoolean>(ref location).Value = value;
	}

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[Intrinsic]
	public static byte Read(ref byte location)
	{
		return Unsafe.As<byte, VolatileByte>(ref location).Value;
	}

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[Intrinsic]
	public static void Write(ref byte location, byte value)
	{
		Unsafe.As<byte, VolatileByte>(ref location).Value = value;
	}

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[Intrinsic]
	public static short Read(ref short location)
	{
		return Unsafe.As<short, VolatileInt16>(ref location).Value;
	}

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[Intrinsic]
	public static void Write(ref short location, short value)
	{
		Unsafe.As<short, VolatileInt16>(ref location).Value = value;
	}

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[Intrinsic]
	public static int Read(ref int location)
	{
		return Unsafe.As<int, VolatileInt32>(ref location).Value;
	}

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[Intrinsic]
	public static void Write(ref int location, int value)
	{
		Unsafe.As<int, VolatileInt32>(ref location).Value = value;
	}

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[Intrinsic]
	public static IntPtr Read(ref IntPtr location)
	{
		return Unsafe.As<IntPtr, VolatileIntPtr>(ref location).Value;
	}

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[Intrinsic]
	public static void Write(ref IntPtr location, IntPtr value)
	{
		Unsafe.As<IntPtr, VolatileIntPtr>(ref location).Value = value;
	}

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[Intrinsic]
	[CLSCompliant(false)]
	public static sbyte Read(ref sbyte location)
	{
		return Unsafe.As<sbyte, VolatileSByte>(ref location).Value;
	}

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[CLSCompliant(false)]
	[Intrinsic]
	public static void Write(ref sbyte location, sbyte value)
	{
		Unsafe.As<sbyte, VolatileSByte>(ref location).Value = value;
	}

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[Intrinsic]
	public static float Read(ref float location)
	{
		return Unsafe.As<float, VolatileSingle>(ref location).Value;
	}

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[Intrinsic]
	public static void Write(ref float location, float value)
	{
		Unsafe.As<float, VolatileSingle>(ref location).Value = value;
	}

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache.</returns>
	/// <param name="location">The field to read.</param>
	[CLSCompliant(false)]
	[Intrinsic]
	public static ushort Read(ref ushort location)
	{
		return Unsafe.As<ushort, VolatileUInt16>(ref location).Value;
	}

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[CLSCompliant(false)]
	[Intrinsic]
	public static void Write(ref ushort location, ushort value)
	{
		Unsafe.As<ushort, VolatileUInt16>(ref location).Value = value;
	}

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache.</returns>
	/// <param name="location">The field to read.</param>
	[CLSCompliant(false)]
	[Intrinsic]
	public static uint Read(ref uint location)
	{
		return Unsafe.As<uint, VolatileUInt32>(ref location).Value;
	}

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[CLSCompliant(false)]
	[Intrinsic]
	public static void Write(ref uint location, uint value)
	{
		Unsafe.As<uint, VolatileUInt32>(ref location).Value = value;
	}

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[CLSCompliant(false)]
	[Intrinsic]
	public static UIntPtr Read(ref UIntPtr location)
	{
		return Unsafe.As<UIntPtr, VolatileUIntPtr>(ref location).Value;
	}

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[CLSCompliant(false)]
	[Intrinsic]
	public static void Write(ref UIntPtr location, UIntPtr value)
	{
		Unsafe.As<UIntPtr, VolatileUIntPtr>(ref location).Value = value;
	}

	/// <summary>Reads the object reference from the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The reference to <paramref name="T" /> that was read. This reference is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	/// <typeparam name="T">The type of field to read. This must be a reference type, not a value type.</typeparam>
	[Intrinsic]
	public static T Read<T>(ref T location) where T : class
	{
		return Unsafe.As<T>(Unsafe.As<T, VolatileObject>(ref location).Value);
	}

	/// <summary>Writes the specified object reference to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method. </summary>
	/// <param name="location">The field where the object reference is written.</param>
	/// <param name="value">The object reference to write. The reference is written immediately so that it is visible to all processors in the computer.</param>
	/// <typeparam name="T">The type of field to write. This must be a reference type, not a value type. </typeparam>
	[Intrinsic]
	public static void Write<T>(ref T location, T value) where T : class
	{
		Unsafe.As<T, VolatileObject>(ref location).Value = value;
	}

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern long Read(ref long location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[CLSCompliant(false)]
	public static extern ulong Read(ref ulong location);

	/// <summary>Reads the value of the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.</summary>
	/// <returns>The value that was read. This value is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
	/// <param name="location">The field to read.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern double Read(ref double location);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a memory operation appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref long location, long value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[CLSCompliant(false)]
	public static extern void Write(ref ulong location, ulong value);

	/// <summary>Writes the specified value to the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears before this method in the code, the processor cannot move it after this method.</summary>
	/// <param name="location">The field where the value is written.</param>
	/// <param name="value">The value to write. The value is written immediately so that it is visible to all processors in the computer.</param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern void Write(ref double location, double value);
}
