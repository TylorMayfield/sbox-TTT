using Sandbox;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TTT;

/// <summary>
/// Manages the carriables held by a player.
/// </summary>
public sealed class Inventory : IEnumerable<Carriable>
{
	public Player Owner { get; private init; }

	public Carriable Active
	{
		get => Owner.ActiveCarriable;
		private set => Owner.ActiveCarriable = value;
	}

	public int Count => _list.Count;
	private readonly List<Carriable> _list = new();

	private readonly Dictionary<SlotType, int> _slotCapacity = new()
	{
		{ SlotType.Primary, 1 },
		{ SlotType.Secondary, 1 },
		{ SlotType.Melee, 1 },
		{ SlotType.OffensiveEquipment, 1 },
		{ SlotType.UtilityEquipment, 3 },
		{ SlotType.Grenade, 1 }
	};

	private readonly Dictionary<AmmoType, bool> _hasAmmoType = new()
	{
		{ AmmoType.None, false },
		{ AmmoType.PistolSMG, false },
		{ AmmoType.Shotgun, false },
		{ AmmoType.Sniper, false },
		{ AmmoType.Magnum, false },
		{ AmmoType.Rifle, false },
	};

	public Inventory( Player player ) => Owner = player;

	public bool Add( Carriable carriable, bool makeActive = false )
	{
		if ( !Networking.IsHost )
			return false;

		if ( !carriable.IsValid() )
			return false;

		if ( carriable.Owner is not null )
			return false;

		if ( !CanAdd( carriable ) )
			return false;

		carriable.GameObject.Parent = Owner.GameObject;
		carriable.OnCarryStart( Owner );
		_list.Add( carriable );

		_slotCapacity[carriable.Info.Slot] -= 1;

		if ( carriable is Weapon weapon )
			_hasAmmoType[weapon.Info.AmmoType] = true;

		if ( makeActive )
			SetActive( carriable );

		return true;
	}

	public bool CanAdd( Carriable carriable )
	{
		if ( !HasFreeSlot( carriable.Info.Slot ) )
			return false;

		if ( !carriable.CanCarry( Owner ) )
			return false;

		return true;
	}

	public bool Contains( Carriable carriable )
	{
		return _list.Contains( carriable );
	}

	public void Pickup( Carriable carriable )
	{
		if ( Add( carriable ) )
			Sound.Play( "pickup_weapon", Owner.WorldPosition );
	}

	public bool HasFreeSlot( SlotType slotType )
	{
		return _slotCapacity[slotType] > 0;
	}

	public bool HasWeaponOfAmmoType( AmmoType ammoType )
	{
		return ammoType != AmmoType.None && _hasAmmoType[ammoType];
	}

	public void OnUse( Carriable carriable )
	{
		if ( !Networking.IsHost )
			return;

		if ( !carriable.CanCarry( Owner ) )
			return;

		if ( HasFreeSlot( carriable.Info.Slot ) )
		{
			Add( carriable );
			return;
		}

		var sameSlot = _list.FindAll( x => x.Info.Slot == carriable.Info.Slot );

		if ( Active is not null && Active.Info.Slot == carriable.Info.Slot )
		{
			if ( DropActive() is not null )
				Add( carriable, true );
		}
		else if ( sameSlot.Count == 1 )
		{
			if ( Drop( sameSlot[0] ) )
				Add( carriable, false );
		}
	}

	public bool SetActive( Carriable carriable )
	{
		if ( Active == carriable )
			return false;

		if ( !Contains( carriable ) )
			return false;

		Active = carriable;
		return true;
	}

	public T Find<T>() where T : Carriable
	{
		foreach ( var carriable in _list )
		{
			if ( carriable is T t )
				return t;
		}

		return null;
	}

	public bool Drop( Carriable carriable )
	{
		if ( !Networking.IsHost )
			return false;

		if ( !Contains( carriable ) )
			return false;

		if ( !carriable.Info.CanDrop )
			return false;

		_list.Remove( carriable );
		carriable.OnCarryDrop( Owner );

		_slotCapacity[carriable.Info.Slot] += 1;

		if ( carriable is Weapon weapon )
			_hasAmmoType[weapon.Info.AmmoType] = false;

		return true;
	}

	public Carriable DropActive()
	{
		if ( !Networking.IsHost )
			return null;

		if ( Drop( Active ) )
		{
			var active = Active;
			Active = null;
			return active;
		}

		return null;
	}

	public void DropAll()
	{
		if ( !Networking.IsHost )
			return;

		foreach ( var carriable in _list.ToArray() )
			Drop( carriable );

		Active = null;
	}

	public void DeleteContents()
	{
		if ( !Networking.IsHost )
			return;

		foreach ( var carriable in _list.ToArray() )
			carriable.GameObject.Destroy();

		Active = null;
		_list.Clear();
	}

	public IEnumerator<Carriable> GetEnumerator() => _list.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
