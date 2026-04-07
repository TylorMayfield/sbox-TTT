using Sandbox;
using System;
using System.Collections.Generic;

namespace TTT;

public partial class Player
{
	private readonly Dictionary<AmmoType, int> _ammo = new()
	{
		{ AmmoType.None, 0 },
		{ AmmoType.PistolSMG, 0 },
		{ AmmoType.Shotgun, 0 },
		{ AmmoType.Sniper, 0 },
		{ AmmoType.Magnum, 0 },
		{ AmmoType.Rifle, 0 },
	};

	private static readonly Dictionary<AmmoType, int> _maxAmmoCapacity = new()
	{
		{ AmmoType.None, 0 },
		{ AmmoType.PistolSMG, 60 },
		{ AmmoType.Shotgun, 16 },
		{ AmmoType.Sniper, 20 },
		{ AmmoType.Magnum, 12 },
		{ AmmoType.Rifle, 60 },
	};

	public void ClearAmmo()
	{
		foreach ( var key in _ammo.Keys )
			_ammo[key] = 0;
	}

	public int AmmoCount( AmmoType type )
	{
		return _ammo.TryGetValue( type, out var count ) ? count : 0;
	}

	public bool SetAmmo( AmmoType type, int amount )
	{
		if ( !Networking.IsHost )
			return false;

		if ( !_ammo.ContainsKey( type ) )
			return false;

		_ammo[type] = amount;
		return true;
	}

	public bool GiveAll( int amount )
	{
		if ( !Networking.IsHost )
			return false;

		foreach ( AmmoType ammoType in Enum.GetValues( typeof( AmmoType ) ) )
			GiveAmmo( ammoType, amount );

		return true;
	}

	public int GiveAmmo( AmmoType type, int amount )
	{
		if ( !Networking.IsHost )
			return 0;

		var maxCap = _maxAmmoCapacity.TryGetValue( type, out var max ) ? max : 0;
		var ammoPickedUp = Math.Min( amount, maxCap - AmmoCount( type ) );

		if ( ammoPickedUp > 0 )
		{
			SetAmmo( type, AmmoCount( type ) + ammoPickedUp );
			Sound.Play( "pickup_ammo", WorldPosition );
		}

		return ammoPickedUp;
	}

	public int TakeAmmo( AmmoType type, int amount )
	{
		var available = AmmoCount( type );
		amount = Math.Min( available, amount );

		SetAmmo( type, available - amount );

		return amount;
	}
}
