using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using Unity;

namespace System.Threading;

/// <summary>Provides atomic operations for variables that are shared by multiple threads. </summary>
/// <filterpriority>2</filterpriority>
public static class Interlocked
{
	/// <summary>Compares two 32-bit signed integers for equality and, if they are equal, replaces one of the values.</summary>
	/// <returns>The original value in <paramref name="location1" />.</returns>
	/// <param name="location1">The destination, whose value is compared with <paramref name="comparand" /> and possibly replaced. </param>
	/// <param name="value">The value that replaces the destination value if the comparison results in equality. </param>
	/// <param name="comparand">The value that is compared to the value at <paramref name="location1" />. </param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern int CompareExchange(ref int location1, int value, int comparand);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	internal static extern int CompareExchange(ref int location1, int value, int comparand, ref bool succeeded);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	private static extern void CompareExchange(ref object location1, ref object value, ref object comparand, ref object result);

	/// <summary>Compares two objects for reference equality and, if they are equal, replaces one of the objects.</summary>
	/// <returns>The original value in <paramref name="location1" />.</returns>
	/// <param name="location1">The destination object that is compared with <paramref name="comparand" /> and possibly replaced. </param>
	/// <param name="value">The object that replaces the destination object if the comparison results in equality. </param>
	/// <param name="comparand">The object that is compared to the object at <paramref name="location1" />. </param>
	/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static object CompareExchange(ref object location1, object value, object comparand)
	{
		object result = null;
		CompareExchange(ref location1, ref value, ref comparand, ref result);
		return result;
	}

	/// <summary>Compares two single-precision floating point numbers for equality and, if they are equal, replaces one of the values.</summary>
	/// <returns>The original value in <paramref name="location1" />.</returns>
	/// <param name="location1">The destination, whose value is compared with <paramref name="comparand" /> and possibly replaced. </param>
	/// <param name="value">The value that replaces the destination value if the comparison results in equality. </param>
	/// <param name="comparand">The value that is compared to the value at <paramref name="location1" />. </param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern float CompareExchange(ref float location1, float value, float comparand);

	/// <summary>Decrements a specified variable and stores the result, as an atomic operation.</summary>
	/// <returns>The decremented value.</returns>
	/// <param name="location">The variable whose value is to be decremented. </param>
	/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="location" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern int Decrement(ref int location);

	/// <summary>Decrements the specified variable and stores the result, as an atomic operation.</summary>
	/// <returns>The decremented value.</returns>
	/// <param name="location">The variable whose value is to be decremented. </param>
	/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="location" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern long Decrement(ref long location);

	/// <summary>Increments a specified variable and stores the result, as an atomic operation.</summary>
	/// <returns>The incremented value.</returns>
	/// <param name="location">The variable whose value is to be incremented. </param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern int Increment(ref int location);

	/// <summary>Increments a specified variable and stores the result, as an atomic operation.</summary>
	/// <returns>The incremented value.</returns>
	/// <param name="location">The variable whose value is to be incremented. </param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern long Increment(ref long location);

	/// <summary>Sets a 32-bit signed integer to a specified value and returns the original value, as an atomic operation.</summary>
	/// <returns>The original value of <paramref name="location1" />.</returns>
	/// <param name="location1">The variable to set to the specified value. </param>
	/// <param name="value">The value to which the <paramref name="location1" /> parameter is set. </param>
	/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern int Exchange(ref int location1, int value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	private static extern void Exchange(ref object location1, ref object value, ref object result);

	/// <summary>Sets an object to a specified value and returns a reference to the original object, as an atomic operation.</summary>
	/// <returns>The original value of <paramref name="location1" />.</returns>
	/// <param name="location1">The variable to set to the specified value. </param>
	/// <param name="value">The value to which the <paramref name="location1" /> parameter is set. </param>
	/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static object Exchange(ref object location1, object value)
	{
		object result = null;
		Exchange(ref location1, ref value, ref result);
		return result;
	}

	/// <summary>Sets a single-precision floating point number to a specified value and returns the original value, as an atomic operation.</summary>
	/// <returns>The original value of <paramref name="location1" />.</returns>
	/// <param name="location1">The variable to set to the specified value. </param>
	/// <param name="value">The value to which the <paramref name="location1" /> parameter is set. </param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern float Exchange(ref float location1, float value);

	/// <summary>Compares two 64-bit signed integers for equality and, if they are equal, replaces one of the values.</summary>
	/// <returns>The original value in <paramref name="location1" />.</returns>
	/// <param name="location1">The destination, whose value is compared with <paramref name="comparand" /> and possibly replaced. </param>
	/// <param name="value">The value that replaces the destination value if the comparison results in equality. </param>
	/// <param name="comparand">The value that is compared to the value at <paramref name="location1" />. </param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern long CompareExchange(ref long location1, long value, long comparand);

	/// <summary>Compares two platform-specific handles or pointers for equality and, if they are equal, replaces one of them.</summary>
	/// <returns>The original value in <paramref name="location1" />.</returns>
	/// <param name="location1">The destination <see cref="T:System.IntPtr" />, whose value is compared with the value of <paramref name="comparand" /> and possibly replaced by <paramref name="value" />. </param>
	/// <param name="value">The <see cref="T:System.IntPtr" /> that replaces the destination value if the comparison results in equality. </param>
	/// <param name="comparand">The <see cref="T:System.IntPtr" /> that is compared to the value at <paramref name="location1" />. </param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern IntPtr CompareExchange(ref IntPtr location1, IntPtr value, IntPtr comparand);

	/// <summary>Compares two double-precision floating point numbers for equality and, if they are equal, replaces one of the values.</summary>
	/// <returns>The original value in <paramref name="location1" />.</returns>
	/// <param name="location1">The destination, whose value is compared with <paramref name="comparand" /> and possibly replaced. </param>
	/// <param name="value">The value that replaces the destination value if the comparison results in equality. </param>
	/// <param name="comparand">The value that is compared to the value at <paramref name="location1" />. </param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern double CompareExchange(ref double location1, double value, double comparand);

	/// <summary>Compares two instances of the specified reference type <paramref name="T" /> for equality and, if they are equal, replaces one of them.</summary>
	/// <returns>The original value in <paramref name="location1" />.</returns>
	/// <param name="location1">The destination, whose value is compared with <paramref name="comparand" /> and possibly replaced. This is a reference parameter (ref in C#, ByRef in Visual Basic). </param>
	/// <param name="value">The value that replaces the destination value if the comparison results in equality. </param>
	/// <param name="comparand">The value that is compared to the value at <paramref name="location1" />. </param>
	/// <typeparam name="T">The type to be used for <paramref name="location1" />, <paramref name="value" />, and <paramref name="comparand" />. This type must be a reference type.</typeparam>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	[ComVisible(false)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[Intrinsic]
	public unsafe static T CompareExchange<T>(ref T location1, T value, T comparand) where T : class
	{
		if (Unsafe.AsPointer(ref location1) == null)
		{
			throw new NullReferenceException();
		}
		T source = null;
		CompareExchange(ref Unsafe.As<T, object>(ref location1), ref Unsafe.As<T, object>(ref value), ref Unsafe.As<T, object>(ref comparand), ref Unsafe.As<T, object>(ref source));
		return source;
	}

	/// <summary>Sets a 64-bit signed integer to a specified value and returns the original value, as an atomic operation.</summary>
	/// <returns>The original value of <paramref name="location1" />.</returns>
	/// <param name="location1">The variable to set to the specified value. </param>
	/// <param name="value">The value to which the <paramref name="location1" /> parameter is set. </param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern long Exchange(ref long location1, long value);

	/// <summary>Sets a platform-specific handle or pointer to a specified value and returns the original value, as an atomic operation.</summary>
	/// <returns>The original value of <paramref name="location1" />.</returns>
	/// <param name="location1">The variable to set to the specified value. </param>
	/// <param name="value">The value to which the <paramref name="location1" /> parameter is set. </param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern IntPtr Exchange(ref IntPtr location1, IntPtr value);

	/// <summary>Sets a double-precision floating point number to a specified value and returns the original value, as an atomic operation.</summary>
	/// <returns>The original value of <paramref name="location1" />.</returns>
	/// <param name="location1">The variable to set to the specified value. </param>
	/// <param name="value">The value to which the <paramref name="location1" /> parameter is set. </param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern double Exchange(ref double location1, double value);

	/// <summary>Sets a variable of the specified type <paramref name="T" /> to a specified value and returns the original value, as an atomic operation.</summary>
	/// <returns>The original value of <paramref name="location1" />.</returns>
	/// <param name="location1">The variable to set to the specified value. This is a reference parameter (ref in C#, ByRef in Visual Basic). </param>
	/// <param name="value">The value to which the <paramref name="location1" /> parameter is set. </param>
	/// <typeparam name="T">The type to be used for <paramref name="location1" /> and <paramref name="value" />. This type must be a reference type.</typeparam>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	[ComVisible(false)]
	[Intrinsic]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public unsafe static T Exchange<T>(ref T location1, T value) where T : class
	{
		if (Unsafe.AsPointer(ref location1) == null)
		{
			throw new NullReferenceException();
		}
		T source = null;
		Exchange(ref Unsafe.As<T, object>(ref location1), ref Unsafe.As<T, object>(ref value), ref Unsafe.As<T, object>(ref source));
		return source;
	}

	/// <summary>Returns a 64-bit value, loaded as an atomic operation.</summary>
	/// <returns>The loaded value.</returns>
	/// <param name="location">The 64-bit value to be loaded.</param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern long Read(ref long location);

	/// <summary>Adds two 32-bit integers and replaces the first integer with the sum, as an atomic operation.</summary>
	/// <returns>The new value stored at <paramref name="location1" />.</returns>
	/// <param name="location1">A variable containing the first value to be added. The sum of the two values is stored in <paramref name="location1" />.</param>
	/// <param name="value">The value to be added to the integer at <paramref name="location1" />.</param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern int Add(ref int location1, int value);

	/// <summary>Adds two 64-bit integers and replaces the first integer with the sum, as an atomic operation.</summary>
	/// <returns>The new value stored at <paramref name="location1" />.</returns>
	/// <param name="location1">A variable containing the first value to be added. The sum of the two values is stored in <paramref name="location1" />.</param>
	/// <param name="value">The value to be added to the integer at <paramref name="location1" />.</param>
	/// <exception cref="T:System.NullReferenceException">The address of <paramref name="location1" /> is a null pointer. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern long Add(ref long location1, long value);

	/// <summary>Synchronizes memory access as follows: The processor that executes the current thread cannot reorder instructions in such a way that memory accesses before the call to <see cref="M:System.Threading.Interlocked.MemoryBarrier" /> execute after memory accesses that follow the call to <see cref="M:System.Threading.Interlocked.MemoryBarrier" />.</summary>
	public static void MemoryBarrier()
	{
		Thread.MemoryBarrier();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void MemoryBarrierProcessWide();

	[SecuritySafeCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static void SpeculationBarrier()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
