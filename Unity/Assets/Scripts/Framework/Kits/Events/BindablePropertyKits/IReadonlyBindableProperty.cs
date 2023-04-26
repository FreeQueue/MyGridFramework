using System;

namespace Framework.Kits.BindablePropertyKits
{
	public interface IReadonlyBindableProperty<T>
	{
		T Value { get; }
		IUnRegister Register(Action<T> onValueChanged);
		void UnRegister(Action<T> onValueChanged);
		IUnRegister RegisterWithInitValue(Action<T> action);
	}
}