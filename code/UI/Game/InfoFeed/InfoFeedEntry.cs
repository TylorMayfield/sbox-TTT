using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TTT.UI;

public partial class InfoFeedEntry : Panel
{
	public InfoFeedEntry( string message )
	{
		Add.Label( message );
	}

	public InfoFeedEntry( string message, Color color )
	{
		Add.Label( message ).Style.FontColor = color;
	}

	public InfoFeedEntry( Player player, string message )
	{
		Add.Label( Player.Local == player ? "You" : player.SteamName, "entry" ).Style.FontColor = !player.IsRoleKnown ? Color.White : player.Role.Color;
		Add.Label( message );
	}

	public InfoFeedEntry( RoleInfo roleInfo, string message )
	{
		Add.Label( $"{roleInfo.Title}s", "entry" ).Style.FontColor = roleInfo.Color;
		Add.Label( message );
	}

	public InfoFeedEntry( Player left, Player right, string message, string suffix = "" )
	{
		Add.Label( Player.Local == left ? "You" : left.SteamName, "entry" ).Style.FontColor = !left.IsRoleKnown ? Color.White : left.Role.Color;
		Add.Label( message );
		Add.Label( Player.Local == right ? "You" : right.SteamName, "entry" ).Style.FontColor = !right.IsRoleKnown ? Color.White : right.Role.Color;

		if ( !suffix.IsNullOrEmpty() )
			Add.Label( suffix );
	}
}
