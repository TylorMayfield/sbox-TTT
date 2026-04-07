using Sandbox;

namespace TTT;

/// <summary>
/// You should be inheriting all your custom traitor roles from this.
/// </summary>
[Category( "Roles" )]
[ClassName( "ttt_role_traitor" )]
[Title( "Traitor" )]
public class Traitor : Role
{
	protected override void OnSelect( Player player )
	{
		base.OnSelect( player );

		if ( !Networking.IsHost )
		{
			if ( Player.Local?.Team == Team )
				player.Components.Create<UI.RolePlate>();

			return;
		}

		foreach ( var otherPlayer in Utils.GetPlayersWhere( p => p != player ) )
		{
			if ( otherPlayer.Team == Team )
			{
				player.SendRole( otherPlayer.Network.Owner );
				otherPlayer.SendRole( player.Network.Owner );
			}

			if ( otherPlayer.IsMissingInAction )
				otherPlayer.UpdateStatus( player.Network.Owner );
		}
	}
}
