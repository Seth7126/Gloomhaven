using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Importer;

[ExecuteInEditMode]
internal class AlembicCamera : AlembicElement
{
	private aiCamera m_abcSchema;

	private CameraData m_abcData;

	private Camera m_camera;

	internal override aiSchema abcSchema => m_abcSchema;

	public override bool visibility => m_abcData.visibility;

	internal override void AbcSetup(aiObject abcObj, aiSchema abcSchema)
	{
		base.AbcSetup(abcObj, abcSchema);
		m_abcSchema = (aiCamera)abcSchema;
		m_camera = GetOrAddCamera();
		base.abcTreeNode.gameObject.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
	}

	public override void AbcSyncDataEnd()
	{
		if (!base.disposed && m_abcSchema.schema.isDataUpdated)
		{
			m_abcSchema.sample.GetData(ref m_abcData);
			if (base.abcTreeNode.stream.streamDescriptor.Settings.ImportVisibility)
			{
				base.abcTreeNode.gameObject.SetActive(m_abcData.visibility);
			}
			m_camera.focalLength = m_abcData.focalLength;
			m_camera.sensorSize = m_abcData.sensorSize;
			m_camera.lensShift = m_abcData.lensShift;
			m_camera.nearClipPlane = m_abcData.nearClipPlane;
			m_camera.farClipPlane = m_abcData.farClipPlane;
		}
	}
}
