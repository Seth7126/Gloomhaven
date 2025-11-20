using System.Collections.Generic;
using UnityEngine;

namespace GLOOM;

public struct Message
{
	public string title;

	public Sprite image;

	public string information;

	public List<Message> content;

	public Message(string title, Sprite image, string information = null, List<Message> content = null)
	{
		this.title = title;
		this.image = image;
		this.information = information;
		this.content = content;
	}

	public Message(string title, string information)
		: this(title, null, information)
	{
	}
}
