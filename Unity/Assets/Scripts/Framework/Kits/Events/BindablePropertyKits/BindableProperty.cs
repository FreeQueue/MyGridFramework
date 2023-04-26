using System;

namespace Framework.Kits.BindablePropertyKits
{
	public class BindableProperty<T> : IBindableProperty<T>
	{
		public BindableProperty(T defaultValue = default)
		{
			value = defaultValue;
		}

		protected T value;

		public T Value
		{
			get => GetValue();
			set
			{
				if (value == null && this.value == null) return;
				if (value != null && value.Equals(this.value)) return;

				SetValue(value);
				_onValueChanged?.Invoke(value);
			}
		}

		protected virtual void SetValue(T newValue)
		{
			value = newValue;
		}

		protected virtual T GetValue()
		{
			return value;
		}

		public void SetValueWithoutEvent(T newValue)
		{
			value = newValue;
		}

		private Action<T> _onValueChanged = (v) => { };

		public IUnRegister Register(Action<T> onValueChanged)
		{
			_onValueChanged += onValueChanged;
			return new BindablePropertyUnRegister<T>()
			{
				BindableProperty = this,
				OnValueChanged = onValueChanged,
			};
		}

		public IUnRegister RegisterWithInitValue(Action<T> onValueChanged)
		{
			onValueChanged(value);
			return Register(onValueChanged);
		}

		public static implicit operator T(BindableProperty<T> property)
		{
			return property.Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public void UnRegister(Action<T> onValueChanged)
		{
			_onValueChanged -= onValueChanged;
		}
	}
}