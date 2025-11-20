namespace Platforms.Activities;

public interface IActivity : IActivityBase
{
	ITask[] Tasks { get; }
}
