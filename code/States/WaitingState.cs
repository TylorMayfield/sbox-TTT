using Sandbox;

namespace TTT;

public class WaitingState : BaseState
{
	public override string Name { get; } = "Waiting";

	public override void OnSecond()
	{
		if ( !Networking.IsHost )
			return;

		if ( GameManager.Instance.TimeUntilMapSwitch )
			GameManager.Instance.ForceStateChange( new MapSelectionState() );
		else if ( Utils.GetPlayersWhere( p => !p.IsForcedSpectator ).Count >= GameManager.MinPlayers )
			GameManager.Instance.ForceStateChange( new PreRound() );
	}

	public override void OnPlayerJoin( Player player )
	{
		base.OnPlayerJoin( player );

		player.Respawn();
	}

	public override void OnPlayerKilled( Player player )
	{
		base.OnPlayerKilled( player );

		StartRespawnTimer( player );
	}

	protected override void OnStart()
	{
		if ( GameManager.Instance.TotalRoundsPlayed != 0 )
			MapHandler.Cleanup();

		if ( !Networking.IsHost )
			return;

		foreach ( var player in Utils.GetPlayersWhere( _ => true ) )
			player.Respawn();
	}
}
