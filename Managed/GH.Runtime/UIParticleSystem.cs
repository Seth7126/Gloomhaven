using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIParticleSystem : UIBehaviour
{
	[SerializeField]
	private List<ParticleSystem> particles;

	[SerializeField]
	private string alphaMaterialProperty = "_UIAlphaChannel";

	[SerializeField]
	private bool ignoreCanvasGroupsAlpha;

	[SerializeField]
	private bool stopParticlesWhenInvisible;

	private Canvas m_Canvas;

	private readonly List<CanvasGroup> m_CanvasGroupCache = new List<CanvasGroup>();

	private Renderer[] m_ParticleRenderers;

	protected override void Awake()
	{
		base.Awake();
		m_ParticleRenderers = new Renderer[particles.Count];
		for (int i = 0; i < particles.Count; i++)
		{
			if (particles[i] != null)
			{
				Renderer component = particles[i].GetComponent<Renderer>();
				m_ParticleRenderers[i] = component;
				component.material = new Material(component.material);
			}
		}
	}

	protected override void OnCanvasHierarchyChanged()
	{
		base.OnCanvasHierarchyChanged();
		if (base.isActiveAndEnabled && !ignoreCanvasGroupsAlpha)
		{
			RefreshAlpha();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		CalculateCanvas();
		RefreshAlpha();
	}

	protected override void OnCanvasGroupChanged()
	{
		base.OnCanvasGroupChanged();
		if (base.isActiveAndEnabled)
		{
			RefreshAlpha();
		}
	}

	private void RefreshAlpha()
	{
		if (m_Canvas == null)
		{
			CalculateCanvas();
		}
		if (m_Canvas == null || !m_Canvas.isActiveAndEnabled)
		{
			UpdateAlpha(0f);
			return;
		}
		if (ignoreCanvasGroupsAlpha)
		{
			UpdateAlpha(1f);
			return;
		}
		float num = 1f;
		Transform parent = base.transform;
		while (parent != null)
		{
			parent.GetComponents(m_CanvasGroupCache);
			for (int i = 0; i < m_CanvasGroupCache.Count; i++)
			{
				num *= m_CanvasGroupCache[i].alpha;
				if (m_CanvasGroupCache[i].ignoreParentGroups)
				{
					break;
				}
			}
			parent = parent.parent;
		}
		UpdateAlpha(num);
	}

	private void UpdateAlpha(float alpha)
	{
		for (int i = 0; i < particles.Count; i++)
		{
			if (particles[i] == null || !particles[i].gameObject.activeSelf)
			{
				continue;
			}
			m_ParticleRenderers[i].material.SetFloat(alphaMaterialProperty, alpha);
			if (stopParticlesWhenInvisible)
			{
				if (alpha == 0f)
				{
					particles[i].Stop(withChildren: true);
				}
				else if (particles[i].main.loop && particles[i].isStopped)
				{
					particles[i].Play(withChildren: true);
				}
			}
		}
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		if (base.isActiveAndEnabled)
		{
			CalculateCanvas();
			RefreshAlpha();
		}
	}

	private void CalculateCanvas()
	{
		m_Canvas = GetComponentInParent<Canvas>();
	}
}
