using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

namespace TTT.UI;

public partial class CreditTransfer : Panel
{
	private const int CreditAmount = 100;
	private Player _selectedPlayer;

	[ConCmd( "ttt_send_credits" )]
	public static void SendCredits( ulong receiverSteamId, int credits )
	{
		if ( !Networking.IsHost )
			return;

		var sendingPlayer = Utils.GetPlayersWhere( p => p.Network.Owner == Rpc.Caller ).FirstOrDefault();
		if ( sendingPlayer is null )
			return;

		var receivingPlayer = Utils.GetPlayersWhere( p => p.SteamId == receiverSteamId ).FirstOrDefault();
		if ( receivingPlayer is null )
			return;

		sendingPlayer.Credits -= credits;
		receivingPlayer.Credits += credits;
	}

	private bool CanTransferCreditsTo( Player sendingPlayer, Player receivingPlayer )
	{
		return sendingPlayer != receivingPlayer && receivingPlayer.IsAlive && sendingPlayer.Team == receivingPlayer.Team && receivingPlayer.Role.CanUseShop;
	}

	protected override int BuildHash()
	{
		return HashCode.Combine(
			Player.Local?.Credits,
			_selectedPlayer?.SteamId,
			Utils.GetPlayersWhere( p => p.IsAlive ).HashCombine( p => p.Role.GetHashCode() )
		);
	}
}
