using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace TTT;

public struct BannedClient
{
	public long SteamId { get; set; }
	public string Reason { get; set; }
	public DateTime Duration { get; set; }
}

public enum RdmReportStatus
{
	Open,
	Claimed,
	Resolved,
	Dismissed
}

public enum TribunalVerdict
{
	None,
	Guilty,
	NotGuilty,
	NoConsensus
}

public sealed class TribunalVote
{
	public long VoterSteamId { get; set; }
	public string VoterName { get; set; }
	public bool IsGuiltyVote { get; set; }
	public DateTime VotedAt { get; set; }
}

public sealed class ModerationLogEntry
{
	public int Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public int Round { get; set; }
	public string Category { get; set; }
	public string Summary { get; set; }
	public string Details { get; set; }
	public long ActorSteamId { get; set; }
	public string ActorName { get; set; }
	public string ActorRole { get; set; }
	public long TargetSteamId { get; set; }
	public string TargetName { get; set; }
	public string TargetRole { get; set; }
}

public sealed class RdmReport
{
	public int Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public int Round { get; set; }
	public long ReporterSteamId { get; set; }
	public string ReporterName { get; set; }
	public string ReporterRole { get; set; }
	public long AccusedSteamId { get; set; }
	public string AccusedName { get; set; }
	public string AccusedRole { get; set; }
	public string Reason { get; set; }
	public RdmReportStatus Status { get; set; }
	public long ClaimedBySteamId { get; set; }
	public string ClaimedByName { get; set; }
	public string ResolutionNotes { get; set; }
	public DateTime? ClosedAt { get; set; }
	public DateTime? TribunalEndsAt { get; set; }
	public TribunalVerdict TribunalVerdict { get; set; }
	public List<TribunalVote> TribunalVotes { get; set; } = new();
}

public sealed class ModerationPlayerInfo
{
	public long SteamId { get; set; }
	public string Name { get; set; }
	public string Role { get; set; }
	public string Status { get; set; }
	public int Score { get; set; }
	public int Karma { get; set; }
}

public partial class GameManager : Sandbox.GameManager
{
	public static readonly List<BannedClient> BannedClients = new();
	public static readonly List<ModerationLogEntry> ModerationLogs = new();
	public static readonly List<RdmReport> RdmReports = new();

	public const string BanFilePath = "bans.json";
	public const string ModerationLogFilePath = "moderation_logs.json";
	public const string RdmReportFilePath = "rdm_reports.json";

	private const int MaxModerationLogs = 300;
	private const int MaxRdmReports = 150;

	private static int _nextModerationLogId = 1;
	private static int _nextRdmReportId = 1;

	public override bool ShouldConnect( long steamId )
	{
		if ( Karma.SavedPlayerValues.TryGetValue( steamId, out var value ) && value < Karma.MinValue )
			return false;

		for ( var i = BannedClients.Count - 1; i >= 0; i-- )
		{
			var bannedClient = BannedClients[i];
			if ( bannedClient.SteamId != steamId )
				continue;

			if ( bannedClient.Duration >= DateTime.Now )
				return false;

			BannedClients.RemoveAt( i );
		}

		return true;
	}

	internal static void LoadModerationData()
	{
		BannedClients.Clear();
		RdmReports.Clear();
		ModerationLogs.Clear();

		LoadBannedClients();

		var reports = FileSystem.Data.ReadJson<List<RdmReport>>( RdmReportFilePath );
		if ( !reports.IsNullOrEmpty() )
			RdmReports.AddRange( reports.OrderByDescending( report => report.CreatedAt ).Take( MaxRdmReports ) );

		var logs = FileSystem.Data.ReadJson<List<ModerationLogEntry>>( ModerationLogFilePath );
		if ( !logs.IsNullOrEmpty() )
			ModerationLogs.AddRange( logs.OrderByDescending( log => log.CreatedAt ).Take( MaxModerationLogs ) );

		_nextRdmReportId = RdmReports.Count > 0 ? RdmReports.Max( report => report.Id ) + 1 : 1;
		_nextModerationLogId = ModerationLogs.Count > 0 ? ModerationLogs.Max( log => log.Id ) + 1 : 1;
	}

	internal static void SaveModerationData()
	{
		FileSystem.Data.WriteJson( BanFilePath, BannedClients );
		FileSystem.Data.WriteJson( RdmReportFilePath, RdmReports );
		FileSystem.Data.WriteJson( ModerationLogFilePath, ModerationLogs );
	}

	private static void LoadBannedClients()
	{
		var clients = FileSystem.Data.ReadJson<List<BannedClient>>( BanFilePath );
		if ( !clients.IsNullOrEmpty() )
			BannedClients.AddRange( clients );
	}

	private static bool HasAdminAccess( IClient client )
	{
		return client is null || (client.IsValid() && client.IsAdmin);
	}

	private static IEnumerable<IClient> GetAdminClients()
	{
		return Game.Clients.Where( HasAdminAccess );
	}

	private static Player FindPlayerBySteamId( long steamId )
	{
		return Game.Clients
			.Select( client => client.Pawn as Player )
			.FirstOrDefault( player => player.IsValid() && player.SteamId == steamId );
	}

	private static string GetRoleName( Player player )
	{
		return player?.Role?.Title ?? "Unknown";
	}

	private static void AddModerationLog( string category, string summary, string details = "", Player actor = null, Player target = null )
	{
		var entry = new ModerationLogEntry
		{
			Id = _nextModerationLogId++,
			CreatedAt = DateTime.UtcNow,
			Round = Current?.TotalRoundsPlayed ?? 0,
			Category = category,
			Summary = summary,
			Details = details,
			ActorSteamId = actor?.SteamId ?? 0,
			ActorName = actor?.SteamName ?? string.Empty,
			ActorRole = GetRoleName( actor ),
			TargetSteamId = target?.SteamId ?? 0,
			TargetName = target?.SteamName ?? string.Empty,
			TargetRole = GetRoleName( target )
		};

		ModerationLogs.Insert( 0, entry );
		if ( ModerationLogs.Count > MaxModerationLogs )
			ModerationLogs.RemoveRange( MaxModerationLogs, ModerationLogs.Count - MaxModerationLogs );
	}

	private static string BuildReportsJson()
	{
		return JsonSerializer.Serialize( RdmReports.OrderByDescending( report => report.CreatedAt ).ToList() );
	}

	private static string BuildLogsJson()
	{
		return JsonSerializer.Serialize( ModerationLogs.OrderByDescending( log => log.CreatedAt ).Take( 200 ).ToList() );
	}

	private static string BuildPlayersJson()
	{
		var players = Game.Clients
			.Select( client => client.Pawn as Player )
			.Where( player => player.IsValid() )
			.Select( player => new ModerationPlayerInfo
			{
				SteamId = player.SteamId,
				Name = player.SteamName,
				Role = GetRoleName( player ),
				Status = player.Status.ToString(),
				Score = player.Score,
				Karma = (int)player.ActiveKarma
			} )
			.OrderBy( player => player.Name )
			.ToList();

		return JsonSerializer.Serialize( players );
	}

	private static string BuildTribunalReportsJson()
	{
		var reports = RdmReports
			.Where( report => report.TribunalEndsAt is not null || report.TribunalVerdict != TribunalVerdict.None )
			.OrderByDescending( report => report.CreatedAt )
			.Take( 50 )
			.ToList();

		return JsonSerializer.Serialize( reports );
	}

	private static void PushModerationSnapshotToAdmins()
	{
		var admins = GetAdminClients().ToList();
		if ( admins.Count == 0 )
			return;

		ReceiveModerationSnapshot( To.Multiple( admins ), BuildReportsJson(), BuildLogsJson(), BuildPlayersJson() );
	}

	private static void PushTribunalSnapshot()
	{
		ReceiveTribunalSnapshot( To.Everyone, BuildTribunalReportsJson() );
	}

	[ClientRpc]
	private static void ReceiveModerationSnapshot( string reportsJson, string logsJson, string playersJson )
	{
		UI.AdminPage.ReceiveSnapshot( reportsJson, logsJson, playersJson );
	}

	[ClientRpc]
	private static void ReceiveTribunalSnapshot( string reportsJson )
	{
		UI.TribunalPage.ReceiveSnapshot( reportsJson );
	}

	[ConCmd.Admin( Name = "ttt_admin_refresh", Help = "Refreshes the moderation admin console snapshot." )]
	public static void RequestModerationSnapshot()
	{
		if ( !HasAdminAccess( ConsoleSystem.Caller ) )
			return;

		ReceiveModerationSnapshot( To.Single( ConsoleSystem.Caller ), BuildReportsJson(), BuildLogsJson(), BuildPlayersJson() );
	}

	[ConCmd.Server( Name = "ttt_tribunal_refresh", Help = "Refreshes the tribunal snapshot." )]
	public static void RequestTribunalSnapshot()
	{
		if ( ConsoleSystem.Caller is null )
			return;

		ReceiveTribunalSnapshot( To.Single( ConsoleSystem.Caller ), BuildTribunalReportsJson() );
	}

	[ConCmd.Server( Name = "ttt_rdm_report", Help = "Reports a player for potential RDM." )]
	public static void SubmitRdmReport( long accusedSteamId, string reason = "" )
	{
		var reporter = ConsoleSystem.Caller?.Pawn as Player;
		if ( !reporter.IsValid() )
			return;

		var accused = FindPlayerBySteamId( accusedSteamId );
		if ( !accused.IsValid() || accused == reporter )
			return;

		if ( RdmReports.Any( report =>
			report.ReporterSteamId == reporter.SteamId &&
			report.AccusedSteamId == accusedSteamId &&
			report.Round == (Current?.TotalRoundsPlayed ?? 0) &&
			report.Status is RdmReportStatus.Open or RdmReportStatus.Claimed ) )
		{
			UI.TextChat.AddInfoEntry( To.Single( ConsoleSystem.Caller ), "You already have an open RDM report against that player." );
			return;
		}

		reason = reason.Trim();
		if ( reason.IsNullOrEmpty() )
			reason = "Possible RDM";

		var report = new RdmReport
		{
			Id = _nextRdmReportId++,
			CreatedAt = DateTime.UtcNow,
			Round = Current?.TotalRoundsPlayed ?? 0,
			ReporterSteamId = reporter.SteamId,
			ReporterName = reporter.SteamName,
			ReporterRole = reporter.Role.Title,
			AccusedSteamId = accused.SteamId,
			AccusedName = accused.SteamName,
			AccusedRole = accused.Role.Title,
			Reason = reason,
			Status = RdmReportStatus.Open,
			TribunalEndsAt = TribunalEnabled ? DateTime.UtcNow.AddSeconds( TribunalVoteSeconds ) : null
		};

		RdmReports.Insert( 0, report );
		if ( RdmReports.Count > MaxRdmReports )
			RdmReports.RemoveRange( MaxRdmReports, RdmReports.Count - MaxRdmReports );

		AddModerationLog(
			"RDM Report",
			$"{reporter.SteamName} reported {accused.SteamName}",
			$"Reason: {reason}",
			reporter,
			accused
		);

		SaveModerationData();
		PushModerationSnapshotToAdmins();
		PushTribunalSnapshot();

		UI.TextChat.AddInfoEntry( To.Single( ConsoleSystem.Caller ), $"Submitted RDM report against {accused.SteamName}." );

		var admins = GetAdminClients().ToList();
		if ( admins.Count > 0 )
			UI.TextChat.AddInfoEntry( To.Multiple( admins ), $"New RDM report: {reporter.SteamName} reported {accused.SteamName}." );

		if ( TribunalEnabled )
			UI.TextChat.AddInfoEntry( To.Everyone, $"Tribunal opened: {reporter.SteamName} reported {accused.SteamName} for RDM." );
	}

	[ConCmd.Server( Name = "ttt_tribunal_vote", Help = "Votes on an active tribunal report. true = guilty, false = not guilty." )]
	public static void VoteOnRdmReport( int reportId, bool isGuiltyVote )
	{
		var voter = ConsoleSystem.Caller?.Pawn as Player;
		if ( !TribunalEnabled || !voter.IsValid() )
			return;

		var report = RdmReports.FirstOrDefault( entry => entry.Id == reportId );
		if ( report is null || report.Status is RdmReportStatus.Resolved or RdmReportStatus.Dismissed || report.TribunalEndsAt is null )
			return;

		if ( report.ReporterSteamId == voter.SteamId || report.AccusedSteamId == voter.SteamId )
		{
			UI.TextChat.AddInfoEntry( To.Single( ConsoleSystem.Caller ), "You cannot vote on a report you are directly involved in." );
			return;
		}

		var existingVote = report.TribunalVotes.FirstOrDefault( vote => vote.VoterSteamId == voter.SteamId );
		if ( existingVote is null )
		{
			report.TribunalVotes.Add( new TribunalVote
			{
				VoterSteamId = voter.SteamId,
				VoterName = voter.SteamName,
				IsGuiltyVote = isGuiltyVote,
				VotedAt = DateTime.UtcNow
			} );
		}
		else
		{
			existingVote.IsGuiltyVote = isGuiltyVote;
			existingVote.VotedAt = DateTime.UtcNow;
		}

		AddModerationLog(
			"Tribunal Vote",
			$"{voter.SteamName} voted {(isGuiltyVote ? "guilty" : "not guilty")} on report #{report.Id}",
			$"Reporter: {report.ReporterName}; Accused: {report.AccusedName}",
			voter
		);

		EvaluateTribunalOutcome( report, false );
		SaveModerationData();
		PushModerationSnapshotToAdmins();
		PushTribunalSnapshot();
		UI.TextChat.AddInfoEntry( To.Single( ConsoleSystem.Caller ), $"You voted {(isGuiltyVote ? "guilty" : "not guilty")} on report #{report.Id}." );
	}

	[ConCmd.Admin( Name = "ttt_rdm_claim", Help = "Claims an RDM report." )]
	public static void ClaimRdmReport( int reportId )
	{
		if ( !HasAdminAccess( ConsoleSystem.Caller ) )
			return;

		var report = RdmReports.FirstOrDefault( entry => entry.Id == reportId );
		if ( report is null || report.Status is RdmReportStatus.Resolved or RdmReportStatus.Dismissed )
			return;

		report.Status = RdmReportStatus.Claimed;
		report.ClaimedBySteamId = ConsoleSystem.Caller.SteamId;
		report.ClaimedByName = ConsoleSystem.Caller.Name;

		AddModerationLog(
			"Admin Action",
			$"{ConsoleSystem.Caller.Name} claimed RDM report #{report.Id}",
			$"Reporter: {report.ReporterName}; Accused: {report.AccusedName}"
		);

		SaveModerationData();
		PushModerationSnapshotToAdmins();
		PushTribunalSnapshot();
	}

	[ConCmd.Admin( Name = "ttt_rdm_resolve", Help = "Resolves an RDM report." )]
	public static void ResolveRdmReport( int reportId, string notes = "" )
	{
		UpdateRdmReportStatus( reportId, RdmReportStatus.Resolved, notes );
	}

	[ConCmd.Admin( Name = "ttt_rdm_dismiss", Help = "Dismisses an RDM report." )]
	public static void DismissRdmReport( int reportId, string notes = "" )
	{
		UpdateRdmReportStatus( reportId, RdmReportStatus.Dismissed, notes );
	}

	private static void UpdateRdmReportStatus( int reportId, RdmReportStatus status, string notes )
	{
		if ( !HasAdminAccess( ConsoleSystem.Caller ) )
			return;

		var report = RdmReports.FirstOrDefault( entry => entry.Id == reportId );
		if ( report is null )
			return;

		report.Status = status;
		report.ClaimedBySteamId = ConsoleSystem.Caller.SteamId;
		report.ClaimedByName = ConsoleSystem.Caller.Name;
		report.ResolutionNotes = notes?.Trim() ?? string.Empty;
		report.ClosedAt = DateTime.UtcNow;
		report.TribunalEndsAt = null;

		AddModerationLog(
			"Admin Action",
			$"{ConsoleSystem.Caller.Name} marked RDM report #{report.Id} as {status}",
			report.ResolutionNotes.IsNullOrEmpty() ? $"Reporter: {report.ReporterName}; Accused: {report.AccusedName}" : report.ResolutionNotes
		);

		SaveModerationData();
		PushModerationSnapshotToAdmins();
		PushTribunalSnapshot();
	}

	[ConCmd.Admin( Name = "ttt_ban", Help = "Ban the client with the following steam id." )]
	public static void BanPlayer( string rawSteamId, int minutes = default, string reason = "" )
	{
		if ( !long.TryParse( rawSteamId, out var steamId ) )
			return;

		var target = FindPlayerBySteamId( steamId );
		if ( target.IsValid() )
		{
			target.Client.Ban( minutes, reason );
		}
		else
		{
			BannedClients.RemoveAll( client => client.SteamId == steamId );
			BannedClients.Add( new BannedClient
			{
				SteamId = steamId,
				Duration = minutes == default ? DateTime.MaxValue : DateTime.Now.AddMinutes( minutes ),
				Reason = reason
			} );
		}

		AddModerationLog(
			"Admin Action",
			$"{ConsoleSystem.Caller?.Name ?? "Server Console"} banned {target?.SteamName ?? rawSteamId}",
			$"Duration: {(minutes == default ? "permanent" : $"{minutes} minutes")}; Reason: {reason}"
		);

		SaveModerationData();
		PushModerationSnapshotToAdmins();
		PushTribunalSnapshot();
	}

	[ConCmd.Admin( Name = "ttt_ban_remove", Help = "Remove the ban on a client using a steam id." )]
	public static void RemoveBanWithSteamId( string rawSteamId )
	{
		if ( !long.TryParse( rawSteamId, out var steamId ) )
			return;

		if ( BannedClients.RemoveAll( bannedClient => bannedClient.SteamId == steamId ) == 0 )
		{
			Log.Warning( $"Unable to find player with steam id {rawSteamId}" );
			return;
		}

		AddModerationLog( "Admin Action", $"{ConsoleSystem.Caller?.Name ?? "Server Console"} removed ban for {rawSteamId}" );
		SaveModerationData();
		PushModerationSnapshotToAdmins();
		PushTribunalSnapshot();
	}

	[ConCmd.Admin( Name = "ttt_kick", Help = "Kick the client with the following steam id." )]
	public static void KickPlayer( string rawSteamId )
	{
		if ( !long.TryParse( rawSteamId, out var steamId ) )
			return;

		foreach ( var client in Game.Clients )
		{
			if ( client.SteamId != steamId )
				continue;

			AddModerationLog( "Admin Action", $"{ConsoleSystem.Caller?.Name ?? "Server Console"} kicked {client.Name}" );
			SaveModerationData();
			client.Kick();
			PushModerationSnapshotToAdmins();
			PushTribunalSnapshot();
			return;
		}

		Log.Warning( $"Unable to find player with steam id {rawSteamId}" );
	}

	[TTTEvent.Player.TookDamage]
	private static void OnPlayerTookDamage( Player victim )
	{
		if ( !Game.IsServer || victim.LastAttacker is not Player attacker || attacker == victim )
			return;

		var tags = victim.LastDamage.Tags is null ? string.Empty : string.Join( ", ", victim.LastDamage.Tags );
		var weapon = victim.LastAttackerWeaponInfo?.Title ?? victim.LastDamage.Weapon?.ClassName ?? "Unknown";
		var friendlyFire = attacker.Team == victim.Team ? "yes" : "no";

		AddModerationLog(
			"Damage",
			$"{attacker.SteamName} damaged {victim.SteamName} for {victim.LastDamage.Damage:n0}",
			$"Weapon: {weapon}; Friendly fire: {friendlyFire}; Tags: {tags}; Position: {victim.LastDamage.Position}",
			attacker,
			victim
		);
	}

	[TTTEvent.Player.Killed]
	private static void OnPlayerKilled( Player victim )
	{
		if ( !Game.IsServer )
			return;

		var attacker = victim.LastAttacker as Player;
		var weapon = victim.LastAttackerWeaponInfo?.Title ?? victim.LastDamage.Weapon?.ClassName ?? "Unknown";
		var tags = victim.LastDamage.Tags is null ? string.Empty : string.Join( ", ", victim.LastDamage.Tags );
		var summary = attacker.IsValid()
			? $"{attacker.SteamName} killed {victim.SteamName}"
			: $"{victim.SteamName} died";

		var details = $"Weapon: {weapon}; Damage tags: {tags}";
		AddModerationLog( "Kill", summary, details, attacker, victim );
		SaveModerationData();
	}

	[TTTEvent.Player.CorpseFound]
	private static void OnCorpseFound( Player player )
	{
		if ( !Game.IsServer || !player.Corpse.IsValid() || !player.Corpse.Finder.IsValid() )
			return;

		AddModerationLog(
			"Corpse Found",
			$"{player.Corpse.Finder.SteamName} found {player.SteamName}'s corpse",
			$"Victim role: {player.Role.Title}",
			player.Corpse.Finder,
			player
		);
		SaveModerationData();
	}

	private static void EvaluateTribunalOutcome( RdmReport report, bool forceTimeoutResolution )
	{
		if ( report.TribunalEndsAt is null || report.Status is RdmReportStatus.Resolved or RdmReportStatus.Dismissed )
			return;

		var guiltyVotes = report.TribunalVotes.Count( vote => vote.IsGuiltyVote );
		var notGuiltyVotes = report.TribunalVotes.Count - guiltyVotes;
		var evaluation = TribunalRules.Evaluate( guiltyVotes, notGuiltyVotes, TribunalMinVotes, TribunalRequiredRatio, forceTimeoutResolution );
		if ( evaluation.Resolution == TribunalResolution.None )
			return;

		report.ClosedAt = DateTime.UtcNow;
		report.TribunalEndsAt = null;
		report.ResolutionNotes = evaluation.Notes;

		switch ( evaluation.Resolution )
		{
			case TribunalResolution.Guilty:
				report.TribunalVerdict = TribunalVerdict.Guilty;
				report.Status = RdmReportStatus.Resolved;
				break;
			case TribunalResolution.NotGuilty:
				report.TribunalVerdict = TribunalVerdict.NotGuilty;
				report.Status = RdmReportStatus.Dismissed;
				break;
			case TribunalResolution.NoConsensus:
			case TribunalResolution.InsufficientVotes:
				report.TribunalVerdict = TribunalVerdict.NoConsensus;
				report.Status = RdmReportStatus.Dismissed;
				break;
		}

		AddModerationLog( "Tribunal", $"Report #{report.Id} tribunal verdict: {report.TribunalVerdict}", report.ResolutionNotes );
	}

	[GameEvent.Tick.Server]
	private static void TickTribunalReports()
	{
		if ( !TribunalEnabled )
			return;

		var updated = false;
		foreach ( var report in RdmReports )
		{
			if ( report.TribunalEndsAt is null || report.TribunalEndsAt > DateTime.UtcNow )
				continue;

			EvaluateTribunalOutcome( report, true );
			updated = true;
		}

		if ( !updated )
			return;

		SaveModerationData();
		PushModerationSnapshotToAdmins();
		PushTribunalSnapshot();
	}
}
