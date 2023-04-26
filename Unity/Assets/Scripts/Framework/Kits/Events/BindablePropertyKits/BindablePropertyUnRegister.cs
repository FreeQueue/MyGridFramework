using System;

namespace Framework.Kits.BindablePropertyKits
{
	public class BindablePropertyUnRegister<T> : IUnRegister
	{
		public BindableProperty<T> BindableProperty { get; set; }

		public Action<T> OnValueChanged { get; set; }

		public void UnRegister()
		{
			BindableProperty.UnRegister(OnValueChanged);

			BindableProperty = null;
			OnValueChanged = null;
		}
	}
}