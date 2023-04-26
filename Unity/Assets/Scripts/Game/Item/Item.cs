using System.Collections.Generic;
using cfg.item;
using Framework.Kits.DataNodeKits;
using Framework.Kits.ReferencePoolKits;

public class Item:IReference
{
	private ItemData _data;
	public ItemData Data {
		get => _data;
		private set {
			dataMap.Clear();
			_data = value;
		}
	}
	public readonly DataMap dataMap=new DataMap();

	public static ItemData GetData(ItemId itemId) {
		return GameEntry.Tables.TbItem[itemId];
	}
	
	public static Item Create(ItemId itemId) {
		var item= ReferencePool.Get<Item>();
		item.Init(itemId);
		return item;
	}
	
	public void Init(ItemId itemId) {
		Data = GetData(itemId);
	}

	public void Clear() {
		Data = null;
		dataMap.Clear();
	}
}