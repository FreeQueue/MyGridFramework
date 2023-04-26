namespace Framework.Kits.EntityKits
{
	/// <summary>
	/// 实体状态。
	/// </summary>
	public enum EntityStatus : byte
	{
		Unknown = 0,
		WillInit,
		Inited,
		WillShow,
		Showed,
		WillHide,
		Hidden,
		WillRecycle,
		Recycled
	}
}