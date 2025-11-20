using System.Threading.Tasks;

namespace Assets.Script.Checkers;

public class SaveDataHasEnoughSpaceChecker : IChecker
{
	public CheckerType Type => CheckerType.SaveDataHasEnoughSpace;

	public Task<bool> IsOk()
	{
		return SaveData.Instance.IsEnoughSpace();
	}
}
