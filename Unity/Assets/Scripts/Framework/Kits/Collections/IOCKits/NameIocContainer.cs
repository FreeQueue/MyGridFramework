using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.Kits.IOCKits
{
	public class NameIocContainer<TBase>:IEnumerable<TBase>
	{
		private readonly Dictionary<TypeNamePair, TBase> _instances = new Dictionary<TypeNamePair, TBase>();
		public bool Contains<T>(string name = null) {
			return _instances.ContainsKey(new TypeNamePair(typeof(T), name));
		}
		
		public bool Register<T>(T instance, string name = null) where T:TBase{
			var key = new TypeNamePair(typeof(T), name);
			if (_instances.ContainsKey(key)) {
				Debug.LogWarning($"key {key} has register, now override");
				_instances[key] = instance;
				return true;
			}
			else {
				_instances.Add(key, instance);
			}
			return false;
		}
		public bool Register<T>(out T instance,string name = null) where T:TBase,new() {
			instance =new T() ;
			return Register(instance,name);
		}
		public T Get<T>(string name = null) where T:TBase{
			return _instances.TryGetValue(new TypeNamePair(typeof(T), name), out TBase retInstance)
				? (T)retInstance
				: default;
		}

		public IEnumerator<TBase> GetEnumerator() {
			return _instances.Values.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}