using System;
using Framework;
using Framework.Kits.SingletonKits;
using UnityEngine;
using Helper = S100.Modules.BaseModuleHelper;

namespace S100.Modules
{
	public class BaseModule : IModule
	{
		private const int DEFAULT_DPI = 96; // default windows dpi
		private float _gameSpeedBeforePause = 1f;

		private int _frameRate;
		private bool _neverSleep;

		public int FrameRate { get => _frameRate; set => Application.targetFrameRate = _frameRate = value; }

		/// <summary>
		/// 获取或设置游戏速度。
		/// </summary>
		public float GameSpeed { get => Time.timeScale; set => Time.timeScale = value >= 0f ? value : 0f; }

		/// <summary>
		/// 获取游戏是否暂停。
		/// </summary>
		public bool IsGamePaused => GameSpeed <= 0f;

		/// <summary>
		/// 获取是否正常游戏速度。
		/// </summary>
		public bool IsNormalGameSpeed => Math.Abs(GameSpeed - 1f) < 0.0001f;

		/// <summary>
		/// 获取或设置是否允许后台运行。
		/// </summary>
		public bool RunInBackground {
			get => Application.runInBackground;
			set => Application.runInBackground  = value;
		}

		/// <summary>
		/// 获取或设置是否禁止休眠。
		/// </summary>
		public bool NeverSleep {
			get => _neverSleep;
			set {
				_neverSleep = value;
				Screen.sleepTimeout = value ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
			}
		}

		public bool MonoSingletonAutoCreate {
			get => SingletonCreator.MonoSingletonAutoCreate;
			set => SingletonCreator.MonoSingletonAutoCreate= value;
		}

		void IModule.Init() {
			ReadHelper();
			Util.Convert.ScreenDpi = Screen.dpi;
			if (Util.Convert.ScreenDpi <= 0) {
				Util.Convert.ScreenDpi = DEFAULT_DPI;
			}
		}

		public void ReadHelper() {
			MonoSingletonAutoCreate = BaseModuleHelper.Instance.monoSingletonAutoCreate;
			FrameRate = BaseModuleHelper.Instance.frameRate;
			GameSpeed = BaseModuleHelper.Instance.gameSpeed;
			RunInBackground = BaseModuleHelper.Instance.runInBackground;
			NeverSleep = BaseModuleHelper.Instance.neverSleep;
		}
		/// <summary>
		/// 暂停游戏。
		/// </summary>
		public void PauseGame() {
			if (IsGamePaused) return;
			_gameSpeedBeforePause = GameSpeed;
			GameSpeed = 0f;
		}

		/// <summary>
		/// 恢复游戏。
		/// </summary>
		public void ResumeGame() {
			if (!IsGamePaused) return;
			GameSpeed = _gameSpeedBeforePause;
		}

		/// <summary>
		/// 重置为正常游戏速度。
		/// </summary>
		public void ResetNormalGameSpeed() {
			if (IsNormalGameSpeed) return;
			GameSpeed = 1f;
		}
	}
}