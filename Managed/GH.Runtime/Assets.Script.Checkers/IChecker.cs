using System.Threading.Tasks;

namespace Assets.Script.Checkers;

public interface IChecker
{
	CheckerType Type { get; }

	Task<bool> IsOk();
}
