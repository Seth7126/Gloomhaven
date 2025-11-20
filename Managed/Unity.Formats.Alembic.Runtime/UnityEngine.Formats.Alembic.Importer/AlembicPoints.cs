using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Importer;

internal class AlembicPoints : AlembicElement
{
	private aiPoints m_abcSchema;

	private PinnedList<aiPointsData> m_abcData = new PinnedList<aiPointsData>(1);

	private aiPointsSummary m_summary;

	private aiPointsSampleSummary m_sampleSummary;

	internal override aiSchema abcSchema => m_abcSchema;

	public override bool visibility => m_abcData[0].visibility;

	internal override void AbcSetup(aiObject abcObj, aiSchema abcSchema)
	{
		base.AbcSetup(abcObj, abcSchema);
		m_abcSchema = (aiPoints)abcSchema;
		m_abcSchema.GetSummary(ref m_summary);
	}

	public override void AbcPrepareSample()
	{
		if (base.disposed)
		{
			return;
		}
		AlembicPointsCloud component = base.abcTreeNode.gameObject.GetComponent<AlembicPointsCloud>();
		if (component != null)
		{
			m_abcSchema.sort = component.m_sort;
			if (component.m_sort && component.m_sortFrom != null)
			{
				m_abcSchema.sortBasePosition = component.m_sortFrom.position;
			}
		}
	}

	public override void AbcSyncDataBegin()
	{
		if (!base.disposed && m_abcSchema.schema.isDataUpdated)
		{
			aiPointsSample sample = m_abcSchema.sample;
			sample.GetSummary(ref m_sampleSummary);
			AlembicPointsCloud alembicPointsCloud = base.abcTreeNode.gameObject.GetComponent<AlembicPointsCloud>();
			if (alembicPointsCloud == null)
			{
				alembicPointsCloud = base.abcTreeNode.gameObject.AddComponent<AlembicPointsCloud>();
				base.abcTreeNode.gameObject.AddComponent<AlembicPointsRenderer>();
			}
			aiPointsData value = default(aiPointsData);
			alembicPointsCloud.pointsList.ResizeDiscard(m_sampleSummary.count);
			value.points = alembicPointsCloud.pointsList;
			if ((bool)m_summary.hasVelocities)
			{
				alembicPointsCloud.velocitiesList.ResizeDiscard(m_sampleSummary.count);
				value.velocities = alembicPointsCloud.velocitiesList;
			}
			if ((bool)m_summary.hasIDs)
			{
				alembicPointsCloud.idsList.ResizeDiscard(m_sampleSummary.count);
				value.ids = alembicPointsCloud.idsList;
			}
			m_abcData[0] = value;
			sample.FillData(m_abcData);
		}
	}

	public override void AbcSyncDataEnd()
	{
		if (!base.disposed && m_abcSchema.schema.isDataUpdated)
		{
			aiPointsData aiPointsData = m_abcData[0];
			if (base.abcTreeNode.stream.streamDescriptor.Settings.ImportVisibility)
			{
				base.abcTreeNode.gameObject.SetActive(aiPointsData.visibility);
			}
			AlembicPointsCloud component = base.abcTreeNode.gameObject.GetComponent<AlembicPointsCloud>();
			component.BoundsCenter = aiPointsData.boundsCenter;
			component.BoundsExtents = aiPointsData.boundsExtents;
		}
	}
}
