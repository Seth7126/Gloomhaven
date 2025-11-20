using System;
using System.ComponentModel.DataAnnotations;

namespace Manatee.Trello;

[Flags]
public enum NotificationType
{
	Unknown = 0,
	[Display(Description = "addedAttachmentToCard")]
	AddedAttachmentToCard = 1,
	[Display(Description = "addedToBoard")]
	AddedToBoard = 2,
	[Display(Description = "addedToCard")]
	AddedToCard = 4,
	[Display(Description = "addedToOrganization")]
	AddedToOrganization = 8,
	[Display(Description = "addedMemberToCard")]
	AddedMemberToCard = 0x10,
	[Display(Description = "addAdminToBoard")]
	AddAdminToBoard = 0x20,
	[Display(Description = "addAdminToOrganization")]
	AddAdminToOrganization = 0x40,
	[Display(Description = "changeCard")]
	ChangeCard = 0x80,
	[Display(Description = "closeBoard")]
	CloseBoard = 0x100,
	[Display(Description = "commentCard")]
	CommentCard = 0x200,
	[Display(Description = "createdCard")]
	CreatedCard = 0x400,
	[Display(Description = "removedFromBoard")]
	RemovedFromBoard = 0x800,
	[Display(Description = "removedFromCard")]
	RemovedFromCard = 0x1000,
	[Display(Description = "removedMemberFromCard")]
	RemovedMemberFromCard = 0x2000,
	[Display(Description = "removedFromOrganization")]
	RemovedFromOrganization = 0x4000,
	[Display(Description = "mentionedOnCard")]
	MentionedOnCard = 0x8000,
	[Display(Description = "updateCheckItemStateOnCard")]
	UpdateCheckItemStateOnCard = 0x10000,
	[Display(Description = "makeAdminOfBoard")]
	MakeAdminOfBoard = 0x20000,
	[Display(Description = "makeAdminOfOrganization")]
	MakeAdminOfOrganization = 0x40000,
	[Display(Description = "cardDueSoon")]
	CardDueSoon = 0x80000,
	[Display(Description = "addAttachmentToCard")]
	AddAttachmentToCard = 0x100000,
	[Display(Description = "memberJoinedTrello")]
	MemberJoinedTrello = 0x200000,
	[Display(Description = "reactionAdded")]
	ReactionAdded = 0x400000,
	[Display(Description = "reactionRemoved")]
	ReactionRemoved = 0x800000,
	[Display(Description = "reopenBoard")]
	ReopenBoard = 0x1000000,
	[Display(Description = "all")]
	All = 0x1FFFFFF
}
