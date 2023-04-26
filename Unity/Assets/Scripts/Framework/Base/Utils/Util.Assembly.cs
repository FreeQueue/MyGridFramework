//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Extensions;
using UnityEngine;
using SAssembly = System.Reflection.Assembly;
using SType = System.Type;

namespace Framework
{
	partial class Util
	{

		/// <summary>
		///     程序集相关的实用函数。
		/// </summary>
		public static class Assembly
		{
			private static readonly Dictionary<string, SAssembly> _resolvedAssemblies;
			private static readonly Dictionary<string, SAssembly> _externalAssemblies;
			public static IReadOnlyDictionary<string, SAssembly> DefaultAssemblies { get; }
			public static IReadOnlyDictionary<string, SAssembly> ExternalAssemblies => _externalAssemblies;
			public static int Dirty { get; private set; }
			private static int _dirty_AllAssemblies;
			private static SAssembly[] _allAssemblies;
			public static SAssembly[] AllAssemblies {
				get {
					if (_dirty_AllAssemblies != Dirty || _allAssemblies == null) {
						_allAssemblies = _externalAssemblies.Values.Concat(DefaultAssemblies.Values).ToArray();
						_dirty_AllAssemblies = Dirty;
					}
					return _allAssemblies;
				}
			}

			static Assembly() {
				DefaultAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(
					assembly => assembly.HasAttribute<ManageAttribute>()
				).ToDictionary(assembly => assembly.FullName);
				_resolvedAssemblies = new Dictionary<string, SAssembly>();
				_externalAssemblies = new Dictionary<string, SAssembly>();
			}

			public static bool LoadAssembly(byte[] data) {
				var hash = Crypto.SHA1Hash(data);

				if (_resolvedAssemblies.TryGetValue(hash, out SAssembly assembly))
					return AddAssembly(assembly);
				try {
					assembly = SAssembly.Load(data);
					_resolvedAssemblies.Add(hash, assembly);
					return AddAssembly(assembly);
				}
				catch (Exception e) {
					Debug.LogException(e);
					return false;
				}
			}

			private static bool AddAssembly(SAssembly assembly) {
				if (!assembly.HasAttribute<ManageAttribute>()) return false;
				if (_externalAssemblies.TryAdd(assembly.FullName, assembly)) Dirty++;
				return true;
			}

			public static SAssembly GetAssembly(string name) {
				if (_externalAssemblies.TryGetValue(name, out var assembly)) return assembly;
				if (DefaultAssemblies.TryGetValue(name, out assembly)) return assembly;
				return null;
			}

			public static void Clear() {
				_externalAssemblies.Clear();
				Type.TypeCache.Clear();
				Type.CtorCache.Clear();
				Dirty++;
			}

			//标记程序集，只有标记的默认程序集会被管理
			[AttributeUsage(AttributeTargets.Assembly)]
			public sealed class ManageAttribute : Attribute
			{
			}
		}
	}
}