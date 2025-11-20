using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class TilesOcclusionGenerator : MonoBehaviour
{
	public static TilesOcclusionGenerator s_Instance;

	public Shader m_OcclusionShader;

	public Shader m_OcclusionObjectShader;

	public Shader m_PixelCorrectorShader;

	public Shader m_BlurShader;

	public List<MeshRenderer> m_RoomRenderers = new List<MeshRenderer>();

	public List<MeshRenderer> m_ObjectRenderers = new List<MeshRenderer>();

	[HideInInspector]
	public List<TilesOcclusionVolume> m_AwaitingVolumes = new List<TilesOcclusionVolume>();

	public bool m_RenderersUpdated = true;

	private Camera m_Camera;

	private Material m_OcclusionMaterial;

	private Material m_OcclusionObjectMaterial;

	private Material m_PixelCorrectorMaterial;

	private Material m_BlurMaterial;

	private CommandBuffer m_OcclusionBuffer;

	private bool m_VolumesUpdated;

	private int _idPropertyOffsetScale;

	[UsedImplicitly]
	private void Awake()
	{
		s_Instance = this;
		m_Camera = GetComponent<Camera>();
		if (m_Camera == null)
		{
			Debug.LogError("Place this script on camera only");
			base.enabled = false;
		}
		m_OcclusionMaterial = new Material(m_OcclusionShader);
		m_OcclusionObjectMaterial = new Material(m_OcclusionObjectShader);
		m_PixelCorrectorMaterial = new Material(m_PixelCorrectorShader);
		m_BlurMaterial = new Material(m_BlurShader);
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		s_Instance = null;
		m_ObjectRenderers.Clear();
		m_RoomRenderers.Clear();
		m_AwaitingVolumes.Clear();
	}

	private void Update()
	{
		UpdateMaterialCorrector();
		if (m_VolumesUpdated)
		{
			m_VolumesUpdated = false;
			UpdateAwaitingVolumes();
		}
		if (m_RenderersUpdated)
		{
			m_RenderersUpdated = false;
			UpdateCommandBuffers();
		}
		m_OcclusionMaterial.SetFloat(_idPropertyOffsetScale, (CameraController.s_CameraController.Zoom - 30f) / 45f);
	}

	public void AddVolume(TilesOcclusionVolume volume)
	{
		m_VolumesUpdated = true;
		m_AwaitingVolumes.Add(volume);
	}

	public void UpdateAwaitingVolumes(bool removeNotVisibleRenderers = false)
	{
		for (int num = m_AwaitingVolumes.Count - 1; num >= 0; num--)
		{
			if (m_AwaitingVolumes[num].IsVisible())
			{
				m_RenderersUpdated = true;
				MeshRenderer[] renderers = m_AwaitingVolumes[num].Renderers;
				foreach (MeshRenderer item in renderers)
				{
					m_RoomRenderers.Add(item);
				}
				m_AwaitingVolumes.RemoveAt(num);
			}
			else if (removeNotVisibleRenderers)
			{
				m_RenderersUpdated = true;
				MeshRenderer[] renderers = m_AwaitingVolumes[num].Renderers;
				foreach (MeshRenderer item2 in renderers)
				{
					m_RoomRenderers.Remove(item2);
				}
				m_AwaitingVolumes.RemoveAt(num);
			}
		}
	}

	public void AddObjectRenderer(MeshRenderer renderer)
	{
		if (!(renderer == null))
		{
			m_ObjectRenderers.Add(renderer);
			m_RenderersUpdated = true;
		}
	}

	public void RemoveObjectRenderer(MeshRenderer renderer)
	{
		if (!(renderer == null))
		{
			m_ObjectRenderers.Remove(renderer);
			m_RenderersUpdated = true;
		}
	}

	private void UpdateMaterialCorrector()
	{
		m_PixelCorrectorMaterial.SetMatrix("_CameraInvViewMatrix", m_Camera.cameraToWorldMatrix);
		m_PixelCorrectorMaterial.SetMatrix("_CameraViewMatrix", m_Camera.worldToCameraMatrix);
	}

	private void UpdateCommandBuffers()
	{
		m_RoomRenderers.RemoveAll((MeshRenderer item) => item == null);
		m_ObjectRenderers.RemoveAll((MeshRenderer item) => item == null);
		if (m_OcclusionBuffer != null)
		{
			m_Camera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, m_OcclusionBuffer);
			m_OcclusionBuffer.Clear();
			m_OcclusionBuffer.Dispose();
		}
		m_OcclusionBuffer = new CommandBuffer();
		m_OcclusionBuffer.name = "Tile Occlusion Map Generation";
		int num = Shader.PropertyToID("_Temp10");
		m_OcclusionBuffer.GetTemporaryRT(num, -1, -1, 24, FilterMode.Bilinear, GraphicsFormat.R8G8B8A8_SNorm);
		int num2 = Shader.PropertyToID("_Temp11");
		int num3 = Shader.PropertyToID("_Temp12");
		m_OcclusionBuffer.GetTemporaryRT(num2, -2, -2, 0, FilterMode.Bilinear, GraphicsFormat.R8G8B8A8_SNorm);
		m_OcclusionBuffer.GetTemporaryRT(num3, -2, -2, 0, FilterMode.Bilinear, GraphicsFormat.R8G8B8A8_SNorm);
		m_OcclusionBuffer.SetRenderTarget(num);
		m_OcclusionBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 1f), 0f);
		foreach (MeshRenderer roomRenderer in m_RoomRenderers)
		{
			m_OcclusionBuffer.DrawRenderer(roomRenderer, m_OcclusionMaterial, 0, 0);
		}
		m_OcclusionBuffer.Blit(num, num2, m_PixelCorrectorMaterial, 2, 0);
		m_OcclusionBuffer.SetGlobalVector("offsets", new Vector4(4f / (float)Screen.width, 0f, 0f, 0f));
		m_OcclusionBuffer.Blit(num2, num3, m_BlurMaterial);
		m_OcclusionBuffer.SetGlobalVector("offsets", new Vector4(0f, 4f / (float)Screen.height, 0f, 0f));
		m_OcclusionBuffer.Blit(num3, num2, m_BlurMaterial);
		int num4 = Shader.PropertyToID("_TempObjectsRT");
		int num5 = Shader.PropertyToID("_BlurObjectsRT11");
		int num6 = Shader.PropertyToID("_BlurObjectsRT12");
		m_OcclusionBuffer.GetTemporaryRT(num4, -2, -2, 24, FilterMode.Bilinear, GraphicsFormat.R8G8B8A8_SNorm);
		m_OcclusionBuffer.GetTemporaryRT(num5, -4, -4, 0, FilterMode.Bilinear, GraphicsFormat.R8G8B8A8_SNorm);
		m_OcclusionBuffer.GetTemporaryRT(num6, -4, -4, 0, FilterMode.Bilinear, GraphicsFormat.R8G8B8A8_SNorm);
		m_OcclusionBuffer.SetRenderTarget(num4);
		m_OcclusionBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black, 0f);
		foreach (MeshRenderer objectRenderer in m_ObjectRenderers)
		{
			m_OcclusionBuffer.DrawRenderer(objectRenderer, m_OcclusionObjectMaterial);
		}
		m_OcclusionBuffer.Blit(num4, num5);
		m_OcclusionBuffer.SetGlobalVector("offsets", new Vector4(4f / (float)Screen.width, 0f, 0f, 0f));
		m_OcclusionBuffer.Blit(num5, num6, m_BlurMaterial);
		m_OcclusionBuffer.SetGlobalVector("offsets", new Vector4(0f, 4f / (float)Screen.height, 0f, 0f));
		m_OcclusionBuffer.Blit(num6, num5, m_BlurMaterial);
		m_OcclusionBuffer.SetGlobalTexture("_ObjectOcclusion", num5);
		m_OcclusionBuffer.Blit(num2, num3, m_PixelCorrectorMaterial, 3, 0);
		m_OcclusionBuffer.SetGlobalTexture("_TilesOcclusionMap", num3);
		m_OcclusionBuffer.SetGlobalFloat(Shader.PropertyToID("_EnableOcclusionMap"), 1f);
		m_Camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, m_OcclusionBuffer);
		_idPropertyOffsetScale = Shader.PropertyToID("tilesOcclusionOffsetScale");
	}
}
