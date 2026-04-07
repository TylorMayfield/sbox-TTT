using Sandbox;
using Sandbox.UI;

namespace TTT;

public enum AmmoType : byte
{
	/// <summary>
	/// Used for weapons that cannot pick up any additional ammo.
	/// </summary>
	None,
	PistolSMG,
	Shotgun,
	Sniper,
	Magnum,
	Rifle
}

public abstract partial class Ammo : Component, Component.ITriggerListener, ICarriableHint
{
	[Sync] private int CurrentCount { get; set; }

	protected virtual AmmoType Type => AmmoType.None;
	protected virtual int DefaultAmmoCount => 30;
	protected virtual string WorldModelPath => string.Empty;

	private Player _dropper;
	private TimeSince _timeSinceDropped = 0;

	protected override void OnStart()
	{
		if ( !Networking.IsHost )
			return;

		Tags.Add( "interactable" );
		CurrentCount = DefaultAmmoCount;

		// Set up the model renderer
		var renderer = Components.Get<ModelRenderer>( FindMode.InSelf );
		if ( renderer is not null && !WorldModelPath.IsNullOrEmpty() )
			renderer.Model = Model.Load( WorldModelPath );
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( !Networking.IsHost )
			return;

		if ( other.Components.TryGet<Player>( out var player, FindMode.InAncestors )
			&& (player != _dropper || _timeSinceDropped >= 1f) )
		{
			GiveAmmo( player );
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other ) { }

	public static Ammo CreateOnObject( GameObject parent, AmmoType ammoType, int count = 0 )
	{
		if ( !Networking.IsHost )
			return null;

		Ammo ammo = ammoType switch
		{
			AmmoType.PistolSMG => parent.Components.Create<SMGAmmo>(),
			AmmoType.Shotgun => parent.Components.Create<ShotgunAmmo>(),
			AmmoType.Sniper => parent.Components.Create<SniperAmmo>(),
			AmmoType.Magnum => parent.Components.Create<MagnumAmmo>(),
			AmmoType.Rifle => parent.Components.Create<RifleAmmo>(),
			_ => null
		};

		if ( ammo is null )
			return null;

		ammo.CurrentCount = count == 0 ? ammo.DefaultAmmoCount : count;

		return ammo;
	}

	public static GameObject Drop( Player dropper, AmmoType ammoType, int count )
	{
		if ( !Networking.IsHost )
			return null;

		var go = new GameObject( true, $"Ammo ({ammoType})" );
		go.WorldPosition = dropper.WorldPosition + Vector3.Up * 40f;

		go.Components.Create<ModelRenderer>();
		var rb = go.Components.Create<Rigidbody>();

		var ammo = CreateOnObject( go, ammoType, count );
		if ( ammo is null )
		{
			go.Destroy();
			return null;
		}

		if ( rb is not null )
			rb.Velocity = dropper.GetDropVelocity();

		ammo._dropper = dropper;
		ammo._timeSinceDropped = 0;

		go.NetworkSpawn();

		return go;
	}

	private void GiveAmmo( Player player )
	{
		if ( !IsValid || !player.Inventory.HasWeaponOfAmmoType( Type ) )
			return;

		var ammoPickedUp = player.GiveAmmo( Type, CurrentCount );
		CurrentCount -= ammoPickedUp;

		if ( CurrentCount <= 0 )
			GameObject.Destroy();
	}

	Panel ICarriableHint.DisplayHint( Player player ) => new UI.Hint() { HintText = $"{DisplayInfo.For( this ).Name} x{CurrentCount}" };

	void ICarriableHint.Tick( Player player )
	{
		if ( !Input.Pressed( InputAction.Use ) )
			return;

		if ( player.IsAlive )
			GiveAmmo( player );
	}
}
