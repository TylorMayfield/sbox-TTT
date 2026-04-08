using Sandbox;
using System;

namespace TTT;

/// <summary>
/// Allows a prop to be controlled by a spectator.
/// </summary>
public sealed partial class PropPossession : Component
{
	public int Punches { get; private set; }
	public int MaxPunches { get; private set; }
	public Player Owner => _owner;
	public Prop Prop => _prop;

	private const float PunchRechargeTime = 1f;

	private Player _owner;
	private Prop _prop;
	private UI.PunchOMeter _meter;
	private UI.PossessionNameplate _nameplate;
	private TimeUntil _timeUntilNextPunch = 0;
	private TimeUntil _timeUntilRecharge = 0;

	public void Init( Player owner, Prop prop )
	{
		_owner = owner;
		_prop = prop;
		MaxPunches = (int)Math.Min( Math.Max( 0, owner.ActiveKarma / 100 ), 13 );
		Punches = MaxPunches;
	}

	protected override void OnStart()
	{
		if ( Player.Local is Player localPlayer && !localPlayer.IsAlive )
			_nameplate = new( _prop );

		if ( Player.Local == _owner )
		{
			_meter = new( this );
			CameraMode.Current = new FollowEntityCamera( _prop.GameObject );
		}
	}

	protected override void OnDestroy()
	{
		if ( _prop.IsValid() )
			_owner?.CancelPossession();

		_nameplate?.Delete( true );

		if ( Player.Local == _owner )
		{
			_meter?.Delete( true );

			if ( !_owner.IsAlive )
				CameraMode.Current = new FreeCamera();
		}
	}

	public void Punch()
	{
		if ( Punches <= 0 )
			return;

		if ( !_timeUntilNextPunch )
			return;

		var rigidbody = _prop?.Components.Get<Rigidbody>( FindMode.InSelf );
		if ( rigidbody is null )
			return;

		var mass = Math.Min( 150f, rigidbody.PhysicsBody?.Mass ?? 75f );
		var force = 110f * 75f;
		var mf = mass * force;

		_timeUntilNextPunch = 0.15f;

		if ( Input.Pressed( InputAction.Jump ) )
		{
			rigidbody.PhysicsBody?.ApplyForceAt( rigidbody.PhysicsBody.MassCenter, new Vector3( 0, 0, mf ) );
			_timeUntilNextPunch = 0.2f;
		}
		else if ( _owner.InputDirection.x != 0f )
		{
			rigidbody.PhysicsBody?.ApplyForceAt( rigidbody.PhysicsBody.MassCenter, _owner.InputDirection.x * (Vector3.Forward * _owner.ViewAngles.ToRotation()) * mf );
		}
		else if ( _owner.InputDirection.y != 0f )
		{
			rigidbody.PhysicsBody?.ApplyForceAt( rigidbody.PhysicsBody.MassCenter, _owner.InputDirection.y * (Vector3.Left * _owner.ViewAngles.ToRotation()) * mf );
		}

		Punches = Math.Max( Punches - 1, 0 );
	}

	[TTTEvent.Player.Spawned]
	private static void OnPlayerSpawned( Player player )
	{
		player.CancelPossession();
	}

	[TTTEvent.Player.Spawned]
	private void DeleteNameplate( Player player )
	{
		if ( Player.Local == player )
			_nameplate?.Delete( true );
	}

	[TTTEvent.Player.Killed]
	private void CreateNameplate( Player player )
	{
		if ( Player.Local == player )
			_nameplate = new( _prop );
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost )
			return;

		if ( !_timeUntilRecharge )
			return;

		Punches = Math.Min( Punches + 1, MaxPunches );
		_timeUntilRecharge = PunchRechargeTime;
	}
}
