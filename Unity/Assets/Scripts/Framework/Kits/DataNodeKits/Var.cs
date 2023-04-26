using System;
using Framework.Kits.ReferencePoolKits;
using UnityEngine;

namespace Framework.Kits.DataNodeKits
{

	public abstract class Var:IReference
	{
		public abstract Type Type { get; }
		public abstract object GetValue();
		public abstract bool TryGetValue<T>(out T value);
		public abstract bool TrySetValue<T>(T value);
		public abstract void Clear();
	}

	public class Var<T> : Var
	{
		public Var() {

		}
		public Var(T value) {
			Value = value;
		}
		public override Type Type => typeof(T);
		public T Value { get; set; }
		public override void Clear() {
			Value = default;
		}
		public override object GetValue() {
			return Value;
		}
		public override bool TryGetValue<TTry>(out TTry value) where TTry : default {
			if (Value is TTry @try) {
				value = @try;
				return true;
			}
			Debug.Log($"错误的解析类型，实际类型为{Type},解析类型为{typeof(TTry)}");
			value = default;
			return false;
		}
		public override bool TrySetValue<TTry>(TTry value) {
			if (value is T cast) {
				Value = cast;
				return true;
			}
			Debug.Log($"错误的赋值类型，实际类型为{Type},赋值类型为{typeof(TTry)}");
			return false;
		}
	}
}