using System;

namespace AsmodeeNet.Utils;

public class SystemRandomGenerator : RandomGenerator
{
	private Random _random;

	public SystemRandomGenerator()
	{
		_random = new Random();
	}

	public SystemRandomGenerator(uint seed)
	{
		base.Seed = seed;
	}

	public override uint Rand()
	{
		_seed = (uint)_random.Next();
		return _seed;
	}

	public override uint Range(uint min, uint max)
	{
		_seed = (uint)_random.Next((int)min, (int)max);
		return _seed;
	}

	protected override void SeedUpdated()
	{
		_random = new Random((int)_seed);
	}
}
