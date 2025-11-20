using System.Threading.Tasks;

namespace Assets.Script.Checkers;

public class AlwaysOkChecker : IChecker
{
	public CheckerType Type => CheckerType.AlwaysOk;

	public Task<bool> IsOk()
	{
		return Task.FromResult(result: true);
	}
}
