//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Framework.Kits.EntityKits
{
	/// <summary>
	/// 实体。
	/// </summary>
	public class Entity : MonoBehaviour
	{
		public int Id { get; private set; }
		public EntityInfo Info{ get; private set; }
		internal void Init(int id,EntityInfo info) {
			Id = id;
			Info = info;
			OnInit();
		}

		protected internal virtual void OnInit() {
		}

		/// <summary>
		/// 实体显示。
		/// </summary>
		/// <param name="userData">用户自定义数据。</param>
		protected internal virtual void OnShow(object userData) {
		}

		/// <summary>
		/// 实体隐藏。
		/// </summary>
		/// <param name="isShutdown">是否是关闭实体管理器时触发。</param>
		/// <param name="userData">用户自定义数据。</param>
		protected internal virtual void OnHide(bool isShutdown, object userData) {
		}

		/// <summary>
		/// 实体轮询。
		/// </summary>
		/// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
		/// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
		protected internal virtual void OnUpdate(float elapseSeconds, float realElapseSeconds) {
		}
		
		/// <summary>
		/// 实体回收。
		/// </summary>
		///  <param name="isShutdown">是否是清空对象池时触发。</param>
		protected internal virtual void OnRelease(bool isShutdown) {
		}
	}
}