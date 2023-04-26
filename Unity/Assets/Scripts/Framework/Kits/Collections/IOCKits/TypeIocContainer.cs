using System;
using System.Collections;
using System.Collections.Generic;

namespace Framework.Kits.IOCKits
{
	public class TypeIocContainer<TBase>:IEnumerable<TBase>
	{
		private readonly Dictionary<Type, TBase> _instances = new Dictionary<Type, TBase>();
		public bool Contains<T>() where T:TBase{
			return _instances.ContainsKey(typeof(T));
		}
		public void Register<T>(T instance)  where T:TBase{
			Type key = typeof(T);
			if (_instances.ContainsKey(key)) {
				_instances[key] = instance;
			}
			else {
				_instances.Add(key, instance);
			}
		}
		public T Register<T>() where T:TBase,new() {
			var instance =new T() ;
			Register(instance);
			return instance;
		}
		public T Get<T>() where T:TBase {
			return _instances.TryGetValue(typeof(T), out TBase retInstance) ? (T)retInstance: default;
		}
		
		public IEnumerator<TBase> GetEnumerator() {
			return _instances.Values.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}