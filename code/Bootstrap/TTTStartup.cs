using Sandbox;
using System.Linq;

namespace TTT;

/// <summary>
/// Boots the TTT systems into whatever map the project launches.
/// This lets map-first launch configs like flatgrass behave like a classic gamemode.
/// </summary>
public sealed class TTTStartup : GameObjectSystem<TTTStartup>, ISceneStartup
{
	public TTTStartup( Scene scene ) : base( scene )
	{
	}

	void ISceneStartup.OnHostPreInitialize( SceneFile scene )
	{
	}

	void ISceneStartup.OnHostInitialize()
	{
		Log.Info( $"[TTT] Host startup on scene '{Scene?.Name}'" );
		EnsureGameManagerExists();
		EnsureSpawnPointsExist();
	}

	void ISceneStartup.OnClientInitialize()
	{
		Log.Info( $"[TTT] Client startup on scene '{Scene?.Name}' (main menu visible: {Game.IsMainMenuVisible})" );

		if ( Game.IsMainMenuVisible )
			return;

		if ( UI.Hud.Instance is null )
		{
			_ = new UI.Hud();
		}
	}

	private void EnsureGameManagerExists()
	{
		if ( Scene.GetAllComponents<GameManager>().Any() )
		{
			Log.Info( "[TTT] Existing GameManager found in scene" );
			return;
		}

		var gameRoot = new GameObject( true, "TTT Game Root" );
		gameRoot.Flags = GameObjectFlags.NotSaved;
		gameRoot.Components.Create<GameManager>();
		gameRoot.NetworkSpawn();
		Log.Info( "[TTT] Spawned fallback GameManager" );
	}

	private void EnsureSpawnPointsExist()
	{
		if ( Scene.GetAllComponents<SpawnPoint>().Any() )
		{
			Log.Info( "[TTT] Scene already contains spawn points" );
			return;
		}

		var center = new Vector3( 0f, 0f, 128f );
		var radius = 256f;

		for ( var i = 0; i < 8; i++ )
		{
			var angle = i / 8f * 360f;
			var rotation = Rotation.FromYaw( angle );
			var spawn = new GameObject( true, $"TTT Spawn {i + 1}" );
			spawn.Flags = GameObjectFlags.NotSaved;
			spawn.WorldPosition = center + rotation.Forward * radius;
			spawn.WorldRotation = rotation.RotateAroundAxis( Vector3.Up, 180f );
			spawn.Components.Create<SpawnPoint>();
		}

		Log.Info( "[TTT] Spawned fallback spawn points" );
	}
}
