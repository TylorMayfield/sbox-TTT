using Sandbox;

namespace TTT;

public class FollowEntityCamera : CameraMode
{
	private GameObject _followedObject;
	private Vector3 _focusPoint;

	public FollowEntityCamera( GameObject go )
	{
		_followedObject = go;

		if ( go?.Components.TryGet<Player>( out var player ) == true )
			Spectating.Player = player;

		_focusPoint = go?.WorldPosition ?? Vector3.Zero;
	}

	public override void BuildInput()
	{
		if ( !_followedObject.IsValid() )
		{
			Current = new FreeCamera();
			return;
		}

		if ( _followedObject.Components.TryGet<Corpse>( out _ ) && Input.Pressed( InputAction.Jump ) )
		{
			Current = new FreeCamera();
			return;
		}

		if ( Spectating.Player.IsValid() )
		{
			if ( Input.Pressed( InputAction.Jump ) )
			{
				Current = new FirstPersonCamera( Spectating.Player );
				return;
			}

			if ( Input.Pressed( InputAction.PrimaryAttack ) )
				Spectating.FindPlayer( false );
			else if ( Input.Pressed( InputAction.SecondaryAttack ) )
				Spectating.FindPlayer( true );

			_followedObject = Spectating.Player?.GameObject;
		}
	}

	public override void FrameSimulate()
	{
		var player = Player.Local;
		if ( player is null || !_followedObject.IsValid() )
			return;

		var cam = Game.ActiveScene?.Camera;
		if ( cam is null )
			return;

		_focusPoint = Vector3.Lerp( _focusPoint, _followedObject.WorldPosition, Time.Delta * 5.0f );

		var tr = Game.ActiveScene.Trace.Ray( _focusPoint, _focusPoint + player.ViewAngles.ToRotation().Forward * -130 )
			.StaticOnly()
			.Run();

		cam.WorldRotation = player.ViewAngles.ToRotation();
		cam.WorldPosition = tr.EndPosition;
	}
}
