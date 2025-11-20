using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackModifier : MonoBehaviour
{
	[SerializeField]
	private Image mainImage;

	[SerializeField]
	private Image additionalImage;

	[SerializeField]
	private TextMeshProUGUI numberText;

	public void Init(string modifier, bool isAvailable)
	{
		mainImage.sprite = UIInfoTools.Instance.GetAttackModifierSprite(modifier);
		mainImage.color = new Color(1f, 1f, 1f, isAvailable ? 1f : (10f / 51f));
	}

	public void DisplayAnimation(Vector3 fromPosition, Vector3 toPosition, float moveTime)
	{
		base.transform.position = fromPosition;
		LeanTween.move(base.gameObject, toPosition, moveTime).setEase(LeanTweenType.easeOutExpo);
	}
}
