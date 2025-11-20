using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Manatee.Trello;

public struct ActionType : IEquatable<ActionType>, IComparable<ActionType>, IComparable
{
	public static readonly ActionType Unknown;

	[Display(Description = "acceptEnterpriseJoinRequest")]
	public static readonly ActionType AcceptEnterpriseJoinRequest;

	[Display(Description = "addAdminToBoard")]
	public static readonly ActionType AddAdminToBoard;

	[Display(Description = "addAdminToOrganization")]
	public static readonly ActionType AddAdminToOrganization;

	[Display(Description = "addAttachmentToCard")]
	public static readonly ActionType AddAttachmentToCard;

	[Display(Description = "addBoardsPinnedToMember")]
	public static readonly ActionType AddBoardsPinnedToMember;

	[Display(Description = "addChecklistToCard")]
	public static readonly ActionType AddChecklistToCard;

	[Display(Description = "addLabelToCard")]
	public static readonly ActionType AddLabelToCard;

	[Display(Description = "addMemberToBoard")]
	public static readonly ActionType AddMemberToBoard;

	[Display(Description = "addMemberToCard")]
	public static readonly ActionType AddMemberToCard;

	[Display(Description = "addMemberToOrganization")]
	public static readonly ActionType AddMemberToOrganization;

	[Display(Description = "addToOrganizationBoard")]
	public static readonly ActionType AddToOrganizationBoard;

	[Display(Description = "addOrganizationToEnterprise")]
	public static readonly ActionType AddOrganizationToEnterprise;

	[Display(Description = "addToEnterprisePluginWhitelist")]
	public static readonly ActionType AddToEnterprisePluginWhitelist;

	[Display(Description = "commentCard")]
	public static readonly ActionType CommentCard;

	[Display(Description = "convertToCardFromCheckItem")]
	public static readonly ActionType ConvertToCardFromCheckItem;

	[Display(Description = "copyBoard")]
	public static readonly ActionType CopyBoard;

	[Display(Description = "copyCard")]
	public static readonly ActionType CopyCard;

	[Display(Description = "copyChecklist")]
	public static readonly ActionType CopyChecklist;

	[Display(Description = "copyCommentCard")]
	public static readonly ActionType CopyCommentCard;

	[Display(Description = "createBoard")]
	public static readonly ActionType CreateBoard;

	[Display(Description = "createBoardInvitation")]
	public static readonly ActionType CreateBoardInvitation;

	[Display(Description = "createBoardPreference")]
	public static readonly ActionType CreateBoardPreference;

	[Display(Description = "createCard")]
	public static readonly ActionType CreateCard;

	[Display(Description = "createChecklist")]
	public static readonly ActionType CreateChecklist;

	[Display(Description = "createCustomField")]
	public static readonly ActionType CreateCustomField;

	[Display(Description = "createLabel")]
	public static readonly ActionType CreateLabel;

	[Display(Description = "createList")]
	public static readonly ActionType CreateList;

	[Display(Description = "createOrganization")]
	public static readonly ActionType CreateOrganization;

	[Display(Description = "createOrganizationInvitation")]
	public static readonly ActionType CreateOrganizationInvitation;

	[Display(Description = "deactivatedMemberInBoard")]
	public static readonly ActionType DeactivatedMemberInBoard;

	[Display(Description = "deactivatedMemberInEnterprise")]
	public static readonly ActionType DeactivatedMemberInEnterprise;

	[Display(Description = "deactivatedMemberInOrganization")]
	public static readonly ActionType DeactivatedMemberInOrganization;

	[Display(Description = "deleteAttachmentFromCard")]
	public static readonly ActionType DeleteAttachmentFromCard;

	[Display(Description = "deleteBoardInvitation")]
	public static readonly ActionType DeleteBoardInvitation;

	[Display(Description = "deleteCard")]
	public static readonly ActionType DeleteCard;

	[Display(Description = "deleteCheckItem")]
	public static readonly ActionType DeleteCheckItem;

	[Display(Description = "deleteCustomField")]
	public static readonly ActionType DeleteCustomField;

	[Display(Description = "deleteLabel")]
	public static readonly ActionType DeleteLabel;

	[Display(Description = "deleteOrganizationInvitation")]
	public static readonly ActionType DeleteOrganizationInvitation;

	[Display(Description = "disableEnterprisePluginWhitelist")]
	public static readonly ActionType DisableEnterprisePluginWhitelist;

	[Display(Description = "disablePlugin")]
	public static readonly ActionType DisablePlugin;

	[Display(Description = "disablePowerUp")]
	public static readonly ActionType DisablePowerUp;

	[Display(Description = "emailCard")]
	public static readonly ActionType EmailCard;

	[Display(Description = "enableEnterprisePluginWhitelist")]
	public static readonly ActionType EnableEnterprisePluginWhitelist;

	[Display(Description = "enablePlugin")]
	public static readonly ActionType EnablePlugin;

	[Display(Description = "enablePowerUp")]
	public static readonly ActionType EnablePowerUp;

	[Display(Description = "makeAdminOfBoard")]
	public static readonly ActionType MakeAdminOfBoard;

	[Display(Description = "makeAdminOfOrganization")]
	public static readonly ActionType MakeAdminOfOrganization;

	[Display(Description = "makeNormalMemberOfBoard")]
	public static readonly ActionType MakeNormalMemberOfBoard;

	[Display(Description = "makeNormalMemberOfOrganization")]
	public static readonly ActionType MakeNormalMemberOfOrganization;

	[Display(Description = "makeObserverOfBoard")]
	public static readonly ActionType MakeObserverOfBoard;

	[Display(Description = "memberJoinedTrello")]
	public static readonly ActionType MemberJoinedTrello;

	[Display(Description = "moveCardFromBoard")]
	public static readonly ActionType MoveCardFromBoard;

	[Display(Description = "moveCardToBoard")]
	public static readonly ActionType MoveCardToBoard;

	[Display(Description = "moveListFromBoard")]
	public static readonly ActionType MoveListFromBoard;

	[Display(Description = "moveListToBoard")]
	public static readonly ActionType MoveListToBoard;

	[Display(Description = "reactionAdded")]
	public static readonly ActionType ReactionAdded;

	[Display(Description = "reactionRemoved")]
	public static readonly ActionType ReactionRemoved;

	[Display(Description = "reactivatedMemberInBoard")]
	public static readonly ActionType ReactivatedMemberInBoard;

	[Display(Description = "reactivatedMemberInEnterprise")]
	public static readonly ActionType ReactivatedMemberInEnterprise;

	[Display(Description = "reactivatedMemberInOrganization")]
	public static readonly ActionType ReactivatedMemberInOrganization;

	[Display(Description = "removeAdminFromBoard")]
	public static readonly ActionType RemoveAdminFromBoard;

	[Display(Description = "removeAdminFromOrganization")]
	public static readonly ActionType RemoveAdminFromOrganization;

	[Display(Description = "removeBoardsPinnedFromMember")]
	public static readonly ActionType RemoveBoardsPinnedFromMember;

	[Display(Description = "removeChecklistFromCard")]
	public static readonly ActionType RemoveChecklistFromCard;

	[Display(Description = "removeFromEnterprisePluginWhitelist")]
	public static readonly ActionType RemoveFromEnterprisePluginWhitelist;

	[Display(Description = "removeFromOrganizationBoard")]
	public static readonly ActionType RemoveFromOrganizationBoard;

	[Display(Description = "removeLabelFromCard")]
	public static readonly ActionType RemoveLabelFromCard;

	[Display(Description = "removeMemberFromBoard")]
	public static readonly ActionType RemoveMemberFromBoard;

	[Display(Description = "removeMemberFromCard")]
	public static readonly ActionType RemoveMemberFromCard;

	[Display(Description = "removeMemberFromOrganization")]
	public static readonly ActionType RemoveMemberFromOrganization;

	[Display(Description = "removeOrganizationFromEnterprise")]
	public static readonly ActionType RemoveOrganizationFromEnterprise;

	[Display(Description = "reopenBoard")]
	public static readonly ActionType ReopenBoard;

	[Display(Description = "unconfirmedBoardInvitation")]
	public static readonly ActionType UnconfirmedBoardInvitation;

	[Display(Description = "unconfirmedOrganizationInvitation")]
	public static readonly ActionType UnconfirmedOrganizationInvitation;

	[Display(Description = "updateBoard")]
	public static readonly ActionType UpdateBoard;

	[Display(Description = "updateCard")]
	public static readonly ActionType UpdateCard;

	[Display(Description = "updateCheckItem")]
	public static readonly ActionType UpdateCheckItem;

	[Display(Description = "updateCheckItemStateOnCard")]
	public static readonly ActionType UpdateCheckItemStateOnCard;

	[Display(Description = "updateCustomField")]
	public static readonly ActionType UpdateCustomField;

	[Display(Description = "updateCustomFieldItem")]
	public static readonly ActionType UpdateCustomFieldItem;

	[Display(Description = "updateChecklist")]
	public static readonly ActionType UpdateChecklist;

	[Display(Description = "updateLabel")]
	public static readonly ActionType UpdateLabel;

	[Display(Description = "updateList")]
	public static readonly ActionType UpdateList;

	[Display(Description = "updateMember")]
	public static readonly ActionType UpdateMember;

	[Display(Description = "updateOrganization")]
	public static readonly ActionType UpdateOrganization;

	[Display(Description = "voteOnCard")]
	public static readonly ActionType VoteOnCard;

	private static readonly int FieldCount;

	private static readonly List<ActionType> FieldValues;

	private BitArray _array;

	private string _description;

	public static ActionType DefaultForCardActions { get; }

	[Display(Description = "all")]
	public static ActionType All { get; }

	private BitArray Bits => _array ?? (_array = new BitArray(FieldCount));

	static ActionType()
	{
		DefaultForCardActions = CommentCard | UpdateCard;
		List<FieldInfo> list = typeof(ActionType).GetTypeInfo().DeclaredFields.Where((FieldInfo f) => f.IsStatic && f.IsPublic).ToList();
		FieldCount = list.Count;
		FieldValues = new List<ActionType>();
		for (int num = 0; num < list.Count; num++)
		{
			FieldInfo fieldInfo = list[num];
			ActionType actionType = new ActionType
			{
				_description = fieldInfo.GetCustomAttribute<DisplayAttribute>()?.Description
			};
			actionType.Bits.Set(num, value: true);
			fieldInfo.SetValue(null, actionType);
			FieldValues.Add(actionType);
		}
		ActionType actionType2 = FieldValues.Aggregate(Unknown, (ActionType c, ActionType a) => a | c);
		actionType2._description = "all";
		All = actionType2;
	}

	public static ActionType operator |(ActionType lhs, ActionType rhs)
	{
		return new ActionType
		{
			_array = new BitArray(lhs.Bits).Or(rhs.Bits)
		};
	}

	public static ActionType operator &(ActionType lhs, ActionType rhs)
	{
		return new ActionType
		{
			_array = new BitArray(lhs.Bits).And(rhs.Bits)
		};
	}

	public static ActionType operator ^(ActionType lhs, ActionType rhs)
	{
		return new ActionType
		{
			_array = new BitArray(lhs.Bits).Xor(rhs.Bits)
		};
	}

	public static bool operator ==(ActionType lhs, ActionType rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(ActionType lhs, ActionType rhs)
	{
		return !(lhs == rhs);
	}

	public override string ToString()
	{
		if (_description != null)
		{
			return _description;
		}
		List<string> list = new List<string>();
		for (int i = 0; i < FieldCount; i++)
		{
			if (Bits[i])
			{
				list.Add(FieldValues[i]._description);
			}
		}
		return string.Join(",", list);
	}

	public override bool Equals(object obj)
	{
		if (obj is ActionType other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 17;
		for (int i = 0; i < Bits.Length; i++)
		{
			if (Bits[i])
			{
				num ^= i;
			}
		}
		return num;
	}

	public bool Equals(ActionType other)
	{
		for (int i = 0; i < Bits.Length; i++)
		{
			if (Bits[i] != other.Bits[i])
			{
				return false;
			}
		}
		return true;
	}

	public int CompareTo(ActionType other)
	{
		for (int num = Bits.Length - 1; num >= 0; num--)
		{
			bool flag = Bits[num];
			bool flag2 = other.Bits[num];
			if (flag && !flag2)
			{
				return 1;
			}
			if (!flag && flag2)
			{
				return -1;
			}
		}
		return 0;
	}

	int IComparable.CompareTo(object obj)
	{
		if (obj is ActionType)
		{
			return CompareTo((ActionType)obj);
		}
		return -1;
	}

	public TypeCode GetTypeCode()
	{
		return TypeCode.Object;
	}

	public bool HasFlag(ActionType flags)
	{
		return (this & flags) == flags;
	}

	public IEnumerable<ActionType> GetFlags()
	{
		return FieldValues.Where(HasFlag).ToList();
	}

	public static string[] GetNames()
	{
		return FieldValues.Select((ActionType x) => x._description).ToArray();
	}

	public static ActionType[] GetValues()
	{
		return FieldValues.ToArray();
	}

	public static bool TryParse(string s, out ActionType result)
	{
		result = default(ActionType);
		if (string.IsNullOrEmpty(s))
		{
			return true;
		}
		string[] array = s.Split(new char[1] { ',' });
		foreach (string f in array)
		{
			ActionType item = FieldValues.FirstOrDefault((ActionType x) => string.Equals(x._description, f.Trim(), StringComparison.OrdinalIgnoreCase));
			if (item._description == null)
			{
				result = Unknown;
				return false;
			}
			result.Bits.Set(FieldValues.IndexOf(item), value: true);
		}
		return true;
	}
}
