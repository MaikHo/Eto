using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Eto.Forms
{
	public interface ILayout : IInstanceWidget
	{
		void OnPreLoad ();
		
		void OnLoad ();

		void OnLoadComplete ();

		void Update ();

		void AttachedToContainer();
	}

	public interface IPositionalLayout : ILayout
	{
		void Add (Control child, int x, int y);

		void Move (Control child, int x, int y);

		void Remove (Control child);
	}

	public abstract class Layout : InstanceWidget, ISupportInitialize
	{
		ILayout handler;
		Container container;

		public bool Initializing { get; private set; }
		
		public bool Loaded { get; private set; }

		public virtual Layout InnerLayout
		{
			get { return this; }
		}
		
		public abstract IEnumerable<Control> Controls {
			get;
		}
		
		public event EventHandler<EventArgs> PreLoad;

		public virtual void OnPreLoad (EventArgs e)
		{
			if (PreLoad != null)
				PreLoad (this, e);
			handler.OnPreLoad ();
		}
		
		
		public event EventHandler<EventArgs> Load;

		public virtual void OnLoad (EventArgs e)
		{
			Loaded = true;
			if (Load != null)
				Load (this, e);
			handler.OnLoad ();
		}

		public event EventHandler<EventArgs> LoadComplete;

		public virtual void OnLoadComplete (EventArgs e)
		{
			if (LoadComplete != null)
				LoadComplete (this, e);
			handler.OnLoadComplete ();
		}

		protected Layout (Generator g, Container container, Type type, bool initialize = true)
			: base(g, type, false)
		{
			handler = (ILayout)Handler;
			this.container = container;
			if (initialize) {
				Initialize ();
				if (this.Container != null)
					this.Container.Layout = this;
			}
		}

		protected Layout (Generator g, Container container, ILayout handler, bool initialize = true)
			: base (g, handler, false)
		{
			this.handler = (ILayout)Handler;
			this.container = container;
			if (initialize) {
				Initialize ();
				if (this.Container != null)
					this.Container.Layout = this;
			}
		}
		
		public virtual Container Container {
			get { return container; }
			protected internal set {
				container = value;
				handler.AttachedToContainer ();
			}
		}
		
		public Layout ParentLayout {
			get { return (Container != null) ? Container.ParentLayout : null; }
		}
		
		public virtual void Update ()
		{
			UpdateContainers (this.Container);
			handler.Update ();
		}
		
		void UpdateContainers (Container container)
		{
			foreach (var c in container.Controls.OfType<Container>()) {
				if (c.Layout != null) {
					UpdateContainers (c);
					c.Layout.Update ();
				}
			}
		}
		
		protected void SetInnerLayout (bool load)
		{
			if (Container != null)
				Container.SetInnerLayout (load);
		}

		public virtual void BeginInit ()
		{
			Initializing = true;
		}

		public virtual void EndInit ()
		{
			Initializing = false;
		}
	}
}
