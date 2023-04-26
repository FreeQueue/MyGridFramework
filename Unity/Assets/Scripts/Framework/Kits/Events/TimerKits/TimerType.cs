namespace Framework.Kits.TimerKits
{
	/// <summary>
	/// timer 类型
	/// </summary>
	public enum TimerType
	{
		/// <summary>
		/// 默认 无
		/// </summary>
		None,

		/// <summary>
		/// 等待执行一次
		/// </summary>
		OnceWait,

		/// <summary>
		/// 执行一次
		/// </summary>
		Once,

		/// <summary>
		/// 重复执行
		/// </summary>
		Repeated,
	}
}