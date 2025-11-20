using System;

namespace PrefabAsyncInstance;

public interface IMainPrefab
{
	event Action OnCreate;

	event Action OnRemove;

	event Action OnDead;
}
