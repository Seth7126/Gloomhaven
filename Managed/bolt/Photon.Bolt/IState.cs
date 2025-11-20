using System;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Bolt;

[Documentation]
public interface IState : IDisposable
{
	Animator Animator { get; }

	IEnumerable<Animator> AllAnimators { get; }

	void SetAnimator(Animator animator);

	void AddAnimator(Animator animator);

	void AddCallback(string path, PropertyCallback callback);

	void AddCallback(string path, PropertyCallbackSimple callback);

	void RemoveCallback(string path, PropertyCallback callback);

	void RemoveCallback(string path, PropertyCallbackSimple callback);

	void RemoveAllCallbacks();

	void SetDynamic(string property, object value);

	object GetDynamic(string property);

	bool TrySetDynamic(string property, object value);

	bool TryGetDynamic(string property, out object value);

	void SetTeleport(NetworkTransform transform);

	void SetTransforms(NetworkTransform transform, Transform simulate);

	void SetTransforms(NetworkTransform transform, Transform simulate, Transform render);

	void ForceTransform(NetworkTransform transform, Vector3 position);

	void ForceTransform(NetworkTransform transform, Vector3 position, Quaternion rotation);
}
