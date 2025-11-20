using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Importer;

internal class AlembicXform : AlembicElement
{
	private aiXform m_abcSchema;

	private aiXformData m_abcData;

	internal override aiSchema abcSchema => m_abcSchema;

	public override bool visibility => m_abcData.visibility;

	internal override void AbcSetup(aiObject abcObj, aiSchema abcSchema)
	{
		base.AbcSetup(abcObj, abcSchema);
		m_abcSchema = (aiXform)abcSchema;
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
			Transform component = base.abcTreeNode.gameObject.GetComponent<Transform>();
			if ((bool)m_abcData.inherits || (bool)base.abcObject.parent == (bool)base.abcObject.context.topObject)
			{
				component.localPosition = m_abcData.translation;
				component.localRotation = m_abcData.rotation;
				component.localScale = m_abcData.scale;
			}
			else
			{
				component.position = m_abcData.translation;
				component.rotation = m_abcData.rotation;
				component.localScale = m_abcData.scale;
			}
		}
	}
}
