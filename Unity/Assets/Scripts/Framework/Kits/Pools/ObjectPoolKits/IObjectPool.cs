namespace Framework.Kits.ObjectPoolKits
{
	public interface IObjectPool<T>
	{
		public void Register(string name, T target, bool spawned);
		/// <summary>
		///     检查对象。
		/// </summary>
		/// <param name="name">对象名称。</param>
		/// <returns>要检查的对象是否存在。</returns>
		public bool CanSpawn(string name);
		
		/// <summary>
		///     获取对象。
		/// </summary>
		/// <param name="name">对象名称。</param>
		/// <returns>要获取的对象。</returns>
		public T Spawn(string name);
		
		/// <summary>
		///     回收对象。
		/// </summary>
		/// <param name="target">要回收的对象。</param>
		public void Unspawn(T target);
	}
}