using System.Collections.Generic;
using Framework.Kits.MiniYamlKits;

namespace OpenRA.Traits
{
	//TODO
	// public interface IWorldLoaded
	// {
	// 	void WorldLoaded(World w);
	// }
	//
	// public interface INotifyGameLoading
	// {
	// 	void GameLoading(World w);
	// }
	//
	// public interface INotifyGameLoaded
	// {
	// 	void GameLoaded(World w);
	// }
	//
	// public interface INotifyGameSaved
	// {
	// 	void GameSaved(World w);
	// }

	public interface IGameSaveTraitData
	{
		List<MiniYamlNode> IssueTraitData(Actor self);
		void ResolveTraitData(Actor self, List<MiniYamlNode> data);
	}

	// public interface IRuleSetLoaded<in TInfo>
	// {
	// 	void RuleSetLoaded(Ruleset rules, TInfo info);
	// }
	//
	// public interface IRuleSetLoaded : IRuleSetLoaded<ActorInfo>, ITraitInfoInterface
	// {
	// }
}