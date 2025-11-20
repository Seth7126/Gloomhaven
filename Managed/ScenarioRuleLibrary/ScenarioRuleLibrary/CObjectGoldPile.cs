using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{PrefabName} : {PropGuid}")]
public class CObjectGoldPile : CObjectProp, ISerializable
{
	public CObjectGoldPile()
	{
	}

	public CObjectGoldPile(CObjectGoldPile state, ReferenceDictionary references)
		: base(state, references)
	{
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	public CObjectGoldPile(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CObjectGoldPile(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
	}

	public CObjectGoldPile(string name, ScenarioManager.ObjectImportType type, string mapGuid, CActor actor)
		: base(name, type, null, null, null, actor, mapGuid)
	{
	}

	public CObjectGoldPile(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
	}

	public override bool AutomaticActivate(CActor actor)
	{
		return false;
	}

	public static List<Tuple<int, string>> Compare(CObjectGoldPile gold1, CObjectGoldPile gold2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(CObjectProp.Compare(gold1, gold2, isMPCompare));
		return list;
	}
}
