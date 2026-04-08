using Sandbox;
using System.Linq;

namespace TTT;

public static class MapHandler
{
	public static int WeaponCount = 0;

	public static void CountMapWeapons()
	{
		if ( !Networking.IsHost )
			return;

		WeaponCount = 0;

		foreach ( var go in Game.ActiveScene.GetAllObjects( true ) )
		{
			if ( go.Components.TryGet<Weapon>( out _ ) || go.Components.TryGet<Ammo>( out _ ) )
				WeaponCount += 1;
		}
	}

	public static void Cleanup()
	{
		foreach ( var go in Game.ActiveScene.GetAllObjects( true ).ToList() )
		{
			if ( go.Tags.Has( "map" ) || go == Game.ActiveScene.Root )
				continue;

			if ( go.Tags.Has( "player" ) || go.Tags.Has( "manager" ) )
				continue;

			go.Destroy();
		}
	}
}
