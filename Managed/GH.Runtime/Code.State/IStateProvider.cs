using System;

namespace Code.State;

public interface IStateProvider
{
	TTag GetLatestState<TTag>(params TTag[] tags) where TTag : Enum;
}
