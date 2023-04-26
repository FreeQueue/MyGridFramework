//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------


using Framework.Extensions;
using Framework.Kits.ObjectPoolKits;

namespace Framework.Kits.EntityKits
{
	partial class EntityGroup
	{
		/// <summary>
		/// 实体实例对象。
		/// </summary>
		private sealed class EntityObject : Object<Entity>
		{
			protected override void OnSpawn() {
				target.gameObject.SetActive(true);
			}
			protected override void OnUnspawn() {
				target.gameObject.SetActive(false);
				target.Parent(target.Info.EntityGroup._entityPoolRoot)
					.LocalScaleIdentity()
					.LocalRotationIdentity()
					.LocalPositionIdentity();
			}
			protected internal override void Release(bool isShutdown) {
				target.OnRelease(isShutdown);
				target.DestroyGameObj();
			}
		}
	}
}