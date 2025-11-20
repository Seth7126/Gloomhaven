using System;

namespace Code.State;

public class StateFilterByType : IStateFilter
{
	private Type[] _types;

	public StateFilterByType(params Type[] types)
	{
		_types = types;
	}

	public bool IsValid(IState state)
	{
		for (int i = 0; i < _types.Length; i++)
		{
			if (state.GetType() == _types[i])
			{
				return true;
			}
		}
		return false;
	}
}
