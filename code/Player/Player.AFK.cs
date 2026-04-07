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
	private Entity _lastAfkObservedActiveChild;

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
			SendAfkHeartbeat();
		}
	}

	[TTTEvent.Player.Spawned]
	private static void ResetAfkTrackingOnSpawn( Player player )
	{
		if ( !Game.IsServer || !player.IsValid() )
			return;

		player.ResetAfkTracking();
	}

	private void ResetAfkTracking()
	{
		_timeSinceLastServerActivity = 0f;
		_lastAfkObservedPosition = Position;
		_lastAfkObservedInput = InputDirection;
		_lastAfkObservedViewAngles = ViewAngles;
		_lastAfkObservedActiveChild = ActiveChildInput;
	}

	[ConCmd.Server( Name = "ttt_afk_heartbeat" )]
	public static void SendAfkHeartbeat()
	{
		var player = ConsoleSystem.Caller?.Pawn as Player;
		if ( !player.IsValid() || player.Client.IsBot )
			return;

		player._timeSinceLastServerActivity = 0f;
	}

	[GameEvent.Tick.Server]
	private static void TickAfkTracking()
	{
		foreach ( var client in Game.Clients )
		{
			if ( client.Pawn is not Player player || !player.IsValid() )
				continue;

			player.TickAfkTrackingInternal();
		}
	}

	private void TickAfkTrackingInternal()
	{
		if ( Client.IsBot || IsForcedSpectator )
		{
			ResetAfkTracking();
			return;
		}

		if ( !IsAlive )
		{
			ResetAfkTracking();
			return;
		}

		var moved = Position.Distance( _lastAfkObservedPosition ) > 1f;
		var changedInput = !_lastAfkObservedInput.AlmostEqual( InputDirection, 0.001f );
		var changedView = MathF.Abs( ViewAngles.pitch - _lastAfkObservedViewAngles.pitch ) > 0.25f
			|| MathF.Abs( ViewAngles.yaw - _lastAfkObservedViewAngles.yaw ) > 0.25f
			|| MathF.Abs( ViewAngles.roll - _lastAfkObservedViewAngles.roll ) > 0.25f;
		var changedActiveChild = _lastAfkObservedActiveChild != ActiveChildInput;
		var velocityActivity = Velocity.Length > 5f;

		if ( moved || changedInput || changedView || changedActiveChild || velocityActivity )
			_timeSinceLastServerActivity = 0f;

		_lastAfkObservedPosition = Position;
		_lastAfkObservedInput = InputDirection;
		_lastAfkObservedViewAngles = ViewAngles;
		_lastAfkObservedActiveChild = ActiveChildInput;

		if ( _isHandlingAfkPunishment || _timeSinceLastServerActivity <= GameManager.AFKTimer )
			return;

		_isHandlingAfkPunishment = true;
		BeginAfkPunishment();
	}

	private async void BeginAfkPunishment()
	{
		Game.AssertServer();

		if ( !IsAlive )
		{
			FinalizeAfkPunishment();
			return;
		}

		Client.SetValue( "forced_spectator", true );

		if ( GameManager.AfkFunDeath )
		{
			SetAnimParameter( "b_attack", true );
			Velocity += Vector3.Up * 320f;
			Particles.Create( "particles/discombobulator/explode.vpcf", Position + Vector3.Up * 32f );
			Sound.FromWorld( "discombobulator_explode-1", Position );
			UI.TextChat.AddInfoEntry( To.Everyone, $"{SteamName} was claimed by the idle gods." );

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
		Game.AssertServer();

		if ( GameManager.AfkAutoKick && Client.IsValid() )
		{
			var kickDelay = MathF.Max( GameManager.AfkKickDelay, 0f );
			if ( kickDelay > 0f )
				await GameTask.DelaySeconds( kickDelay );

			if ( Client.IsValid() )
				Client.Kick();
		}

		ResetAfkTracking();
		_isHandlingAfkPunishment = false;
	}
}
