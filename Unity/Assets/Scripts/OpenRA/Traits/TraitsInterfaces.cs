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

using System.Diagnostics.CodeAnalysis;

namespace OpenRA.Traits
{
	public interface ITraitInfoInterface
	{
	}

	public interface IActivityInterface
	{
	}

	[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Not a real interface, but more like a tag.")]
	public interface IRequires<T> where T : class, ITraitInfoInterface
	{
	}

	[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Not a real interface, but more like a tag.")]
	public interface INotBefore<T> where T : class, ITraitInfoInterface
	{
	}
}