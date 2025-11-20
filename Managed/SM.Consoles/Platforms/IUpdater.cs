using System;

namespace Platforms;

public interface IUpdater
{
	void SubscribeForUpdate(Action onUpdate);

	void UnsubscribeFromUpdate(Action onUpdate);

	void SubscribeForFixedUpdate(Action onUpdate);

	void UnsubscribeFromFixedUpdate(Action onUpdate);

	void SubscribeForCooldown(Action onUpdate, float сooldownInSeconds);

	void UnsubscribeFromCooldown(Action onUpdate);

	void ExecuteInMainThread(Action onUpdate);
}
