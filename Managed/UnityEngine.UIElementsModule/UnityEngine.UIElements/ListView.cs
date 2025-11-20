using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UIElements;

public class ListView : BaseVerticalCollectionView
{
	public new class UxmlFactory : UxmlFactory<ListView, UxmlTraits>
	{
	}

	public new class UxmlTraits : BindableElement.UxmlTraits
	{
		private readonly UxmlIntAttributeDescription m_FixedItemHeight = new UxmlIntAttributeDescription
		{
			name = "fixed-item-height",
			obsoleteNames = new string[1] { "itemHeight, item-height" },
			defaultValue = BaseVerticalCollectionView.s_DefaultItemHeight
		};

		private readonly UxmlEnumAttributeDescription<CollectionVirtualizationMethod> m_VirtualizationMethod = new UxmlEnumAttributeDescription<CollectionVirtualizationMethod>
		{
			name = "virtualization-method",
			defaultValue = CollectionVirtualizationMethod.FixedHeight
		};

		private readonly UxmlBoolAttributeDescription m_ShowBorder = new UxmlBoolAttributeDescription
		{
			name = "show-border",
			defaultValue = false
		};

		private readonly UxmlEnumAttributeDescription<SelectionType> m_SelectionType = new UxmlEnumAttributeDescription<SelectionType>
		{
			name = "selection-type",
			defaultValue = SelectionType.Single
		};

		private readonly UxmlEnumAttributeDescription<AlternatingRowBackground> m_ShowAlternatingRowBackgrounds = new UxmlEnumAttributeDescription<AlternatingRowBackground>
		{
			name = "show-alternating-row-backgrounds",
			defaultValue = AlternatingRowBackground.None
		};

		private readonly UxmlBoolAttributeDescription m_ShowFoldoutHeader = new UxmlBoolAttributeDescription
		{
			name = "show-foldout-header",
			defaultValue = false
		};

		private readonly UxmlStringAttributeDescription m_HeaderTitle = new UxmlStringAttributeDescription
		{
			name = "header-title",
			defaultValue = string.Empty
		};

		private readonly UxmlBoolAttributeDescription m_ShowAddRemoveFooter = new UxmlBoolAttributeDescription
		{
			name = "show-add-remove-footer",
			defaultValue = false
		};

		private readonly UxmlBoolAttributeDescription m_Reorderable = new UxmlBoolAttributeDescription
		{
			name = "reorderable",
			defaultValue = false
		};

		private readonly UxmlEnumAttributeDescription<ListViewReorderMode> m_ReorderMode = new UxmlEnumAttributeDescription<ListViewReorderMode>
		{
			name = "reorder-mode",
			defaultValue = ListViewReorderMode.Simple
		};

		private readonly UxmlBoolAttributeDescription m_ShowBoundCollectionSize = new UxmlBoolAttributeDescription
		{
			name = "show-bound-collection-size",
			defaultValue = true
		};

		private readonly UxmlBoolAttributeDescription m_HorizontalScrollingEnabled = new UxmlBoolAttributeDescription
		{
			name = "horizontal-scrolling",
			defaultValue = false
		};

		public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
		{
			get
			{
				yield break;
			}
		}

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			int value = 0;
			ListView listView = (ListView)ve;
			listView.reorderable = m_Reorderable.GetValueFromBag(bag, cc);
			if (m_FixedItemHeight.TryGetValueFromBag(bag, cc, ref value))
			{
				listView.fixedItemHeight = value;
			}
			listView.reorderMode = m_ReorderMode.GetValueFromBag(bag, cc);
			listView.virtualizationMethod = m_VirtualizationMethod.GetValueFromBag(bag, cc);
			listView.showBorder = m_ShowBorder.GetValueFromBag(bag, cc);
			listView.selectionType = m_SelectionType.GetValueFromBag(bag, cc);
			listView.showAlternatingRowBackgrounds = m_ShowAlternatingRowBackgrounds.GetValueFromBag(bag, cc);
			listView.showFoldoutHeader = m_ShowFoldoutHeader.GetValueFromBag(bag, cc);
			listView.headerTitle = m_HeaderTitle.GetValueFromBag(bag, cc);
			listView.showAddRemoveFooter = m_ShowAddRemoveFooter.GetValueFromBag(bag, cc);
			listView.showBoundCollectionSize = m_ShowBoundCollectionSize.GetValueFromBag(bag, cc);
			listView.horizontalScrollingEnabled = m_HorizontalScrollingEnabled.GetValueFromBag(bag, cc);
		}
	}

	private bool m_ShowBoundCollectionSize = true;

	private bool m_ShowFoldoutHeader;

	private string m_HeaderTitle;

	private Label m_EmptyListLabel;

	private Foldout m_Foldout;

	private TextField m_ArraySizeField;

	private VisualElement m_Footer;

	private Button m_AddButton;

	private Button m_RemoveButton;

	private Action<IEnumerable<int>> m_ItemAddedCallback;

	private Action<IEnumerable<int>> m_ItemRemovedCallback;

	private Action m_ItemsSourceSizeChangedCallback;

	private ListViewController m_ListViewController;

	private ListViewReorderMode m_ReorderMode;

	public new static readonly string ussClassName = "unity-list-view";

	public new static readonly string itemUssClassName = ussClassName + "__item";

	public static readonly string emptyLabelUssClassName = ussClassName + "__empty-label";

	public static readonly string reorderableUssClassName = ussClassName + "__reorderable";

	public static readonly string reorderableItemUssClassName = reorderableUssClassName + "-item";

	public static readonly string reorderableItemContainerUssClassName = reorderableItemUssClassName + "__container";

	public static readonly string reorderableItemHandleUssClassName = reorderableUssClassName + "-handle";

	public static readonly string reorderableItemHandleBarUssClassName = reorderableItemHandleUssClassName + "-bar";

	public static readonly string footerUssClassName = ussClassName + "__footer";

	public static readonly string foldoutHeaderUssClassName = ussClassName + "__foldout-header";

	public static readonly string arraySizeFieldUssClassName = ussClassName + "__size-field";

	public static readonly string listViewWithHeaderUssClassName = ussClassName + "--with-header";

	public static readonly string listViewWithFooterUssClassName = ussClassName + "--with-footer";

	public static readonly string scrollViewWithFooterUssClassName = ussClassName + "__scroll-view--with-footer";

	internal static readonly string footerAddButtonName = ussClassName + "__add-button";

	internal static readonly string footerRemoveButtonName = ussClassName + "__remove-button";

	public bool showBoundCollectionSize
	{
		get
		{
			return m_ShowBoundCollectionSize;
		}
		set
		{
			if (m_ShowBoundCollectionSize != value)
			{
				m_ShowBoundCollectionSize = value;
				SetupArraySizeField();
			}
		}
	}

	internal override bool sourceIncludesArraySize => showBoundCollectionSize && base.binding != null && !showFoldoutHeader;

	public bool showFoldoutHeader
	{
		get
		{
			return m_ShowFoldoutHeader;
		}
		set
		{
			if (m_ShowFoldoutHeader == value)
			{
				return;
			}
			m_ShowFoldoutHeader = value;
			EnableInClassList(listViewWithHeaderUssClassName, value);
			if (m_ShowFoldoutHeader)
			{
				if (m_Foldout != null)
				{
					return;
				}
				m_Foldout = new Foldout
				{
					name = foldoutHeaderUssClassName,
					text = m_HeaderTitle
				};
				m_Foldout.AddToClassList(foldoutHeaderUssClassName);
				m_Foldout.tabIndex = 1;
				base.hierarchy.Add(m_Foldout);
				m_Foldout.Add(base.scrollView);
			}
			else if (m_Foldout != null)
			{
				m_Foldout?.RemoveFromHierarchy();
				m_Foldout = null;
				base.hierarchy.Add(base.scrollView);
			}
			SetupArraySizeField();
			UpdateEmpty();
			if (showAddRemoveFooter)
			{
				EnableFooter(enabled: true);
			}
		}
	}

	public string headerTitle
	{
		get
		{
			return m_HeaderTitle;
		}
		set
		{
			m_HeaderTitle = value;
			if (m_Foldout != null)
			{
				m_Foldout.text = m_HeaderTitle;
			}
		}
	}

	public bool showAddRemoveFooter
	{
		get
		{
			return m_Footer != null;
		}
		set
		{
			EnableFooter(value);
		}
	}

	internal new ListViewController viewController => m_ListViewController;

	public ListViewReorderMode reorderMode
	{
		get
		{
			return m_ReorderMode;
		}
		set
		{
			if (value != m_ReorderMode)
			{
				m_ReorderMode = value;
				InitializeDragAndDropController();
				Rebuild();
			}
		}
	}

	public event Action<IEnumerable<int>> itemsAdded;

	public event Action<IEnumerable<int>> itemsRemoved;

	private void SetupArraySizeField()
	{
		if (sourceIncludesArraySize || !showFoldoutHeader || !showBoundCollectionSize)
		{
			m_ArraySizeField?.RemoveFromHierarchy();
			m_ArraySizeField = null;
			return;
		}
		m_ArraySizeField = new TextField
		{
			name = arraySizeFieldUssClassName
		};
		m_ArraySizeField.AddToClassList(arraySizeFieldUssClassName);
		m_ArraySizeField.RegisterValueChangedCallback(OnArraySizeFieldChanged);
		m_ArraySizeField.isDelayed = true;
		m_ArraySizeField.focusable = true;
		base.hierarchy.Add(m_ArraySizeField);
		UpdateArraySizeField();
	}

	private void EnableFooter(bool enabled)
	{
		EnableInClassList(listViewWithFooterUssClassName, enabled);
		base.scrollView.EnableInClassList(scrollViewWithFooterUssClassName, enabled);
		if (enabled)
		{
			if (m_Footer == null)
			{
				m_Footer = new VisualElement
				{
					name = footerUssClassName
				};
				m_Footer.AddToClassList(footerUssClassName);
				m_RemoveButton = new Button(OnRemoveClicked)
				{
					name = footerRemoveButtonName,
					text = "-"
				};
				m_Footer.Add(m_RemoveButton);
				m_AddButton = new Button(OnAddClicked)
				{
					name = footerAddButtonName,
					text = "+"
				};
				m_Footer.Add(m_AddButton);
			}
			if (m_Foldout != null)
			{
				m_Foldout.contentContainer.Add(m_Footer);
			}
			else
			{
				base.hierarchy.Add(m_Footer);
			}
		}
		else
		{
			m_RemoveButton?.RemoveFromHierarchy();
			m_AddButton?.RemoveFromHierarchy();
			m_Footer?.RemoveFromHierarchy();
			m_RemoveButton = null;
			m_AddButton = null;
			m_Footer = null;
		}
	}

	private void AddItems(int itemCount)
	{
		viewController.AddItems(itemCount);
	}

	private void RemoveItems(List<int> indices)
	{
		viewController.RemoveItems(indices);
	}

	private void OnArraySizeFieldChanged(ChangeEvent<string> evt)
	{
		if (!int.TryParse(evt.newValue, out var result) || result < 0)
		{
			m_ArraySizeField.SetValueWithoutNotify(evt.previousValue);
			return;
		}
		int itemCount = viewController.GetItemCount();
		if (result > itemCount)
		{
			viewController.AddItems(result - itemCount);
		}
		else if (result < itemCount)
		{
			int num = itemCount;
			for (int num2 = num - 1; num2 >= result; num2--)
			{
				viewController.RemoveItem(num2);
			}
		}
	}

	private void UpdateArraySizeField()
	{
		if (HasValidDataAndBindings())
		{
			m_ArraySizeField?.SetValueWithoutNotify(viewController.GetItemCount().ToString());
		}
	}

	private void UpdateEmpty()
	{
		if (!HasValidDataAndBindings())
		{
			return;
		}
		if (base.itemsSource.Count == 0 && !sourceIncludesArraySize)
		{
			if (m_EmptyListLabel == null)
			{
				m_EmptyListLabel = new Label("List is Empty");
				m_EmptyListLabel.AddToClassList(emptyLabelUssClassName);
				base.scrollView.contentViewport.Add(m_EmptyListLabel);
			}
		}
		else
		{
			m_EmptyListLabel?.RemoveFromHierarchy();
			m_EmptyListLabel = null;
		}
	}

	private void OnAddClicked()
	{
		AddItems(1);
		if (base.binding == null)
		{
			SetSelection(base.itemsSource.Count - 1);
			ScrollToItem(-1);
			return;
		}
		base.schedule.Execute((Action)delegate
		{
			SetSelection(base.itemsSource.Count - 1);
			ScrollToItem(-1);
		}).ExecuteLater(100L);
	}

	private void OnRemoveClicked()
	{
		if (base.selectedIndices.Any())
		{
			viewController.RemoveItems(base.selectedIndices.ToList());
			ClearSelection();
		}
		else if (base.itemsSource.Count > 0)
		{
			int index = base.itemsSource.Count - 1;
			viewController.RemoveItem(index);
		}
	}

	private protected override void CreateVirtualizationController()
	{
		CreateVirtualizationController<ReusableListViewItem>();
	}

	private protected override void CreateViewController()
	{
		SetViewController(new ListViewController());
	}

	internal void SetViewController(ListViewController controller)
	{
		if (m_ItemAddedCallback == null)
		{
			m_ItemAddedCallback = OnItemAdded;
		}
		if (m_ItemRemovedCallback == null)
		{
			m_ItemRemovedCallback = OnItemsRemoved;
		}
		if (m_ItemsSourceSizeChangedCallback == null)
		{
			m_ItemsSourceSizeChangedCallback = OnItemsSourceSizeChanged;
		}
		if (m_ListViewController != null)
		{
			m_ListViewController.itemsAdded -= m_ItemAddedCallback;
			m_ListViewController.itemsRemoved -= m_ItemRemovedCallback;
			m_ListViewController.itemsSourceSizeChanged -= m_ItemsSourceSizeChangedCallback;
		}
		SetViewController((CollectionViewController)controller);
		m_ListViewController = controller;
		if (m_ListViewController != null)
		{
			m_ListViewController.itemsAdded += m_ItemAddedCallback;
			m_ListViewController.itemsRemoved += m_ItemRemovedCallback;
			m_ListViewController.itemsSourceSizeChanged += m_ItemsSourceSizeChangedCallback;
		}
	}

	private void OnItemAdded(IEnumerable<int> indices)
	{
		this.itemsAdded?.Invoke(indices);
	}

	private void OnItemsRemoved(IEnumerable<int> indices)
	{
		this.itemsRemoved?.Invoke(indices);
	}

	private void OnItemsSourceSizeChanged()
	{
		RefreshItems();
	}

	internal override ListViewDragger CreateDragger()
	{
		if (m_ReorderMode == ListViewReorderMode.Simple)
		{
			return new ListViewDragger(this);
		}
		return new ListViewDraggerAnimated(this);
	}

	internal override ICollectionDragAndDropController CreateDragAndDropController()
	{
		return new ListViewReorderableDragAndDropController(this);
	}

	public ListView()
	{
		AddToClassList(ussClassName);
	}

	public ListView(IList itemsSource, float itemHeight = -1f, Func<VisualElement> makeItem = null, Action<VisualElement, int> bindItem = null)
		: base(itemsSource, itemHeight, makeItem, bindItem)
	{
		AddToClassList(ussClassName);
	}

	private protected override void PostRefresh()
	{
		UpdateArraySizeField();
		UpdateEmpty();
		base.PostRefresh();
	}
}
