namespace Photon.Bolt.Internal;

public interface IDebugDrawerObjectArray
{
	bool IsVisible { get; set; }

	string GetName();

	string GetValue();

	IDebugDrawerObjectArray[] GetChildren();
}
