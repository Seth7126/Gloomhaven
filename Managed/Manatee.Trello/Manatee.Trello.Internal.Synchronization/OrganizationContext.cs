using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class OrganizationContext : DeletableSynchronizationContext<IJsonOrganization>
{
	private static readonly Dictionary<string, object> Parameters;

	private static readonly Organization.Fields MemberFields;

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

	public ReadOnlyActionCollection Actions { get; }

	public BoardCollection Boards { get; }

	public ReadOnlyMemberCollection Members { get; }

	public OrganizationMembershipCollection Memberships { get; }

	public ReadOnlyPowerUpDataCollection PowerUpData { get; }

	public OrganizationPreferencesContext OrganizationPreferencesContext { get; }

	public virtual bool HasValidId => IdRule.Instance.Validate(base.Data.Id, null) == null;

	static OrganizationContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = Organization.Fields.Description | Organization.Fields.DisplayName | Organization.Fields.LogoHash | Organization.Fields.Name | Organization.Fields.Preferences | Organization.Fields.Url | Organization.Fields.Website;
		SynchronizationContext<IJsonOrganization>.Properties = new Dictionary<string, Property<IJsonOrganization>>
		{
			{
				"Description",
				new Property<IJsonOrganization, string>((IJsonOrganization d, TrelloAuthorization a) => d.Desc, delegate(IJsonOrganization d, string o)
				{
					d.Desc = o;
				})
			},
			{
				"DisplayName",
				new Property<IJsonOrganization, string>((IJsonOrganization d, TrelloAuthorization a) => d.DisplayName, delegate(IJsonOrganization d, string o)
				{
					d.DisplayName = o;
				})
			},
			{
				"Id",
				new Property<IJsonOrganization, string>((IJsonOrganization d, TrelloAuthorization a) => d.Id, delegate(IJsonOrganization d, string o)
				{
					d.Id = o;
				})
			},
			{
				"IsBusinessClass",
				new Property<IJsonOrganization, bool?>((IJsonOrganization d, TrelloAuthorization a) => d.PaidAccount, delegate(IJsonOrganization d, bool? o)
				{
					d.PaidAccount = o;
				})
			},
			{
				"Name",
				new Property<IJsonOrganization, string>((IJsonOrganization d, TrelloAuthorization a) => d.Name, delegate(IJsonOrganization d, string o)
				{
					d.Name = o;
				})
			},
			{
				"Preferences",
				new Property<IJsonOrganization, IJsonOrganizationPreferences>((IJsonOrganization d, TrelloAuthorization a) => d.Prefs, delegate(IJsonOrganization d, IJsonOrganizationPreferences o)
				{
					d.Prefs = o;
				})
			},
			{
				"Url",
				new Property<IJsonOrganization, string>((IJsonOrganization d, TrelloAuthorization a) => d.Url, delegate(IJsonOrganization d, string o)
				{
					d.Url = o;
				})
			},
			{
				"Website",
				new Property<IJsonOrganization, string>((IJsonOrganization d, TrelloAuthorization a) => d.Website, delegate(IJsonOrganization d, string o)
				{
					d.Website = o;
				})
			},
			{
				"ValidForMerge",
				new Property<IJsonOrganization, bool>((IJsonOrganization d, TrelloAuthorization a) => d.ValidForMerge, delegate(IJsonOrganization d, bool o)
				{
					d.ValidForMerge = o;
				}, isHidden: true)
			}
		};
	}

	public OrganizationContext(string id, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		base.Data.Id = id;
		Actions = new ReadOnlyActionCollection(typeof(Organization), () => base.Data.Id, auth);
		Actions.Refreshed += delegate
		{
			OnMerged(new List<string> { "Actions" });
		};
		Boards = new BoardCollection(typeof(Organization), () => base.Data.Id, auth);
		Boards.Refreshed += delegate
		{
			OnMerged(new List<string> { "Boards" });
		};
		Members = new ReadOnlyMemberCollection(EntityRequestType.Organization_Read_Members, () => base.Data.Id, auth);
		Members.Refreshed += delegate
		{
			OnMerged(new List<string> { "Members" });
		};
		Memberships = new OrganizationMembershipCollection(() => base.Data.Id, auth);
		Memberships.Refreshed += delegate
		{
			OnMerged(new List<string> { "Memberships" });
		};
		PowerUpData = new ReadOnlyPowerUpDataCollection(EntityRequestType.Organization_Read_PowerUpData, () => base.Data.Id, auth);
		PowerUpData.Refreshed += delegate
		{
			OnMerged(new List<string> { "PowerUpData" });
		};
		OrganizationPreferencesContext = new OrganizationPreferencesContext(base.Auth);
		OrganizationPreferencesContext organizationPreferencesContext = OrganizationPreferencesContext;
		organizationPreferencesContext.SubmitRequested = (Func<CancellationToken, Task>)Delegate.Combine(organizationPreferencesContext.SubmitRequested, (Func<CancellationToken, Task>)((CancellationToken ct) => HandleSubmitRequested("Preferences", ct)));
		base.Data.Prefs = OrganizationPreferencesContext.Data;
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
			int num = Enum.GetValues(typeof(Organization.Fields)).Cast<Organization.Fields>().ToList()
				.Cast<int>()
				.Sum();
			Organization.Fields enumerationValue = (Organization.Fields)((int)((uint)num & (uint)MemberFields) & (int)Organization.DownloadedFields);
			Parameters["fields"] = enumerationValue.GetDescription();
			Organization.Fields fields = (Organization.Fields)((int)((uint)num & (uint)Organization.DownloadedFields) & (int)(~MemberFields));
			if (fields.HasFlag(Organization.Fields.Actions))
			{
				Parameters["actions"] = "all";
				Parameters["actions_format"] = "list";
			}
			if (fields.HasFlag(Organization.Fields.Boards))
			{
				Parameters["boards"] = "open";
				Parameters["board_fields"] = BoardContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(Organization.Fields.Members))
			{
				Parameters["members"] = "all";
				Parameters["member_fields"] = MemberContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(Organization.Fields.Memberships))
			{
				Parameters["memberships"] = "all";
				Parameters["memberships_member"] = "true";
				Parameters["membership_member_fields"] = MemberContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(Organization.Fields.PowerUpData))
			{
				Parameters["pluginData"] = "true";
			}
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Organization_Read_Refresh, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override Dictionary<string, object> GetParameters()
	{
		return CurrentParameters;
	}

	protected override Endpoint GetDeleteEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Organization_Write_Delete, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override async Task SubmitData(IJsonOrganization json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Organization_Write_Update, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}

	protected override void ApplyDependentChanges(IJsonOrganization json)
	{
		if (OrganizationPreferencesContext.HasChanges)
		{
			json.Prefs = OrganizationPreferencesContext.GetChanges();
			OrganizationPreferencesContext.ClearChanges();
		}
	}

	protected override IEnumerable<string> MergeDependencies(IJsonOrganization json, bool overwrite)
	{
		List<string> list = (from p in OrganizationPreferencesContext.Merge(json.Prefs, overwrite)
			select "Preferences." + p).ToList();
		if (json.Actions != null)
		{
			Actions.Update(json.Actions.Select((IJsonAction a) => a.GetFromCache<Action, IJsonAction>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Actions");
		}
		if (json.Boards != null)
		{
			Boards.Update(json.Boards.Select((IJsonBoard a) => a.GetFromCache<Board, IJsonBoard>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Boards");
		}
		if (json.Members != null)
		{
			Members.Update(json.Members.Select((IJsonMember a) => a.GetFromCache<Member, IJsonMember>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Members");
		}
		if (json.Memberships != null)
		{
			Memberships.Update(json.Memberships.Select((IJsonOrganizationMembership a) => a.TryGetFromCache<OrganizationMembership, IJsonOrganizationMembership>(overwrite) ?? new OrganizationMembership(a, base.Data.Id, base.Auth)));
			list.Add("Memberships");
		}
		if (json.PowerUpData != null)
		{
			PowerUpData.Update(json.PowerUpData.Select((IJsonPowerUpData a) => a.GetFromCache<PowerUpData, IJsonPowerUpData>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("PowerUpData");
		}
		return list;
	}
}
