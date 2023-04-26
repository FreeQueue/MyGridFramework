using YooAsset;

namespace Framework.Kits.ResourceKits
{
	internal static class YooAssetExtension
	{
		public static string ToStr(this AssetInfo assetInfo) {
			return $"AssetInfo[Address:{assetInfo.Address}\nAssetPath:{assetInfo.AssetPath}]";
		}
	}
}