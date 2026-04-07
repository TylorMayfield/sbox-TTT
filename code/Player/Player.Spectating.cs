using Sandbox;

namespace TTT;

public partial class Player
{
	public bool IsForcedSpectator => Network.Owner?.GetValue( "forced_spectator", false ) == true;
	public bool IsSpectator => Status == PlayerStatus.Spectator;

	public void MakeSpectator()
	{
		CharController.Enabled = false;
		Renderer.Enabled = false;
		Health = 0f;
		Status = PlayerStatus.Dead;
	}
}
