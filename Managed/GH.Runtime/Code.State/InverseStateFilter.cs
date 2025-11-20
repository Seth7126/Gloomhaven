namespace Code.State;

public class InverseStateFilter : IStateFilter
{
	private readonly IStateFilter _filter;

	public InverseStateFilter(IStateFilter filter)
	{
		_filter = filter;
	}

	public bool IsValid(IState state)
	{
		return !_filter.IsValid(state);
	}
}
