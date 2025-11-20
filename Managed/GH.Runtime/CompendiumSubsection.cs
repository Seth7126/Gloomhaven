using System;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;

[Serializable]
public class CompendiumSubsection : MonoBehaviour
{
	[HideInInspector]
	public string Title;

	[HideInInspector]
	public Guid Id = Guid.NewGuid();

	[SerializeField]
	public ReferenceToSprite ReferenceToScreenshot;

	public GameObject Content;

	public bool IsEmpty()
	{
		if (Content == null)
		{
			return Title.IsNullOrEmpty();
		}
		return false;
	}

	public void SetTitleLocKeyFromContent()
	{
		if (!(Content != null))
		{
			return;
		}
		TextMeshProUGUI[] componentsInChildren = Content.GetComponentsInChildren<TextMeshProUGUI>();
		int num = 0;
		if (num < componentsInChildren.Length)
		{
			TextMeshProUGUI textMeshProUGUI = componentsInChildren[num];
			if (textMeshProUGUI.name.StartsWith("Title = "))
			{
				Title = textMeshProUGUI.name.Replace("Title = ", "");
			}
		}
	}
}
