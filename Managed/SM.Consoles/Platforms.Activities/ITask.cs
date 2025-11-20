namespace Platforms.Activities;

public interface ITask : IActivityBase
{
	ISubTask[] SubTasks { get; }
}
