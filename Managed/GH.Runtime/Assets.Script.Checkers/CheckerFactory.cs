using SM.Utils;

namespace Assets.Script.Checkers;

public static class CheckerFactory
{
	public static IChecker Create(CheckerType checkerType)
	{
		switch (checkerType)
		{
		case CheckerType.SaveDataHasEnoughSpace:
			return new SaveDataHasEnoughSpaceChecker();
		case CheckerType.AlwaysOk:
			return new AlwaysOkChecker();
		case CheckerType.NeverOk:
			return new NeverOkChecker();
		default:
			LogUtils.LogError(string.Format("There is no implementation for {0}. {1} was returned", checkerType, "AlwaysOkChecker"));
			return new AlwaysOkChecker();
		}
	}
}
