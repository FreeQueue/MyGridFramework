using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Framework.Kits.SingletonKits
{
	public class SerializedScriptableSingleton<T> : SerializedScriptableObject
		where T : SerializedScriptableSingleton<T>
	{
		private static T _instance;
		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					string[] findAssets = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
					if (findAssets == null || findAssets.Length == 0)
					{
						Debug.LogError($"Please create ScriptableObject typeof {typeof(T)} first...");
					}
					else if (findAssets.Length > 1)
					{
						Debug.LogError($"ScriptableObject typeof {typeof(T)} exist multiple，please check they...");
					}
					else
					{
						_instance = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(findAssets[0]));
					}
				}
				return _instance;
			}
		}
	}
}