using System.Collections.Generic;
using EPOOutline;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using UnityEngine;

[RequireComponent(typeof(Outlinable))]
public class OutlineWrapper : MonoBehaviour
{
	public bool IsObject = true;

	private CActor m_ActorAttachedToObject;

	[ConditionalField("IsObject", null, true)]
	public ScenarioManager.ObjectImportType ObjectImportType = ScenarioManager.ObjectImportType.None;

	private Outlinable m_Outlinable;

	private void Awake()
	{
		m_Outlinable = GetComponent<Outlinable>();
	}

	private void OnEnable()
	{
		if (!(m_Outlinable != null))
		{
			return;
		}
		if (IsObject)
		{
			if (m_ActorAttachedToObject == null)
			{
				m_ActorAttachedToObject = GetComponentInParent<UnityGameEditorObject>()?.PropObject?.RuntimeAttachedActor;
			}
			if (m_ActorAttachedToObject == null)
			{
				m_Outlinable.OutlineParameters.Color = UIInfoTools.GetPropOutlineColor(ObjectImportType);
			}
			else
			{
				m_Outlinable.OutlineParameters.Color = UIInfoTools.GetCharacterOutlineColor(m_ActorAttachedToObject.OriginalType);
				WorldspaceUITools.Instance.RegisterActorOutlinable(m_ActorAttachedToObject, new List<Outlinable> { m_Outlinable });
			}
		}
		WorldspaceUITools.Instance.AddOutlinableToList(m_Outlinable);
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		if (m_Outlinable != null && WorldspaceUITools.Instance != null)
		{
			WorldspaceUITools.Instance.RemoveOutlinableFromList(m_Outlinable);
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (m_Outlinable != null && WorldspaceUITools.Instance != null)
		{
			WorldspaceUITools.Instance.RemoveOutlinableFromList(m_Outlinable);
		}
	}
}
