using System.Globalization;

namespace System.ComponentModel;

/// <summary>Provides the base implementation for the <see cref="T:System.ComponentModel.INestedContainer" /> interface, which enables containers to have an owning component.</summary>
public class NestedContainer : Container, INestedContainer, IContainer, IDisposable
{
	private class Site : INestedSite, ISite, IServiceProvider
	{
		private string _name;

		public IComponent Component { get; }

		public IContainer Container { get; }

		public bool DesignMode
		{
			get
			{
				IComponent owner = ((NestedContainer)Container).Owner;
				if (owner != null && owner.Site != null)
				{
					return owner.Site.DesignMode;
				}
				return false;
			}
		}

		public string FullName
		{
			get
			{
				if (_name != null)
				{
					string ownerName = ((NestedContainer)Container).OwnerName;
					string text = _name;
					if (ownerName != null)
					{
						text = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", ownerName, text);
					}
					return text;
				}
				return _name;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (value == null || _name == null || !value.Equals(_name))
				{
					((NestedContainer)Container).ValidateName(Component, value);
					_name = value;
				}
			}
		}

		internal Site(IComponent component, NestedContainer container, string name)
		{
			Component = component;
			Container = container;
			_name = name;
		}

		public object GetService(Type service)
		{
			if (!(service == typeof(ISite)))
			{
				return ((NestedContainer)Container).GetService(service);
			}
			return this;
		}
	}

	/// <summary>Gets the owning component for this nested container.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.IComponent" /> that owns this nested container.</returns>
	public IComponent Owner { get; }

	/// <summary>Gets the name of the owning component.</summary>
	/// <returns>The name of the owning component.</returns>
	protected virtual string OwnerName
	{
		get
		{
			string result = null;
			if (Owner != null && Owner.Site != null)
			{
				result = ((!(Owner.Site is INestedSite nestedSite)) ? Owner.Site.Name : nestedSite.FullName);
			}
			return result;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.NestedContainer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.ComponentModel.IComponent" /> that owns this nested container.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="owner" /> is null.</exception>
	public NestedContainer(IComponent owner)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		Owner = owner;
		Owner.Disposed += OnOwnerDisposed;
	}

	/// <summary>Creates a site for the component within the container.</summary>
	/// <returns>The newly created <see cref="T:System.ComponentModel.ISite" />.</returns>
	/// <param name="component">The <see cref="T:System.ComponentModel.IComponent" /> to create a site for.</param>
	/// <param name="name">The name to assign to <paramref name="component" />, or null to skip the name assignment.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="component" /> is null.</exception>
	protected override ISite CreateSite(IComponent component, string name)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		return new Site(component, this, name);
	}

	/// <summary>Releases the resources used by the nested container.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Owner.Disposed -= OnOwnerDisposed;
		}
		base.Dispose(disposing);
	}

	/// <summary>Gets the service object of the specified type, if it is available.</summary>
	/// <returns>An <see cref="T:System.Object" /> that implements the requested service, or null if the service cannot be resolved.</returns>
	/// <param name="service">The <see cref="T:System.Type" /> of the service to retrieve.</param>
	protected override object GetService(Type service)
	{
		if (service == typeof(INestedContainer))
		{
			return this;
		}
		return base.GetService(service);
	}

	private void OnOwnerDisposed(object sender, EventArgs e)
	{
		Dispose();
	}
}
