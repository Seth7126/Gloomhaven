#define DEBUG
using System;
using Photon.Bolt.Exceptions;

namespace Photon.Bolt;

public class BitSet : BoltObject
{
	public const int BITSET_LONGS = 16;

	internal static readonly BitSet Full;

	private ulong Bits0;

	private ulong Bits1;

	private ulong Bits2;

	private ulong Bits3;

	private ulong Bits4;

	private ulong Bits5;

	private ulong Bits6;

	private ulong Bits7;

	private ulong Bits8;

	private ulong Bits9;

	private ulong Bits10;

	private ulong Bits11;

	private ulong Bits12;

	private ulong Bits13;

	private ulong Bits14;

	private ulong Bits15;

	public bool IsZero => Bits0 == 0L && Bits1 == 0L && Bits2 == 0L && Bits3 == 0L && Bits4 == 0L && Bits5 == 0L && Bits6 == 0L && Bits7 == 0L && Bits8 == 0L && Bits9 == 0L && Bits10 == 0L && Bits11 == 0L && Bits12 == 0L && Bits13 == 0L && Bits14 == 0L && Bits15 == 0;

	public ulong this[int index]
	{
		get
		{
			return index switch
			{
				0 => Bits0, 
				1 => Bits1, 
				2 => Bits2, 
				3 => Bits3, 
				4 => Bits4, 
				5 => Bits5, 
				6 => Bits6, 
				7 => Bits7, 
				8 => Bits8, 
				9 => Bits9, 
				10 => Bits10, 
				11 => Bits11, 
				12 => Bits12, 
				13 => Bits13, 
				14 => Bits14, 
				15 => Bits15, 
				_ => throw new IndexOutOfRangeException(), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				Bits0 = value;
				break;
			case 1:
				Bits1 = value;
				break;
			case 2:
				Bits2 = value;
				break;
			case 3:
				Bits3 = value;
				break;
			case 4:
				Bits4 = value;
				break;
			case 5:
				Bits5 = value;
				break;
			case 6:
				Bits6 = value;
				break;
			case 7:
				Bits7 = value;
				break;
			case 8:
				Bits8 = value;
				break;
			case 9:
				Bits9 = value;
				break;
			case 10:
				Bits10 = value;
				break;
			case 11:
				Bits11 = value;
				break;
			case 12:
				Bits12 = value;
				break;
			case 13:
				Bits13 = value;
				break;
			case 14:
				Bits14 = value;
				break;
			case 15:
				Bits15 = value;
				break;
			default:
				throw new IndexOutOfRangeException();
			}
		}
	}

	static BitSet()
	{
		Full = new BitSet();
		Full.Bits0 |= ulong.MaxValue;
		Full.Bits1 |= ulong.MaxValue;
		Full.Bits2 |= ulong.MaxValue;
		Full.Bits3 |= ulong.MaxValue;
		Full.Bits4 |= ulong.MaxValue;
		Full.Bits5 |= ulong.MaxValue;
		Full.Bits6 |= ulong.MaxValue;
		Full.Bits7 |= ulong.MaxValue;
		Full.Bits8 |= ulong.MaxValue;
		Full.Bits9 |= ulong.MaxValue;
		Full.Bits10 |= ulong.MaxValue;
		Full.Bits11 |= ulong.MaxValue;
		Full.Bits12 |= ulong.MaxValue;
		Full.Bits13 |= ulong.MaxValue;
		Full.Bits14 |= ulong.MaxValue;
		Full.Bits15 |= ulong.MaxValue;
	}

	public BitSet(ulong bits0, ulong bits1, ulong bits2, ulong bits3, ulong bits4, ulong bits5, ulong bits6, ulong bits7, ulong bits8, ulong bits9, ulong bits10, ulong bits11, ulong bits12, ulong bits13, ulong bits14, ulong bits15)
	{
		Bits0 = bits0;
		Bits1 = bits1;
		Bits2 = bits2;
		Bits3 = bits3;
		Bits4 = bits4;
		Bits5 = bits5;
		Bits6 = bits6;
		Bits7 = bits7;
		Bits8 = bits8;
		Bits9 = bits9;
		Bits10 = bits10;
		Bits11 = bits11;
		Bits12 = bits12;
		Bits13 = bits13;
		Bits14 = bits14;
		Bits15 = bits15;
	}

	public BitSet()
	{
	}

	public void Set(int bit)
	{
		switch (bit / 64)
		{
		case 0:
			Bits0 |= (ulong)(1L << bit % 64);
			break;
		case 1:
			Bits1 |= (ulong)(1L << bit % 64);
			break;
		case 2:
			Bits2 |= (ulong)(1L << bit % 64);
			break;
		case 3:
			Bits3 |= (ulong)(1L << bit % 64);
			break;
		case 4:
			Bits4 |= (ulong)(1L << bit % 64);
			break;
		case 5:
			Bits5 |= (ulong)(1L << bit % 64);
			break;
		case 6:
			Bits6 |= (ulong)(1L << bit % 64);
			break;
		case 7:
			Bits7 |= (ulong)(1L << bit % 64);
			break;
		case 8:
			Bits8 |= (ulong)(1L << bit % 64);
			break;
		case 9:
			Bits9 |= (ulong)(1L << bit % 64);
			break;
		case 10:
			Bits10 |= (ulong)(1L << bit % 64);
			break;
		case 11:
			Bits11 |= (ulong)(1L << bit % 64);
			break;
		case 12:
			Bits12 |= (ulong)(1L << bit % 64);
			break;
		case 13:
			Bits13 |= (ulong)(1L << bit % 64);
			break;
		case 14:
			Bits14 |= (ulong)(1L << bit % 64);
			break;
		case 15:
			Bits15 |= (ulong)(1L << bit % 64);
			break;
		default:
			throw new IndexOutOfRangeException();
		}
		Assert.False(IsZero);
	}

	public void Clear(int bit)
	{
		switch (bit / 64)
		{
		case 0:
			Bits0 &= (ulong)(~(1L << bit % 64));
			break;
		case 1:
			Bits1 &= (ulong)(~(1L << bit % 64));
			break;
		case 2:
			Bits2 &= (ulong)(~(1L << bit % 64));
			break;
		case 3:
			Bits3 &= (ulong)(~(1L << bit % 64));
			break;
		case 4:
			Bits4 &= (ulong)(~(1L << bit % 64));
			break;
		case 5:
			Bits5 &= (ulong)(~(1L << bit % 64));
			break;
		case 6:
			Bits6 &= (ulong)(~(1L << bit % 64));
			break;
		case 7:
			Bits7 &= (ulong)(~(1L << bit % 64));
			break;
		case 8:
			Bits8 &= (ulong)(~(1L << bit % 64));
			break;
		case 9:
			Bits9 &= (ulong)(~(1L << bit % 64));
			break;
		case 10:
			Bits10 &= (ulong)(~(1L << bit % 64));
			break;
		case 11:
			Bits11 &= (ulong)(~(1L << bit % 64));
			break;
		case 12:
			Bits12 &= (ulong)(~(1L << bit % 64));
			break;
		case 13:
			Bits13 &= (ulong)(~(1L << bit % 64));
			break;
		case 14:
			Bits14 &= (ulong)(~(1L << bit % 64));
			break;
		case 15:
			Bits15 &= (ulong)(~(1L << bit % 64));
			break;
		default:
			throw new IndexOutOfRangeException();
		}
	}

	public void Combine(BitSet other)
	{
		Bits0 |= other.Bits0;
		Bits1 |= other.Bits1;
		Bits2 |= other.Bits2;
		Bits3 |= other.Bits3;
		Bits4 |= other.Bits4;
		Bits5 |= other.Bits5;
		Bits6 |= other.Bits6;
		Bits7 |= other.Bits7;
		Bits8 |= other.Bits8;
		Bits9 |= other.Bits9;
		Bits10 |= other.Bits10;
		Bits11 |= other.Bits11;
		Bits12 |= other.Bits12;
		Bits13 |= other.Bits13;
		Bits14 |= other.Bits14;
		Bits15 |= other.Bits15;
	}

	public void ClearAll()
	{
		Bits0 = 0uL;
		Bits1 = 0uL;
		Bits2 = 0uL;
		Bits3 = 0uL;
		Bits4 = 0uL;
		Bits5 = 0uL;
		Bits6 = 0uL;
		Bits7 = 0uL;
		Bits8 = 0uL;
		Bits9 = 0uL;
		Bits10 = 0uL;
		Bits11 = 0uL;
		Bits12 = 0uL;
		Bits13 = 0uL;
		Bits14 = 0uL;
		Bits15 = 0uL;
	}

	public bool IsSet(int bit)
	{
		ulong num = (ulong)(1L << bit % 64);
		return (bit / 64) switch
		{
			0 => (Bits0 & num) == num, 
			1 => (Bits1 & num) == num, 
			2 => (Bits2 & num) == num, 
			3 => (Bits3 & num) == num, 
			4 => (Bits4 & num) == num, 
			5 => (Bits5 & num) == num, 
			6 => (Bits6 & num) == num, 
			7 => (Bits7 & num) == num, 
			8 => (Bits8 & num) == num, 
			9 => (Bits9 & num) == num, 
			10 => (Bits10 & num) == num, 
			11 => (Bits11 & num) == num, 
			12 => (Bits12 & num) == num, 
			13 => (Bits13 & num) == num, 
			14 => (Bits14 & num) == num, 
			15 => (Bits15 & num) == num, 
			_ => throw new IndexOutOfRangeException("The max properties per state is " + 1024), 
		};
	}
}
