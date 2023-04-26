// #region Copyright & License Information
// /*
//  * Copyright (c) The OpenRA Developers and Contributors
//  * This file is part of OpenRA, which is free software. It is made
//  * available to you under the terms of the GNU General Public License
//  * as published by the Free Software Foundation, either version 3 of
//  * the License, or (at your option) any later version. For more
//  * information, see COPYING.
//  */
// #endregion
//
// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using Framework.Kits.BitSetKits;
// using Framework.Kits.CollectionsUnmanaged;
// using Framework.Kits.MiniYamlKits;
// using Framework.Kits.RandomAlgorithmKits;
// using OpenRA.Traits;
//
// namespace OpenRA
// {
// 	public sealed class World : IDisposable
// 	{
// 		internal readonly TraitDictionary TraitDict = new TraitDictionary();
// 		private readonly SortedDictionary<uint, Actor> _actors = new SortedDictionary<uint, Actor>();
// 		private readonly List<IEffect> _effects = new List<IEffect>();
// 		
// 		public LongBitSet<PlayerBitMask> AllPlayersMask = default;
// 		public readonly LongBitSet<PlayerBitMask> NoPlayersMask = default;
//
// 		public Player[] Players = Array.Empty<Player>();
//
// 		public event Action<Player> RenderPlayerChanged;
//
// 		public void SetPlayers(IEnumerable<Player> players, Player localPlayer)
// 		{
// 			if (Players.Length > 0)
// 				throw new InvalidOperationException("Players are fixed once they have been set.");
// 			Players = players.ToArray();
// 			SetLocalPlayer(localPlayer);
// 		}
//
// 		public Player LocalPlayer { get; private set; }
//
// 		public event Action GameOver = () => { };
//
// 		/// <summary>Indicates that the game has ended.</summary>
// 		/// <remarks>Should only be set in <see cref="EndGame"/>.</remarks>
// 		public bool IsGameOver { get; private set; }
// 		public void EndGame()
// 		{
// 			if (!IsGameOver)
// 			{
// 				SetPauseState(true);
// 				IsGameOver = true;
//
// 				foreach (IGameOver t in WorldActor.TraitsImplementing<IGameOver>())
// 					t.GameOver(this);
//
// 				_gameInfo.FinalGameTick = WorldTick;
// 				GameOver();
// 			}
// 		}
//
// 		private Player _renderPlayer;
// 		public Player RenderPlayer
// 		{
// 			get => _renderPlayer;
//
// 			set
// 			{
// 				if (LocalPlayer == null || LocalPlayer.UnlockedRenderPlayer)
// 				{
// 					_renderPlayer = value;
//
// 					RenderPlayerChanged?.Invoke(value);
// 				}
// 			}
// 		}
//
// 		public bool FogObscures(Actor a) { return RenderPlayer != null && !a.CanBeViewedByPlayer(RenderPlayer); }
// 		public bool FogObscures(CPos p) { return RenderPlayer != null && !RenderPlayer.Shroud.IsVisible(p); }
// 		public bool FogObscures(WPos pos) { return RenderPlayer != null && !RenderPlayer.Shroud.IsVisible(pos); }
// 		public bool ShroudObscures(CPos p) { return RenderPlayer != null && !RenderPlayer.Shroud.IsExplored(p); }
// 		public bool ShroudObscures(MPos uv) { return RenderPlayer != null && !RenderPlayer.Shroud.IsExplored(uv); }
// 		public bool ShroudObscures(WPos pos) { return RenderPlayer != null && !RenderPlayer.Shroud.IsExplored(pos); }
// 		public bool ShroudObscures(PPos uv) { return RenderPlayer != null && !RenderPlayer.Shroud.IsExplored(uv); }
//
// 		public bool IsReplay => OrderManager.Connection is ReplayConnection;
//
// 		public bool IsLoadingGameSave => OrderManager.NetFrameNumber <= OrderManager.GameSaveLastFrame;
//
// 		public int GameSaveLoadingPercentage => OrderManager.NetFrameNumber * 100 / OrderManager.GameSaveLastFrame;
//
// 		private void SetLocalPlayer(Player localPlayer)
// 		{
// 			if (localPlayer == null)
// 				return;
//
// 			if (!Players.Contains(localPlayer))
// 				throw new ArgumentException("The local player must be one of the players in the world.", nameof(localPlayer));
//
// 			if (IsReplay)
// 				return;
//
// 			LocalPlayer = localPlayer;
//
// 			// Set the property backing field directly
// 			_renderPlayer = LocalPlayer;
// 		}
//
// 		public readonly Actor WorldActor;
//
// 		public readonly Map Map;
//
// 		public readonly IActorMap ActorMap;
// 		public readonly ScreenMap ScreenMap;
// 		public readonly WorldType Type;
//
// 		public readonly IValidateOrder[] OrderValidators;
// 		private readonly INotifyPlayerDisconnected[] _notifyDisconnected;
//
// 		private readonly GameInformation _gameInfo;
//
// 		// Hide the OrderManager from mod code
// 		public void IssueOrder(Order o) { OrderManager.IssueOrder(o); }
//
// 		private readonly Type _defaultOrderGeneratorType;
//
// 		private IOrderGenerator _orderGenerator;
// 		public IOrderGenerator OrderGenerator
// 		{
// 			get => _orderGenerator;
//
// 			set
// 			{
// 				Sync.AssertUnsynced("The current order generator may not be changed from synced code");
// 				_orderGenerator?.Deactivate();
//
// 				_orderGenerator = value;
// 			}
// 		}
//
// 		public readonly ISelection Selection;
// 		public readonly IControlGroups ControlGroups;
//
// 		public void CancelInputMode() { OrderGenerator = (IOrderGenerator)modData.ObjectCreator.CreateBasic(_defaultOrderGeneratorType); }
//
// 		public bool ToggleInputMode<T>() where T : IOrderGenerator, new()
// 		{
// 			if (OrderGenerator is T)
// 			{
// 				CancelInputMode();
// 				return false;
// 			}
// 			else
// 			{
// 				OrderGenerator = new T();
// 				return true;
// 			}
// 		}
//
// 		public bool RulesContainTemporaryBlocker { get; }
//
// 		private bool _wasLoadingGameSave;
//
// 		internal World(string mapUid, ModData modData, OrderManager orderManager, WorldType type)
// 		{
// 			this.modData = modData;
// 			Type = type;
// 			OrderManager = orderManager;
// 			using (new PerfTimer("PrepareMap"))
// 				Map = modData.PrepareMap(mapUid);
//
// 			if (string.IsNullOrEmpty(modData.Manifest.DefaultOrderGenerator))
// 				throw new InvalidDataException("mod.yaml must define a DefaultOrderGenerator");
//
// 			_defaultOrderGeneratorType = modData.ObjectCreator.FindType(modData.Manifest.DefaultOrderGenerator);
// 			if (_defaultOrderGeneratorType == null)
// 				throw new InvalidDataException($"{modData.Manifest.DefaultOrderGenerator} is not a valid DefaultOrderGenerator");
//
// 			_orderGenerator = (IOrderGenerator)modData.ObjectCreator.CreateBasic(_defaultOrderGeneratorType);
//
// 			var gameSpeeds = modData.Manifest.Get<GameSpeeds>();
// 			var gameSpeedName = orderManager.LobbyInfo.GlobalSettings.OptionOrDefault("gamespeed", gameSpeeds.DefaultSpeed);
// 			GameSpeed = gameSpeeds.Speeds[gameSpeedName];
// 			Timestep = ReplayTimestep = GameSpeed.Timestep;
//
// 			SharedRandom = new MersenneTwister(orderManager.LobbyInfo.GlobalSettings.RandomSeed);
// 			LocalRandom = new MersenneTwister();
//
// 			ModelCache = modData.ModelSequenceLoader.CacheModels(Map, modData, Map.Rules.ModelSequences);
//
// 			SystemActors worldActorType = type == WorldType.Editor ? SystemActors.EditorWorld : SystemActors.World;
// 			WorldActor = CreateActor(worldActorType.ToString(), new TypeDictionary());
// 			ActorMap = WorldActor.Trait<IActorMap>();
// 			ScreenMap = WorldActor.Trait<ScreenMap>();
// 			Selection = WorldActor.Trait<ISelection>();
// 			ControlGroups = WorldActor.Trait<IControlGroups>();
// 			OrderValidators = WorldActor.TraitsImplementing<IValidateOrder>().ToArray();
// 			_notifyDisconnected = WorldActor.TraitsImplementing<INotifyPlayerDisconnected>().ToArray();
//
// 			LongBitSet<PlayerBitMask>.Reset();
//
// 			// Create an isolated RNG to simplify synchronization between client and server player faction/spawn assignments
// 			var playerRandom = new MersenneTwister(orderManager.LobbyInfo.GlobalSettings.RandomSeed);
// 			foreach (ICreatePlayers cmp in WorldActor.TraitsImplementing<ICreatePlayers>())
// 				cmp.CreatePlayers(this, playerRandom);
//
// 			Game.Sound.SoundVolumeModifier = 1.0f;
//
// 			_gameInfo = new GameInformation
// 			{
// 				Mod = Game.ModData.Manifest.Id,
// 				Version = Game.ModData.Manifest.Metadata.Version,
//
// 				MapUid = Map.Uid,
// 				MapTitle = Map.Title
// 			};
//
// 			RulesContainTemporaryBlocker = Map.Rules.Actors.Any(a => a.Value.HasTraitInfo<ITemporaryBlockerInfo>());
// 			gameSettings = Game.Settings.Game;
// 		}
//
// 		public void AddToMaps(Actor self, IOccupySpace ios)
// 		{
// 			ActorMap.AddInfluence(self, ios);
// 			ActorMap.AddPosition(self, ios);
// 			ScreenMap.AddOrUpdate(self);
// 		}
//
// 		public void UpdateMaps(Actor self, IOccupySpace ios)
// 		{
// 			if (!self.IsInWorld)
// 				return;
//
// 			ScreenMap.AddOrUpdate(self);
// 			ActorMap.UpdatePosition(self, ios);
// 		}
//
// 		public void RemoveFromMaps(Actor self, IOccupySpace ios)
// 		{
// 			ActorMap.RemoveInfluence(self, ios);
// 			ActorMap.RemovePosition(self, ios);
// 			ScreenMap.Remove(self);
// 		}
//
// 		public void LoadComplete(WorldRenderer wr)
// 		{
// 			if (IsLoadingGameSave)
// 			{
// 				_wasLoadingGameSave = true;
// 				Game.Sound.DisableAllSounds = true;
// 				foreach (INotifyGameLoading nsr in WorldActor.TraitsImplementing<INotifyGameLoading>())
// 					nsr.GameLoading(this);
// 			}
//
// 			// ScreenMap must be initialized before anything else
// 			using (new PerfTimer("ScreenMap.WorldLoaded"))
// 				ScreenMap.WorldLoaded(this, wr);
//
// 			foreach (IWorldLoaded iwl in WorldActor.TraitsImplementing<IWorldLoaded>())
// 			{
// 				// These have already been initialized
// 				if (iwl == ScreenMap)
// 					continue;
//
// 				using (new PerfTimer(iwl.GetType().Name + ".WorldLoaded"))
// 					iwl.WorldLoaded(this, wr);
// 			}
//
// 			foreach (Player p in Players)
// 				foreach (IWorldLoaded iwl in p.PlayerActor.TraitsImplementing<IWorldLoaded>())
// 					using (new PerfTimer(iwl.GetType().Name + ".WorldLoaded"))
// 						iwl.WorldLoaded(this, wr);
//
// 			_gameInfo.StartTimeUtc = DateTime.UtcNow;
// 			foreach (Player player in Players)
// 				_gameInfo.AddPlayer(player, OrderManager.LobbyInfo);
//
// 			_gameInfo.DisabledSpawnPoints = OrderManager.LobbyInfo.DisabledSpawnPoints;
//
// 			if (OrderManager.Connection is NetworkConnection nc && nc.Recorder != null)
// 				nc.Recorder.Metadata = new ReplayMetadata(_gameInfo);
// 		}
//
// 		public void SetWorldOwner(Player p)
// 		{
// 			WorldActor.Owner = p;
// 		}
//
// 		public Actor CreateActor(string name, TypeDictionary initDict)
// 		{
// 			return CreateActor(true, name, initDict);
// 		}
//
// 		public Actor CreateActor(bool addToWorld, ActorReference reference)
// 		{
// 			return CreateActor(addToWorld, reference.Type, reference.InitDict);
// 		}
//
// 		public Actor CreateActor(bool addToWorld, string name, TypeDictionary initDict)
// 		{
// 			var a = new Actor(this, name, initDict);
// 			a.Initialize(addToWorld);
// 			return a;
// 		}
//
// 		public void Add(Actor a)
// 		{
// 			a.IsInWorld = true;
// 			_actors.Add(a.ActorID, a);
// 			ActorAdded(a);
//
// 			foreach (INotifyAddedToWorld t in a.TraitsImplementing<INotifyAddedToWorld>())
// 				t.AddedToWorld(a);
// 		}
//
// 		public void Remove(Actor a)
// 		{
// 			a.IsInWorld = false;
// 			_actors.Remove(a.ActorID);
// 			ActorRemoved(a);
//
// 			foreach (INotifyRemovedFromWorld t in a.TraitsImplementing<INotifyRemovedFromWorld>())
// 				t.RemovedFromWorld(a);
// 		}
//
// 		public void Add(IEffect e)
// 		{
// 			_effects.Add(e);
//
// 			if (!(e is ISpatiallyPartitionable))
// 				unpartitionedEffects.Add(e);
//
// 			if (e is ISync se)
// 				syncedEffects.Add(se);
// 		}
//
// 		public void Remove(IEffect e)
// 		{
// 			_effects.Remove(e);
//
// 			if (!(e is ISpatiallyPartitionable))
// 				unpartitionedEffects.Remove(e);
//
// 			if (e is ISync se)
// 				syncedEffects.Remove(se);
// 		}
//
// 		public void RemoveAll(Predicate<IEffect> predicate)
// 		{
// 			_effects.RemoveAll(predicate);
// 			unpartitionedEffects.RemoveAll(e => predicate(e));
// 			syncedEffects.RemoveAll(e => predicate((IEffect)e));
// 		}
//
// 		public void AddFrameEndTask(Action<World> a) { frameEndActions.Enqueue(a); }
//
// 		public event Action<Actor> ActorAdded = _ => { };
// 		public event Action<Actor> ActorRemoved = _ => { };
//
// 		public bool Paused { get; internal set; }
// 		public bool PredictedPaused { get; internal set; }
//
// 		public int WorldTick { get; private set; }
//
// 		private readonly Dictionary<int, MiniYaml> _gameSaveTraitData = new Dictionary<int, MiniYaml>();
// 		internal void AddGameSaveTraitData(int traitIndex, MiniYaml yaml)
// 		{
// 			_gameSaveTraitData[traitIndex] = yaml;
// 		}
//
// 		public void SetPauseState(bool paused)
// 		{
// 			if (IsGameOver)
// 				return;
//
// 			IssueOrder(Order.FromTargetString("PauseGame", paused ? "Pause" : "UnPause", false));
// 			PredictedPaused = paused;
// 		}
//
// 		public void SetLocalPauseState(bool paused)
// 		{
// 			Paused = PredictedPaused = paused;
// 		}
//
// 		public void Tick()
// 		{
// 			if (_wasLoadingGameSave && !IsLoadingGameSave)
// 			{
// 				foreach (var kv in _gameSaveTraitData)
// 				{
// 					var tp = TraitDict.ActorsWithTrait<IGameSaveTraitData>()
// 						.Skip(kv.Key)
// 						.FirstOrDefault();
//
// 					if (tp.Actor == null)
// 						break;
//
// 					tp.Trait.ResolveTraitData(tp.Actor, kv.Value.Nodes);
// 				}
//
// 				_gameSaveTraitData.Clear();
//
// 				Game.Sound.DisableAllSounds = false;
// 				foreach (INotifyGameLoaded nsr in WorldActor.TraitsImplementing<INotifyGameLoaded>())
// 					nsr.GameLoaded(this);
//
// 				_wasLoadingGameSave = false;
// 			}
//
// 			// Allow users to pause the shellmap via the settings menu
// 			// Some traits initialize important state during the first tick, so we must allow it to tick at least once
// 			if (!Paused && (Type != WorldType.Shellmap || !gameSettings.PauseShellmap || WorldTick == 0))
// 			{
// 				WorldTick++;
//
// 				using (new PerfSample("tick_actors"))
// 					foreach (Actor a in _actors.Values)
// 						a.Tick();
//
// 				ApplyToActorsWithTraitTimed<ITick>((actor, trait) => trait.Tick(actor), "Trait");
//
// 				_effects.DoTimed(e => e.Tick(this), "Effect");
// 			}
//
// 			while (frameEndActions.Count != 0)
// 				frameEndActions.Dequeue()(this);
// 		}
//
// 		// For things that want to update their render state once per tick, ignoring pause state
// 		public void TickRender(WorldRenderer wr)
// 		{
// 			ApplyToActorsWithTraitTimed<ITickRender>((actor, trait) => trait.TickRender(wr, actor), "Render");
// 			ScreenMap.TickRender();
// 		}
//
// 		public IEnumerable<Actor> Actors => _actors.Values;
// 		public IEnumerable<IEffect> Effects => _effects;
// 		public IEnumerable<IEffect> UnpartitionedEffects => unpartitionedEffects;
// 		public IEnumerable<ISync> SyncedEffects => syncedEffects;
//
// 		public Actor GetActorById(uint actorId)
// 		{
// 			if (_actors.TryGetValue(actorId, out Actor a))
// 				return a;
// 			return null;
// 		}
//
// 		private uint _nextAid = 0;
// 		internal uint NextAid()
// 		{
// 			return _nextAid++;
// 		}
//
// 		public int SyncHash()
// 		{
// 			// using (new PerfSample("synchash"))
// 			{
// 				var n = 0;
// 				var ret = 0;
//
// 				// Hash all the actors.
// 				foreach (Actor a in Actors)
// 					ret += n++ * (int)(1 + a.ActorID) * Sync.HashActor(a);
//
// 				// Hash fields marked with the ISync interface.
// 				foreach (Actor actor in ActorsHavingTrait<ISync>())
// 					foreach (Actor.SyncHash syncHash in actor.SyncHashes)
// 						ret += n++ * (int)(1 + actor.ActorID) * syncHash.Hash();
//
// 				// Hash game state relevant effects such as projectiles.
// 				foreach (var sync in SyncedEffects)
// 					ret += n++ * Sync.Hash(sync);
//
// 				// Hash the shared random number generator.
// 				ret += SharedRandom.Last;
//
// 				// Hash player RenderPlayer status
// 				foreach (Player p in Players)
// 					if (p.UnlockedRenderPlayer)
// 						ret += Sync.HashPlayer(p);
//
// 				return ret;
// 			}
// 		}
//
// 		public IEnumerable<TraitPair<T>> ActorsWithTrait<T>()
// 		{
// 			return TraitDict.ActorsWithTrait<T>();
// 		}
//
// 		public void ApplyToActorsWithTraitTimed<T>(Action<Actor, T> action, string text)
// 		{
// 			TraitDict.ApplyToActorsWithTraitTimed(action, text);
// 		}
//
// 		public void ApplyToActorsWithTrait<T>(Action<Actor, T> action)
// 		{
// 			TraitDict.ApplyToActorsWithTrait(action);
// 		}
//
// 		public IEnumerable<Actor> ActorsHavingTrait<T>()
// 		{
// 			return TraitDict.ActorsHavingTrait<T>();
// 		}
//
// 		public IEnumerable<Actor> ActorsHavingTrait<T>(Func<T, bool> predicate)
// 		{
// 			return TraitDict.ActorsHavingTrait(predicate);
// 		}
//
// 		public void OnPlayerWinStateChanged(Player player)
// 		{
// 			var pi = _gameInfo.GetPlayer(player);
// 			if (pi != null)
// 			{
// 				pi.Outcome = player.WinState;
// 				pi.OutcomeTimestampUtc = DateTime.UtcNow;
// 			}
// 		}
//
// 		internal void OnClientDisconnected(int clientId)
// 		{
// 			foreach (Player player in Players.Where(p => p.ClientIndex == clientId && p.PlayerReference.Playable))
// 			{
// 				foreach (INotifyPlayerDisconnected np in _notifyDisconnected)
// 					np.PlayerDisconnected(WorldActor, player);
//
// 				foreach (Player p in Players)
// 					p.PlayerDisconnected(player);
//
// 				var pi = _gameInfo.GetPlayer(player);
// 				if (pi != null)
// 					pi.DisconnectFrame = OrderManager.NetFrameNumber;
// 			}
// 		}
//
// 		public void RequestGameSave(string filename)
// 		{
// 			// Allow traits to save arbitrary data that will be passed back via IGameSaveTraitData.ResolveTraitData
// 			// at the end of the save restoration
// 			// TODO: This will need to be generalized to a request / response pair for multiplayer game saves
// 			var i = 0;
// 			foreach (var tp in TraitDict.ActorsWithTrait<IGameSaveTraitData>())
// 			{
// 				var data = tp.Trait.IssueTraitData(tp.Actor);
// 				if (data != null)
// 				{
// 					var yaml = new List<MiniYamlNode>() { new MiniYamlNode(i.ToString(), new MiniYaml("", data)) };
// 					IssueOrder(Order.FromTargetString("GameSaveTraitData", yaml.WriteToString(), true));
// 				}
//
// 				i++;
// 			}
//
// 			IssueOrder(Order.FromTargetString("CreateGameSave", filename, true));
// 		}
//
// 		public bool Disposing;
//
// 		public void Dispose()
// 		{
// 			Disposing = true;
//
// 			OrderGenerator?.Deactivate();
//
// 			frameEndActions.Clear();
//
// 			Game.Sound.StopAudio();
// 			Game.Sound.StopVideo();
// 			if (IsLoadingGameSave)
// 				Game.Sound.DisableAllSounds = false;
//
// 			ModelCache.Dispose();
//
// 			// Dispose newer actors first, and the world actor last
// 			foreach (Actor a in _actors.Values.Reverse())
// 				a.Dispose();
//
// 			// Actor disposals are done in a FrameEndTask
// 			while (frameEndActions.Count != 0)
// 				frameEndActions.Dequeue()(this);
//
// 			// HACK: The shellmap OrderManager is owned by its world in order to avoid
// 			// problems with having multiple OMs active when joining a game lobby from the main menu.
// 			// A matching check in Game.JoinInner handles OM disposal for all other cases.
// 			if (Type == WorldType.Shellmap)
// 				OrderManager.Dispose();
//
// 			Map.Dispose();
//
// 			Game.FinishBenchmark();
// 		}
//
// 		public void OutOfSync()
// 		{
// 			EndGame();
//
// 			// In the event the replay goes out of sync, it becomes no longer usable. For polish we permanently pause the world.
// 			ReplayTimestep = 0;
// 		}
// 	}
//
// }

