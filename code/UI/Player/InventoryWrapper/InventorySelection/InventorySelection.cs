using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TTT.UI;

public partial class InventorySelection : Panel
{
	private static readonly string[] _slotInputButtons = new[]
	{
		InputAction.Slot0,
		InputAction.Slot1,
		InputAction.Slot2,
		InputAction.Slot3,
		InputAction.Slot4,
		InputAction.Slot5,
		InputAction.Slot6,
		InputAction.Slot7,
		InputAction.Slot8,
		InputAction.Slot9
	};

	public static int GetKeyboardNumberPressed()
	{
		for ( var i = 0; i < _slotInputButtons.Length; i++ )
			if ( Input.Pressed( _slotInputButtons[i] ) )
				return i;

		return -1;
	}

	public override void Tick()
	{
		var player = Player.Local;
		if ( player is null || !player.IsAlive )
			return;

		if ( !Children.Any() )
			return;

		if ( QuickChat.Instance.IsEnabled() )
			return;

		var childrenList = Children.ToList();
		var activeCarriable = player.ActiveCarriable;
		var keyboardIndexPressed = GetKeyboardNumberPressed();

		if ( keyboardIndexPressed != -1 )
		{
			List<Carriable> weaponsOfSlotTypeSelected = new();
			var activeCarriableOfSlotTypeIndex = -1;

			for ( var i = 0; i < childrenList.Count; ++i )
			{
				if ( childrenList[i] is InventorySlot slot )
				{
					if ( (int)slot.Carriable.Info.Slot == keyboardIndexPressed - 1 )
					{
						weaponsOfSlotTypeSelected.Add( slot.Carriable );

						if ( slot.Carriable == activeCarriable )
							activeCarriableOfSlotTypeIndex = weaponsOfSlotTypeSelected.Count - 1;
					}
				}
			}

			if ( activeCarriable is null || activeCarriableOfSlotTypeIndex == -1 )
			{
				player.ActiveChildInput = weaponsOfSlotTypeSelected.FirstOrDefault();
			}
			else
			{
				activeCarriableOfSlotTypeIndex = GetNextWeaponIndex( activeCarriableOfSlotTypeIndex, weaponsOfSlotTypeSelected.Count );
				player.ActiveChildInput = weaponsOfSlotTypeSelected[activeCarriableOfSlotTypeIndex];
			}
		}

		var mouseWheelIndex = Input.MouseWheel;
		if ( mouseWheelIndex != 0 )
		{
			var activeCarriableIndex = childrenList.FindIndex( p =>
				p is InventorySlot slot && slot.Carriable == activeCarriable );

			var newSelectedIndex = ClampSlotIndex( -(int)mouseWheelIndex.y + activeCarriableIndex, childrenList.Count - 1 );
			player.ActiveChildInput = (childrenList[newSelectedIndex] as InventorySlot)?.Carriable;
		}
	}

	private int GetNextWeaponIndex( int index, int count )
	{
		return ClampSlotIndex( index + 1, count - 1 );
	}

	private int ClampSlotIndex( int index, int maxIndex )
	{
		return index > maxIndex ? 0 : index < 0 ? maxIndex : index;
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( Hud.DisplayedPlayer.Inventory.HashCombine( carriable => carriable.GameObject?.Id.GetHashCode() ?? 0 ) );
	}
}
