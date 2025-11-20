using UnityEngine;

public abstract class MonoDisableProviderBase : MonoBehaviour, IDetailDisablerProvider
{
	public abstract void StartDisable();
}
