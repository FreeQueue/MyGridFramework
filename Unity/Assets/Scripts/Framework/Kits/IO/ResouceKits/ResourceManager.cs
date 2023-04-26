using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

namespace Framework.Kits.ResourceKits
{
	public class ResourceManager
	{
		//public static int assetsPackageMaxFailTimes = 2;
		public static ResourceMode mode = ResourceMode.OfflinePlayMode;

		private static readonly Dictionary<string, AssetsPackageStatus> _assetsPackageStatus = new();
		private static readonly Dictionary<string, AssetOperationHandle> _assetOperationHandles = new();
		private static readonly Dictionary<string, SubAssetsOperationHandle> _subAssetOperationHandles = new();
		private static readonly Dictionary<string, RawFileOperationHandle> _rawFileOperationHandles = new();
		private static readonly Dictionary<string, SceneOperationHandle> _sceneOperationHandles = new();

		private static readonly HashSet<InstantiateOperation> _instantiatingOperations = new();

		public static double AssetsPackageWaitSeconds { get; set; } = 60;
		public static AssetsPackage DefaultPackage { get; private set; }

		public static void Init() {
			YooAssets.Initialize();
		}

		public static async UniTaskVoid InitWithDefaultPackage(string packageName) {
			Init();
			AssetsPackage package = await InitializeAssetsPackageAsync(packageName);
			DefaultPackage = package;
			YooAssets.SetDefaultAssetsPackage(package);
		}
		public static void Shutdown() {
			YooAssets.Destroy();
		}

		public static bool HasAssetsPackage(string packageName) {
			return YooAssets.HasAssetsPackage(packageName);
		}
		public static AssetsPackageStatus GetAssetsPackageStatus(string packageName) {
			return _assetsPackageStatus.ContainsKey(packageName)
				? _assetsPackageStatus[packageName]
				: AssetsPackageStatus.None;
		}
		public static int GetAssetsPackageInitFailTimes(string packageName) {
			if (!_assetsPackageStatus.ContainsKey(packageName)) return 0;
			AssetsPackageStatus status = _assetsPackageStatus[packageName];
			return status >= AssetsPackageStatus.Failed ? status - AssetsPackageStatus.Failed + 1 : 0;
		}
		public static AssetsPackage GetAssetsPackage(string packageName) {
			return string.IsNullOrEmpty(packageName) ? DefaultPackage : YooAssets.GetAssetsPackage(packageName);
		}
		public static AssetsPackage CreateAssetsPackage(string packageName) {
			return YooAssets.CreateAssetsPackage(packageName);
		}
		public static void UnloadAssetsPackage(string packageName) {
			AssetsPackage package = GetAssetsPackage(packageName);
			package.UnloadUnusedAssets();
		}
		private static InitializeParameters GetInitializeParameters(string packageName) {
			InitializeParameters parameters = mode switch {
				ResourceMode.EditorSimulateMode => new EditorSimulateModeParameters {
					SimulatePatchManifestPath = EditorSimulateModeHelper.SimulateBuild(packageName),
				},
				ResourceMode.OfflinePlayMode => new OfflinePlayModeParameters(),
				ResourceMode.HostPlayMode => new HostPlayModeParameters(),
				_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
			};
			return parameters;
		}
		public static async UniTask<AssetsPackage> InitializeAssetsPackageAsync(
			string packageName, IProgress<float> progress = null
		) {
			InitializeParameters parameters = GetInitializeParameters(packageName);
			AssetsPackage package = HasAssetsPackage(packageName)
				? GetAssetsPackage(packageName)
				: CreateAssetsPackage(packageName);

			InitializationOperation handle = package.InitializeAsync(parameters);
			try {
				_assetsPackageStatus[packageName] = AssetsPackageStatus.Waiting;
				await handle.ToUniTask(progress, PlayerLoopTiming.FixedUpdate);
				_assetsPackageStatus[packageName] = AssetsPackageStatus.Inited;
			}
			catch (Exception e) {
				Debug.LogError($"{nameof(InitializeAssetsPackageAsync)} failed with {e}");
				if (_assetsPackageStatus[packageName] == AssetsPackageStatus.Failed) {
					_assetsPackageStatus[packageName] += 1;
				}
				else {
					_assetsPackageStatus[packageName] = AssetsPackageStatus.Failed;
				}
				throw;
			}
			return package;
		}

		public static async UniTask<AssetsPackage> GetAssetsPackageAsync(
			string packageName, int tolerantFailTimes = 0, IProgress<float> progress = null
		) {
			while (true) {
				AssetsPackageStatus status = GetAssetsPackageStatus(packageName);
				switch (status) {
					case AssetsPackageStatus.Waiting:
						try {
							var cts = new CancellationTokenSource(TimeSpan.FromSeconds(AssetsPackageWaitSeconds));
							await UniTask.WaitUntil(
								() => GetAssetsPackageStatus(packageName) == AssetsPackageStatus.Inited,
								PlayerLoopTiming.FixedUpdate,
								cts.Token);
							return GetAssetsPackage(packageName);
						}
						catch (OperationCanceledException) {
							Debug.LogError($"Wait {nameof(InitializeAssetsPackageAsync)} timeout.");
							tolerantFailTimes--;
							break;
						}
					case AssetsPackageStatus.Inited:
						return GetAssetsPackage(packageName);
					case AssetsPackageStatus.None: break;
					case AssetsPackageStatus.Failed:
					default:
						int actualFailTimes = GetAssetsPackageInitFailTimes(packageName);
						if (actualFailTimes > tolerantFailTimes)
							throw new TimeoutException(
								string.Format("{0} failed;{1}:{2};{3}:{4}",
									nameof(GetAssetsPackageAsync),
									nameof(tolerantFailTimes), tolerantFailTimes,
									nameof(actualFailTimes), actualFailTimes));
						break;
				}
				try {
					return await InitializeAssetsPackageAsync(packageName, progress);
				}
				catch (Exception) {
					// ignored
				}
			}
		}

		#region LoadObject
		private static async UniTask<AssetOperationHandle> AssetOperationAsync<T>(
			string location, IProgress<float> progress = null, string packageName = null
		) where T : Object {
			if (!_assetOperationHandles.TryGetValue(location, out AssetOperationHandle handle) || !handle.IsValid) {
				AssetsPackage package = await GetAssetsPackageAsync(packageName);
				handle = package.LoadAssetAsync<T>(location);
				_assetOperationHandles[location] = handle;
				Debug.Log(string.Format("Load object {0}.\nPath:{1}",
					location, package.GetAssetInfo(location).AssetPath));
			}
			await handle.ToUniTask(progress, PlayerLoopTiming.FixedUpdate);
			return handle;
		}

		private static AssetOperationHandle AssetOperationSync<T>(
			string location, string packageName = null
		) where T : Object {
			if (!_assetOperationHandles.TryGetValue(location, out AssetOperationHandle handle) || !handle.IsValid) {
				AssetsPackage package = GetAssetsPackage(packageName);
				handle = package.LoadAssetSync<T>(location);
				_assetOperationHandles[location] = handle;
				Debug.Log(string.Format("Load object {0}.\nPath:{1}",
					location, package.GetAssetInfo(location).AssetPath));
			}
			return handle;
		}

		public static async UniTask<T> LoadObjectAsync<T>(
			string location, IProgress<float> progress = null, string packageName = null
		) where T : Object {
			AssetOperationHandle handle = await AssetOperationAsync<T>(location, progress, packageName);
			return handle.AssetObject as T;
		}
		public static async UniTask<GameObject> LoadGameObjectAsync(
			string location, IProgress<float> progress = null, string packageName = null
		) {
			return await LoadObjectAsync<GameObject>(location, progress, packageName);
		}

		public static T LoadObjectSync<T>(string location, string packageName = null) where T : Object {
			AssetOperationHandle handle = AssetOperationSync<T>(location, packageName);
			return handle.AssetObject as T;
		}
		public static GameObject LoadGameObjectSync(string location, string packageName = null) {
			return LoadObjectSync<GameObject>(location, packageName);
		}
		#endregion

		#region InstantiateGameObject
		public static async UniTask<GameObject> InstantiateGameObjectAsync(
			string location, IProgress<float> progress = null, string packageName = null
		) {
			AssetOperationHandle assetHandle
				= await AssetOperationAsync<GameObject>(location, packageName: packageName);
			InstantiateOperation handle = assetHandle.InstantiateAsync();
			_instantiatingOperations.Add(handle);
			try {
				await handle.ToUniTask(progress, PlayerLoopTiming.FixedUpdate);
			}
			finally {
				handle.Cancel();
				_instantiatingOperations.Remove(handle);
			}
			return handle.Result;
		}

		public static GameObject InstantiateGameObjectSync(string location, string packageName = null) {
			AssetOperationHandle assetHandle = AssetOperationSync<GameObject>(location, packageName: packageName);
			return assetHandle.InstantiateSync();
		}

		public static void CancelAllInstantiatingOperation() {
			foreach (InstantiateOperation handle in _instantiatingOperations) {
				handle.Cancel();
			}
			_instantiatingOperations.Clear();
		}
		#endregion

		#region LoadSubObject
		private static async UniTask<SubAssetsOperationHandle> SubAssetsOperationAsync<T>(
			string location, IProgress<float> progress = null, string packageName = null
		) where T : Object {
			if (!_subAssetOperationHandles.TryGetValue(location, out SubAssetsOperationHandle handle) ||
				!handle.IsValid) {
				AssetsPackage package = await GetAssetsPackageAsync(packageName);
				handle = package.LoadSubAssetsAsync<T>(location);
				_subAssetOperationHandles[location] = handle;
				Debug.Log(string.Format("Load subObjects {0}.\nPath:{1}",
					location, package.GetAssetInfo(location).AssetPath));
			}
			if (!handle.IsDone) await handle.ToUniTask(progress, PlayerLoopTiming.FixedUpdate);
			return handle;
		}

		private static SubAssetsOperationHandle SubAssetsOperationSync<T>(string location, string packageName = null)
			where T : Object {
			if (!_subAssetOperationHandles.TryGetValue(location, out SubAssetsOperationHandle handle) ||
				!handle.IsValid) {
				AssetsPackage package = GetAssetsPackage(packageName);
				handle = package.LoadSubAssetsSync<T>(location);
				_subAssetOperationHandles[location] = handle;
				Debug.Log(string.Format("Load subObjects {0}.\nPath:{1}",
					location, package.GetAssetInfo(location).AssetPath));
			}
			return handle;
		}

		public static async UniTask<T[]> LoadSubObjectsAsync<T>(
			string location, IProgress<float> progress = null, string packageName = null
		) where T : Object {
			SubAssetsOperationHandle handle = await SubAssetsOperationAsync<T>(location, progress, packageName);
			return handle.GetSubAssetObjects<T>();
		}
		public static T[] LoadSubObjectsSync<T>(string location, string packageName = null) where T : Object {
			SubAssetsOperationHandle handle = SubAssetsOperationSync<T>(location, packageName);
			return handle.GetSubAssetObjects<T>();
		}
		public static async UniTask<T> LoadSubObjectAsync<T>(
			string location, string subObjectName, IProgress<float> progress = null, string packageName = null
		) where T : Object {
			SubAssetsOperationHandle handle = await SubAssetsOperationAsync<T>(location, progress, packageName);
			return handle.GetSubAssetObject<T>(subObjectName);
		}
		public static T LoadSubObjectSync<T>(
			string location, string subObjectName, string packageName = null
		) where T : Object {
			SubAssetsOperationHandle handle = SubAssetsOperationSync<T>(location, packageName);
			return handle.GetSubAssetObject<T>(subObjectName);
		}
		public static async UniTask<Sprite> LoadSubSpriteAsync(
			string location, string spriteName, IProgress<float> progress = null, string packageName = null
		) {
			return await LoadSubObjectAsync<Sprite>(location, spriteName, progress, packageName);
		}
		public static Sprite LoadSubSpriteAsync(string location, string spriteName, string packageName = null) {
			return LoadSubObjectSync<Sprite>(location, spriteName, packageName);
		}
		#endregion

		#region LoadRawFile
		private static async UniTask<RawFileOperationHandle> RawFileOperationAsync(
			string location, IProgress<float> progress = null, string packageName = null
		) {
			if (!_rawFileOperationHandles.TryGetValue(location, out RawFileOperationHandle handle) || !handle.IsValid) {
				AssetsPackage package = await GetAssetsPackageAsync(packageName);
				handle = package.LoadRawFileAsync(location);
				_rawFileOperationHandles[location] = handle;
				Debug.Log(string.Format("Load rawFile {0}.\nPath:{1}",
					location, package.GetAssetInfo(location).AssetPath));
			}
			if (!handle.IsDone) await handle.ToUniTask(progress, PlayerLoopTiming.FixedUpdate);
			return handle;
		}
		private static RawFileOperationHandle RawFileOperationSync(string location, string packageName = null) {
			AssetsPackage package = GetAssetsPackage(packageName);
			return package.LoadRawFileSync(location);
		}
		public static async UniTask<byte[]> LoadRawDataAsync(
			string location, IProgress<float> progress = null, string packageName = null
		) {
			return (await RawFileOperationAsync(location, progress, packageName)).GetRawFileData();
		}
		public static byte[] LoadRawDataSync(string location, string packageName = null) {
			return RawFileOperationSync(location, packageName).GetRawFileData();
		}
		public static async UniTask<string> LoadRawTextAsync(
			string location, IProgress<float> progress = null, string packageName = null
		) {
			return (await RawFileOperationAsync(location, progress, packageName)).GetRawFileText();
		}
		public static string LoadRawTextSync(string location, string packageName = null) {
			return RawFileOperationSync(location, packageName).GetRawFileText();
		}
		#endregion

		#region LoadScene
		public static async UniTask<Scene> LoadSceneAsync(
			string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true,
			IProgress<float> progress = null, string packageName = null
		) {
			if (!_sceneOperationHandles.TryGetValue(location, out SceneOperationHandle handle) || !handle.IsValid) {
				AssetsPackage package = await GetAssetsPackageAsync(packageName);
				handle = package.LoadSceneAsync(location, sceneMode, activateOnLoad);
				_sceneOperationHandles[location] = handle;
				Debug.Log(string.Format("Load scene {0}.\nPath:{1}",
					location, package.GetAssetInfo(location).AssetPath));
			}
			if (!handle.IsDone) {
				await handle.ToUniTask(progress, PlayerLoopTiming.FixedUpdate);
			}
			return handle.SceneObject;
		}
		public static async UniTask UnLoadSceneAsync(
			string location, IProgress<float> progress = null
		) {
			if (_sceneOperationHandles.TryGetValue(location, out SceneOperationHandle loadHandle) &&
				loadHandle.IsValid) {
				UnloadSceneOperation handle = loadHandle.UnloadAsync();
				Debug.Log($"Unload scene {location}.");
				await handle.ToUniTask(progress, PlayerLoopTiming.FixedUpdate);
				_sceneOperationHandles.Remove(location);
			}
		}
		#endregion

	}
}