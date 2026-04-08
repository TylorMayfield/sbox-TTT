using Sandbox;
using System;
using System.Linq;

namespace TTT;

public partial class GameManager
{
	[ConCmd( "ttt_respawn" )]
	public static void RespawnPlayer( int id = 0 )
	{
		if ( !Networking.IsHost )
			return;

		if ( !HasAdminAccess( Rpc.Caller ) )
			return;

		Player player;
		if ( id == 0 )
			player = Instance?.FindPlayerByConnection( Rpc.Caller );
		else
			player = Utils.GetPlayersWhere( p => p.SteamId == (ulong)id ).FirstOrDefault();

		if ( !player.IsValid() )
			return;

		player.Respawn();
	}

	[ConCmd( "ttt_giveitem" )]
	public static void GiveItem( string itemName )
	{
		if ( !Networking.IsHost )
			return;

		if ( !HasAdminAccess( Rpc.Caller ) )
			return;

		if ( itemName.IsNullOrEmpty() )
			return;

		var player = Instance?.FindPlayerByConnection( Rpc.Caller );
		if ( !player.IsValid() )
			return;

		var itemInfo = GameResource.GetInfo<ItemInfo>( itemName );
		if ( itemInfo is null )
		{
			Log.Error( $"{itemName} isn't a valid Item!" );
			return;
		}

		if ( itemInfo is CarriableInfo )
			player.Inventory.Add( TypeLibrary.Create<Carriable>( itemInfo.ClassName ) );
		else if ( itemInfo is PerkInfo )
			player.Perks.Add( TypeLibrary.Create<Perk>( itemInfo.ClassName ) );
	}

	[ConCmd( "ttt_givecredits" )]
	public static void GiveCredits( int credits )
	{
		if ( !Networking.IsHost )
			return;

		if ( !HasAdminAccess( Rpc.Caller ) )
			return;

		var player = Instance?.FindPlayerByConnection( Rpc.Caller );
		if ( !player.IsValid() )
			return;

		player.Credits += credits;
	}

	[ConCmd( "ttt_givedamage" )]
	public static void GiveDamage( float damage )
	{
		if ( !Networking.IsHost )
			return;

		if ( !HasAdminAccess( Rpc.Caller ) )
			return;

		var player = Instance?.FindPlayerByConnection( Rpc.Caller );
		if ( !player.IsValid() )
			return;

		player.TakeDamage( new DamageInfo { Damage = damage } );
	}

	[ConCmd( "ttt_setrole" )]
	public static void SetRole( string roleName )
	{
		if ( !Networking.IsHost )
			return;

		if ( !HasAdminAccess( Rpc.Caller ) )
			return;

		if ( Instance?.State is not InProgress )
			return;

		var player = Instance?.FindPlayerByConnection( Rpc.Caller );
		if ( !player.IsValid() )
			return;

		var roleInfo = GameResource.GetInfo<RoleInfo>( roleName );
		if ( roleInfo is null )
		{
			Log.Error( $"{roleName} isn't a valid Role!" );
			return;
		}

		player.SetRole( roleInfo );
	}

	[ConCmd( "ttt_setkarma" )]
	public static void SetKarma( int karma, ulong steamId = 0 )
	{
		if ( !Networking.IsHost )
			return;

		if ( !HasAdminAccess( Rpc.Caller ) )
			return;

		Player player;
		if ( steamId == 0 )
			player = Instance?.FindPlayerByConnection( Rpc.Caller );
		else
			player = Utils.GetPlayersWhere( p => p.SteamId == steamId ).FirstOrDefault();

		if ( !player.IsValid() )
			return;

		player.BaseKarma = karma;
		player.ActiveKarma = karma;
		Karma.ApplyRoundModifiers( player );
	}

	[ConCmd( "ttt_force_restart" )]
	public static void ForceRestart()
	{
		if ( !Networking.IsHost )
			return;

		if ( !HasAdminAccess( Rpc.Caller ) )
			return;

		Instance?.ChangeState( new PreRound() );
	}

	[ConCmd( "ttt_change_map" )]
	public static async void ChangeMap( string mapIdent )
	{
		if ( !Networking.IsHost )
			return;

		if ( !HasAdminAccess( Rpc.Caller ) )
			return;

		var package = await Package.Fetch( mapIdent, true );
		if ( package is not null && package.PackageType == Package.Type.Map )
		{
			var options = new SceneLoadOptions();
			if ( options.SetScene( mapIdent ) )
				Game.ChangeScene( options );
		}
		else
			Log.Error( $"{mapIdent} does not exist as a s&box map!" );
	}

	[ConCmd( "ttt_rtv" )]
	public static void RockTheVote()
	{
		if ( !Networking.IsHost )
			return;

		var caller = Rpc.Caller;
		if ( caller is null )
			return;

		if ( caller.HasRockedTheVote() )
			return;

		caller.SetValue( "!rtv", true );

		if ( Instance is null )
			return;

		Instance.RTVCount += 1;
		UI.TextChat.AddInfoEntry( $"{caller.DisplayName} has rocked the vote! ({Instance.RTVCount}/{MathF.Round( Connection.All.Count * RTVThreshold )})" );
	}

	[ConCmd( "kill" )]
	public static void KillSelf()
	{
		if ( !Networking.IsHost )
			return;

		var player = Instance?.FindPlayerByConnection( Rpc.Caller );
		if ( !player.IsValid() )
			return;

		player.Kill();
	}
}
