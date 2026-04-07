using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sandbox;
using Sandbox.UI;

namespace TTT.UI;

public partial class RoleSummary : Panel
{
	public static Panel Instance { get; set; }
	private static readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	private static List<RoleSummaryPlayer> _innocents = new();
	private static List<RoleSummaryPlayer> _detectives = new();
	private static List<RoleSummaryPlayer> _traitors = new();
	private static List<RoundHighlight> _highlights = new();
	private static readonly Dictionary<long, float> _damageDealt = new();
	private static readonly Dictionary<long, float> _burnDamageDealt = new();
	private static readonly Dictionary<long, int> _kills = new();
	private static readonly Dictionary<long, int> _fallDeaths = new();
	private static readonly Dictionary<long, string> _playerNames = new();
	private static string _latestSummaryJson = string.Empty;

	public RoleSummary() => Instance = this;

	[TTTEvent.Round.Start]
	private static void OnRoundStart()
	{
		_damageDealt.Clear();
		_burnDamageDealt.Clear();
		_kills.Clear();
		_fallDeaths.Clear();
		_playerNames.Clear();
		_latestSummaryJson = string.Empty;

		if ( !Game.IsServer )
			return;

		ClearData();
	}

	[TTTEvent.Player.TookDamage]
	private static void OnPlayerTookDamage( Player victim )
	{
		if ( !Game.IsServer || GameManager.Current.State is not InProgress )
			return;

		if ( victim.LastAttacker is not Player attacker || attacker == victim )
			return;

		_playerNames[attacker.SteamId] = attacker.SteamName;
		_playerNames[victim.SteamId] = victim.SteamName;
		_damageDealt[attacker.SteamId] = _damageDealt.GetValueOrDefault( attacker.SteamId ) + victim.LastDamage.Damage;

		if ( victim.LastDamage.HasTag( DamageTags.Burn ) )
			_burnDamageDealt[attacker.SteamId] = _burnDamageDealt.GetValueOrDefault( attacker.SteamId ) + victim.LastDamage.Damage;
	}

	[TTTEvent.Player.Killed]
	private static void OnPlayerKilled( Player victim )
	{
		if ( !Game.IsServer || GameManager.Current.State is not InProgress )
			return;

		if ( victim.LastAttacker is Player attacker && attacker != victim )
		{
			_playerNames[attacker.SteamId] = attacker.SteamName;
			_kills[attacker.SteamId] = _kills.GetValueOrDefault( attacker.SteamId ) + 1;
		}

		if ( victim.LastDamage.HasTag( DamageTags.Fall ) )
		{
			_playerNames[victim.SteamId] = victim.SteamName;
			_fallDeaths[victim.SteamId] = _fallDeaths.GetValueOrDefault( victim.SteamId ) + 1;
		}
	}

	[TTTEvent.Round.End]
	private static void OnRoundEnd( Team winningTeam, WinType winType )
	{
		if ( !Game.IsServer )
			return;

		_latestSummaryJson = JsonSerializer.Serialize( BuildSnapshot(), _jsonOptions );
		RoleSummary.SendData( _latestSummaryJson );
	}

	[ClientRpc]
	public static void SendData( string snapshotJson )
	{
		var snapshot = JsonSerializer.Deserialize<RoundSummarySnapshot>( snapshotJson ?? string.Empty, _jsonOptions ) ?? new RoundSummarySnapshot();
		_innocents = snapshot.Innocents ?? new();
		_detectives = snapshot.Detectives ?? new();
		_traitors = snapshot.Traitors ?? new();
		_highlights = snapshot.Highlights ?? new();

		Instance?.StateHasChanged();
	}

	[ClientRpc]
	public static void ClearData()
	{
		_innocents = new();
		_detectives = new();
		_traitors = new();
		_highlights = new();

		Instance?.StateHasChanged();
	}

	[ConCmd.Server( Name = "ttt_roundsummary_request" )]
	public static void RequestLatestSummary()
	{
		if ( ConsoleSystem.Caller is null || _latestSummaryJson.IsNullOrEmpty() )
			return;

		SendData( To.Single( ConsoleSystem.Caller ), _latestSummaryJson );
	}

	private static RoundSummarySnapshot BuildSnapshot()
	{
		return new RoundSummarySnapshot
		{
			Innocents = BuildPlayerList( Role.GetPlayers<Innocent>() ),
			Detectives = BuildPlayerList( Role.GetPlayers<Detective>() ),
			Traitors = BuildPlayerList( Role.GetPlayers<Traitor>() ),
			Highlights = BuildHighlights()
		};
	}

	private static List<RoleSummaryPlayer> BuildPlayerList( IEnumerable<Player> players )
	{
		return players
			.OrderByDescending( player => player.Score )
			.Select( player => new RoleSummaryPlayer
			{
				SteamId = player.SteamId,
				SteamName = player.SteamName,
				BaseKarma = player.BaseKarma,
				Score = player.Score
			} )
			.ToList();
	}

	private static List<RoundHighlight> BuildHighlights()
	{
		var highlights = new List<RoundHighlight>();

		highlights.Add( BuildTopFloatHighlight( "Most Damage", _damageDealt, "damage dealt", "No meaningful damage dealt." ) );
		highlights.Add( BuildTopIntHighlight( "Most Kills", _kills, "kills", "No kills this round." ) );
		highlights.Add( BuildTopFloatHighlight( "Most Burn Damage", _burnDamageDealt, "burn damage", "Nobody got roasted." ) );
		highlights.Add( BuildFallDeathsHighlight() );

		return highlights;
	}

	private static RoundHighlight BuildTopFloatHighlight( string title, Dictionary<long, float> values, string suffix, string emptyText )
	{
		var topEntries = values
			.Where( entry => entry.Value > 0f )
			.OrderByDescending( entry => entry.Value )
			.ToList();

		if ( topEntries.Count == 0 )
			return new RoundHighlight { Title = title, PrimaryText = "Nobody", SecondaryText = emptyText };

		var bestValue = topEntries[0].Value;
		var winners = topEntries
			.Where( entry => MathF.Abs( entry.Value - bestValue ) < 0.01f )
			.Select( entry => GetPlayerName( entry.Key ) )
			.ToList();

		return new RoundHighlight
		{
			Title = title,
			PrimaryText = string.Join( ", ", winners ),
			SecondaryText = $"{MathF.Round( bestValue )} {suffix}" + (winners.Count > 1 ? " each" : string.Empty)
		};
	}

	private static RoundHighlight BuildTopIntHighlight( string title, Dictionary<long, int> values, string suffix, string emptyText )
	{
		var topEntries = values
			.Where( entry => entry.Value > 0 )
			.OrderByDescending( entry => entry.Value )
			.ToList();

		if ( topEntries.Count == 0 )
			return new RoundHighlight { Title = title, PrimaryText = "Nobody", SecondaryText = emptyText };

		var bestValue = topEntries[0].Value;
		var winners = topEntries
			.Where( entry => entry.Value == bestValue )
			.Select( entry => GetPlayerName( entry.Key ) )
			.ToList();

		return new RoundHighlight
		{
			Title = title,
			PrimaryText = string.Join( ", ", winners ),
			SecondaryText = $"{bestValue} {suffix}" + (winners.Count > 1 ? " each" : string.Empty)
		};
	}

	private static RoundHighlight BuildFallDeathsHighlight()
	{
		var fallenPlayers = _fallDeaths
			.Where( entry => entry.Value > 0 )
			.OrderByDescending( entry => entry.Value )
			.Select( entry => entry.Value > 1 ? $"{GetPlayerName( entry.Key )} ({entry.Value})" : GetPlayerName( entry.Key ) )
			.ToList();

		if ( fallenPlayers.Count == 0 )
		{
			return new RoundHighlight
			{
				Title = "Fell To Their Death",
				PrimaryText = "Nobody",
				SecondaryText = "Everyone stuck the landing."
			};
		}

		return new RoundHighlight
		{
			Title = "Fell To Their Death",
			PrimaryText = string.Join( ", ", fallenPlayers ),
			SecondaryText = fallenPlayers.Count == 1 ? "One fatal fall." : $"{fallenPlayers.Count} players took a fatal fall."
		};
	}

	private static string GetPlayerName( long steamId )
	{
		if ( _playerNames.TryGetValue( steamId, out var playerName ) && !playerName.IsNullOrEmpty() )
			return playerName;

		return Game.Clients
			.Select( client => client.Pawn as Player )
			.FirstOrDefault( player => player.IsValid() && player.SteamId == steamId )?.SteamName ?? "Unknown Player";
	}

	internal class RoundHighlight
	{
		public string Title { get; set; }
		public string PrimaryText { get; set; }
		public string SecondaryText { get; set; }
	}

	internal class RoleSummaryPlayer
	{
		public long SteamId { get; set; }
		public string SteamName { get; set; }
		public float BaseKarma { get; set; }
		public int Score { get; set; }
	}

	internal class RoundSummarySnapshot
	{
		public List<RoleSummaryPlayer> Innocents { get; set; } = new();
		public List<RoleSummaryPlayer> Detectives { get; set; } = new();
		public List<RoleSummaryPlayer> Traitors { get; set; } = new();
		public List<RoundHighlight> Highlights { get; set; } = new();
	}
}
