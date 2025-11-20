namespace XUnity.AutoTranslator.Plugin.Core;

internal class KeyAnd<T>
{
	public UntranslatedText Key { get; set; }

	public T Item { get; set; }

	public KeyAnd(UntranslatedText key, T item)
	{
		Key = key;
		Item = item;
	}
}
