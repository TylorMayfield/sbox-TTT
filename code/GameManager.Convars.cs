using Sandbox;
using System;
using System.Linq;

namespace TTT;

public partial class GameManager
{
#if DEBUG
	#region Debug
	[ConVar( "ttt_round_debug" )]
	public static bool PreventWin { get; set; }
	#endregion
#endif

	#region Round
	[ConVar( "ttt_preround_time" )]
	public static int PreRoundTime { get; set; } = 20;

	[ConVar( "ttt_inprogress_time" )]
	public static int InProgressTime { get; set; } = 300;

	[ConVar( "ttt_inprogress_secs_per_death" )]
	public static int InProgressSecondsPerDeath { get; set; } = 15;

	[ConVar( "ttt_postround_time" )]
	public static int PostRoundTime { get; set; } = 15;

	[ConVar( "ttt_mapselection_time" )]
	public static int MapSelectionTime { get; set; } = 15;
	#endregion

	#region Map
	[ConVar( "ttt_default_map" )]
	public static string DefaultMap { get; set; } = "facepunch.flatgrass";

	[ConVar( "ttt_rtv_threshold" )]
	public static float RTVThreshold { get; set; } = 0.66f;

	[ConVar( "ttt_round_limit" )]
	public static int RoundLimit { get; set; } = 6;

	[ConVar( "ttt_time_limit" )]
	public static int TimeLimit { get; set; } = 4500;
	#endregion

	#region Minimum Players
	[ConVar( "ttt_min_players" )]
	public static int MinPlayers { get; set; } = 2;
	#endregion

	#region Movement
	[ConVar( "ttt_bhop_enabled" )]
	public static bool BhopEnabled { get; set; } = true;

	[ConVar( "ttt_bhop_autojump" )]
	public static bool BhopAutoJump { get; set; } = true;

	[ConVar( "ttt_bhop_air_acceleration" )]
	public static float BhopAirAcceleration { get; set; } = 85f;

	[ConVar( "ttt_bhop_air_control" )]
	public static float BhopAirControl { get; set; } = 30f;

	[ConVar( "ttt_bhop_ground_friction" )]
	public static float BhopGroundFriction { get; set; } = 4.5f;

	[ConVar( "ttt_bhop_speed_cap_multiplier" )]
	public static float BhopSpeedCapMultiplier { get; set; } = 0f;
	#endregion

	#region AFK Timers
	[ConVar( "ttt_afk_timer" )]
	public static int AFKTimer { get; set; } = 180;

	[ConVar( "ttt_afk_auto_kick" )]
	public static bool AfkAutoKick { get; set; } = false;

	[ConVar( "ttt_afk_fun_death" )]
	public static bool AfkFunDeath { get; set; } = true;

	[ConVar( "ttt_afk_kick_delay" )]
	public static float AfkKickDelay { get; set; } = 1.5f;
	#endregion

	#region Credits
	[ConVar( "ttt_credits_award_pct" )]
	public static float CreditsAwardPercentage { get; set; } = 0.35f;

	[ConVar( "ttt_credits_award_size" )]
	public static int CreditsAwarded { get; set; } = 100;

	[ConVar( "ttt_credits_traitordeath" )]
	public static int DetectiveTraitorDeathReward { get; set; } = 100;

	[ConVar( "ttt_credits_detectivekill" )]
	public static int TraitorDetectiveKillReward { get; set; } = 100;
	#endregion

	#region Voice Chat
	[ConVar( "ttt_proximity_chat" )]
	public static bool ProximityChat { get; set; }
	#endregion

	#region Corpse Interaction
	[ConVar( "ttt_hang_body_roles" )]
	public static string HangBodyRoles { get; set; } = "traitor,detective,innocent";

	public static bool CanHangBodies( Role role )
	{
		if ( role is null )
			return false;

		var configuredRoles = HangBodyRoles?
			.Split( ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries )
			.Select( value => value.ToLowerInvariant() )
			.ToHashSet();

		if ( configuredRoles is null || configuredRoles.Count == 0 )
			return false;

		if ( configuredRoles.Contains( "*" ) || configuredRoles.Contains( "all" ) )
			return true;

		return configuredRoles.Contains( role.Title.ToLowerInvariant() )
			|| configuredRoles.Contains( role.Info.ClassName.ToLowerInvariant() )
			|| configuredRoles.Contains( role.GetType().Name.ToLowerInvariant() );
	}
	#endregion

	#region Tribunal
	[ConVar( "ttt_tribunal_enabled" )]
	public static bool TribunalEnabled { get; set; } = true;

	[ConVar( "ttt_tribunal_vote_seconds" )]
	public static int TribunalVoteSeconds { get; set; } = 90;

	[ConVar( "ttt_tribunal_min_votes" )]
	public static int TribunalMinVotes { get; set; } = 3;

	[ConVar( "ttt_tribunal_required_ratio" )]
	public static float TribunalRequiredRatio { get; set; } = 0.6f;
	#endregion

	#region RDM Punishments
	[ConVar( "ttt_rdm_guilty_punishments" )]
	public static string RdmGuiltyPunishments { get; set; } = "slay,half_damage";

	[ConVar( "ttt_rdm_slay_rounds" )]
	public static int RdmSlayRounds { get; set; } = 1;

	[ConVar( "ttt_rdm_damage_scale" )]
	public static float RdmDamageScale { get; set; } = 0.5f;

	[ConVar( "ttt_rdm_damage_rounds" )]
	public static int RdmDamageRounds { get; set; } = 1;

	[ConVar( "ttt_rdm_ban_minutes" )]
	public static int RdmBanMinutes { get; set; } = 60;
	#endregion
}
