using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Caching;

internal static class CachingObjectFactory
{
	private static readonly Dictionary<Type, Type> JsonTypeMap;

	private static readonly Dictionary<Type, Func<IJsonCacheable, TrelloAuthorization, object[], ICacheable>> JsonFactory;

	static CachingObjectFactory()
	{
		JsonTypeMap = new Dictionary<Type, Type>
		{
			{
				typeof(IJsonAction),
				typeof(Action)
			},
			{
				typeof(IJsonAttachment),
				typeof(Attachment)
			},
			{
				typeof(IJsonImagePreview),
				typeof(ImagePreview)
			},
			{
				typeof(IJsonBoard),
				typeof(Board)
			},
			{
				typeof(IJsonBoardBackground),
				typeof(BoardBackground)
			},
			{
				typeof(IJsonCard),
				typeof(Card)
			},
			{
				typeof(IJsonCheckList),
				typeof(CheckList)
			},
			{
				typeof(IJsonCommentReaction),
				typeof(CommentReaction)
			},
			{
				typeof(IJsonLabel),
				typeof(Label)
			},
			{
				typeof(IJsonList),
				typeof(List)
			},
			{
				typeof(IJsonMember),
				typeof(Member)
			},
			{
				typeof(IJsonOrganization),
				typeof(Organization)
			},
			{
				typeof(IJsonNotification),
				typeof(Notification)
			},
			{
				typeof(IJsonStarredBoard),
				typeof(StarredBoard)
			},
			{
				typeof(IJsonToken),
				typeof(Token)
			}
		};
		JsonFactory = new Dictionary<Type, Func<IJsonCacheable, TrelloAuthorization, object[], ICacheable>>
		{
			{
				typeof(Action),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new Action((IJsonAction)j, a)
			},
			{
				typeof(Board),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new Board((IJsonBoard)j, a)
			},
			{
				typeof(BoardBackground),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new BoardBackground((string)((p != null) ? p[0] : null), (IJsonBoardBackground)j, a)
			},
			{
				typeof(Card),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new Card((IJsonCard)j, a)
			},
			{
				typeof(CheckList),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new CheckList((IJsonCheckList)j, a)
			},
			{
				typeof(CommentReaction),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => _BuildCommentReaction((IJsonCommentReaction)j, a, p)
			},
			{
				typeof(CustomField),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => ((IJsonCustomField)j)._BuildCustomField(a, p)
			},
			{
				typeof(CustomFieldDefinition),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new CustomFieldDefinition((IJsonCustomFieldDefinition)j, a)
			},
			{
				typeof(DropDownOption),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new DropDownOption((IJsonCustomDropDownOption)j, a)
			},
			{
				typeof(ImagePreview),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new ImagePreview((IJsonImagePreview)j)
			},
			{
				typeof(Label),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new Label((IJsonLabel)j, a)
			},
			{
				typeof(List),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new List((IJsonList)j, a)
			},
			{
				typeof(Member),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new Member((IJsonMember)j, a)
			},
			{
				typeof(Organization),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new Organization((IJsonOrganization)j, a)
			},
			{
				typeof(StarredBoard),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new StarredBoard((string)p[0], (IJsonStarredBoard)j, a)
			},
			{
				typeof(Notification),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new Notification((IJsonNotification)j, a)
			},
			{
				typeof(Token),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new Token((IJsonToken)j, a)
			},
			{
				typeof(IPowerUp),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => _BuildConfiguredPowerUp((IJsonPowerUp)j, a) ?? new UnknownPowerUp((IJsonPowerUp)j, a)
			},
			{
				typeof(PowerUpData),
				(IJsonCacheable j, TrelloAuthorization a, object[] p) => new PowerUpData((IJsonPowerUpData)j, a)
			}
		};
	}

	public static ICacheable GetFromCache(this IJsonCacheable json, TrelloAuthorization auth)
	{
		if (json == null)
		{
			return null;
		}
		Type type = json.GetType();
		if (!type.IsInterface)
		{
			type = JsonTypeMap.Keys.Intersect(type.GetInterfaces()).FirstOrDefault();
			if (type == null)
			{
				throw new InvalidOperationException("Type `" + json.GetType().Name + "` implements more than one Manatee.Trello.Json interface.  Please provide separate classes for each interface.");
			}
		}
		return TrelloConfiguration.Cache.Find<ICacheable>(json.Id) ?? JsonFactory[JsonTypeMap[type]](json, auth, null);
	}

	public static T GetFromCache<T>(this IJsonCacheable json, TrelloAuthorization auth) where T : class, ICacheable
	{
		if (json == null)
		{
			return null;
		}
		T val = json.TryGetFromCache<T>();
		if (val == null)
		{
			TrelloConfiguration.Log.Debug(typeof(T).Name + " with ID " + json.Id + " not found.  Building...");
			return (T)JsonFactory[typeof(T)](json, auth, null);
		}
		return val;
	}

	public static T GetFromCache<T, TJson>(this TJson json, TrelloAuthorization auth, bool overwrite = true, params object[] parameters) where T : class, ICacheable, IMergeJson<TJson> where TJson : IJsonCacheable
	{
		if (json == null)
		{
			return null;
		}
		T val = json.TryGetFromCache<T, TJson>(overwrite);
		if (val == null)
		{
			TrelloConfiguration.Log.Debug(typeof(T).Name + " with ID " + json.Id + " not found.  Building...");
			return (T)JsonFactory[typeof(T)](json, auth, parameters);
		}
		return val;
	}

	public static T TryGetFromCache<T>(this IJsonCacheable json) where T : class, ICacheable
	{
		return TrelloConfiguration.Cache.Find<T>(json.Id);
	}

	public static T TryGetFromCache<T, TJson>(this TJson json, bool overwrite = true) where T : class, ICacheable, IMergeJson<TJson> where TJson : IJsonCacheable
	{
		T val = TrelloConfiguration.Cache.Find<T>(json.Id);
		if (val != null)
		{
			val.Merge(json, overwrite);
			return val;
		}
		return val;
	}

	private static IPowerUp _BuildConfiguredPowerUp(IJsonPowerUp json, TrelloAuthorization auth)
	{
		if (!TrelloConfiguration.RegisteredPowerUps.TryGetValue(json.Id, out var value))
		{
			return null;
		}
		return value(json, auth);
	}

	private static CommentReaction _BuildCommentReaction(IJsonCommentReaction json, TrelloAuthorization auth, object[] parameters)
	{
		string ownerId = parameters[0] as string;
		return new CommentReaction(json, ownerId, auth);
	}

	private static CustomField _BuildCustomField(this IJsonCustomField json, TrelloAuthorization auth, object[] parameters)
	{
		string cardId = (string)parameters[0];
		return json.Type switch
		{
			CustomFieldType.Text => new TextField(json, cardId, auth), 
			CustomFieldType.DropDown => new DropDownField(json, cardId, auth), 
			CustomFieldType.CheckBox => new CheckBoxField(json, cardId, auth), 
			CustomFieldType.DateTime => new DateTimeField(json, cardId, auth), 
			CustomFieldType.Number => new NumberField(json, cardId, auth), 
			_ => null, 
		};
	}
}
