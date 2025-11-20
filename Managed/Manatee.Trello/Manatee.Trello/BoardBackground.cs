using System;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class BoardBackground : IBoardBackground, ICacheable, IMergeJson<IJsonBoardBackground>
{
	private static BoardBackground _blue;

	private static BoardBackground _orange;

	private static BoardBackground _green;

	private static BoardBackground _red;

	private static BoardBackground _purple;

	private static BoardBackground _pink;

	private static BoardBackground _lime;

	private static BoardBackground _sky;

	private static BoardBackground _grey;

	private readonly Field<WebColor> _color;

	private readonly Field<WebColor> _bottomColor;

	private readonly Field<WebColor> _topColor;

	private readonly Field<string> _image;

	private readonly Field<bool?> _isTiled;

	private readonly Field<BoardBackgroundBrightness?> _brightness;

	private readonly Field<BoardBackgroundType?> _type;

	private readonly BoardBackgroundContext _context;

	public static BoardBackground Blue => _blue ?? (_blue = new BoardBackground("blue"));

	public static BoardBackground Orange => _orange ?? (_orange = new BoardBackground("orange"));

	public static BoardBackground Green => _green ?? (_green = new BoardBackground("green"));

	public static BoardBackground Red => _red ?? (_red = new BoardBackground("red"));

	public static BoardBackground Purple => _purple ?? (_purple = new BoardBackground("purple"));

	public static BoardBackground Pink => _pink ?? (_pink = new BoardBackground("pink"));

	public static BoardBackground Lime => _lime ?? (_lime = new BoardBackground("lime"));

	public static BoardBackground Sky => _sky ?? (_sky = new BoardBackground("sky"));

	public static BoardBackground Grey => _grey ?? (_grey = new BoardBackground("grey"));

	public WebColor BottomColor => _bottomColor.Value;

	public BoardBackgroundBrightness? Brightness => _brightness.Value;

	public WebColor Color => _color.Value;

	public string Id { get; }

	public string Image => _image.Value;

	public bool? IsTiled => _isTiled.Value;

	public IReadOnlyCollection<IImagePreview> ScaledImages { get; }

	public WebColor TopColor => _topColor.Value;

	public BoardBackgroundType? Type => _type.Value;

	internal IJsonBoardBackground Json
	{
		get
		{
			return _context.Data;
		}
		set
		{
			_context.Merge(value);
		}
	}

	internal BoardBackground(string ownerId, IJsonBoardBackground json, TrelloAuthorization auth)
	{
		Id = json.Id;
		_context = new BoardBackgroundContext(ownerId, auth);
		_context.Merge(json);
		_brightness = new Field<BoardBackgroundBrightness?>(_context, "Brightness");
		_color = new Field<WebColor>(_context, "Color");
		_topColor = new Field<WebColor>(_context, "TopColor");
		_bottomColor = new Field<WebColor>(_context, "BottomColor");
		_image = new Field<string>(_context, "Image");
		_isTiled = new Field<bool?>(_context, "IsTiled");
		_type = new Field<BoardBackgroundType?>(_context, "Type");
		ScaledImages = new ReadOnlyBoardBackgroundScalesCollection(_context, auth);
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	private BoardBackground(string id)
	{
		Id = id;
		_context = new BoardBackgroundContext(TrelloAuthorization.Default);
		Json.Id = id;
		_brightness = new Field<BoardBackgroundBrightness?>(_context, "Brightness");
		_color = new Field<WebColor>(_context, "Color");
		_topColor = new Field<WebColor>(_context, "TopColor");
		_bottomColor = new Field<WebColor>(_context, "BottomColor");
		_image = new Field<string>(_context, "Image");
		_isTiled = new Field<bool?>(_context, "IsTiled");
		_type = new Field<BoardBackgroundType?>(_context, "Type");
		ScaledImages = new ReadOnlyBoardBackgroundScalesCollection(_context, TrelloAuthorization.Default);
		TrelloConfiguration.Cache.Add(this);
	}

	public Task Delete(CancellationToken ct = default(CancellationToken))
	{
		if (Type != BoardBackgroundType.Custom)
		{
			throw new InvalidOperationException("Cannot delete Trello-provided board backgrounds.");
		}
		return _context.Delete(ct);
	}

	public override string ToString()
	{
		return Id;
	}

	void IMergeJson<IJsonBoardBackground>.Merge(IJsonBoardBackground json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}
}
