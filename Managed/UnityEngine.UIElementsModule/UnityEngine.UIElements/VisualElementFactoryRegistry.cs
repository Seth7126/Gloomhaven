using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine.UIElements;

internal class VisualElementFactoryRegistry
{
	private static Dictionary<string, List<IUxmlFactory>> s_Factories;

	internal static Dictionary<string, List<IUxmlFactory>> factories
	{
		get
		{
			if (s_Factories == null)
			{
				s_Factories = new Dictionary<string, List<IUxmlFactory>>();
				RegisterEngineFactories();
				RegisterUserFactories();
			}
			return s_Factories;
		}
	}

	protected static void RegisterFactory(IUxmlFactory factory)
	{
		if (factories.TryGetValue(factory.uxmlQualifiedName, out var value))
		{
			foreach (IUxmlFactory item in value)
			{
				if ((object)item.GetType() == factory.GetType())
				{
					throw new ArgumentException("A factory for the type " + factory.GetType().FullName + " was already registered");
				}
			}
			value.Add(factory);
		}
		else
		{
			value = new List<IUxmlFactory>();
			value.Add(factory);
			factories.Add(factory.uxmlQualifiedName, value);
		}
	}

	internal static bool TryGetValue(string fullTypeName, out List<IUxmlFactory> factoryList)
	{
		factoryList = null;
		return factories.TryGetValue(fullTypeName, out factoryList);
	}

	private static void RegisterEngineFactories()
	{
		IUxmlFactory[] array = new IUxmlFactory[34]
		{
			new UxmlRootElementFactory(),
			new UxmlTemplateFactory(),
			new UxmlStyleFactory(),
			new UxmlAttributeOverridesFactory(),
			new Button.UxmlFactory(),
			new VisualElement.UxmlFactory(),
			new IMGUIContainer.UxmlFactory(),
			new Image.UxmlFactory(),
			new Label.UxmlFactory(),
			new RepeatButton.UxmlFactory(),
			new ScrollView.UxmlFactory(),
			new Scroller.UxmlFactory(),
			new Slider.UxmlFactory(),
			new SliderInt.UxmlFactory(),
			new MinMaxSlider.UxmlFactory(),
			new GroupBox.UxmlFactory(),
			new RadioButton.UxmlFactory(),
			new RadioButtonGroup.UxmlFactory(),
			new Toggle.UxmlFactory(),
			new TextField.UxmlFactory(),
			new TemplateContainer.UxmlFactory(),
			new Box.UxmlFactory(),
			new DropdownField.UxmlFactory(),
			new HelpBox.UxmlFactory(),
			new PopupWindow.UxmlFactory(),
			new ProgressBar.UxmlFactory(),
			new ListView.UxmlFactory(),
			new TwoPaneSplitView.UxmlFactory(),
			new InternalTreeView.UxmlFactory(),
			new TreeView.UxmlFactory(),
			new Foldout.UxmlFactory(),
			new BindableElement.UxmlFactory(),
			new TextElement.UxmlFactory(),
			new ButtonStripField.UxmlFactory()
		};
		IUxmlFactory[] array2 = array;
		foreach (IUxmlFactory factory in array2)
		{
			RegisterFactory(factory);
		}
	}

	internal static void RegisterUserFactories()
	{
		HashSet<string> hashSet = new HashSet<string>(ScriptingRuntime.GetAllUserAssemblies());
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			if (!hashSet.Contains(assembly.GetName().Name + ".dll") || assembly.GetName().Name == "UnityEngine.UIElementsModule")
			{
				continue;
			}
			Type[] types = assembly.GetTypes();
			Type[] array2 = types;
			foreach (Type type in array2)
			{
				if (typeof(IUxmlFactory).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract && !type.IsGenericType)
				{
					IUxmlFactory factory = (IUxmlFactory)Activator.CreateInstance(type);
					RegisterFactory(factory);
				}
			}
		}
	}
}
