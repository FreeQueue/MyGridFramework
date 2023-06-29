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

namespace S100
{
	public interface ITraitInfo
	{
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class RequireTraitAttribute : Attribute
	{
		public RequireTraitAttribute(params Type[] types) {
			Types = types;
		}

		public Type[] Types { get; }
	}
	
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class OptionTraitAttribute : Attribute
	{
		public OptionTraitAttribute(params Type[] types) {
			Types = types;
		}

		public Type[] Types { get; }
	}
}