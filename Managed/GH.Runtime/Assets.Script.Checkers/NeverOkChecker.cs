using System.Threading.Tasks;

namespace Assets.Script.Checkers;

public class NeverOkChecker : IChecker
{
	public CheckerType Type => CheckerType.NeverOk;

	public Task<bool> IsOk()
	{
		return Task.FromResult(result: false);
	}
}
