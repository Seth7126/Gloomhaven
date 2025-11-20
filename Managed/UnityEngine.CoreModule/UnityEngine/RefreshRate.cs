using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[NativeType("Runtime/Graphics/RefreshRate.h")]
public struct RefreshRate : IEquatable<RefreshRate>
{
	[RequiredMember]
	public uint numerator;

	[RequiredMember]
	public uint denominator;

	public double value
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (double)numerator / (double)denominator;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(RefreshRate other)
	{
		return numerator == other.numerator && denominator == other.denominator;
	}
}
