using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class TokenContext : DeletableSynchronizationContext<IJsonToken>
{
	private static readonly Dictionary<string, object> Parameters;

	private static readonly Token.Fields MemberFields;

	public static Dictionary<string, object> CurrentParameters
	{
		get
		{
			lock (Parameters)
			{
				if (!Parameters.Any())
				{
					GenerateParameters();
				}
				return new Dictionary<string, object>(Parameters);
			}
		}
	}

	public TokenPermissionContext MemberPermissions { get; }

	public TokenPermissionContext BoardPermissions { get; }

	public TokenPermissionContext OrganizationPermissions { get; }

	public virtual bool HasValidId => IdRule.Instance.Validate(base.Data.Id, null) == null;

	static TokenContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = Token.Fields.Id | Token.Fields.Member | Token.Fields.DateCreated | Token.Fields.DateExpires | Token.Fields.Permissions;
		SynchronizationContext<IJsonToken>.Properties = new Dictionary<string, Property<IJsonToken>>
		{
			{
				"AppName",
				new Property<IJsonToken, string>((IJsonToken d, TrelloAuthorization a) => d.Identifier, delegate(IJsonToken d, string o)
				{
					d.Identifier = o;
				})
			},
			{
				"Member",
				new Property<IJsonToken, Member>((IJsonToken d, TrelloAuthorization a) => d.Member?.GetFromCache<Member, IJsonMember>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonToken d, Member o)
				{
					d.Member = o?.Json;
				})
			},
			{
				"DateCreated",
				new Property<IJsonToken, DateTime?>((IJsonToken d, TrelloAuthorization a) => d.DateCreated, delegate(IJsonToken d, DateTime? o)
				{
					d.DateCreated = o;
				})
			},
			{
				"DateExpires",
				new Property<IJsonToken, DateTime?>((IJsonToken d, TrelloAuthorization a) => d.DateExpires, delegate(IJsonToken d, DateTime? o)
				{
					d.DateExpires = o;
				})
			},
			{
				"Id",
				new Property<IJsonToken, string>((IJsonToken d, TrelloAuthorization a) => d.Id, delegate(IJsonToken d, string o)
				{
					d.Id = o;
				})
			}
		};
	}

	public TokenContext(string tokenValue, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		base.Data.TokenValue = tokenValue;
		base.Data.Permissions = new List<IJsonTokenPermission>();
		MemberPermissions = new TokenPermissionContext(base.Auth);
		base.Data.Permissions.Add(MemberPermissions.Data);
		BoardPermissions = new TokenPermissionContext(base.Auth);
		base.Data.Permissions.Add(BoardPermissions.Data);
		OrganizationPermissions = new TokenPermissionContext(base.Auth);
		base.Data.Permissions.Add(OrganizationPermissions.Data);
	}

	public static void UpdateParameters()
	{
		lock (Parameters)
		{
			Parameters.Clear();
		}
	}

	private static void GenerateParameters()
	{
		lock (Parameters)
		{
			Parameters.Clear();
			int num = Enum.GetValues(typeof(Token.Fields)).Cast<Token.Fields>().ToList()
				.Cast<int>()
				.Sum();
			Token.Fields enumerationValue = (Token.Fields)((int)((uint)num & (uint)MemberFields) & (int)Token.DownloadedFields);
			Parameters["fields"] = enumerationValue.GetDescription();
			Token.Fields fields = (Token.Fields)((int)((uint)num & (uint)Token.DownloadedFields) & (int)(~MemberFields));
			if (fields.HasFlag(Token.Fields.Member))
			{
				Parameters["member"] = "true";
				Parameters["member_fields"] = MemberContext.CurrentParameters["fields"];
			}
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Token_Read_Refresh, new Dictionary<string, object> { 
		{
			"_token",
			base.Data.TokenValue
		} });
	}

	protected override Dictionary<string, object> GetParameters()
	{
		return CurrentParameters;
	}

	protected override Endpoint GetDeleteEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Token_Write_Delete, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.TokenValue
		} });
	}

	protected override IEnumerable<string> MergeDependencies(IJsonToken json, bool overwrite)
	{
		if (json.Permissions == null)
		{
			return Enumerable.Empty<string>();
		}
		return MemberPermissions.Merge(json.Permissions.FirstOrDefault((IJsonTokenPermission p) => p.ModelType == TokenModelType.Member), overwrite).Concat(BoardPermissions.Merge(json.Permissions.FirstOrDefault((IJsonTokenPermission p) => p.ModelType == TokenModelType.Board), overwrite)).Concat(OrganizationPermissions.Merge(json.Permissions.FirstOrDefault((IJsonTokenPermission p) => p.ModelType == TokenModelType.Organization), overwrite));
	}
}
