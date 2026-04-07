using Sandbox;
using System;

namespace TTT;

public partial class Player
{
	private TimeSince _timeSinceLastServerActivity = 0f;
	private static TimeSince _timeSinceLastAfkHeartbeat = 0f;
	private bool _isHandlingAfkPunishment;
	private Vector3 _lastAfkObservedPosition;
	private Vector3 _lastAfkObservedInput;
	private Angles _lastAfkObservedViewAngles;
	private Carriable _lastAfkObservedActiveCarriable;

	private void CheckAFK()
	{
		if ( _isHandlingAfkPunishment )
		{
			Input.StopProcessing = true;
			return;
		}

		var hasUiLikeActivity = Input.MouseDelta != Vector2.Zero
			|| Input.Down( InputAction.Score )
			|| Input.Pressed( InputAction.Menu )
			|| Input.Pressed( InputAction.Use )
			|| Input.Pressed( InputAction.PrimaryAttack )
			|| Input.Pressed( InputAction.SecondaryAttack );

		if ( hasUiLikeActivity && _timeSinceLastAfkHeartbeat > 1f )
		{
			_timeSinceLastAfkHeartbeat = 0f;
			SendAfkHeartbeatToServer();
		}
	}

	[TTTEvent.Player.Spawned]
	private static void ResetAfkTrackingOnSpawn( Player player )
	{
		if ( !Networking.IsHost || !player.IsValid() )
			return;

		player.ResetAfkTracking();
	}

	private void ResetAfkTracking()
	{
		_timeSinceLastServerActivity = 0f;
		_lastAfkObservedPosition = WorldPosition;
		_lastAfkObservedInput = InputDirection;
		_lastAfkObservedViewAngles = ViewAngles;
		_lastAfkObservedActiveCarriable = ActiveCarriable;
	}

	[ConCmd( "ttt_afk_heartbeat" )]
	public static void SendAfkHeartbeatToServer()
	{
		if ( !Networking.IsHost )
			return;

		var player = GameManager.Instance?.FindPlayerByConnection( Rpc.Caller );
		if ( !player.IsValid() || player.Network.Owner?.IsBot == true )
			return;

		player._timeSinceLastServerActivity = 0f;
	}

	[GameEvent.Tick]
	private static void TickAfkTracking()
	{
		if ( !Networking.IsHost )
			return;

		foreach ( var player in Utils.GetPlayersWhere( _ => true ) )
			player.TickAfkTrackingInternal();
	}

	private void TickAfkTrackingInternal()
	{
		if ( Network.Owner?.IsBot == true || IsForcedSpectator )
		{
			ResetAfkTracking();
			return;
		}

		if ( !IsAlive )
		{
			ResetAfkTracking();
			return;
		}

		var moved = WorldPosition.Distance( _lastAfkObservedPosition ) > 1f;
		var changedInput = !_lastAfkObservedInput.AlmostEqual( InputDirection, 0.001f );
		var changedView = MathF.Abs( ViewAngles.pitch - _lastAfkObservedViewAngles.pitch ) > 0.25f
			|| MathF.Abs( ViewAngles.yaw - _lastAfkObservedViewAngles.yaw ) > 0.25f
			|| MathF.Abs( ViewAngles.roll - _lastAfkObservedViewAngles.roll ) > 0.25f;
		var changedActiveChild = _lastAfkObservedActiveCarriable != ActiveCarriable;
		var velocityActivity = CharController.Velocity.Length > 5f;

		if ( moved || changedInput || changedView || changedActiveChild || velocityActivity )
			_timeSinceLastServerActivity = 0f;

		_lastAfkObservedPosition = WorldPosition;
		_lastAfkObservedInput = InputDirection;
		_lastAfkObservedViewAngles = ViewAngles;
		_lastAfkObservedActiveCarriable = ActiveCarriable;

		if ( _isHandlingAfkPunishment || _timeSinceLastServerActivity <= GameManager.AFKTimer )
			return;

		_isHandlingAfkPunishment = true;
		BeginAfkPunishment();
	}

	private async void BeginAfkPunishment()
	{
		if ( !Networking.IsHost )
			return;

		if ( !IsAlive )
		{
			FinalizeAfkPunishment();
			return;
		}

		Network.Owner?.SetValue( "forced_spectator", true );

		if ( GameManager.AfkFunDeath )
		{
			Renderer.Set( "b_attack", true );
			CharController.Punch( Vector3.Up * 320f );
			SceneParticles.PlayInstant( Scene, "particles/discombobulator/explode.vpcf", Transform.World.WithPosition( WorldPosition + Vector3.Up * 32f ) );
			Sound.Play( "discombobulator_explode-1", WorldPosition );
			UI.TextChat.AddInfoEntry( $"{SteamName} was claimed by the idle gods." );

			await GameTask.DelaySeconds( 0.35f );

			if ( IsValid() && IsAlive )
			{
				var damage = DamageInfo.Generic( float.MaxValue )
					.WithAttacker( this )
					.WithTag( DamageTags.Silent );
				damage = damage.WithTag( DamageTags.Explode );
				damage = damage.WithTag( DamageTags.Avoidable );
				TakeDamage( damage );
			}
		}
		else if ( IsAlive )
		{
			Kill();
		}

		FinalizeAfkPunishment();
	}

	private async void FinalizeAfkPunishment()
	{
		if ( !Networking.IsHost )
			return;

		var owner = Network.Owner;

		if ( GameManager.AfkAutoKick && owner is not null )
		{
			var kickDelay = MathF.Max( GameManager.AfkKickDelay, 0f );
			if ( kickDelay > 0f )
				await GameTask.DelaySeconds( kickDelay );

			owner.Kick();
		}

		ResetAfkTracking();
		_isHandlingAfkPunishment = false;
	}
}
