using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using cfg.item;
using Framework.Kits.ConfigKits;
using Framework.Extensions;

public class RecipeConfig:IConfig
{
	private readonly Dictionary<ItemId, HashSet<RecipeData>> _recipeByMaterialDic
		= new Dictionary<ItemId, HashSet<RecipeData>>();
	private readonly Dictionary<ItemId, HashSet<RecipeData>> _recipeByProductDic
		= new Dictionary<ItemId, HashSet<RecipeData>>();

	void IConfig.Init() {
		//TODO recipes判重？
		_recipeByMaterialDic.Clear();
		_recipeByProductDic.Clear();
		foreach (RecipeData recipe in GameEntry.Tables.TbRecipe.DataList) {
			HashSet<RecipeData> set = _recipeByMaterialDic.GetOrNew(recipe.Material1);
			set.Add(recipe);
			set = _recipeByMaterialDic.GetOrNew(recipe.Material2);
			set.Add(recipe);
			if (recipe.Product == null) continue;
			set = _recipeByProductDic.GetOrNew(recipe.Product.Value);
			set.Add(recipe);
		}
	}

	[return:MaybeNull]
	public HashSet<RecipeData> GetRecipesByMaterial(ItemId itemId) {
		return _recipeByMaterialDic.ContainsKey(itemId) ? _recipeByMaterialDic[itemId] : null;
	}
	
	[return:MaybeNull]
	public HashSet<RecipeData> GetRecipesByProduct(ItemId itemId) {
		return _recipeByProductDic.ContainsKey(itemId) ? _recipeByProductDic[itemId] : null;
	}

	[return:MaybeNull]
	public RecipeData GetRecipesByMaterials(ItemId material1, ItemId material2) {
		return GameEntry.Tables.TbRecipe.Get(material1, material2)??GameEntry.Tables.TbRecipe.Get(material2, material1);
	}
}