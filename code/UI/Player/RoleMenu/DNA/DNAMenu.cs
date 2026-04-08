using System;
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace TTT.UI;

public partial class DNAMenu : Panel
{
	private DNAScanner _dnaScanner;

	private bool AutoScan { get; set; } = false;

	public override void Tick()
	{
		var player = Player.Local;
		if ( player is null )
			return;

		_dnaScanner ??= player.Inventory.Find<DNAScanner>();
		if ( !_dnaScanner.IsValid() )
			return;

		if ( _dnaScanner.AutoScan != AutoScan )
			SetAutoScan( AutoScan );
	}

	[ConCmd( "ttt_dna_set_autoscan" )]
	public static void SetAutoScan( bool enabled )
	{
		if ( !Networking.IsHost )
			return;

		var player = Utils.GetPlayersWhere( p => p.Network.Owner == Rpc.Caller ).FirstOrDefault();
		if ( player is null )
			return;

		var scanner = player.Inventory.Find<DNAScanner>();
		if ( scanner is null )
			return;

		scanner.AutoScan = enabled;
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( _dnaScanner?.IsCharging, _dnaScanner?.SlotText, _dnaScanner?.DNACollected?.HashCombine( d => d.Id ), _dnaScanner?.SelectedId );
	}
}
