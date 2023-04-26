using System.Collections.Generic;
using Framework.Kits.EventKits;

public class FoodSystem:ISystem
{
	private List<Item> _items;
	
	public Unsubscriber Register(Item item) {
		return Unsubscriber.Create(_items,item);
	}
}