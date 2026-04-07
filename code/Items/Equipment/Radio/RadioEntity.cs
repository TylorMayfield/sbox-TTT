using Sandbox;
using System.Linq;

namespace TTT;

public sealed partial class RadioEntity : Component, ICarriableHint
{
	public Player Planter { get; private set; }

	public void Initialize( Player planter )
	{
		Planter = planter;
	}

	protected override void OnDestroy()
	{
		Planter?.Components.RemoveAny<RadioComponent>();
	}

	bool ICarriableHint.CanHint( Player player )
	{
		return player.IsAlive && (Planter is null || player == Planter);
	}

	void ICarriableHint.Tick( Player player )
	{
		if ( !Networking.IsHost )
			return;

		if ( !Input.Down( InputAction.Use ) )
			return;

		player.Inventory.Add( new Radio() );
		GameObject.Destroy();
	}

	[ConCmd( "ttt_radio_play" )]
	public static void PlayRadioCmd( int goId, string sound )
	{
		if ( !Networking.IsHost )
			return;

		var player = Utils.GetPlayersWhere( p => p.Network.Owner == Rpc.Caller ).FirstOrDefault();
		if ( player is null )
			return;

		var radio = Game.ActiveScene?.GetAllComponents<RadioEntity>()
			.FirstOrDefault( r => r.GameObject.Id.GetHashCode() == goId && r.Planter == player );

		if ( radio is null )
			return;

		Sound.Play( sound, radio.WorldPosition );
	}
}
