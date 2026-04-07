using Sandbox;
using System.Collections.Generic;

namespace TTT;

public partial class Player
{
	[Sync] public int Credits { get; set; }

	public List<ItemInfo> PurchasedLimitedShopItems { get; private set; } = new();

	public bool CanPurchase( ItemInfo item )
	{
		if ( !Role.CanUseShop )
			return false;

		if ( Credits < item.Price )
			return false;

		if ( !Team.GetShopItems().Contains( item ) )
			return false;

		if ( item.IsLimited && PurchasedLimitedShopItems.Contains( item ) )
			return false;

		if ( item is CarriableInfo carriable && !Inventory.HasFreeSlot( carriable.Slot ) )
			return false;

		return true;
	}

	[ConCmd( "ttt_purchase_item" )]
	public static void PurchaseItem( string itemResourcePath )
	{
		if ( !Networking.IsHost )
			return;

		var player = GameManager.Instance?.FindPlayerByConnection( Rpc.Caller );
		if ( !player.IsValid() )
			return;

		var itemInfo = ResourceLibrary.Get<ItemInfo>( itemResourcePath );
		if ( itemInfo is null )
			return;

		if ( !player.CanPurchase( itemInfo ) )
			return;

		if ( itemInfo.IsLimited )
			player.PurchasedLimitedShopItems.Add( itemInfo );

		player.Credits -= itemInfo.Price;

		if ( itemInfo is CarriableInfo )
			player.Inventory.Add( TypeLibrary.Create<Carriable>( itemInfo.ClassName ) );
		else if ( itemInfo is PerkInfo )
			player.Perks.Add( TypeLibrary.Create<Perk>( itemInfo.ClassName ) );
	}
}
