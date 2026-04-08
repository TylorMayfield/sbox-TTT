using Sandbox;

namespace TTT;

/// <summary>
/// Handles viewmodel sway and bobbing. Attach this component to the viewmodel GameObject.
/// </summary>
public class ViewModel : Component
{
	protected float SwingInfluence = 0.02f;
	protected float ReturnSpeed = 8.0f;
	protected float MaxOffsetLength = 4.0f;
	protected float BobCycleTime = 8;
	protected Vector3 BobDirection = new( 0.0f, 0.35f, 0.12f );

	private bool _activated = false;
	private Vector3 _swingOffset;
	private float _lastPitch;
	private float _lastYaw;
	private float _bobAnim;
	private float _yawInertia;
	private float _pitchInertia;

	protected SkinnedModelRenderer Renderer { get; private set; }

	protected override void OnStart()
	{
		Renderer = Components.Get<SkinnedModelRenderer>( FindMode.InSelf );
	}

	protected override void OnUpdate()
	{
		PlaceViewmodel();
	}

	protected virtual void PlaceViewmodel()
	{
		var cam = Game.ActiveScene?.Camera;
		if ( cam is null )
			return;

		var inPos = cam.WorldPosition;
		var inRot = cam.WorldRotation;

		if ( !_activated )
		{
			_lastPitch = inRot.Pitch();
			_lastYaw = inRot.Yaw();

			_yawInertia = 0;
			_pitchInertia = 0;

			_activated = true;
		}

		if ( Renderer is not null )
		{
			var cameraBoneIndex = Renderer.Model?.BoneCount > 0
				? Renderer.Model.Bones.GetBone( "camera" )?.Index ?? -1
				: -1;

			if ( cameraBoneIndex != -1 )
			{
				var boneTx = Renderer.GetBoneWorldTransform( cameraBoneIndex );
				if ( boneTx.HasValue )
					inRot *= WorldRotation.Inverse * boneTx.Value.Rotation;
			}
		}

		WorldPosition = inPos;
		WorldRotation = inRot;

		var newPitch = WorldRotation.Pitch();
		var newYaw = WorldRotation.Yaw();

		_pitchInertia = Angles.NormalizeAngle( newPitch - _lastPitch );
		_yawInertia = Angles.NormalizeAngle( _lastYaw - newYaw );

		var playerVelocity = Player.Local?.CharController?.Velocity ?? Vector3.Zero;
		var verticalDelta = playerVelocity.z * Time.Delta;
		var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
		verticalDelta *= 1.0f - System.MathF.Abs( viewDown.Cross( Vector3.Down ).y );
		var pitchDelta = (_pitchInertia - verticalDelta) * 0.65f;
		var yawDelta = _yawInertia * 0.65f;

		var offset = CalcSwingOffset( pitchDelta, yawDelta );
		offset += CalcBobbingOffset( playerVelocity );

		WorldPosition += WorldRotation * offset;

		_lastPitch = newPitch;
		_lastYaw = newYaw;
	}

	protected Vector3 CalcSwingOffset( float pitchDelta, float yawDelta )
	{
		var swingVelocity = new Vector3( 0, yawDelta, pitchDelta );

		_swingOffset -= _swingOffset * ReturnSpeed * Time.Delta;
		_swingOffset += swingVelocity * SwingInfluence;

		if ( _swingOffset.Length > MaxOffsetLength )
			_swingOffset = _swingOffset.Normal * MaxOffsetLength;

		return _swingOffset;
	}

	protected Vector3 CalcBobbingOffset( Vector3 velocity )
	{
		_bobAnim += Time.Delta * BobCycleTime;

		var twoPI = System.MathF.PI * 2.0f;

		if ( _bobAnim > twoPI )
			_bobAnim -= twoPI;

		var speed = new Vector2( velocity.x, velocity.y ).Length;
		speed = speed > 10.0 ? speed : 0.0f;
		var offset = BobDirection * (speed * 0.00225f) * System.MathF.Cos( _bobAnim );
		offset = offset.WithZ( -System.MathF.Abs( offset.z ) );

		return offset;
	}
}
