public interface IContentUpdateMonitor
{
	void NotifyContentPlacementStarted();

	void NotifyContentPlacementComplete();

	void NotifyContentRemovalStarted();

	void NotifyContentRemovalComplete();
}
