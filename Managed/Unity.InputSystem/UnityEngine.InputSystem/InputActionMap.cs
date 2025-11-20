using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem;

[Serializable]
public sealed class InputActionMap : ICloneable, ISerializationCallbackReceiver, IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	internal struct DeviceArray
	{
		private bool m_HaveValue;

		private int m_DeviceCount;

		private InputDevice[] m_DeviceArray;

		public int IndexOf(InputDevice device)
		{
			return m_DeviceArray.IndexOfReference(device, m_DeviceCount);
		}

		public bool Remove(InputDevice device)
		{
			int num = IndexOf(device);
			if (num < 0)
			{
				return false;
			}
			m_DeviceArray.EraseAtWithCapacity(ref m_DeviceCount, num);
			return true;
		}

		public ReadOnlyArray<InputDevice>? Get()
		{
			if (!m_HaveValue)
			{
				return null;
			}
			return new ReadOnlyArray<InputDevice>(m_DeviceArray, 0, m_DeviceCount);
		}

		public bool Set(ReadOnlyArray<InputDevice>? devices)
		{
			if (!devices.HasValue)
			{
				if (!m_HaveValue)
				{
					return false;
				}
				if (m_DeviceCount > 0)
				{
					Array.Clear(m_DeviceArray, 0, m_DeviceCount);
				}
				m_DeviceCount = 0;
				m_HaveValue = false;
			}
			else
			{
				ReadOnlyArray<InputDevice> value = devices.Value;
				if (m_HaveValue && value.Count == m_DeviceCount && value.HaveEqualReferences(m_DeviceArray, m_DeviceCount))
				{
					return false;
				}
				if (m_DeviceCount > 0)
				{
					m_DeviceArray.Clear(ref m_DeviceCount);
				}
				m_HaveValue = true;
				m_DeviceCount = 0;
				ArrayHelpers.AppendListWithCapacity(ref m_DeviceArray, ref m_DeviceCount, value);
			}
			return true;
		}
	}

	[Serializable]
	internal struct BindingOverrideListJson
	{
		public List<BindingOverrideJson> bindings;
	}

	[Serializable]
	internal struct BindingOverrideJson
	{
		public string action;

		public string id;

		public string path;

		public string interactions;

		public string processors;

		public static BindingOverrideJson FromBinding(InputBinding binding)
		{
			return new BindingOverrideJson
			{
				action = binding.action,
				id = binding.m_Id,
				path = binding.overridePath,
				interactions = binding.overrideInteractions,
				processors = binding.overrideProcessors
			};
		}
	}

	[Serializable]
	internal struct BindingJson
	{
		public string name;

		public string id;

		public string path;

		public string interactions;

		public string processors;

		public string groups;

		public string action;

		public bool isComposite;

		public bool isPartOfComposite;

		public InputBinding ToBinding()
		{
			return new InputBinding
			{
				name = (string.IsNullOrEmpty(name) ? null : name),
				m_Id = (string.IsNullOrEmpty(id) ? null : id),
				path = path,
				action = (string.IsNullOrEmpty(action) ? null : action),
				interactions = (string.IsNullOrEmpty(interactions) ? null : interactions),
				processors = (string.IsNullOrEmpty(processors) ? null : processors),
				groups = (string.IsNullOrEmpty(groups) ? null : groups),
				isComposite = isComposite,
				isPartOfComposite = isPartOfComposite
			};
		}

		public static BindingJson FromBinding(ref InputBinding binding)
		{
			return new BindingJson
			{
				name = binding.name,
				id = binding.m_Id,
				path = binding.path,
				action = binding.action,
				interactions = binding.interactions,
				processors = binding.processors,
				groups = binding.groups,
				isComposite = binding.isComposite,
				isPartOfComposite = binding.isPartOfComposite
			};
		}
	}

	[Serializable]
	internal struct ReadActionJson
	{
		public string name;

		public string type;

		public string id;

		public string expectedControlType;

		public string expectedControlLayout;

		public string processors;

		public string interactions;

		public bool passThrough;

		public bool initialStateCheck;

		public BindingJson[] bindings;

		public InputAction ToAction(string actionName = null)
		{
			if (!string.IsNullOrEmpty(expectedControlLayout))
			{
				expectedControlType = expectedControlLayout;
			}
			InputActionType inputActionType = InputActionType.Value;
			if (!string.IsNullOrEmpty(type))
			{
				inputActionType = (InputActionType)Enum.Parse(typeof(InputActionType), type, ignoreCase: true);
			}
			else if (passThrough)
			{
				inputActionType = InputActionType.PassThrough;
			}
			else if (initialStateCheck)
			{
				inputActionType = InputActionType.Value;
			}
			else if (!string.IsNullOrEmpty(expectedControlType) && (expectedControlType == "Button" || expectedControlType == "Key"))
			{
				inputActionType = InputActionType.Button;
			}
			return new InputAction(actionName ?? name, inputActionType)
			{
				m_Id = (string.IsNullOrEmpty(id) ? null : id),
				m_ExpectedControlType = ((!string.IsNullOrEmpty(expectedControlType)) ? expectedControlType : null),
				m_Processors = processors,
				m_Interactions = interactions,
				wantsInitialStateCheck = initialStateCheck
			};
		}
	}

	[Serializable]
	internal struct WriteActionJson
	{
		public string name;

		public string type;

		public string id;

		public string expectedControlType;

		public string processors;

		public string interactions;

		public bool initialStateCheck;

		public static WriteActionJson FromAction(InputAction action)
		{
			return new WriteActionJson
			{
				name = action.m_Name,
				type = action.m_Type.ToString(),
				id = action.m_Id,
				expectedControlType = action.m_ExpectedControlType,
				processors = action.processors,
				interactions = action.interactions,
				initialStateCheck = action.wantsInitialStateCheck
			};
		}
	}

	[Serializable]
	internal struct ReadMapJson
	{
		public string name;

		public string id;

		public ReadActionJson[] actions;

		public BindingJson[] bindings;
	}

	[Serializable]
	internal struct WriteMapJson
	{
		public string name;

		public string id;

		public WriteActionJson[] actions;

		public BindingJson[] bindings;

		public static WriteMapJson FromMap(InputActionMap map)
		{
			WriteActionJson[] array = null;
			BindingJson[] array2 = null;
			InputAction[] array3 = map.m_Actions;
			if (array3 != null)
			{
				int num = array3.Length;
				array = new WriteActionJson[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = WriteActionJson.FromAction(array3[i]);
				}
			}
			InputBinding[] array4 = map.m_Bindings;
			if (array4 != null)
			{
				int num2 = array4.Length;
				array2 = new BindingJson[num2];
				for (int j = 0; j < num2; j++)
				{
					array2[j] = BindingJson.FromBinding(ref array4[j]);
				}
			}
			return new WriteMapJson
			{
				name = map.name,
				id = map.id.ToString(),
				actions = array,
				bindings = array2
			};
		}
	}

	[Serializable]
	internal struct WriteFileJson
	{
		public WriteMapJson[] maps;

		public static WriteFileJson FromMap(InputActionMap map)
		{
			return new WriteFileJson
			{
				maps = new WriteMapJson[1] { WriteMapJson.FromMap(map) }
			};
		}

		public static WriteFileJson FromMaps(IEnumerable<InputActionMap> maps)
		{
			int num = maps.Count();
			if (num == 0)
			{
				return default(WriteFileJson);
			}
			WriteMapJson[] array = new WriteMapJson[num];
			int num2 = 0;
			foreach (InputActionMap map in maps)
			{
				array[num2++] = WriteMapJson.FromMap(map);
			}
			return new WriteFileJson
			{
				maps = array
			};
		}
	}

	[Serializable]
	internal struct ReadFileJson
	{
		public ReadActionJson[] actions;

		public ReadMapJson[] maps;

		public InputActionMap[] ToMaps()
		{
			List<InputActionMap> list = new List<InputActionMap>();
			List<List<InputAction>> list2 = new List<List<InputAction>>();
			List<List<InputBinding>> list3 = new List<List<InputBinding>>();
			ReadActionJson[] array = actions;
			int num = ((array != null) ? array.Length : 0);
			for (int i = 0; i < num; i++)
			{
				ReadActionJson readActionJson = actions[i];
				if (string.IsNullOrEmpty(readActionJson.name))
				{
					throw new InvalidOperationException($"Action number {i + 1} has no name");
				}
				string text = null;
				string text2 = readActionJson.name;
				int num2 = text2.IndexOf('/');
				if (num2 != -1)
				{
					text = text2.Substring(0, num2);
					text2 = text2.Substring(num2 + 1);
					if (string.IsNullOrEmpty(text2))
					{
						throw new InvalidOperationException("Invalid action name '" + readActionJson.name + "' (missing action name after '/')");
					}
				}
				InputActionMap inputActionMap = null;
				int j;
				for (j = 0; j < list.Count; j++)
				{
					if (string.Compare(list[j].name, text, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						inputActionMap = list[j];
						break;
					}
				}
				if (inputActionMap == null)
				{
					inputActionMap = new InputActionMap(text);
					j = list.Count;
					list.Add(inputActionMap);
					list2.Add(new List<InputAction>());
					list3.Add(new List<InputBinding>());
				}
				InputAction inputAction = readActionJson.ToAction(text2);
				list2[j].Add(inputAction);
				if (readActionJson.bindings != null)
				{
					List<InputBinding> list4 = list3[j];
					for (int k = 0; k < readActionJson.bindings.Length; k++)
					{
						BindingJson bindingJson = readActionJson.bindings[k];
						InputBinding item = bindingJson.ToBinding();
						item.action = inputAction.m_Name;
						list4.Add(item);
					}
				}
			}
			ReadMapJson[] array2 = maps;
			int num3 = ((array2 != null) ? array2.Length : 0);
			for (int l = 0; l < num3; l++)
			{
				ReadMapJson readMapJson = maps[l];
				string name = readMapJson.name;
				if (string.IsNullOrEmpty(name))
				{
					throw new InvalidOperationException($"Map number {l + 1} has no name");
				}
				InputActionMap inputActionMap2 = null;
				int m;
				for (m = 0; m < list.Count; m++)
				{
					if (string.Compare(list[m].name, name, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						inputActionMap2 = list[m];
						break;
					}
				}
				if (inputActionMap2 == null)
				{
					inputActionMap2 = new InputActionMap(name)
					{
						m_Id = (string.IsNullOrEmpty(readMapJson.id) ? null : readMapJson.id)
					};
					m = list.Count;
					list.Add(inputActionMap2);
					list2.Add(new List<InputAction>());
					list3.Add(new List<InputBinding>());
				}
				ReadActionJson[] array3 = readMapJson.actions;
				int num4 = ((array3 != null) ? array3.Length : 0);
				for (int n = 0; n < num4; n++)
				{
					ReadActionJson readActionJson2 = readMapJson.actions[n];
					if (string.IsNullOrEmpty(readActionJson2.name))
					{
						throw new InvalidOperationException($"Action number {l + 1} in map '{name}' has no name");
					}
					InputAction inputAction2 = readActionJson2.ToAction();
					list2[m].Add(inputAction2);
					if (readActionJson2.bindings != null)
					{
						List<InputBinding> list5 = list3[m];
						for (int num5 = 0; num5 < readActionJson2.bindings.Length; num5++)
						{
							BindingJson bindingJson2 = readActionJson2.bindings[num5];
							InputBinding item2 = bindingJson2.ToBinding();
							item2.action = inputAction2.m_Name;
							list5.Add(item2);
						}
					}
				}
				BindingJson[] bindings = readMapJson.bindings;
				int num6 = ((bindings != null) ? bindings.Length : 0);
				List<InputBinding> list6 = list3[m];
				for (int num7 = 0; num7 < num6; num7++)
				{
					BindingJson bindingJson3 = readMapJson.bindings[num7];
					InputBinding item3 = bindingJson3.ToBinding();
					list6.Add(item3);
				}
			}
			for (int num8 = 0; num8 < list.Count; num8++)
			{
				InputActionMap inputActionMap3 = list[num8];
				InputAction[] array4 = list2[num8].ToArray();
				InputBinding[] bindings2 = list3[num8].ToArray();
				inputActionMap3.m_Actions = array4;
				inputActionMap3.m_Bindings = bindings2;
				for (int num9 = 0; num9 < array4.Length; num9++)
				{
					array4[num9].m_ActionMap = inputActionMap3;
				}
			}
			return list.ToArray();
		}
	}

	[SerializeField]
	internal string m_Name;

	[SerializeField]
	internal string m_Id;

	[SerializeField]
	internal InputActionAsset m_Asset;

	[SerializeField]
	internal InputAction[] m_Actions;

	[SerializeField]
	internal InputBinding[] m_Bindings;

	[NonSerialized]
	private InputBinding[] m_BindingsForEachAction;

	[NonSerialized]
	private InputControl[] m_ControlsForEachAction;

	[NonSerialized]
	private bool m_ControlsForEachActionInitialized;

	[NonSerialized]
	private bool m_BindingsForEachActionInitialized;

	[NonSerialized]
	internal int m_EnabledActionsCount;

	[NonSerialized]
	internal InputAction m_SingletonAction;

	[NonSerialized]
	internal int m_MapIndexInState = -1;

	[NonSerialized]
	internal InputActionState m_State;

	[NonSerialized]
	private bool m_NeedToResolveBindings;

	[NonSerialized]
	internal InputBinding? m_BindingMask;

	[NonSerialized]
	internal DeviceArray m_Devices;

	[NonSerialized]
	internal CallbackArray<Action<InputAction.CallbackContext>> m_ActionCallbacks;

	[NonSerialized]
	internal Dictionary<string, int> m_ActionIndexByNameOrId;

	internal static int s_DeferBindingResolution;

	public string name => m_Name;

	public InputActionAsset asset => m_Asset;

	public Guid id
	{
		get
		{
			if (string.IsNullOrEmpty(m_Id))
			{
				GenerateId();
			}
			return new Guid(m_Id);
		}
	}

	internal Guid idDontGenerate
	{
		get
		{
			if (string.IsNullOrEmpty(m_Id))
			{
				return default(Guid);
			}
			return new Guid(m_Id);
		}
	}

	public bool enabled => m_EnabledActionsCount > 0;

	public ReadOnlyArray<InputAction> actions => new ReadOnlyArray<InputAction>(m_Actions);

	public ReadOnlyArray<InputBinding> bindings => new ReadOnlyArray<InputBinding>(m_Bindings);

	IEnumerable<InputBinding> IInputActionCollection2.bindings => bindings;

	public ReadOnlyArray<InputControlScheme> controlSchemes
	{
		get
		{
			if (m_Asset == null)
			{
				return default(ReadOnlyArray<InputControlScheme>);
			}
			return m_Asset.controlSchemes;
		}
	}

	public InputBinding? bindingMask
	{
		get
		{
			return m_BindingMask;
		}
		set
		{
			if (!(m_BindingMask == value))
			{
				m_BindingMask = value;
				LazyResolveBindings();
			}
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return m_Devices.Get() ?? m_Asset?.devices;
		}
		set
		{
			if (m_Devices.Set(value))
			{
				LazyResolveBindings();
			}
		}
	}

	public InputAction this[string actionNameOrId]
	{
		get
		{
			if (actionNameOrId == null)
			{
				throw new ArgumentNullException("actionNameOrId");
			}
			return FindAction(actionNameOrId) ?? throw new KeyNotFoundException("Cannot find action '" + actionNameOrId + "'");
		}
	}

	public event Action<InputAction.CallbackContext> actionTriggered
	{
		add
		{
			m_ActionCallbacks.AddCallback(value);
		}
		remove
		{
			m_ActionCallbacks.RemoveCallback(value);
		}
	}

	public InputActionMap()
	{
	}

	public InputActionMap(string name)
		: this()
	{
		m_Name = name;
	}

	public void Dispose()
	{
		m_State?.Dispose();
	}

	internal int FindActionIndex(string nameOrId)
	{
		if (string.IsNullOrEmpty(nameOrId))
		{
			return -1;
		}
		if (m_Actions == null)
		{
			return -1;
		}
		SetUpActionLookupTable();
		int num = m_Actions.Length;
		if (nameOrId.StartsWith("{") && nameOrId.EndsWith("}"))
		{
			int length = nameOrId.Length - 2;
			for (int i = 0; i < num; i++)
			{
				if (string.Compare(m_Actions[i].m_Id, 0, nameOrId, 1, length) == 0)
				{
					return i;
				}
			}
		}
		if (m_ActionIndexByNameOrId.TryGetValue(nameOrId, out var value))
		{
			return value;
		}
		for (int j = 0; j < num; j++)
		{
			if (m_Actions[j].m_Id == nameOrId || string.Compare(m_Actions[j].m_Name, nameOrId, StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				return j;
			}
		}
		return -1;
	}

	private void SetUpActionLookupTable()
	{
		if (m_ActionIndexByNameOrId == null && m_Actions != null)
		{
			m_ActionIndexByNameOrId = new Dictionary<string, int>();
			int num = m_Actions.Length;
			for (int i = 0; i < num; i++)
			{
				InputAction inputAction = m_Actions[i];
				inputAction.MakeSureIdIsInPlace();
				m_ActionIndexByNameOrId[inputAction.name] = i;
				m_ActionIndexByNameOrId[inputAction.m_Id] = i;
			}
		}
	}

	internal void ClearActionLookupTable()
	{
		m_ActionIndexByNameOrId?.Clear();
	}

	private int FindActionIndex(Guid id)
	{
		if (m_Actions == null)
		{
			return -1;
		}
		int num = m_Actions.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_Actions[i].idDontGenerate == id)
			{
				return i;
			}
		}
		return -1;
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		if (actionNameOrId == null)
		{
			throw new ArgumentNullException("actionNameOrId");
		}
		int num = FindActionIndex(actionNameOrId);
		if (num == -1)
		{
			if (throwIfNotFound)
			{
				throw new ArgumentException($"No action '{actionNameOrId}' in '{this}'", "actionNameOrId");
			}
			return null;
		}
		return m_Actions[num];
	}

	public InputAction FindAction(Guid id)
	{
		int num = FindActionIndex(id);
		if (num == -1)
		{
			return null;
		}
		return m_Actions[num];
	}

	public bool IsUsableWithDevice(InputDevice device)
	{
		if (device == null)
		{
			throw new ArgumentNullException("device");
		}
		if (m_Bindings == null)
		{
			return false;
		}
		InputBinding[] array = m_Bindings;
		foreach (InputBinding inputBinding in array)
		{
			string effectivePath = inputBinding.effectivePath;
			if (!string.IsNullOrEmpty(effectivePath) && InputControlPath.Matches(effectivePath, device))
			{
				return true;
			}
		}
		return false;
	}

	public void Enable()
	{
		if (m_Actions != null && m_EnabledActionsCount != m_Actions.Length)
		{
			ResolveBindingsIfNecessary();
			m_State.EnableAllActions(this);
		}
	}

	public void Disable()
	{
		if (enabled)
		{
			m_State.DisableAllActions(this);
		}
	}

	public InputActionMap Clone()
	{
		InputActionMap inputActionMap = new InputActionMap
		{
			m_Name = m_Name
		};
		if (m_Actions != null)
		{
			int num = m_Actions.Length;
			InputAction[] array = new InputAction[num];
			for (int i = 0; i < num; i++)
			{
				InputAction inputAction = m_Actions[i];
				array[i] = new InputAction
				{
					m_Name = inputAction.m_Name,
					m_ActionMap = inputActionMap,
					m_Type = inputAction.m_Type,
					m_Interactions = inputAction.m_Interactions,
					m_Processors = inputAction.m_Processors,
					m_ExpectedControlType = inputAction.m_ExpectedControlType
				};
			}
			inputActionMap.m_Actions = array;
		}
		if (m_Bindings != null)
		{
			int num2 = m_Bindings.Length;
			InputBinding[] array2 = new InputBinding[num2];
			Array.Copy(m_Bindings, 0, array2, 0, num2);
			for (int j = 0; j < num2; j++)
			{
				array2[j].m_Id = null;
			}
			inputActionMap.m_Bindings = array2;
		}
		return inputActionMap;
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public bool Contains(InputAction action)
	{
		if (action == null)
		{
			return false;
		}
		return action.actionMap == this;
	}

	public override string ToString()
	{
		if (m_Asset != null)
		{
			return $"{m_Asset}:{m_Name}";
		}
		if (!string.IsNullOrEmpty(m_Name))
		{
			return m_Name;
		}
		return "<Unnamed Action Map>";
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return actions.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal ReadOnlyArray<InputBinding> GetBindingsForSingleAction(InputAction action)
	{
		if (!m_BindingsForEachActionInitialized)
		{
			SetUpPerActionControlAndBindingArrays();
		}
		return new ReadOnlyArray<InputBinding>(m_BindingsForEachAction, action.m_BindingsStartIndex, action.m_BindingsCount);
	}

	internal ReadOnlyArray<InputControl> GetControlsForSingleAction(InputAction action)
	{
		if (!m_ControlsForEachActionInitialized)
		{
			SetUpPerActionControlAndBindingArrays();
		}
		return new ReadOnlyArray<InputControl>(m_ControlsForEachAction, action.m_ControlStartIndex, action.m_ControlCount);
	}

	private unsafe void SetUpPerActionControlAndBindingArrays()
	{
		if (m_Bindings == null)
		{
			m_ControlsForEachAction = null;
			m_BindingsForEachAction = null;
			m_ControlsForEachActionInitialized = true;
			m_BindingsForEachActionInitialized = true;
			return;
		}
		if (m_SingletonAction != null)
		{
			m_BindingsForEachAction = m_Bindings;
			m_ControlsForEachAction = m_State?.controls;
			m_SingletonAction.m_BindingsStartIndex = 0;
			m_SingletonAction.m_BindingsCount = m_Bindings.Length;
			m_SingletonAction.m_ControlStartIndex = 0;
			m_SingletonAction.m_ControlCount = m_State?.totalControlCount ?? 0;
			if (m_ControlsForEachAction.HaveDuplicateReferences(0, m_SingletonAction.m_ControlCount))
			{
				int num = 0;
				InputControl[] array = new InputControl[m_SingletonAction.m_ControlCount];
				for (int i = 0; i < m_SingletonAction.m_ControlCount; i++)
				{
					if (!array.ContainsReference(m_ControlsForEachAction[i]))
					{
						array[num] = m_ControlsForEachAction[i];
						num++;
					}
				}
				m_ControlsForEachAction = array;
				m_SingletonAction.m_ControlCount = num;
			}
		}
		else
		{
			InputActionState.ActionMapIndices actionMapIndices = m_State?.FetchMapIndices(this) ?? default(InputActionState.ActionMapIndices);
			for (int j = 0; j < m_Actions.Length; j++)
			{
				InputAction obj = m_Actions[j];
				obj.m_BindingsCount = 0;
				obj.m_BindingsStartIndex = -1;
				obj.m_ControlCount = 0;
				obj.m_ControlStartIndex = -1;
			}
			int num2 = m_Bindings.Length;
			for (int k = 0; k < num2; k++)
			{
				InputAction inputAction = FindAction(m_Bindings[k].action);
				if (inputAction != null)
				{
					inputAction.m_BindingsCount++;
				}
			}
			int num3 = 0;
			if (m_State != null && (m_ControlsForEachAction == null || m_ControlsForEachAction.Length != actionMapIndices.controlCount))
			{
				if (actionMapIndices.controlCount == 0)
				{
					m_ControlsForEachAction = null;
				}
				else
				{
					m_ControlsForEachAction = new InputControl[actionMapIndices.controlCount];
				}
			}
			InputBinding[] array2 = null;
			int num4 = 0;
			int num5 = 0;
			while (num5 < m_Bindings.Length)
			{
				InputAction inputAction2 = FindAction(m_Bindings[num5].action);
				if (inputAction2 == null || inputAction2.m_BindingsStartIndex != -1)
				{
					num5++;
					continue;
				}
				inputAction2.m_BindingsStartIndex = ((array2 != null) ? num3 : num5);
				inputAction2.m_ControlStartIndex = num4;
				int bindingsCount = inputAction2.m_BindingsCount;
				int num6 = num5;
				for (int l = 0; l < bindingsCount; l++)
				{
					if (FindAction(m_Bindings[num6].action) != inputAction2)
					{
						if (array2 == null)
						{
							array2 = new InputBinding[m_Bindings.Length];
							num3 = num6;
							Array.Copy(m_Bindings, 0, array2, 0, num6);
						}
						do
						{
							num6++;
						}
						while (FindAction(m_Bindings[num6].action) != inputAction2);
					}
					else if (num5 == num6)
					{
						num5++;
					}
					if (array2 != null)
					{
						array2[num3++] = m_Bindings[num6];
					}
					if (m_State != null && !m_Bindings[num6].isComposite)
					{
						ref InputActionState.BindingState reference = ref m_State.bindingStates[actionMapIndices.bindingStartIndex + num6];
						int controlCount = reference.controlCount;
						if (controlCount > 0)
						{
							int controlStartIndex = reference.controlStartIndex;
							for (int m = 0; m < controlCount; m++)
							{
								InputControl inputControl = m_State.controls[controlStartIndex + m];
								if (!m_ControlsForEachAction.ContainsReference(inputAction2.m_ControlStartIndex, inputAction2.m_ControlCount, inputControl))
								{
									m_ControlsForEachAction[num4] = inputControl;
									num4++;
									inputAction2.m_ControlCount++;
								}
							}
						}
					}
					num6++;
				}
			}
			if (array2 == null)
			{
				m_BindingsForEachAction = m_Bindings;
			}
			else
			{
				m_BindingsForEachAction = array2;
			}
		}
		m_ControlsForEachActionInitialized = true;
		m_BindingsForEachActionInitialized = true;
	}

	internal void ClearCachedActionData()
	{
		m_BindingsForEachActionInitialized = false;
		m_ControlsForEachActionInitialized = false;
		m_BindingsForEachAction = null;
		m_ControlsForEachAction = null;
		m_ActionIndexByNameOrId = null;
	}

	internal void GenerateId()
	{
		m_Id = Guid.NewGuid().ToString();
	}

	internal bool LazyResolveBindings()
	{
		m_ControlsForEachAction = null;
		m_ControlsForEachActionInitialized = false;
		if (m_State == null)
		{
			return false;
		}
		if (s_DeferBindingResolution > 0)
		{
			m_NeedToResolveBindings = true;
			return false;
		}
		ResolveBindings();
		return true;
	}

	internal void ResolveBindingsIfNecessary()
	{
		if ((m_State == null || m_NeedToResolveBindings) && (m_State == null || !m_State.isProcessingControlStateChange))
		{
			ResolveBindings();
		}
	}

	internal void ResolveBindings()
	{
		m_ControlsForEachActionInitialized = false;
		using (InputActionRebindingExtensions.DeferBindingResolution())
		{
			InputActionState.UnmanagedMemory oldState = default(InputActionState.UnmanagedMemory);
			try
			{
				InputBindingResolver resolver = default(InputBindingResolver);
				OneOrMore<InputActionMap, ReadOnlyArray<InputActionMap>> oneOrMore;
				if (m_Asset != null)
				{
					oneOrMore = m_Asset.actionMaps;
					resolver.bindingMask = m_Asset.m_BindingMask;
					for (int i = 0; i < oneOrMore.Count; i++)
					{
						oneOrMore[i].m_NeedToResolveBindings = false;
					}
				}
				else
				{
					oneOrMore = this;
					m_NeedToResolveBindings = false;
				}
				bool flag = false;
				if (m_State != null)
				{
					oldState = m_State.memory.Clone();
					flag = m_State.HasEnabledActions();
					for (int j = 0; j < oneOrMore.Count; j++)
					{
						InputActionMap inputActionMap = oneOrMore[j];
						if (flag)
						{
							m_State.DisableAllActions(inputActionMap);
						}
						if (inputActionMap.m_SingletonAction != null)
						{
							InputActionState.NotifyListenersOfActionChange(InputActionChange.BoundControlsAboutToChange, inputActionMap.m_SingletonAction);
						}
						else if (m_Asset == null)
						{
							InputActionState.NotifyListenersOfActionChange(InputActionChange.BoundControlsAboutToChange, inputActionMap);
						}
					}
					if (m_Asset != null)
					{
						InputActionState.NotifyListenersOfActionChange(InputActionChange.BoundControlsAboutToChange, m_Asset);
					}
					resolver.StartWithArraysFrom(m_State);
					m_State.memory.Dispose();
				}
				for (int k = 0; k < oneOrMore.Count; k++)
				{
					resolver.AddActionMap(oneOrMore[k]);
				}
				if (m_State == null)
				{
					if (m_Asset != null)
					{
						InputActionState inputActionState = new InputActionState();
						for (int l = 0; l < oneOrMore.Count; l++)
						{
							oneOrMore[l].m_State = inputActionState;
						}
						m_Asset.m_SharedStateForAllMaps = inputActionState;
					}
					else
					{
						m_State = new InputActionState();
					}
					m_State.Initialize(resolver);
				}
				else
				{
					m_State.ClaimDataFrom(resolver);
				}
				for (int m = 0; m < oneOrMore.Count; m++)
				{
					InputActionMap inputActionMap2 = oneOrMore[m];
					inputActionMap2.m_ControlsForEachAction = null;
					inputActionMap2.m_ControlsForEachActionInitialized = false;
					if (inputActionMap2.m_SingletonAction != null)
					{
						InputActionState.NotifyListenersOfActionChange(InputActionChange.BoundControlsChanged, inputActionMap2.m_SingletonAction);
					}
					else if (m_Asset == null)
					{
						InputActionState.NotifyListenersOfActionChange(InputActionChange.BoundControlsChanged, inputActionMap2);
					}
				}
				if (m_Asset != null)
				{
					InputActionState.NotifyListenersOfActionChange(InputActionChange.BoundControlsChanged, m_Asset);
				}
				m_State.FinishBindingCompositeSetups();
				if (flag)
				{
					m_State.RestoreActionStates(oldState);
				}
			}
			finally
			{
				oldState.Dispose();
			}
		}
	}

	public int FindBinding(InputBinding mask, out InputAction action)
	{
		int num = FindBindingRelativeToMap(mask);
		if (num == -1)
		{
			action = null;
			return -1;
		}
		action = m_SingletonAction ?? FindAction(bindings[num].action);
		return action.BindingIndexOnMapToBindingIndexOnAction(num);
	}

	internal int FindBindingRelativeToMap(InputBinding mask)
	{
		InputBinding[] array = m_Bindings;
		int num = array.LengthSafe();
		for (int i = 0; i < num; i++)
		{
			if (mask.Matches(ref array[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static InputActionMap[] FromJson(string json)
	{
		if (json == null)
		{
			throw new ArgumentNullException("json");
		}
		return JsonUtility.FromJson<ReadFileJson>(json).ToMaps();
	}

	public static string ToJson(IEnumerable<InputActionMap> maps)
	{
		if (maps == null)
		{
			throw new ArgumentNullException("maps");
		}
		return JsonUtility.ToJson(WriteFileJson.FromMaps(maps), prettyPrint: true);
	}

	public string ToJson()
	{
		return JsonUtility.ToJson(WriteFileJson.FromMap(this), prettyPrint: true);
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		m_State = null;
		m_MapIndexInState = -1;
		if (m_Actions != null)
		{
			int num = m_Actions.Length;
			for (int i = 0; i < num; i++)
			{
				m_Actions[i].m_ActionMap = this;
			}
		}
		ClearCachedActionData();
		ClearActionLookupTable();
	}
}
