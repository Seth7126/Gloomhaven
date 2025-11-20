using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies the template or user control that Dynamic Data uses to display a data field. </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class UIHintAttribute : Attribute
{
	internal class UIHintImplementation
	{
		private IDictionary<string, object> _controlParameters;

		private object[] _inputControlParameters;

		public string UIHint { get; private set; }

		public string PresentationLayer { get; private set; }

		public IDictionary<string, object> ControlParameters
		{
			get
			{
				if (_controlParameters == null)
				{
					_controlParameters = BuildControlParametersDictionary();
				}
				return _controlParameters;
			}
		}

		public UIHintImplementation(string uiHint, string presentationLayer, params object[] controlParameters)
		{
			UIHint = uiHint;
			PresentationLayer = presentationLayer;
			if (controlParameters != null)
			{
				_inputControlParameters = new object[controlParameters.Length];
				Array.Copy(controlParameters, _inputControlParameters, controlParameters.Length);
			}
		}

		public override int GetHashCode()
		{
			string obj = UIHint ?? string.Empty;
			string text = PresentationLayer ?? string.Empty;
			return obj.GetHashCode() ^ text.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			UIHintImplementation uIHintImplementation = (UIHintImplementation)obj;
			if (UIHint != uIHintImplementation.UIHint || PresentationLayer != uIHintImplementation.PresentationLayer)
			{
				return false;
			}
			IDictionary<string, object> controlParameters;
			IDictionary<string, object> controlParameters2;
			try
			{
				controlParameters = ControlParameters;
				controlParameters2 = uIHintImplementation.ControlParameters;
			}
			catch (InvalidOperationException)
			{
				return false;
			}
			if (controlParameters.Count != controlParameters2.Count)
			{
				return false;
			}
			return controlParameters.OrderBy((KeyValuePair<string, object> p) => p.Key).SequenceEqual(controlParameters2.OrderBy((KeyValuePair<string, object> p) => p.Key));
		}

		private IDictionary<string, object> BuildControlParametersDictionary()
		{
			IDictionary<string, object> dictionary = new Dictionary<string, object>();
			object[] inputControlParameters = _inputControlParameters;
			if (inputControlParameters == null || inputControlParameters.Length == 0)
			{
				return dictionary;
			}
			if (inputControlParameters.Length % 2 != 0)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The number of control parameters must be even."));
			}
			for (int i = 0; i < inputControlParameters.Length; i += 2)
			{
				object obj = inputControlParameters[i];
				object value = inputControlParameters[i + 1];
				if (obj == null)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The key parameter at position {0} is null. Every key control parameter must be a string.", i));
				}
				if (!(obj is string text))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The key parameter at position {0} with value '{1}' is not a string. Every key control parameter must be a string.", i, inputControlParameters[i].ToString()));
				}
				if (dictionary.ContainsKey(text))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The key parameter at position {0} with value '{1}' occurs more than once.", i, text));
				}
				dictionary[text] = value;
			}
			return dictionary;
		}
	}

	private UIHintImplementation _implementation;

	/// <summary>Gets or sets the name of the field template to use to display the data field.</summary>
	/// <returns>The name of the field template that displays the data field.</returns>
	public string UIHint => _implementation.UIHint;

	/// <summary>Gets or sets the presentation layer that uses the <see cref="T:System.ComponentModel.DataAnnotations.UIHintAttribute" /> class. </summary>
	/// <returns>The presentation layer that is used by this class.</returns>
	public string PresentationLayer => _implementation.PresentationLayer;

	/// <summary>Gets or sets the <see cref="T:System.Web.DynamicData.DynamicControlParameter" /> object to use to retrieve values from any data source.</summary>
	/// <returns>A collection of key/value pairs. </returns>
	public IDictionary<string, object> ControlParameters => _implementation.ControlParameters;

	/// <summary>Gets the unique identifier for the attribute.</summary>
	/// <returns>The unique identifier for the attribute.</returns>
	public override object TypeId => this;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.UIHintAttribute" /> class by using a specified user control. </summary>
	/// <param name="uiHint">The user control to use to display the data field. </param>
	public UIHintAttribute(string uiHint)
		: this(uiHint, null, new object[0])
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.UIHintAttribute" /> class using the specified user control and specified presentation layer. </summary>
	/// <param name="uiHint">The user control (field template) to use to display the data field.</param>
	/// <param name="presentationLayer">The presentation layer that uses the class. Can be set to "HTML", "Silverlight", "WPF", or "WinForms".</param>
	public UIHintAttribute(string uiHint, string presentationLayer)
		: this(uiHint, presentationLayer, new object[0])
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.UIHintAttribute" /> class by using the specified user control, presentation layer, and control parameters.</summary>
	/// <param name="uiHint">The user control (field template) to use to display the data field.</param>
	/// <param name="presentationLayer">The presentation layer that uses the class. Can be set to "HTML", "Silverlight", "WPF", or "WinForms".</param>
	/// <param name="controlParameters">The object to use to retrieve values from any data sources. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.ComponentModel.DataAnnotations.UIHintAttribute.ControlParameters" /> is null or it is a constraint key.-or-The value of <see cref="P:System.ComponentModel.DataAnnotations.UIHintAttribute.ControlParameters" /> is not a string. </exception>
	public UIHintAttribute(string uiHint, string presentationLayer, params object[] controlParameters)
	{
		_implementation = new UIHintImplementation(uiHint, presentationLayer, controlParameters);
	}

	/// <summary>Gets the hash code for the current instance of the attribute.</summary>
	/// <returns>The attribute instance hash code.</returns>
	public override int GetHashCode()
	{
		return _implementation.GetHashCode();
	}

	/// <summary>Gets a value that indicates whether this instance is equal to the specified object.</summary>
	/// <returns>true if the specified object is equal to this instance; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this instance, or a null reference.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is UIHintAttribute uIHintAttribute))
		{
			return false;
		}
		return _implementation.Equals(uIHintAttribute._implementation);
	}
}
