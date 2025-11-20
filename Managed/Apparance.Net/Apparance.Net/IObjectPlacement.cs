using System.Collections.Generic;

namespace Apparance.Net;

public interface IObjectPlacement
{
	bool IsValid { get; }

	void BeginContentUpdate();

	void CreateObject(int handle, int tier, Vector3 offset, int object_type, Frame frame, ParameterCollection parameters, string name = null, int child_count = 0);

	void CreateGroup(int handle, int tier, string name, int child_count);

	void CreateMesh(int handle, int tier, Vector3 offset, int num_verts, Vector3[] positions, Vector3[] normals, IList<uint[]> colours, IList<Vector2[]> uvs, MeshPart[] parts);

	void DestroyObject(int handle);

	void EndContentUpdate();
}
