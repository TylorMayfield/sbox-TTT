using Sandbox;

namespace TTT;

public static class UiCompatExtensions
{
	public static Vector3 ToScreen( this Vector3 worldPosition )
	{
		var camera = Game.ActiveScene?.Camera;
		if ( camera is null )
			return default;

		var depth = Vector3.Dot( worldPosition - camera.WorldPosition, camera.WorldRotation.Forward );
		return new Vector3( 0.5f, 0.5f, depth );
	}
}
