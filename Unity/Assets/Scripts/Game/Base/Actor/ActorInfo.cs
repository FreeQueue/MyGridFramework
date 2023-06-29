using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Kits.CollectionsUnmanaged;
using Framework.Kits.ConfigKits;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace S100
{
	[CreateAssetMenu(fileName = "ActorInfo", menuName = "ActorInfo", order = 0)]
	public class ActorInfo : ConfigObject
	{
		public readonly string Name;

		private readonly TypeDictionary _traits = new TypeDictionary();

		[OdinSerialize]
		[ValidateInput(nameof(OnValidateTraitInfos))]
		[OnCollectionChanged(nameof(AfterTraitInfosChanged))]
		[ListDrawerSettings(OnTitleBarGUI = nameof(OnTitleBarGUI))]
		private List<TraitInfo> _traitInfos = new List<TraitInfo>();

		public ActorInfo(string name, params TraitInfo[] traitInfos) {
			Name = name;
			foreach (TraitInfo traitInfo in traitInfos) {
				_traits.Add(traitInfo);
			}
			_traits.TrimExcess();
		}

		public bool HasTraitInfo<T>() where T : ITraitInfo {
			return _traits.Contains<T>();
		}
		public T TraitInfo<T>() where T : ITraitInfo {
			return _traits.Get<T>();
		}
		public T TraitInfoOrDefault<T>() where T : ITraitInfo {
			return _traits.GetOrDefault<T>();
		}
		public IEnumerable<T> TraitInfos<T>() where T : ITraitInfo {
			return _traits.WithInterface<T>();
		}
		
		public override void OnInit() {
			foreach (TraitInfo traitInfo in _traitInfos) {
				_traits.Add(traitInfo);
			}
		}
		
		private void OnTitleBarGUI() {
			if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh)) {
				string errorMessage = null;
				if (OnValidateTraitInfos(_traitInfos, ref errorMessage)) {
					Debug.LogError(errorMessage);
				}
			}
		}

		private void AfterTraitInfosChanged(CollectionChangeInfo info, object value) {
			if (info.ChangeType == CollectionChangeType.Add) {
				var traitInfo = (TraitInfo)info.Value;
				var dependencies = traitInfo.GetPrerequisites().Where(dependence =>
					!dependence.IsAbstract &&
					!_traitInfos.Any(traitInfo => dependence.IsAssignableFrom(traitInfo.GetType())));
				foreach (Type dependence in dependencies) {
					_traitInfos.Add((TraitInfo)Activator.CreateInstance(dependence));
				}
			}
		}

		private bool OnValidateTraitInfos(
			List<TraitInfo> traitInfos, ref string errorMessage
		) {
			var source = traitInfos.Select(traitInfo => new {
				TraitInfo = traitInfo,
				Type = traitInfo.GetType(),
				Dependencies = traitInfo.GetPrerequisites().ToList(),
				OptionalDependencies = traitInfo.OptionalPrerequisitesOf().ToList(),
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
				errorMessage = "ActorInfo(\"" + Name + "\") failed to initialize because of the following:\r\n";
				IEnumerable<Type> missing = unresolved
					.SelectMany(u => u.Dependencies.Where(d => !source.Any(s => testResolve(d, s.Type)))).Distinct();

				errorMessage += "Missing:\r\n";
				foreach (Type m in missing) {
					errorMessage += m + " \r\n";
				}

				errorMessage += "Unresolved:\r\n";
				foreach (var u in unresolved) {
					IEnumerable<Type> dependencies = u.Dependencies.Where(d => !resolved.Exists(r => r.Type == d));
					IEnumerable<Type> optDependencies
						= u.OptionalDependencies.Where(d => !resolved.Exists(r => r.Type == d));
					string allDependencies = string.Join(", ",
						dependencies.Select(o => o.ToString()).Concat(optDependencies.Select(o => $"[{o}]")));
					errorMessage += $"{u.Type}: {{ {allDependencies} }}\r\n";
				}
				return false;
			}
			_traitInfos = resolved.Select(r => r.TraitInfo).ToList();
			return true;
		}
	}
}