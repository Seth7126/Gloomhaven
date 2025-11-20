using System;

namespace AsmodeeNet.Utils;

public class LCGRandomGenerator : RandomGenerator
{
	public LCGRandomGenerator()
	{
		ulong num = (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		_seed = (uint)(num & 0xFFFFFFFFu);
	}

	public LCGRandomGenerator(uint seed)
	{
		_seed = seed;
	}

	public override uint Rand()
	{
		ulong num = (ulong)(1664525L * (long)_seed + 1013904223);
		_seed = (uint)(num & 0xFFFFFFFFu);
		return _seed;
	}

	public override uint Range(uint min, uint max)
	{
		uint num = (uint)Math.Abs(max - min);
		return Rand() % num + min;
	}
}
