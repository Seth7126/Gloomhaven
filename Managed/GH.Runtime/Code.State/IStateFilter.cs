namespace Code.State;

public interface IStateFilter
{
	bool IsValid(IState state);
}
