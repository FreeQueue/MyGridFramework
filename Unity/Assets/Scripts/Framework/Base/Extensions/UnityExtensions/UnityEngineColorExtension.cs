/****************************************************************************
 * Copyright (c) 2015 - 2022 liangxiegame UNDER MIT License
 * 
 * http://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using UnityEngine;

namespace Framework.Extensions
{

	public static class UnityEngineColorExtension
	{

		public static Color HtmlStringToColor(this string htmlString) {
			bool parseSucceed = ColorUtility.TryParseHtmlString(htmlString, out Color retColor);
			return parseSucceed ? retColor : Color.black;
		}
	}
}