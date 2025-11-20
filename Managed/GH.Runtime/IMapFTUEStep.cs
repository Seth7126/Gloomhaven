using Assets.Script.Misc;

public interface IMapFTUEStep
{
	EMapFTUEStep Step { get; }

	ICallbackPromise StartStep();

	void FinishStep();
}
