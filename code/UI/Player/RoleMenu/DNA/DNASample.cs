using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace TTT.UI;

public partial class DNASample : Panel
{
	public DNA DNA { get; set; }

	[ConCmd( "ttt_dna_delete_sample" )]
	public static void DeleteSample( int id )
	{
		if ( !Networking.IsHost )
			return;

		var player = Utils.GetPlayersWhere( p => p.Network.Owner == Rpc.Caller ).FirstOrDefault();
		if ( player is null )
			return;

		var scanner = player.Inventory.Find<DNAScanner>();
		if ( scanner is null )
			return;

		foreach ( var dna in scanner.DNACollected )
		{
			if ( dna.Id == id )
			{
				scanner.RemoveDNA( dna );
				return;
			}
		}
	}

	[ConCmd( "ttt_dna_set_active" )]
	public static void SetActiveSample( int id )
	{
		if ( !Networking.IsHost )
			return;

		var player = Utils.GetPlayersWhere( p => p.Network.Owner == Rpc.Caller ).FirstOrDefault();
		if ( player is null )
			return;

		var scanner = player.Inventory.Find<DNAScanner>();
		if ( scanner is null )
			return;

		foreach ( var dna in scanner.DNACollected )
		{
			if ( dna.Id == id )
			{
				scanner.SelectedId = id;
				return;
			}
		}
	}
}
