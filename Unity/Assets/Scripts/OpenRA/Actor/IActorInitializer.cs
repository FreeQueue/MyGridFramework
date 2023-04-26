using OpenRA.Traits;

namespace OpenRA
{
	public interface IActorInitializer
	{
		//TODO
		//World World { get; }
		T GetOrDefault<T>(TraitInfo info) where T : ActorInit;
		T Get<T>(TraitInfo info) where T : ActorInit;
		TValue GetValue<T, TValue>(TraitInfo info) where T : ValueActorInit<TValue>;
		TValue GetValue<T, TValue>(TraitInfo info, TValue fallback) where T : ValueActorInit<TValue>;
		bool Contains<T>(TraitInfo info) where T : ActorInit;

		T GetOrDefault<T>() where T : ActorInit, ISingleInstanceInit;
		T Get<T>() where T : ActorInit, ISingleInstanceInit;
		TU GetValue<T, TU>() where T : ValueActorInit<TU>, ISingleInstanceInit;
		TU GetValue<T, TU>(TU fallback) where T : ValueActorInit<TU>, ISingleInstanceInit;
		bool Contains<T>() where T : ActorInit, ISingleInstanceInit;
	}
}