using System;
using System.Collections;
using System.Collections.Generic;

namespace SRDebugger;

public struct EntryCode : IReadOnlyList<int>, IEnumerable<int>, IEnumerable, IReadOnlyCollection<int>
{
	public readonly int Digit1;

	public readonly int Digit2;

	public readonly int Digit3;

	public readonly int Digit4;

	public int Count => 4;

	public int this[int index] => index switch
	{
		0 => Digit1, 
		1 => Digit2, 
		2 => Digit3, 
		3 => Digit4, 
		_ => throw new ArgumentOutOfRangeException("index"), 
	};

	public EntryCode(int digit1, int digit2, int digit3, int digit4)
	{
		if (digit1 < 0 || digit1 > 9)
		{
			throw new ArgumentException("Pin digit must be between 0 and 9", "digit1");
		}
		if (digit2 < 0 || digit2 > 9)
		{
			throw new ArgumentException("Pin digit must be between 0 and 9", "digit2");
		}
		if (digit3 < 0 || digit3 > 9)
		{
			throw new ArgumentException("Pin digit must be between 0 and 9", "digit3");
		}
		if (digit4 < 0 || digit4 > 9)
		{
			throw new ArgumentException("Pin digit must be between 0 and 9", "digit4");
		}
		Digit1 = digit1;
		Digit2 = digit2;
		Digit3 = digit3;
		Digit4 = digit4;
	}

	public IEnumerator<int> GetEnumerator()
	{
		return new List<int> { Digit1, Digit2, Digit3, Digit4 }.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
