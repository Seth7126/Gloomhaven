namespace UnityEngine.UIElements;

internal interface IGroupManager
{
	IGroupBoxOption GetSelectedOption();

	void OnOptionSelectionChanged(IGroupBoxOption selectedOption);

	void RegisterOption(IGroupBoxOption option);

	void UnregisterOption(IGroupBoxOption option);
}
