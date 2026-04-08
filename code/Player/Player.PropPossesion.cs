using Sandbox;
using System.Linq;

namespace TTT;

public partial class Player
{
	public Prop Prop { get; private set; }

	public void SimulatePossession()
	{
		if ( Input.Pressed( InputAction.Duck ) )
		{
			CancelPossession();
			return;
		}

		if ( Input.Pressed( InputAction.Jump ) || InputDirection.x != 0f || InputDirection.y != 0f )
			Prop?.Components.Get<PropPossession>()?.Punch();
	}

	public void CancelPossession()
	{
		if ( !Networking.IsHost )
			return;

		if ( Prop.IsValid() )
		{
			Prop.Components.RemoveAny<PropPossession>();
		}

		Prop = null;
	}

	[ConCmd( "ttt_possess_prop" )]
	public static void PossessCmd( int goIdHash )
	{
		if ( !Networking.IsHost )
			return;

		var player = GameManager.Instance?.FindPlayerByConnection( Rpc.Caller );
		if ( player is null || player.IsAlive || player.Prop.IsValid() )
			return;

		var prop = Game.ActiveScene?.GetAllComponents<Prop>()
			.FirstOrDefault( p => p.GameObject.Id.GetHashCode() == goIdHash );

		if ( prop is null || !prop.IsValid() )
			return;

		prop.Components.GetOrCreate<PropPossession>();
		player.Prop = prop;
	}
}
