using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework;
using Framework.Kits.ResourceKits;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kits.Framework.Kits.SceneKits
{
	public static class SceneManager
	{
		public static async UniTask<Scene> LoadSceneAsync(
			string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true,
			string packageName = null, IProgress<float> progress = null
		) {
			return await ResourceManager.LoadSceneAsync(location, sceneMode, activateOnLoad, progress, packageName);
		}
		public static async UniTask UnloadSceneAsync(
			string location, IProgress<float> progress = null
		) {
			await ResourceManager.UnLoadSceneAsync(location, progress);
			Debug.Log($"Unload scene {location} success.");
		}

		public static async UniTask UnloadSceneAsync(
			Scene scene, IProgress<float> progress = null, CancellationToken token = default
		) {
			await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene)
				.ToUniTask(progress, PlayerLoopTiming.FixedUpdate, token);
			Debug.Log($"Unload scene {scene} success.");
		}
	}
}