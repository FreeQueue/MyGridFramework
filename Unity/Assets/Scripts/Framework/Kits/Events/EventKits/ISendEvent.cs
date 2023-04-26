using System;

namespace Framework.Kits.EventKits
{
	public interface ISendEvent<TEventArgs>
	{
		/// <summary>
		/// 开始通知事件接收者
		/// </summary>
		/// <param name="receiver"></param>
		/// <returns>返回Unsubscriber，释放后退订</returns>
		public IDisposable StartNotify(IReceiveEvent<TEventArgs> receiver);
	}
}