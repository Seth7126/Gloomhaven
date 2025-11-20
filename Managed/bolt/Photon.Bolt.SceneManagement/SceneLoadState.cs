using Photon.Bolt.Internal;

namespace Photon.Bolt.SceneManagement;

internal struct SceneLoadState
{
	public const int STATE_LOADING = 1;

	public const int STATE_LOADING_DONE = 2;

	public const int STATE_CALLBACK_INVOKED = 3;

	public int State;

	public Scene Scene;

	public IProtocolToken Token;

	public SceneLoadState BeginLoad(int index, IProtocolToken token)
	{
		return new SceneLoadState
		{
			Scene = new Scene(index, (Scene.Sequence + 1) & 0xFF),
			State = 1,
			Token = token
		};
	}

	public static SceneLoadState DefaultRemote()
	{
		return new SceneLoadState
		{
			Scene = new Scene(255, 255),
			State = 3,
			Token = null
		};
	}

	public static SceneLoadState DefaultLocal()
	{
		int index = BoltNetworkInternal.GetActiveSceneIndex();
		return new SceneLoadState
		{
			Scene = new Scene(index, 255),
			State = 2,
			Token = null
		};
	}
}
