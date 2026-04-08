using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TTT;

public partial class MapSelectionState : BaseState
{
	public Dictionary<Connection, string> Votes { get; private set; } = new();

	public override string Name { get; } = "Map Selection";
	public override int Duration => GameManager.MapSelectionTime;

	public const string MapsFile = "maps.txt";

	protected override void OnTimeUp()
	{
		if ( Votes.Count == 0 )
		{
			var options = new SceneLoadOptions();
			if ( options.SetScene( Game.Random.FromList( GameManager.Instance.MapVoteIdents.ToList() ) ?? GameManager.DefaultMap ) )
				Game.ChangeScene( options );
			return;
		}

		var winningMap = Votes.GroupBy( x => x.Value )
			.OrderBy( x => x.Count() )
			.Last().Key;

		var sceneOptions = new SceneLoadOptions();
		if ( sceneOptions.SetScene( winningMap ) )
			Game.ChangeScene( sceneOptions );
	}

	protected override void OnStart()
	{
		UI.FullScreenHintMenu.Instance?.ForceOpen( new UI.MapSelectionMenu() );

		if ( Networking.IsHost )
			_ = LoadMapIdents();
	}

	[ConCmd( "ttt_map_vote" )]
	public static void SetVote( string map )
	{
		if ( !Networking.IsHost )
			return;

		if ( GameManager.Instance.State is not MapSelectionState state )
			return;

		var player = Utils.GetPlayersWhere( p => p.Network.Owner == Rpc.Caller ).FirstOrDefault();
		if ( player is null )
			return;

		state.Votes[Rpc.Caller] = map;
	}

	private static async Task LoadMapIdents()
	{
		var maps = await GetLocalMapIdents();
		if ( maps.IsNullOrEmpty() )
			maps = await GetRemoteMapIdents();

		maps.Shuffle();
		GameManager.Instance.MapVoteIdents = maps;
	}

	private static async Task<List<string>> GetLocalMapIdents()
	{
		var maps = new List<string>();

		var rawMaps = FileSystem.Data.ReadAllText( MapsFile );
		if ( rawMaps.IsNullOrEmpty() )
			return maps;

		var splitMaps = rawMaps.Split( "\n" );
		foreach ( var rawMap in splitMaps )
		{
			var mapIdent = rawMap.Trim();
			var package = await Package.Fetch( mapIdent, true );

			if ( package is not null && package.PackageType == Package.Type.Map )
				maps.Add( mapIdent );
			else
				Log.Error( $"{mapIdent} does not exist as a s&box map!" );
		}

		return maps;
	}

	private static async Task<List<string>> GetRemoteMapIdents()
	{
		var queryResult = await Package.FindAsync( "type:map", take: 99 );
		return queryResult?.Packages.Select( p => p.FullIdent ).ToList() ?? new();
	}
}
