using Sandbox;
using System;
using System.Collections.Generic;

namespace TTT;

public static class Scoring
{
	[TTTEvent.Player.Killed]
	private static void OnPlayerKilled( Player player )
	{
		if ( !Networking.IsHost )
			return;

		if ( GameManager.Instance.State is not InProgress )
			return;

		if ( !player.KilledByPlayer )
		{
			player.RoundScore -= player.Role.Scoring.SuicidePenalty;
		}
		else
		{
			if ( player.LastAttacker?.Components.TryGet<Player>( out var attacker ) != true )
				return;

			if ( attacker.Team != player.Team )
				attacker.RoundScore += player.Role.Scoring.AttackerKillReward;
			else
				attacker.RoundScore -= attacker.Role.Scoring.TeamKillPenalty;
		}
	}

	[TTTEvent.Player.CorpseFound]
	private static void OnCorpseFound( Player player )
	{
		if ( !Networking.IsHost )
			return;

		var finder = player.Corpse.Finder;
		finder.RoundScore += finder.Role.Scoring.CorpseFoundReward;
	}

	[TTTEvent.Round.End]
	private static void OnRoundEnd( Team winningTeam, WinType winType )
	{
		if ( !Networking.IsHost )
			return;

		var allPlayers = Utils.GetPlayersWhere( _ => true );
		var alivePlayersCount = new List<int>( new int[3] );
		var deadPlayersCount = new List<int>( new int[3] );

		foreach ( var player in allPlayers )
		{
			if ( !player.IsAlive )
			{
				deadPlayersCount[(int)player.Team]++;
				continue;
			}

			player.RoundScore += player.Role.Scoring.SurviveBonus;
			alivePlayersCount[(int)player.Team]++;
		}

		var traitorBonus = (int)MathF.Ceiling( deadPlayersCount[1] / 2f );
		var innocentBonus = alivePlayersCount[1];

		if ( winType != WinType.TimeUp )
			traitorBonus += alivePlayersCount[2];
		else
			traitorBonus -= (int)MathF.Floor( alivePlayersCount[1] / 2f );

		foreach ( var player in allPlayers )
		{
			var bonus = player.Team == Team.Traitors ? traitorBonus : innocentBonus;
			player.RoundScore += bonus;

			// Add the score gained this round to the actual score.
			player.Score += player.RoundScore;
			player.RoundScore = 0;
		}
	}
}
