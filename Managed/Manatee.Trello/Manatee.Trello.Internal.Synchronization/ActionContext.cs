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

internal class ActionContext : DeletableSynchronizationContext<IJsonAction>
{
	private static readonly Dictionary<string, object> Parameters;

	private static readonly Action.Fields MemberFields;

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

	public ActionDataContext ActionDataContext { get; }

	public CommentReactionCollection Reactions { get; }

	public virtual bool HasValidId => IdRule.Instance.Validate(base.Data.Id, null) == null;

	static ActionContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = Action.Fields.Data | Action.Fields.Date | Action.Fields.Type;
		SynchronizationContext<IJsonAction>.Properties = new Dictionary<string, Property<IJsonAction>>
		{
			{
				"Creator",
				new Property<IJsonAction, Member>((IJsonAction d, TrelloAuthorization a) => d.MemberCreator.GetFromCache<Member, IJsonMember>(a, overwrite: false, Array.Empty<object>()), delegate(IJsonAction d, Member o)
				{
					if (o != null)
					{
						d.MemberCreator = o.Json;
					}
				})
			},
			{
				"Date",
				new Property<IJsonAction, DateTime?>((IJsonAction d, TrelloAuthorization a) => d.Date, delegate(IJsonAction d, DateTime? o)
				{
					d.Date = o;
				})
			},
			{
				"Id",
				new Property<IJsonAction, string>((IJsonAction d, TrelloAuthorization a) => d.Id, delegate(IJsonAction d, string o)
				{
					d.Id = o;
				})
			},
			{
				"Text",
				new Property<IJsonAction, string>((IJsonAction d, TrelloAuthorization a) => d.Data.Text, delegate(IJsonAction d, string o)
				{
					d.Text = o;
				})
			},
			{
				"Type",
				new Property<IJsonAction, ActionType?>((IJsonAction d, TrelloAuthorization a) => d.Type, delegate(IJsonAction d, ActionType? o)
				{
					d.Type = o;
				})
			}
		};
	}

	public ActionContext(string id, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		base.Data.Id = id;
		ActionDataContext = new ActionDataContext(base.Auth);
		ActionDataContext actionDataContext = ActionDataContext;
		actionDataContext.SubmitRequested = (Func<CancellationToken, Task>)Delegate.Combine(actionDataContext.SubmitRequested, (Func<CancellationToken, Task>)((CancellationToken ct) => HandleSubmitRequested("Text", ct)));
		base.Data.Data = ActionDataContext.Data;
		Reactions = new CommentReactionCollection(() => base.Data.Id, auth);
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
			int num = Enum.GetValues(typeof(Action.Fields)).Cast<Action.Fields>().ToList()
				.Cast<int>()
				.Sum();
			Action.Fields enumerationValue = (Action.Fields)((int)((uint)num & (uint)MemberFields) & (int)Action.DownloadedFields);
			Parameters["fields"] = enumerationValue.GetDescription();
			Action.Fields fields = (Action.Fields)((int)((uint)num & (uint)Action.DownloadedFields) & (int)(~MemberFields));
			if (fields.HasFlag(Action.Fields.Creator))
			{
				Parameters["memberCreator"] = "true";
				Parameters["memberCreator_fields"] = MemberContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(Action.Fields.Reactions))
			{
				Parameters["reactions"] = "true";
			}
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Action_Read_Refresh, new Dictionary<string, object> { 
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
		return EndpointFactory.Build(EntityRequestType.Action_Write_Delete, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override async Task SubmitData(IJsonAction json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Action_Write_Update, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
		base.Data.Data = ActionDataContext.Data;
	}

	protected override void ApplyDependentChanges(IJsonAction json)
	{
		base.Data.Data = ActionDataContext.Data;
		if (ActionDataContext.HasChanges)
		{
			json.Data = ActionDataContext.GetChanges();
			ActionDataContext.ClearChanges();
		}
	}

	protected override IEnumerable<string> MergeDependencies(IJsonAction json, bool overwrite)
	{
		List<string> list = ActionDataContext.Merge(json.Data, overwrite).ToList();
		if (json.Reactions != null)
		{
			Reactions.Update(json.Reactions.Select((IJsonCommentReaction a) => a.GetFromCache<CommentReaction, IJsonCommentReaction>(base.Auth, overwrite, new object[1] { base.Data.Id })));
			list.Add("Actions");
		}
		return list;
	}
}
