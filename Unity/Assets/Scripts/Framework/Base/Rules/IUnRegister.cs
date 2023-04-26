using System.Collections.Generic;

namespace Framework
{
	public interface IUnRegister
	{
		void UnRegister();
	}

	public interface IUnRegisterList
	{
		ICollection<IUnRegister> UnregisterList { get; }
	}

	public static class UnRegisterListExtension
	{
		public static void AddToUnregisterList(this IUnRegister self, IUnRegisterList unRegisterList) {
			unRegisterList.UnregisterList.Add(self);
		}

		public static void UnRegisterAll(this IUnRegisterList self) {
			foreach (IUnRegister unRegister in self.UnregisterList) {
				unRegister.UnRegister();
			}

			self.UnregisterList.Clear();
		}
	}
}