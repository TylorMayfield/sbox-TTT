using Sandbox;

namespace TTT;

public class FreeCamera : CameraMode
{
	private const int BaseMoveSpeed = 300;
	private float _moveSpeed = 1f;
	private Angles _lookAngles;
	private Vector3 _position;
	private Vector3 _moveInput;

	public FreeCamera()
	{
		Spectating.Player = null;
		_position = Game.ActiveScene?.Camera?.WorldPosition ?? Vector3.Zero;
		_lookAngles = Game.ActiveScene?.Camera?.WorldRotation.Angles() ?? new Angles();
	}

	public override void BuildInput()
	{
		_moveSpeed = 1f;

		if ( Input.Down( InputAction.Run ) )
			_moveSpeed = 5f;

		if ( Input.Down( InputAction.Duck ) )
			_moveSpeed = 0.2f;

		if ( Input.Pressed( InputAction.Jump ) )
		{
			var alivePlayer = Game.Random.FromList( Utils.GetPlayersWhere( p => p.IsAlive ) );
			if ( alivePlayer.IsValid() )
				Current = new FollowEntityCamera( alivePlayer.GameObject );
		}

		if ( Input.Pressed( InputAction.Use ) )
			FindSpectateTarget();

		_moveInput = Input.AnalogMove;
		_lookAngles += Input.AnalogLook;
	}

	public override void FrameSimulate()
	{
		var rotation = Rotation.From( _lookAngles );
		var mv = _moveInput.Normal * BaseMoveSpeed * RealTime.Delta * rotation * _moveSpeed;
		_position += mv;

		var cam = Game.ActiveScene?.Camera;
		if ( cam is null )
			return;

		cam.WorldPosition = _position;
		cam.WorldRotation = rotation;
	}

	private void FindSpectateTarget()
	{
		var player = Player.Local;
		if ( player is null )
			return;

		if ( player.HoveredCarriable is null && player.HoveredPlayer is Player hoveredPlayer )
			Current = new FirstPersonCamera( hoveredPlayer );
	}
}
