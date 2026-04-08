using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace TTT;

public partial class Player
{
	public static List<RoleButton> RoleButtons { get; set; } = new();
	public static List<UI.RoleButtonMarker> RoleButtonMarkers { get; set; } = new();
	public static RoleButton FocusedButton { get; set; }

	public void ClearButtons()
	{
		foreach ( var marker in RoleButtonMarkers )
			marker.Delete( true );

		RoleButtons.Clear();
		RoleButtonMarkers.Clear();
		FocusedButton = null;
	}

	[ConCmd( "ttt_activate_role_button" )]
	public static void ActivateRoleButtonCmd( int goId )
	{
		if ( !Networking.IsHost )
			return;

		var player = GameManager.Instance?.FindPlayerByConnection( Rpc.Caller );
		if ( player is null )
			return;

		// Find the role button by GameObject network ID
		var button = Game.ActiveScene?.GetAllComponents<RoleButton>()
			.FirstOrDefault( b => b.GameObject.Id.GetHashCode() == goId );

		if ( button is null )
			return;

		if ( button.CanUse( player ) )
			button.Press( player );
	}

	public void ActivateRoleButton()
	{
		if ( FocusedButton is null || !Input.Pressed( InputAction.Use ) )
			return;

		ActivateRoleButtonCmd( FocusedButton.GameObject.Id.GetHashCode() );
	}
}
