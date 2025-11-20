namespace Code.State;

public static class StateFilterUtils
{
	public static IStateFilter InverseFilter(this IStateFilter filter)
	{
		return new InverseStateFilter(filter);
	}
}
