using Sandbox;

namespace TTT;

/// <summary>
/// Movement simulation using CharacterController component.
/// The WalkController class is kept as a thin wrapper for compatibility,
/// but the actual movement is now delegated to CharacterController.
/// </summary>
public partial class Player
{
	private const float EyeHeight = 64f;
	private const float DuckEyeHeight = 28f;
	private const float BaseWalkSpeed = 110f;
	private const float BaseDefaultSpeed = 230f;

	public bool IsDucking => CharController.IsOnGround && Input.Down( InputAction.Duck );

	private void SimulateMovement()
	{
		if ( !IsAlive )
			return;

		var applyKarmaScale = KarmaSpeedScale;
		var walkSpeed = BaseWalkSpeed * applyKarmaScale;
		var defaultSpeed = BaseDefaultSpeed * applyKarmaScale;

		// Update eye position
		var eyeHeight = IsDucking ? DuckEyeHeight : EyeHeight;
		EyeLocalPosition = Vector3.Up * (eyeHeight * WorldScale.z);
		EyeRotation = ViewAngles.ToRotation();

		// Compute wish velocity
		var wishVelocity = new Vector3( InputDirection.x, InputDirection.y, 0 );
		wishVelocity = wishVelocity.Normal * wishVelocity.Length.Clamp( 0, 1 );
		wishVelocity *= ViewAngles.WithPitch( 0 ).ToRotation();
		wishVelocity = wishVelocity.WithZ( 0 );

		var speed = Input.Down( InputAction.Run ) ? walkSpeed : defaultSpeed;
		if ( IsDucking )
			speed *= 0.5f;

		wishVelocity *= speed;
		WishVelocity = wishVelocity;

		// Handle jump
		if ( CharController.IsOnGround && Input.Pressed( InputAction.Jump ) )
		{
			float jumpSpeed = 268f * 1.2f;
			if ( IsDucking )
				jumpSpeed *= 0.8f;

			CharController.Punch( Vector3.Up * jumpSpeed );
		}

		if ( CharController.IsOnGround )
		{
			CharController.ApplyFriction( 6f );
			CharController.Accelerate( WishVelocity );
		}
		else if ( GameManager.BhopEnabled )
		{
			CharController.Accelerate( WishVelocity * GameManager.BhopAirControl );
		}

		CharController.Move();

		// Update animation
		Renderer.Set( "b_jump", !CharController.IsOnGround );
	}
}
