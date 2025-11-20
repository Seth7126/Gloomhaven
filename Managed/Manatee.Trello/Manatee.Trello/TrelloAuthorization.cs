using System;
using Manatee.Trello.Internal;

namespace Manatee.Trello;

public class TrelloAuthorization : IEquatable<TrelloAuthorization>
{
	private string _appKey;

	public static TrelloAuthorization Default { get; }

	internal static TrelloAuthorization Null { get; }

	public string AppKey
	{
		get
		{
			return _appKey;
		}
		set
		{
			if (value.IsNullOrWhiteSpace())
			{
				throw new ArgumentNullException("value");
			}
			_appKey = value;
		}
	}

	public string UserToken { get; set; }

	static TrelloAuthorization()
	{
		Null = new TrelloAuthorization();
		Default = new TrelloAuthorization();
	}

	public bool Equals(TrelloAuthorization other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (string.Equals(_appKey, other._appKey))
		{
			return string.Equals(UserToken, other.UserToken);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as TrelloAuthorization);
	}

	public override int GetHashCode()
	{
		return (((_appKey != null) ? _appKey.GetHashCode() : 0) * 397) ^ ((UserToken != null) ? UserToken.GetHashCode() : 0);
	}

	public static bool operator ==(TrelloAuthorization left, TrelloAuthorization right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(TrelloAuthorization left, TrelloAuthorization right)
	{
		return !object.Equals(left, right);
	}
}
