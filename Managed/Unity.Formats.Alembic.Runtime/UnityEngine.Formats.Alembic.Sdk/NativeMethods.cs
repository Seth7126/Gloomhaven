using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

internal static class NativeMethods
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct aiXform
	{
		[DllImport("abci")]
		public static extern aiXformSample aiSchemaGetSample(IntPtr schema);
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct aiCamera
	{
		[DllImport("abci")]
		public static extern aiCameraSample aiSchemaGetSample(IntPtr schema);
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct aiPolyMesh
	{
		[DllImport("abci")]
		public static extern aiPolyMeshSample aiSchemaGetSample(IntPtr schema);
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct aiSubD
	{
		[DllImport("abci")]
		public static extern aiPolyMeshSample aiSchemaGetSample(IntPtr schema);
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct aiPoints
	{
		[DllImport("abci")]
		public static extern aiPointsSample aiSchemaGetSample(IntPtr schema);
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct aiCurves
	{
		[DllImport("abci")]
		public static extern aiCurvesSample aiSchemaGetSample(IntPtr schema);
	}

	[DllImport("abci")]
	public static extern aeContext aeCreateContext();

	[DllImport("abci")]
	public static extern void aeDestroyContext(IntPtr ctx);

	[DllImport("abci")]
	public static extern bool aiContextGetIsHDF5(IntPtr ctx);

	[DllImport("abci", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	public static extern IntPtr aiContextGetApplication(IntPtr ctx);

	[DllImport("abci")]
	public static extern void aeSetConfig(IntPtr ctx, AlembicExportOptions conf);

	[DllImport("abci", BestFitMapping = false, ThrowOnUnmappableChar = true)]
	public static extern Bool aeOpenArchive(IntPtr ctx, string path);

	[DllImport("abci")]
	public static extern aeObject aeGetTopObject(IntPtr ctx);

	[DllImport("abci")]
	public static extern int aeAddTimeSampling(IntPtr ctx, float start_time);

	[DllImport("abci")]
	public static extern void aeAddTime(IntPtr ctx, float time, int tsi = -1);

	[DllImport("abci")]
	public static extern void aeMarkFrameBegin(IntPtr ctx);

	[DllImport("abci")]
	public static extern void aeMarkFrameEnd(IntPtr ctx);

	[DllImport("abci", BestFitMapping = false, ThrowOnUnmappableChar = true)]
	public static extern aeObject aeNewXform(IntPtr self, string name, int tsi);

	[DllImport("abci", BestFitMapping = false, ThrowOnUnmappableChar = true)]
	public static extern aeObject aeNewCamera(IntPtr self, string name, int tsi);

	[DllImport("abci", BestFitMapping = false, ThrowOnUnmappableChar = true)]
	public static extern aeObject aeNewPoints(IntPtr self, string name, int tsi);

	[DllImport("abci", BestFitMapping = false, ThrowOnUnmappableChar = true)]
	public static extern aeObject aeNewPolyMesh(IntPtr self, string name, int tsi);

	[DllImport("abci")]
	public static extern void aeXformWriteSample(IntPtr self, ref aeXformData data);

	[DllImport("abci")]
	public static extern void aeCameraWriteSample(IntPtr self, ref CameraData data);

	[DllImport("abci")]
	public static extern void aePolyMeshWriteSample(IntPtr self, ref aePolyMeshData data);

	[DllImport("abci", BestFitMapping = false, ThrowOnUnmappableChar = true)]
	public static extern int aePolyMeshAddFaceSet(IntPtr self, string name);

	[DllImport("abci")]
	public static extern void aePointsWriteSample(IntPtr self, ref aePointsData data);

	[DllImport("abci", BestFitMapping = false, ThrowOnUnmappableChar = true)]
	public static extern aeProperty aeNewProperty(IntPtr self, string name, aePropertyType type);

	[DllImport("abci")]
	public static extern void aeMarkForceInvisible(IntPtr self);

	[DllImport("abci")]
	public static extern void aePropertyWriteArraySample(IntPtr self, IntPtr data, int num_data);

	[DllImport("abci")]
	public static extern void aePropertyWriteScalarSample(IntPtr self, ref float data);

	[DllImport("abci")]
	public static extern void aePropertyWriteScalarSample(IntPtr self, ref int data);

	[DllImport("abci")]
	public static extern void aePropertyWriteScalarSample(IntPtr self, ref Bool data);

	[DllImport("abci")]
	public static extern void aePropertyWriteScalarSample(IntPtr self, ref Vector2 data);

	[DllImport("abci")]
	public static extern void aePropertyWriteScalarSample(IntPtr self, ref Vector3 data);

	[DllImport("abci")]
	public static extern void aePropertyWriteScalarSample(IntPtr self, ref Vector4 data);

	[DllImport("abci")]
	public static extern void aePropertyWriteScalarSample(IntPtr self, ref Matrix4x4 data);

	[DllImport("abci")]
	public static extern int aeGenerateRemapIndices(IntPtr dstIndices, IntPtr points, IntPtr weights4, int numPoints);

	[DllImport("abci")]
	public static extern void aeApplyMatrixP(IntPtr dstPoints, int num, ref Matrix4x4 mat);

	[DllImport("abci")]
	public static extern void aeApplyMatrixV(IntPtr dstVectors, int num, ref Matrix4x4 mat);

	[DllImport("abci", BestFitMapping = false, ThrowOnUnmappableChar = true)]
	public static extern void aiClearContextsWithPath(string path);

	[DllImport("abci")]
	public static extern aiContext aiContextCreate(int uid);

	[DllImport("abci")]
	public static extern void aiContextDestroy(IntPtr ctx);

	[DllImport("abci", BestFitMapping = false, ThrowOnUnmappableChar = true)]
	public static extern Bool aiContextLoad(IntPtr ctx, string path);

	[DllImport("abci")]
	public static extern void aiContextSetConfig(IntPtr ctx, ref aiConfig conf);

	[DllImport("abci")]
	public static extern int aiContextGetTimeSamplingCount(IntPtr ctx);

	[DllImport("abci")]
	public static extern aiTimeSampling aiContextGetTimeSampling(IntPtr ctx, int i);

	[DllImport("abci")]
	public static extern void aiContextGetTimeRange(IntPtr ctx, out double begin, out double end);

	[DllImport("abci")]
	public static extern aiObject aiContextGetTopObject(IntPtr ctx);

	[DllImport("abci")]
	public static extern void aiContextUpdateSamples(IntPtr ctx, double time);

	[DllImport("abci")]
	public static extern int aiTimeSamplingGetSampleCount(IntPtr self);

	[DllImport("abci")]
	public static extern double aiTimeSamplingGetTime(IntPtr self, int index);

	[DllImport("abci")]
	public static extern void aiTimeSamplingGetRange(IntPtr self, ref double start, ref double end);

	[DllImport("abci")]
	public static extern aiContext aiObjectGetContext(IntPtr obj);

	[DllImport("abci")]
	public static extern int aiObjectGetNumChildren(IntPtr obj);

	[DllImport("abci")]
	public static extern aiObject aiObjectGetChild(IntPtr obj, int i);

	[DllImport("abci")]
	public static extern aiObject aiObjectGetParent(IntPtr obj);

	[DllImport("abci")]
	public static extern void aiObjectSetEnabled(IntPtr obj, Bool v);

	[DllImport("abci")]
	public static extern IntPtr aiObjectGetName(IntPtr obj);

	[DllImport("abci")]
	public static extern IntPtr aiObjectGetFullName(IntPtr obj);

	[DllImport("abci")]
	public static extern UnityEngine.Formats.Alembic.Sdk.aiXform aiObjectAsXform(IntPtr obj);

	[DllImport("abci")]
	public static extern UnityEngine.Formats.Alembic.Sdk.aiCamera aiObjectAsCamera(IntPtr obj);

	[DllImport("abci")]
	public static extern UnityEngine.Formats.Alembic.Sdk.aiPoints aiObjectAsPoints(IntPtr obj);

	[DllImport("abci")]
	public static extern UnityEngine.Formats.Alembic.Sdk.aiCurves aiObjectAsCurves(IntPtr obj);

	[DllImport("abci")]
	public static extern UnityEngine.Formats.Alembic.Sdk.aiPolyMesh aiObjectAsPolyMesh(IntPtr obj);

	[DllImport("abci")]
	public static extern UnityEngine.Formats.Alembic.Sdk.aiSubD aiObjectAsSubD(IntPtr obj);

	[DllImport("abci")]
	public static extern void aiSchemaUpdateSample(IntPtr schema, ref aiSampleSelector ss);

	[DllImport("abci")]
	public static extern void aiSchemaSync(IntPtr schema);

	[DllImport("abci")]
	public static extern aiSample aiSchemaGetSample(IntPtr schema);

	[DllImport("abci")]
	public static extern Bool aiSchemaIsConstant(IntPtr schema);

	[DllImport("abci")]
	public static extern Bool aiSchemaIsDataUpdated(IntPtr schema);

	[DllImport("abci")]
	public static extern int aiSchemaGetNumProperties(IntPtr schema);

	[DllImport("abci")]
	public static extern aiProperty aiSchemaGetPropertyByIndex(IntPtr schema, int i);

	[DllImport("abci", BestFitMapping = false, ThrowOnUnmappableChar = true)]
	public static extern aiProperty aiSchemaGetPropertyByName(IntPtr schema, string name);

	[DllImport("abci")]
	public static extern void aiPolyMeshGetSummary(IntPtr schema, ref aiMeshSummary dst);

	[DllImport("abci")]
	public static extern void aiSubDGetSummary(IntPtr schema, ref aiMeshSummary dst);

	[DllImport("abci")]
	public static extern void aiPointsSetSort(IntPtr schema, Bool v);

	[DllImport("abci")]
	public static extern void aiPointsSetSortBasePosition(IntPtr schema, Vector3 v);

	[DllImport("abci")]
	public static extern void aiPointsGetSummary(IntPtr schema, ref aiPointsSummary dst);

	[DllImport("abci")]
	public static extern void aiCurvesGetSummary(IntPtr schema, ref aiCurvesSummary dst);

	[DllImport("abci")]
	public static extern void aiXformGetData(IntPtr sample, ref aiXformData data);

	[DllImport("abci")]
	public static extern void aiCameraGetData(IntPtr sample, ref CameraData dst);

	[DllImport("abci")]
	public static extern void aiPolyMeshGetSampleSummary(IntPtr sample, ref aiMeshSampleSummary dst);

	[DllImport("abci")]
	public static extern int aiPolyMeshGetSplitSummaries(IntPtr sample, IntPtr dst);

	[DllImport("abci")]
	public static extern void aiPolyMeshGetSubmeshSummaries(IntPtr sample, IntPtr dst);

	[DllImport("abci")]
	public static extern void aiPolyMeshFillVertexBuffer(IntPtr sample, IntPtr vbs, IntPtr ibs);

	[DllImport("abci")]
	public static extern void aiPointsGetSampleSummary(IntPtr sample, ref aiPointsSampleSummary dst);

	[DllImport("abci")]
	public static extern void aiPointsFillData(IntPtr sample, IntPtr dst);

	[DllImport("abci")]
	public static extern void aiCurvesGetSampleSummary(IntPtr sample, ref aiCurvesSampleSummary dst);

	[DllImport("abci")]
	public static extern void aiCurvesFillData(IntPtr sample, IntPtr dst);

	[DllImport("abci")]
	public static extern IntPtr aiPropertyGetName(IntPtr prop);

	[DllImport("abci")]
	public static extern aiPropertyType aiPropertyGetType(IntPtr prop);

	[DllImport("abci")]
	public static extern void aiPropertyGetData(IntPtr prop, aiPropertyData oData);

	[DllImport("abci")]
	public static extern aiSampleSelector aiTimeToSampleSelector(double time);

	[DllImport("abci")]
	public static extern void aiCleanup();
}
