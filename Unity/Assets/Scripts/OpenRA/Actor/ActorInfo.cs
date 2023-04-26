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
using Framework;
using Framework.Kits.BitSetKits;
using Framework.Kits.CollectionsUnmanaged;
using Framework.Kits.MiniYamlKits;
using Framework.Kits.YamlSerializeKits;
using OpenRA.Traits;

namespace OpenRA
{
	/// <summary>
	///     A unit/building inside the game. Every rules starts with one and adds trait to it.
	/// </summary>
	public class ActorInfo
	{
		/// <summary>
		///     The actor name can be anything, but the sprites used in the Render*: traits default to this one.
		///     If you add an ^ in front of the name, the engine will recognize this as a collection of traits
		///     that can be inherited by others (using Inherits:) and not a real unit.
		///     You can remove inherited traits by adding a - in front of them as in -TraitName: to inherit everything, but this
		///     trait.
		/// </summary>
		public const string ABSTRACT_ACTOR_PREFIX = "^";
		public const char TRAIT_INSTANCE_SEPARATOR = '@';
		private readonly TypeDictionary _traits = new TypeDictionary();

		public readonly string Name;
		private List<TraitInfo> _constructOrderCache;

		public ActorInfo(string name, MiniYaml node) {
			try {
				Name = name;

				foreach (MiniYamlNode t in node.Nodes) {
					try {
						// HACK: The linter does not want to crash when a trait doesn't exist but only print an error instead
						// LoadTraitInfo will only return null to signal us to abort here if the linter is running
						TraitInfo trait = LoadTraitInfo(t.Key, t.Value);
						if (trait != null) {
							_traits.Add(trait);
						}
					}
					catch (FieldLoader.MissingFieldsException e) {
						throw new YamlException(e.Message);
					}
				}

				_traits.TrimExcess();
			}
			catch (YamlException e) {
				throw new YamlException($"Actor type {name}: {e.Message}");
			}
		}

		public ActorInfo(string name, params TraitInfo[] traitInfos) {
			Name = name;
			foreach (TraitInfo t in traitInfos) {
				_traits.Add(t);
			}
			_traits.TrimExcess();
		}

		private static TraitInfo LoadTraitInfo(string traitName, MiniYaml yaml) {
			if (!string.IsNullOrEmpty(yaml.Value)) {
				throw new YamlException($"Junk value `{yaml.Value}` on trait node {traitName}");
			}

			// HACK: The linter does not want to crash when a trait doesn't exist but only print an error instead
			// ObjectCreator will only return null to signal us to abort here if the linter is running
			string[] traitInstance = traitName.Split(TRAIT_INSTANCE_SEPARATOR);
			var info = Util.Type.Create<TraitInfo>(traitInstance[0] + "Info");
			if (info == null) {
				return null;
			}

			try {
				if (traitInstance.Length > 1) {
					//TODO 为什么要用反射来设置InstanceName?
					info.GetType().GetField(nameof(info.InstanceName)).SetValue(info, traitInstance[1]);
				}

				FieldLoader.Load(info, yaml);
			}
			catch (FieldLoader.MissingFieldsException e) {
				string header = "Trait name " + traitName + ": " +
								(e.Missing.Length > 1 ? "Required properties missing" : "Required property missing");
				throw new FieldLoader.MissingFieldsException(e.Missing, header);
			}

			return info;
		}

		public IEnumerable<TraitInfo> TraitsInConstructOrder() {
			if (_constructOrderCache != null) {
				return _constructOrderCache;
			}

			var source = _traits.WithInterface<TraitInfo>().Select(i => new {
				Trait = i,
				Type = i.GetType(),
				Dependencies = PrerequisitesOf(i).ToList(),
				OptionalDependencies = OptionalPrerequisitesOf(i).ToList(),
			}).ToList();

			//已解析队列
			var resolved = source.Where(s => s.Dependencies.Count == 0 && s.OptionalDependencies.Count == 0).ToList();
			//未解析队列
			var unresolved = source.Except(resolved);
			
			Func<Type, Type, bool> testResolve = (a, b) => a == b || a.IsAssignableFrom(b);

			//未解析队列中满足条件的项
			var more = unresolved.Where(u =>
				u.Dependencies.All(dependence => //所有必要依赖项在已解析队列中存在且未解析队列中不包含
					resolved.Exists(r => testResolve(dependence, r.Type)) && 
					!unresolved.Any(u1 => testResolve(dependence, u1.Type))) && 
				u.OptionalDependencies.All(d => //所有可选依赖项在未解析队列中不包含
					!unresolved.Any(u1 => testResolve(d, u1.Type))));

			// 循环，直至没有可解析的Trait
			while (more.Any()) {
				resolved.AddRange(more);
			}

			if (unresolved.Any()) {
				string exceptionString
					= "ActorInfo(\"" + Name + "\") failed to initialize because of the following:\r\n";
				IEnumerable<Type> missing = unresolved
					.SelectMany(u => u.Dependencies.Where(d => !source.Any(s => testResolve(d, s.Type)))).Distinct();

				exceptionString += "Missing:\r\n";
				foreach (Type m in missing) {
					exceptionString += m + " \r\n";
				}

				exceptionString += "Unresolved:\r\n";
				foreach (var u in unresolved) {
					IEnumerable<Type> dependencies = u.Dependencies.Where(d => !resolved.Exists(r => r.Type == d));
					IEnumerable<Type> optDependencies
						= u.OptionalDependencies.Where(d => !resolved.Exists(r => r.Type == d));
					string allDependencies = string.Join(", ",
						dependencies.Select(o => o.ToString()).Concat(optDependencies.Select(o => $"[{o}]")));
					exceptionString += $"{u.Type}: {{ {allDependencies} }}\r\n";
				}

				throw new YamlException(exceptionString);
			}

			_constructOrderCache = resolved.Select(r => r.Trait).ToList();
			return _constructOrderCache;
		}

		public static IEnumerable<Type> PrerequisitesOf(TraitInfo info) {
			return info
				.GetType()
				.GetInterfaces()
				.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IRequires<>))
				.Select(t => t.GetGenericArguments()[0]);
		}

		public static IEnumerable<Type> OptionalPrerequisitesOf(TraitInfo info) {
			return info
				.GetType()
				.GetInterfaces()
				.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(INotBefore<>))
				.Select(t => t.GetGenericArguments()[0]);
		}

		public bool HasTraitInfo<T>() where T : ITraitInfoInterface {
			return _traits.Contains<T>();
		}
		public T TraitInfo<T>() where T : ITraitInfoInterface {
			return _traits.Get<T>();
		}
		public T TraitInfoOrDefault<T>() where T : ITraitInfoInterface {
			return _traits.GetOrDefault<T>();
		}
		public IEnumerable<T> TraitInfos<T>() where T : ITraitInfoInterface {
			return _traits.WithInterface<T>();
		}

		public BitSet<TargetableType> GetAllTargetTypes() {
			// PERF: Avoid LINQ.
			BitSet<TargetableType> targetTypes = default;
			foreach (ITargetableInfo targetable in TraitInfos<ITargetableInfo>()) {
				targetTypes = targetTypes.Union(targetable.GetTargetTypes());
			}
			return targetTypes;
		}
	}
}