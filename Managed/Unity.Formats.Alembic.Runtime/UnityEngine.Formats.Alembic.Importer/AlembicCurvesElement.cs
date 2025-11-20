using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Importer;

internal class AlembicCurvesElement : AlembicElement
{
	private aiCurves m_abcSchema;

	private PinnedList<aiCurvesData> m_abcData = new PinnedList<aiCurvesData>(1);

	private aiCurvesSummary m_summary;

	private aiCurvesSampleSummary m_sampleSummary;

	internal bool CreateRenderingComponent { get; set; }

	internal override aiSchema abcSchema => m_abcSchema;

	public override bool visibility => m_abcData[0].visibility;

	internal override void AbcSetup(aiObject abcObj, aiSchema abcSchema)
	{
		base.AbcSetup(abcObj, abcSchema);
		m_abcSchema = (aiCurves)abcSchema;
		m_abcSchema.GetSummary(ref m_summary);
	}

	public override void AbcSyncDataBegin()
	{
		if (!base.disposed && m_abcSchema.schema.isDataUpdated)
		{
			aiCurvesSample sample = m_abcSchema.sample;
			sample.GetSummary(ref m_sampleSummary);
			AlembicCurves orAddComponent = base.abcTreeNode.gameObject.GetOrAddComponent<AlembicCurves>();
			if (CreateRenderingComponent)
			{
				base.abcTreeNode.gameObject.GetOrAddComponent<AlembicCurvesRenderer>();
			}
			aiCurvesData value = default(aiCurvesData);
			if ((bool)m_summary.hasPositions)
			{
				orAddComponent.positionsList.ResizeDiscard(m_sampleSummary.positionCount);
				orAddComponent.velocitiesList.ResizeDiscard(m_sampleSummary.positionCount);
				orAddComponent.curveOffsets.ResizeDiscard(m_sampleSummary.numVerticesCount);
				value.positions = orAddComponent.positionsList;
				value.velocities = orAddComponent.velocitiesList;
				value.numVertices = orAddComponent.curveOffsets;
			}
			if ((bool)m_summary.hasWidths)
			{
				orAddComponent.widths.ResizeDiscard(m_sampleSummary.positionCount);
				value.widths = orAddComponent.widths;
			}
			if ((bool)m_summary.hasUVs)
			{
				orAddComponent.uvs.ResizeDiscard(m_sampleSummary.positionCount);
				value.uvs = orAddComponent.uvs;
			}
			m_abcData[0] = value;
			sample.FillData(m_abcData);
		}
	}

	public override void AbcSyncDataEnd()
	{
		if (!base.disposed && m_abcSchema.schema.isDataUpdated)
		{
			aiCurvesData aiCurvesData = m_abcData[0];
			if (base.abcTreeNode.stream.streamDescriptor.Settings.ImportVisibility)
			{
				base.abcTreeNode.gameObject.SetActive(aiCurvesData.visibility);
			}
			AlembicCurves component = base.abcTreeNode.gameObject.GetComponent<AlembicCurves>();
			int num = 0;
			for (int i = 0; i < component.CurveOffsets.Length; i++)
			{
				int num2 = component.CurveOffsets[i];
				component.CurveOffsets[i] = num;
				num += num2;
			}
			component.InvokeOnUpdate(component);
		}
	}
}
