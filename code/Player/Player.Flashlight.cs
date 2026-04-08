using Sandbox;

namespace TTT;

public partial class Player
{
	[Sync] public bool FlashlightEnabled { get; private set; } = false;
	private TimeSince _timeSinceLightToggled;

	/// <summary>
	/// The third person / world flashlight.
	/// </summary>
	private SpotLight _worldLight;

	/// <summary>
	/// The first person / view flashlight.
	/// </summary>
	private SpotLight _viewLight;

	public void SimulateFlashlight()
	{
		var toggle = Input.Pressed( InputAction.Flashlight );

		if ( _worldLight.IsValid() )
		{
			var eyeTransform = Renderer.GetAttachment( "eyes" ) ?? Transform.World;
			_worldLight.WorldTransform = eyeTransform;

			if ( ActiveCarriable.IsValid() )
			{
				var muzzleTransform = ActiveCarriable.WorldRenderer.GetAttachment( "muzzle" );
				if ( muzzleTransform.HasValue )
					_worldLight.WorldTransform = muzzleTransform.Value;
			}
		}

		if ( _timeSinceLightToggled > 0.25f && toggle )
		{
			FlashlightEnabled = !FlashlightEnabled;

			Sound.Play( "flashlight-toggle", WorldPosition );

			if ( _worldLight.IsValid() )
				_worldLight.Enabled = FlashlightEnabled;

			_timeSinceLightToggled = 0;
		}
	}

	protected void CreateFlashlight()
	{
		if ( Networking.IsHost )
		{
			_worldLight = CreateSpotLight();
			_worldLight.Tags.Add( "flashlight_world" );
			FlashlightEnabled = false;
		}

		if ( !IsProxy )
		{
			_viewLight = CreateSpotLight();
			_viewLight.Tags.Add( "flashlight_view" );
			_viewLight.Tags.Add( "viewmodel" );
			_viewLight.Enabled = FlashlightEnabled;
		}
	}

	protected void DeleteFlashlight()
	{
		_worldLight?.GameObject.Destroy();
		_worldLight = null;
		_viewLight?.GameObject.Destroy();
		_viewLight = null;
	}

	private void FrameUpdateFlashlight()
	{
		if ( !_viewLight.IsValid() )
			return;

		_viewLight.Enabled = FlashlightEnabled && !IsProxy;

		if ( !_viewLight.Enabled )
			return;

		_viewLight.WorldTransform = new Transform( EyePosition, EyeRotation );

		if ( !ActiveCarriable.IsValid() )
			return;

		var muzzleTransform = ActiveCarriable.ViewModelRenderer?.GetAttachment( "muzzle" );
		if ( !muzzleTransform.HasValue )
			return;

		var mz = muzzleTransform.Value;

		// Check for obstruction between muzzle and eyes
		var muzzleTrace = Scene.Trace.Ray( mz.Position, EyePosition )
			.Size( 2 )
			.IgnoreGameObject( GameObject )
			.IgnoreGameObject( ActiveCarriable.GameObject )
			.Run();

		var downOffset = Vector3.Down * 2f;
		var origin = mz.Position + downOffset;

		if ( muzzleTrace.Hit )
			origin = muzzleTrace.EndPosition + (mz.Rotation.Backward * muzzleTrace.Distance) + downOffset;

		var destination = origin + mz.Rotation.Forward * _viewLight.Radius;
		var direction = destination - origin;

		var fwdTrace = Scene.Trace.Box( BBox.FromPositionAndSize( Vector3.Zero, 2f ), origin, destination )
			.IgnoreGameObject( GameObject )
			.IgnoreGameObject( ActiveCarriable.GameObject )
			.Run();

		var pullbackAmount = 0.0f;
		const float pullbackThreshold = 48f;
		if ( fwdTrace.Distance < pullbackThreshold )
			pullbackAmount = (pullbackThreshold - fwdTrace.Distance).Remap( 0, pullbackThreshold, 0.0f, 0.045f );

		origin -= direction * pullbackAmount;

		_viewLight.WorldPosition = origin;
		_viewLight.WorldRotation = mz.Rotation;
	}

	private SpotLight CreateSpotLight()
	{
		var go = new GameObject( true, "Flashlight" );
		go.Parent = GameObject;
		var light = go.Components.Create<SpotLight>();
		light.Enabled = false;
		light.Shadows = true;
		light.Radius = 1024f;
		light.Attenuation = 1.0f;
		light.LightColor = Color.White;
		light.ConeInner = 20f;
		light.ConeOuter = 40f;
		light.FogStrength = 1.0f;
		light.Cookie = Texture.Load( "materials/effects/lightcookie.vtex" );
		return light;
	}
}
