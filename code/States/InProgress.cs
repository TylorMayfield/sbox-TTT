using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace TTT;

public partial class InProgress : BaseState
{
	public List<Player> AlivePlayers { get; set; }
	public List<Player> Spectators { get; set; }

	/// <summary>
	/// Fake timer shown to Innocents. The real timer increments with each death.
	/// </summary>
	public TimeUntil FakeTime { get; private set; }
	public string FakeTimeFormatted => FakeTime.Relative.TimerFormat();

	public override string Name { get; } = "In Progress";
	public override int Duration => GameManager.InProgressTime;

	private int _innocentTeamDeathCount = 0;

	public override void OnPlayerKilled( Player player )
	{
		base.OnPlayerKilled( player );

		TimeLeft += GameManager.InProgressSecondsPerDeath;

		if ( player.Team == Team.Innocents )
			_innocentTeamDeathCount += 1;

		var playerCount = Team.Innocents.GetCount();
		if ( playerCount > 0 )
		{
			var percentDead = (float)_innocentTeamDeathCount / playerCount;
			if ( percentDead >= GameManager.CreditsAwardPercentage )
			{
				GivePlayersCredits<Traitor>( GameManager.CreditsAwarded );
				_innocentTeamDeathCount = 0;
			}
		}

		if ( player.Role is Traitor )
			GivePlayersCredits<Detective>( GameManager.DetectiveTraitorDeathReward );
		else if ( player.Role is Detective && player.LastAttacker is Player attacker && attacker.IsAlive && attacker.Team == Team.Traitors )
			GiveTraitorCredits( attacker );

		AlivePlayers.Remove( player );
		Spectators.Add( player );

		player.UpdateMissingInAction();
	}

	public override void OnPlayerJoin( Player player )
	{
		base.OnPlayerJoin( player );

		player.Status = PlayerStatus.Spectator;
		player.UpdateStatus();

		Spectators.Add( player );
	}

	public override void OnPlayerLeave( Player player )
	{
		base.OnPlayerLeave( player );

		AlivePlayers.Remove( player );
		Spectators.Remove( player );
	}

	protected override void OnStart()
	{
		Event.Run( TTTEvent.Round.Start );

		if ( !Networking.IsHost )
			return;

		FakeTime = TimeLeft;

		MapHandler.CountMapWeapons();

		// If the map isn't armed for TTT, give players a fixed loadout.
		if ( MapHandler.WeaponCount == 0 )
		{
			foreach ( var player in AlivePlayers )
				GiveFixedLoadout( player );
		}

		// Clean up corpses from last round
		foreach ( var corpse in Game.ActiveScene.GetAllComponents<Corpse>() )
			corpse.GameObject.Destroy();
	}

	private static void GiveFixedLoadout( Player player )
	{
		if ( player.Inventory.Add( new MP5() ) )
			player.GiveAmmo( AmmoType.PistolSMG, 120 );

		if ( player.Inventory.Add( new Revolver() ) )
			player.GiveAmmo( AmmoType.Magnum, 20 );
	}

	public override void OnSecond()
	{
		if ( !Networking.IsHost )
			return;

#if DEBUG
		if ( GameManager.PreventWin )
		{
			TimeLeft += 1f;
			return;
		}
#endif

		var result = CheckForElimination();
		if ( result != Team.None )
		{
			PostRound.Load( result, WinType.Elimination );
			return;
		}

		if ( TimeLeft )
			OnTimeUp();
	}

	protected override void OnTimeUp()
	{
		PostRound.Load( Team.Innocents, WinType.TimeUp );
	}

	private Team CheckForElimination()
	{
		var aliveTeams = new HashSet<Team>();
		foreach ( var player in AlivePlayers )
			aliveTeams.Add( player.Team );

		return aliveTeams.Count == 0 ? Team.Traitors : aliveTeams.Count == 1 ? aliveTeams.First() : Team.None;
	}

	private static void GivePlayersCredits<T>( int credits ) where T : Role
	{
		foreach ( var player in Utils.GetPlayersWhere( p => p.IsAlive && p.Role is T ) )
		{
			player.Credits += credits;
			UI.InfoFeed.AddRoleEntry(
				player,
				GameResource.GetInfo<RoleInfo>( typeof( T ) ),
				$"You have been awarded {credits} credits for your performance."
			);
		}
	}

	private static void GiveTraitorCredits( Player traitor )
	{
		traitor.Credits += GameManager.TraitorDetectiveKillReward;
		UI.InfoFeed.AddEntry( traitor, $"have received {GameManager.TraitorDetectiveKillReward} credits for killing a Detective" );
	}
}
