using Sandbox;
using Sandbox.Diagnostics;
using System.Collections.Generic;

namespace TTT;

[Title( "TTT Game Manager" )]
public partial class GameManager : Component, Component.INetworkListener
{
	public static GameManager Instance { get; private set; }

	public BaseState State { get; private set; }

	[Sync] public int TotalRoundsPlayed { get; set; }
	[Sync] public RealTimeUntil TimeUntilMapSwitch { get; set; }

	public List<string> MapVoteIdents { get; set; } = new();
	public int RTVCount { get; set; }

	[Property, ResourceType( "prefab" )]
	public PrefabFile PlayerPrefab { get; set; }

	protected override void OnStart()
	{
		Instance = this;

		if ( Networking.IsHost )
		{
			LoadModerationData();
			TimeUntilMapSwitch = TimeLimit;
			ForceStateChange( new WaitingState() );
		}

		LoadResources();
	}

	protected override void OnUpdate()
	{
		State?.OnTick();

		if ( Networking.IsHost )
			TickTribunalReports();
	}

	// INetworkListener - called on the host when a player connects
	public void OnActive( Connection channel )
	{
		if ( !Networking.IsHost )
			return;

		if ( !CanConnect( channel ) )
		{
			channel.Kick( "You are not allowed to join this server." );
			return;
		}

		// Spawn a player game object for this connection
		GameObject playerGo;

		if ( PlayerPrefab != null )
		{
			playerGo = SceneUtility.GetPrefabScene( PlayerPrefab ).Clone();
		}
		else
		{
			// Create player object programmatically
			playerGo = new GameObject( true, $"Player ({channel.DisplayName})" );
			playerGo.Tags.Add( "player" );

			var renderer = playerGo.Components.Create<SkinnedModelRenderer>();
			renderer.Model = Model.Load( "models/citizen/citizen.vmdl" );

			var cc = playerGo.Components.Create<CharacterController>();
			cc.Height = 72f;
			cc.Radius = 16f;

			var player = playerGo.Components.Create<Player>();
		}

		playerGo.NetworkSpawn( channel );

		var playerComp = playerGo.Components.Get<Player>();
		playerComp.OnConnectionActive( channel );

		State.OnPlayerJoin( playerComp );
		OnPlayerJoinedModeration( playerComp );

		UI.TextChat.AddInfoEntry( $"{channel.DisplayName} has joined" );
	}

	public void OnDisconnected( Connection channel )
	{
		var player = FindPlayerByConnection( channel );

		if ( player.IsValid() )
		{
			Karma.SaveKarma( player );
			State.OnPlayerLeave( player );
		}

		UI.TextChat.AddInfoEntry( $"{channel.DisplayName} has left" );

		// Only delete if alive; keep dead body on disconnect
		if ( player.IsValid() && player.IsAlive )
			player.GameObject.Destroy();
	}

	/// <summary>
	/// Changes the state if minimum players is met. Otherwise, force changes to WaitingState.
	/// </summary>
	public void ChangeState( BaseState state )
	{
		Assert.NotNull( state );

		var hasMinimumPlayers = Utils.GetPlayersWhere( p => !p.IsForcedSpectator ).Count >= MinPlayers;
		ForceStateChange( hasMinimumPlayers ? state : new WaitingState() );
	}

	/// <summary>
	/// Force changes a state regardless of player count.
	/// </summary>
	public void ForceStateChange( BaseState state )
	{
		State?.Finish();
		State = state;
		State.Start();
	}

	public bool CanHearPlayerVoice( Connection source, Connection dest )
	{
		var sourcePl = FindPlayerByConnection( source );
		var destPl = FindPlayerByConnection( dest );

		if ( sourcePl is null || destPl is null )
			return false;

		if ( destPl.MuteFilter == MuteFilter.All )
			return false;

		if ( !sourcePl.IsAlive && !destPl.CanHearSpectators )
			return false;

		if ( sourcePl.IsAlive && !destPl.CanHearAlivePlayers )
			return false;

		return true;
	}

	public void MoveToSpawnpoint( Player player )
	{
		var spawnpoints = Game.ActiveScene.GetAllComponents<SpawnPoint>().ToList();
		if ( spawnpoints.Count == 0 )
			return;

		var sp = Game.Random.FromList( spawnpoints );
		player.WorldPosition = sp.WorldPosition;
		player.WorldRotation = sp.WorldRotation;
	}

	public Player FindPlayerByConnection( Connection connection )
	{
		return Utils.GetPlayersWhere( p => p.Network.Owner == connection ).FirstOrDefault();
	}

	private static void LoadResources()
	{
		Detective.Hat = ResourceLibrary.Get<Clothing>( "models/detective_hat/detective_hat.clothing" );

		Player.ClothingPresets.Add( new List<Clothing>() );
		Player.ClothingPresets[0].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/hat/balaclava/balaclava.clothing" ) );
		Player.ClothingPresets[0].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/hair/eyebrows/eyebrows_black.clothing" ) );
		Player.ClothingPresets[0].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/jacket/longsleeve/longsleeve.clothing" ) );
		Player.ClothingPresets[0].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/gloves/tactical_gloves/tactical_gloves.clothing" ) );
		Player.ClothingPresets[0].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/trousers/jeans/jeans_black.clothing" ) );
		Player.ClothingPresets[0].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/vest/tactical_vest/models/tactical_vest.clothing" ) );
		Player.ClothingPresets[0].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shoes/trainers/trainers.clothing" ) );

		Player.ClothingPresets.Add( new List<Clothing>() );
		Player.ClothingPresets[1].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/hat/balaclava/balaclava.clothing" ) );
		Player.ClothingPresets[1].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/hair/eyebrows/eyebrows.clothing" ) );
		Player.ClothingPresets[1].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shirt/flannel_shirt/variations/blue_shirt/blue_shirt.clothing" ) );
		Player.ClothingPresets[1].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/gloves/tactical_gloves/tactical_gloves.clothing" ) );
		Player.ClothingPresets[1].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/trousers/cargopants/cargo_pants_army.clothing" ) );
		Player.ClothingPresets[1].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/vest/tactical_vest/models/tactical_vest.clothing" ) );
		Player.ClothingPresets[1].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shoes/trainers/trainers.clothing" ) );

		Player.ClothingPresets.Add( new List<Clothing>() );
		Player.ClothingPresets[2].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/hat/beanie/beanie_red.clothing" ) );
		Player.ClothingPresets[2].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/hair/eyebrows/eyebrows_black.clothing" ) );
		Player.ClothingPresets[2].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/hair/goatee/goatee_black.clothing" ) );
		Player.ClothingPresets[2].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/gloves/tactical_gloves/army_gloves.clothing" ) );
		Player.ClothingPresets[2].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/trousers/cargopants/cargo_pants_army.clothing" ) );
		Player.ClothingPresets[2].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/makeup/face_tattoos/face_tattoos.clothing" ) );
		Player.ClothingPresets[2].Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shoes/boots/army_boots.clothing" ) );
		Player.ClothingPresets[2].Add( ResourceLibrary.Get<Clothing>( "models/citizen/skin/muscley/muscley_02.clothing" ) );

		Player.ChangeClothingPreset();
	}
}
