using Sandbox;

namespace TTT;

public class FirstPersonCamera : CameraMode
{
	public FirstPersonCamera( Player viewer = null )
	{
		Spectating.Player = viewer;
	}

	public override void BuildInput()
	{
		if ( Player.Local is not Player player || player.Status == PlayerStatus.Alive )
			return;

		if ( !Spectating.Player.IsValid() || Input.Pressed( InputAction.Jump ) )
		{
			Current = new FreeCamera();
			return;
		}

		if ( Input.Pressed( InputAction.PrimaryAttack ) )
			Spectating.FindPlayer( false );

		if ( Input.Pressed( InputAction.SecondaryAttack ) )
			Spectating.FindPlayer( true );
	}

	public override void FrameSimulate()
	{
		var cam = Game.ActiveScene?.Camera;
		if ( cam is null )
			return;

		var target = UI.Hud.DisplayedPlayer;
		if ( target is null )
			return;

		cam.WorldPosition = target.EyePosition;
		cam.WorldRotation = !target.IsProxy
			? target.ViewAngles.ToRotation()
			: Rotation.Slerp( cam.WorldRotation, target.EyeRotation, Time.Delta * 20f );

		if ( target.ActiveCarriable is Scout || target.ActiveCarriable is Binoculars )
			return;

		cam.FieldOfView = Screen.CreateVerticalFieldOfView( Preferences.FieldOfView );
	}
}
