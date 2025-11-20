using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

public class CharacterPortraitsProvider : Singleton<CharacterPortraitsProvider>
{
	private Dictionary<ICharacterPortraitUser, string> _users = new Dictionary<ICharacterPortraitUser, string>();

	private Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

	private Texture2D _megaTexture;

	private bool _isDirty;

	private Dictionary<string, Rect> _coordsInMegaTexture = new Dictionary<string, Rect>();

	private bool _useSingleTexture => PlatformLayer.Setting.SimplifiedUI;

	protected override void Awake()
	{
		if (!Singleton<CharacterPortraitsProvider>.IsInitialized)
		{
			SetInstance(this);
		}
		else
		{
			Object.Destroy(this);
		}
	}

	private void LateUpdate()
	{
		if (_isDirty)
		{
			UpdateMegaTexture();
			UpdateAllUsers();
			_isDirty = false;
		}
	}

	protected override void OnDestroy()
	{
		if (Singleton<CharacterPortraitsProvider>.Instance == this)
		{
			while (_users.Count > 0)
			{
				RemoveUser(_users.First().Key);
			}
			_users.Clear();
			SetInstance(null);
			_megaTexture = null;
			_textures.Clear();
			_coordsInMegaTexture.Clear();
		}
	}

	public void RegisterNewUser(ICharacterPortraitUser user, CActor actor)
	{
		if (_users.TryGetValue(user, out var _))
		{
			RemoveUser(user);
		}
		string nameTexture = GetNameTexture(actor);
		bool flag = false;
		if (!_textures.TryGetValue(nameTexture, out var _))
		{
			Texture2D texture2D = LoadCActorPortrait(actor);
			if (texture2D == null)
			{
				return;
			}
			_textures.Add(nameTexture, texture2D);
			flag = true;
		}
		_users.Add(user, nameTexture);
		if (_useSingleTexture)
		{
			if (flag)
			{
				_isDirty = true;
			}
			else
			{
				user.UpdateTexture(_megaTexture, _coordsInMegaTexture[nameTexture]);
			}
		}
		else
		{
			user.UpdateTexture(_textures[nameTexture], new Rect(0f, 0f, 1f, 1f));
		}
	}

	public void RemoveUser(ICharacterPortraitUser user)
	{
		if (_users.TryGetValue(user, out var value))
		{
			_users.Remove(user);
			user.UnloadedTexture();
			if (_textures.TryGetValue(value, out var _) && !_useSingleTexture)
			{
				_textures.Remove(value);
			}
		}
	}

	private string GetNameTexture(CActor actor)
	{
		return actor.Class.DefaultModel.ToLowerInvariant();
	}

	private Texture2D LoadCActorPortrait(CActor actor)
	{
		return AssetBundleManager.Instance.LoadAssetFromBundle<Sprite>("misc_characterportraits", actor.Class.DefaultModel.ToLowerInvariant(), "characterportraits", "png", null, suppressError: true).texture;
	}

	private void UpdateMegaTexture()
	{
		int count = _textures.Count;
		Vector2Int zero = Vector2Int.zero;
		foreach (KeyValuePair<string, Texture2D> texture in _textures)
		{
			Texture value = texture.Value;
			zero.x = Mathf.Max(zero.x, value.width);
			zero.y = Mathf.Max(zero.y, value.height);
		}
		if (_megaTexture != null)
		{
			Object.Destroy(_megaTexture);
		}
		Vector2Int vector2Int = new Vector2Int(zero.x * count, zero.y);
		_megaTexture = new Texture2D(vector2Int.x, vector2Int.y, TextureFormat.RGBA32, mipChain: false);
		_megaTexture.name = "MegaTexture_CharacterPortraits";
		_coordsInMegaTexture.Clear();
		int num = 0;
		foreach (KeyValuePair<string, Texture2D> texture2 in _textures)
		{
			if (texture2.Value.isReadable)
			{
				_megaTexture.SetPixels32(zero.x * num, 0, zero.x, zero.y, texture2.Value.GetPixels32(0), 0);
				_coordsInMegaTexture[texture2.Key] = new Rect((float)zero.x / (float)vector2Int.x * (float)num, 0f, (float)zero.x / (float)vector2Int.x, 1f);
			}
			else
			{
				_coordsInMegaTexture[texture2.Key] = new Rect(0f, 0f, 1f, 1f);
			}
			num++;
		}
		_megaTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
	}

	private void UpdateAllUsers()
	{
		foreach (KeyValuePair<ICharacterPortraitUser, string> user in _users)
		{
			user.Key.UpdateTexture(_megaTexture, _coordsInMegaTexture[user.Value]);
		}
	}
}
