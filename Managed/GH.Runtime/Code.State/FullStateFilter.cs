namespace Code.State;

public class FullStateFilter : IStateFilter
{
	public bool IsValid(IState state)
	{
		return false;
	}
}
