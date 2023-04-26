//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Framework.Kits.ObjectPoolKits
{
	/// <summary>
	///     对象信息。
	/// </summary>
	[StructLayout(LayoutKind.Auto)]
	public readonly struct ObjectInfo
	{
		/// <summary>
		///     获取对象名称。
		/// </summary>
		public IConvertible Name { get; }

		/// <summary>
		///     获取对象自定义释放检查标记。
		/// </summary>
		public bool CustomCanReleaseFlag { get; }

		/// <summary>
		///     获取对象上次使用时间。
		/// </summary>
		public DateTime LastUseTime { get; }

		/// <summary>
		///     获取对象是否正在使用。
		/// </summary>
		public bool IsInUse => SpawnCount > 0;

		/// <summary>
		///     获取对象的获取计数。
		/// </summary>
		public int SpawnCount { get; }
		/// <summary>
		///     初始化对象信息的新实例。
		/// </summary>
		/// <param name="name">对象名称。</param>
		/// <param name="customCanReleaseFlag">对象自定义释放检查标记。</param>
		/// <param name="lastUseTime">对象上次使用时间。</param>
		/// <param name="spawnCount">对象的获取计数。</param>
		public ObjectInfo(
			IConvertible name, bool customCanReleaseFlag, DateTime lastUseTime, int spawnCount
		) {
			Name = name;
			CustomCanReleaseFlag = customCanReleaseFlag;
			LastUseTime = lastUseTime;
			SpawnCount = spawnCount;
		}
	}
}