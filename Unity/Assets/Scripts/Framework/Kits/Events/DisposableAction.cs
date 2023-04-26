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

namespace Framework.Kits.Events
{
	public sealed class DisposableAction : IDisposable
	{
		public DisposableAction(Action onDispose, Action onFinalize)
		{
			this._onDispose = onDispose;
			this._onFinalize = onFinalize;
		}

		private readonly Action _onDispose;
		private readonly Action _onFinalize;
		private bool _disposed;

		public void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;
			_onDispose();
			GC.SuppressFinalize(this);
		}

		~DisposableAction()
		{
			_onFinalize();
		}
	}
}
