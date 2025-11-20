using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICard : ICanWebhook, ICacheable, IQueryable, IRefreshable
{
	IReadOnlyActionCollection Actions { get; }

	IAttachmentCollection Attachments { get; }

	IBadges Badges { get; }

	IBoard Board { get; }

	ICheckListCollection CheckLists { get; }

	ICommentCollection Comments { get; }

	DateTime CreationDate { get; }

	IReadOnlyCollection<ICustomField> CustomFields { get; }

	string Description { get; set; }

	DateTime? DueDate { get; set; }

	bool? IsArchived { get; set; }

	bool? IsComplete { get; set; }

	bool? IsSubscribed { get; set; }

	ICardLabelCollection Labels { get; }

	DateTime? LastActivity { get; }

	IList List { get; set; }

	IMemberCollection Members { get; }

	string Name { get; set; }

	Position Position { get; set; }

	IReadOnlyCollection<IPowerUpData> PowerUpData { get; }

	int? ShortId { get; }

	string ShortUrl { get; }

	ICardStickerCollection Stickers { get; }

	string Url { get; }

	IReadOnlyMemberCollection VotingMembers { get; }

	ICheckList this[string key] { get; }

	ICheckList this[int index] { get; }

	event Action<ICard, IEnumerable<string>> Updated;

	Task Delete(CancellationToken ct = default(CancellationToken));
}
