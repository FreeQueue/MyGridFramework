namespace Framework.Kits.EventKits
{
	public interface IReceiveEvent<TEventArgs>
	{
		public void SubscribePublisher(ISendEvent<TEventArgs> sender);
		public void UnsubscribePublisher();
		internal void Receive(int id,TEventArgs eventArgs);
		internal void ReceiveNow(int id,TEventArgs eventArgs);
	}

}