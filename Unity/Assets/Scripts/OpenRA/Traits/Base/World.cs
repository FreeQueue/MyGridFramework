using Framework;

namespace OpenRA.Traits
{
	[RequireExplicitImpl]
	public interface INotifyAddedToWorld
	{
		void AddedToWorld(Actor self);
	}

	[RequireExplicitImpl]
	public interface INotifyRemovedFromWorld
	{
		void RemovedFromWorld(Actor self);
	}

	[RequireExplicitImpl]
	public interface ITick
	{
		void Tick(Actor self);
	}

	//TODO
	// [RequireExplicitImpl]
	// public interface ICreatePlayers
	// {
	// 	void CreatePlayers(World w, IRandom playerRandom);
	// }
	//
	// [RequireExplicitImpl]
	// public interface IGameOver
	// {
	// 	void GameOver(World world);
	// }
}