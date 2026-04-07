using Sandbox;
using System;

namespace TTT;

public static class ClientExtensions
{
	public static void Ban( this Connection client, int minutes = default, string reason = "" )
	{
		client.Kick( reason );
		GameManager.BannedClients.Add
		(
			new BannedClient
			{
				SteamId = client.SteamId,
				Duration = minutes == default ? DateTime.MaxValue : DateTime.Now.AddMinutes( minutes ),
				Reason = reason
			}
		);
	}

	public static bool HasRockedTheVote( this Connection client )
	{
		return client.GetValue<bool>( "!rtv" );
	}
}
