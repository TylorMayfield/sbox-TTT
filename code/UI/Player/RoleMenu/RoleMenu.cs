using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace TTT.UI;

public partial class RoleMenu : Panel
{
	private enum Tab
	{
		Shop,
		DNA,
		Radio,
		CreditTransfer
	}

	// Tab => Condition that allows access to the tab.
	private readonly Dictionary<Tab, Func<bool>> _access = new()
	{
		{ Tab.Shop, () => Player.Local?.Role.CanUseShop == true },
		{ Tab.DNA, () => Player.Local?.Inventory.Find<DNAScanner>() is not null },
		{ Tab.Radio, () => Player.Local?.Components.Get<RadioComponent>( FindMode.InSelf ) is not null },
		{ Tab.CreditTransfer, () => Player.Local?.Role.CanUseShop == true }
	};

	private Tab _currentTab;

	private bool HasTabAccess()
	{
		if ( _access[_currentTab].Invoke() )
			return true;

		foreach ( var tabEntry in _access )
		{
			if ( tabEntry.Value.Invoke() )
			{
				_currentTab = tabEntry.Key;
				return true;
			}
		}

		return false;
	}

	public override void Tick() => SetClass( "fade-in", Input.Down( InputAction.View ) && HasTabAccess() );

	protected override int BuildHash()
	{
		var player = Player.Local;
		return HashCode.Combine( player?.IsAlive, player?.Role, player?.Credits, _access.HashCombine( a => a.Value.Invoke().GetHashCode() ), _currentTab );
	}
}
