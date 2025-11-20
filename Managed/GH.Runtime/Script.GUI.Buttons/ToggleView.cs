using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.Buttons;

public class ToggleView : MonoBehaviour
{
	[SerializeField]
	private Image _hoverImage;

	[SerializeField]
	private Image _tick;

	[SerializeField]
	private Image _selectedImage;

	[SerializeField]
	private bool _isHovered;

	[SerializeField]
	private bool _isSelected;

	private void OnEnable()
	{
		UpdateView();
	}

	public void UpdateView()
	{
		ToggleHovered(_isHovered);
		ToggleSelected(_isSelected);
	}

	public void Hover(bool hovered)
	{
		_isHovered = true;
		ToggleHovered(hovered);
	}

	public void Select(bool selected)
	{
		ToggleSelected(selected);
	}

	private void ToggleHovered(bool isActive)
	{
		if (_hoverImage != null)
		{
			_hoverImage.enabled = isActive;
		}
	}

	private void ToggleSelected(bool isActive)
	{
		if (_selectedImage != null)
		{
			_selectedImage.enabled = isActive;
		}
		if (_tick != null)
		{
			_tick.enabled = isActive;
		}
	}
}
