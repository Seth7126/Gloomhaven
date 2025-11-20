using UnityEngine;

public abstract class CustomOptionEntryEnableStrategy : MonoBehaviour, IOptionEntryStrategy
{
	public abstract void Enable(UIOptionEntry uiOptionEntry, bool enable);
}
