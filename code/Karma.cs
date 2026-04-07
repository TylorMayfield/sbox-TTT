using Sandbox;
using System;
using System.Collections.Generic;

namespace TTT;

public static class Karma
{
	[ConVar( "ttt_karma_enabled" )]
	public static bool Enabled { get; set; } = true;

	[ConVar( "ttt_karma_low_autokick" )]
	public static bool LowAutoKick { get; set; } = true;

	[ConVar( "ttt_karma_start" )]
	public static int StartValue { get; set; } = 1000;

	[ConVar( "ttt_karma_max" )]
	public static int MaxValue { get; set; } = 1100;

	[ConVar( "ttt_karma_min" )]
	public static int MinValue { get; set; } = 500;

	[ConVar( "ttt_karma_min_speed_scale" )]
	public static float MinSpeedScale { get; set; } = 0.85f;

	public static Dictionary<ulong, float> SavedPlayerValues { get; private set; } = new();

	public const float CleanBonus = 30;
	public const float FallOff = 0.25f;
	public const float RoundHeal = 5;

	public static float GetHurtReward( float damage, float multiplier )
	{
		return MaxValue * Math.Clamp( damage * multiplier, 0, 1 );
	}

	public static float GetHurtPenalty( float victimKarma, float damage, float multiplier )
	{
		return victimKarma * Math.Clamp( damage * multiplier, 0, 1 );
	}

	public static float GetKillReward( float multiplier )
	{
		return MaxValue * Math.Clamp( multiplier, 0, 1 );
	}

	public static float GetKillPenalty( float victimKarma, float multiplier )
	{
		return victimKarma * Math.Clamp( multiplier, 0, 1 );
	}

	private static void GivePenalty( Player player, float penalty )
	{
		player.ActiveKarma = Math.Max( player.ActiveKarma - penalty, 0 );
		player.TimeUntilClean = Math.Min( Math.Max( player.TimeUntilClean * penalty * 0.2f, penalty ), float.MaxValue );
	}

	private static void GiveReward( Player player, float reward )
	{
		reward = DecayMultiplier( player ) * reward;
		player.ActiveKarma = Math.Min( player.ActiveKarma + reward, MaxValue );
	}

	private static float DecayMultiplier( Player player )
	{
		if ( FallOff <= 0 || player.ActiveKarma < StartValue )
			return 1;

		if ( player.ActiveKarma >= MaxValue )
			return 1;

		var baseDiff = MaxValue - StartValue;
		var plyDiff = player.ActiveKarma - StartValue;
		var half = Math.Clamp( FallOff, 0.1f, 0.99f );

		return MathF.Exp( -0.69314718f / (baseDiff * half) * plyDiff );
	}

	public static float GetDamageFactor( float baseKarma )
	{
		return KarmaRules.GetDamageFactor( Enabled, baseKarma, StartValue );
	}

	public static float GetSpeedFactor( float damageFactor )
	{
		return KarmaRules.GetSpeedFactor( Enabled, damageFactor, MinSpeedScale );
	}

	public static void ApplyRoundModifiers( Player player )
	{
		if ( !player.IsValid() )
			return;

		player.DamageFactor = GetDamageFactor( player.BaseKarma );
		player.KarmaSpeedScale = GetSpeedFactor( player.DamageFactor );
	}

	[TTTEvent.Player.Spawned]
	private static void Apply( Player player )
	{
		if ( GameManager.Instance?.State is not PreRound )
			return;

		player.TimeUntilClean = 0;
		ApplyRoundModifiers( player );
	}

	[TTTEvent.Player.TookDamage]
	private static void OnPlayerTookDamage( Player player )
	{
		if ( !Networking.IsHost )
			return;

		if ( GameManager.Instance?.State is not InProgress )
			return;

		var attacker = player.LastAttacker?.Components.Get<Player>( FindMode.InSelf );

		if ( !attacker.IsValid() || !player.IsValid() )
			return;

		if ( attacker == player )
			return;

		var damage = player.LastDamage.Damage;

		if ( attacker.Team == player.Team )
		{
			if ( !player.TimeUntilClean )
				return;

			if ( player.LastDamage.IsAvoidable() )
				return;

			var penalty = GetHurtPenalty( player.ActiveKarma, damage, attacker.Role.Karma.TeamHurtPenaltyMultiplier );
			GivePenalty( attacker, penalty );
		}
		else
		{
			var reward = GetHurtReward( damage, player.Role.Karma.AttackerHurtRewardMultiplier );
			GiveReward( attacker, reward );
		}
	}

	[TTTEvent.Player.Killed]
	private static void OnPlayerKilled( Player player )
	{
		if ( !Networking.IsHost )
			return;

		if ( GameManager.Instance?.State is not InProgress )
			return;

		var attacker = player.LastAttacker?.Components.Get<Player>( FindMode.InSelf );

		if ( !attacker.IsValid() || !player.IsValid() )
			return;

		if ( attacker == player )
			return;

		if ( attacker.Team == player.Team )
		{
			if ( !player.TimeUntilClean )
				return;

			if ( player.LastDamage.IsAvoidable() )
				return;

			var penalty = GetKillPenalty( player.ActiveKarma, attacker.Role.Karma.TeamKillPenaltyMultiplier );
			GivePenalty( attacker, penalty );
		}
		else
		{
			var reward = GetKillReward( player.Role.Karma.AttackerKillRewardMultiplier );
			GiveReward( attacker, reward );
		}
	}

	private static void RoundIncrement( Player player )
	{
		if ( (!player.IsAlive && !player.KilledByPlayer) || player.IsSpectator )
			return;

		var reward = RoundHeal;

		if ( player.TimeUntilClean )
			reward += CleanBonus;

		GiveReward( player, reward );
	}

	private static bool CheckAutoKick( Player player )
	{
		return LowAutoKick && player.BaseKarma < MinValue;
	}

	[TTTEvent.Round.End]
	private static void OnRoundEnd( Team winningTeam, WinType winType )
	{
		if ( !Networking.IsHost )
			return;

		foreach ( var player in Utils.GetPlayersWhere( _ => true ) )
		{
			RoundIncrement( player );
			Rebase( player );

			if ( Enabled && CheckAutoKick( player ) )
				player.Network.Owner?.Kick();
		}
	}

	private static void Rebase( Player player )
	{
		player.BaseKarma = player.ActiveKarma;
	}

	public static void SaveKarma( Player player )
	{
		if ( player.IsValid() )
			SavedPlayerValues[player.SteamId] = player.ActiveKarma;
	}
}
