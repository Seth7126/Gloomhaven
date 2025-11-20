using System;

namespace Script.GUI.SMNavigation;

public interface IShowActivity
{
	Action OnShow { get; set; }

	Action OnHide { get; set; }

	Action<bool> OnActivityChanged { get; set; }

	bool IsActive { get; }
}
