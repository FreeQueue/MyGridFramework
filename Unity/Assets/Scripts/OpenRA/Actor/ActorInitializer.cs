#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Kits.CollectionsUnmanaged;
using OpenRA.Traits;

namespace OpenRA
{

	public class ActorInitializer : IActorInitializer
	{
		public readonly Actor Self;
		internal TypeDictionary Dict;

		//TODO
		//public World World => Self.World;

		public ActorInitializer(Actor actor, TypeDictionary dict) {
			Self = actor;
			Dict = dict;
		}

		public T GetOrDefault<T>(TraitInfo info) where T : ActorInit {
			IEnumerable<T> inits = Dict.WithInterface<T>();

			// 使用实例名称标记的特征首选具有相同名称的 init。
			// 如果更具体的 init 不可用，请回退到未命名的 init。
			// 如果定义了重复的初始化，则采用最后一个以匹配标准 yaml 覆盖预期
			if (!string.IsNullOrEmpty(info?.InstanceName)) {
				return inits.LastOrDefault(i => i.InstanceName == info.InstanceName) ??
						inits.LastOrDefault(i => string.IsNullOrEmpty(i.InstanceName));
			}

			// 未标记的特征将仅使用未标记的初始化
			return inits.LastOrDefault(i => string.IsNullOrEmpty(i.InstanceName));
		}

		public T Get<T>(TraitInfo info) where T : ActorInit {
			var init = GetOrDefault<T>(info);
			if (init == null) {
				throw new InvalidOperationException($"TypeDictionary does not contain instance of type `{typeof(T)}`");
			}
			return init;
		}

		public TValue GetValue<T, TValue>(TraitInfo info) where T : ValueActorInit<TValue> {
			return Get<T>(info).Value;
		}

		public TValue GetValue<T, TValue>(TraitInfo info, TValue fallback) where T : ValueActorInit<TValue> {
			var init = GetOrDefault<T>(info);
			return init != null ? init.Value : fallback;
		}

		public bool Contains<T>(TraitInfo info) where T : ActorInit {
			return GetOrDefault<T>(info) != null;
		}

		public T GetOrDefault<T>() where T : ActorInit, ISingleInstanceInit {
			return Dict.GetOrDefault<T>();
		}

		public T Get<T>() where T : ActorInit, ISingleInstanceInit {
			return Dict.Get<T>();
		}

		public TValue GetValue<T, TValue>() where T : ValueActorInit<TValue>, ISingleInstanceInit {
			return Get<T>().Value;
		}

		public TValue GetValue<T, TValue>(TValue fallback) where T : ValueActorInit<TValue>, ISingleInstanceInit {
			var init = GetOrDefault<T>();
			return init != null ? init.Value : fallback;
		}

		public bool Contains<T>() where T : ActorInit, ISingleInstanceInit {
			return GetOrDefault<T>() != null;
		}
	}



}