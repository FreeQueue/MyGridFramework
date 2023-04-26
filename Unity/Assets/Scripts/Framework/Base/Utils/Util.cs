//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;

namespace Framework
{
    /// <summary>
    ///     实用函数集。
    /// </summary>
    public static partial class Util
	{
		public static Lazy<T> Lazy<T>(Func<T> p) { return new Lazy<T>(p); }
	}
}