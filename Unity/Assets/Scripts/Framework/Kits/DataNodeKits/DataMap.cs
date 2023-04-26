using System.Collections.Generic;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.DataNodeKits
{
	public class DataMap:IReference
	{
		private Dictionary<string, Var> _datas;
		
		public bool TryGetData<T>(string key,out T data) {
			if (_datas.TryGetValue(key, out Var var)) {
				return var.TryGetValue(out data) ;
			};
			data = default;
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns>是否原值存在且符合类型</returns>
		public bool SetData<T>(string key,T data) {
			if (_datas.TryGetValue(key, out Var var)) {
				if (var.TrySetValue(data)) {
					return true;
				}
				ReferencePool.Release(var);
			}
			var newVar= ReferencePool.Get<Var<T>>();
			_datas[key] = newVar;
			return false;
		}
		
		public void Clear() {
			foreach (Var var in _datas.Values) {
				ReferencePool.Release(var);
			}
			_datas.Clear();
		}
	}
}