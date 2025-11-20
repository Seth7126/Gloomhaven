using UnityEngine;

public class RFX4_EffectEvent : MonoBehaviour
{
	public GameObject CharacterEffect;

	public Transform CharacterAttachPoint;

	public GameObject CharacterEffect2;

	public Transform CharacterAttachPoint2;

	public GameObject Effect;

	public Transform AttachPoint;

	public GameObject AdditionalEffect;

	public Transform AdditionalEffectAttachPoint;

	private void OnEnable()
	{
		if (Effect != null)
		{
			Effect.SetActive(value: false);
		}
		if (AdditionalEffect != null)
		{
			AdditionalEffect.SetActive(value: false);
		}
		if (CharacterEffect != null)
		{
			CharacterEffect.SetActive(value: false);
		}
		if (CharacterEffect2 != null)
		{
			CharacterEffect2.SetActive(value: false);
		}
	}

	public void ActivateEffect()
	{
		if (!(Effect == null))
		{
			Effect.SetActive(value: true);
		}
	}

	public void ActivateAdditionalEffect()
	{
		if (!(AdditionalEffect == null))
		{
			AdditionalEffect.SetActive(value: true);
		}
	}

	public void ActivateCharacterEffect()
	{
		if (!(CharacterEffect == null))
		{
			CharacterEffect.SetActive(value: true);
		}
	}

	public void ActivateCharacterEffect2()
	{
		if (!(CharacterEffect2 == null))
		{
			CharacterEffect2.SetActive(value: true);
		}
	}

	private void LateUpdate()
	{
		if (Effect != null && AttachPoint != null)
		{
			Effect.transform.position = AttachPoint.position;
		}
		if (AdditionalEffect != null && AdditionalEffectAttachPoint != null)
		{
			AdditionalEffect.transform.position = AdditionalEffectAttachPoint.position;
		}
		if (CharacterEffect != null && CharacterAttachPoint != null)
		{
			CharacterEffect.transform.position = CharacterAttachPoint.position;
		}
		if (CharacterEffect2 != null && CharacterAttachPoint2 != null)
		{
			CharacterEffect2.transform.position = CharacterAttachPoint2.position;
		}
	}
}
