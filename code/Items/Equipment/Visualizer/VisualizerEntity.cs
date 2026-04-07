using Sandbox;

namespace TTT;

public sealed partial class VisualizerEntity : Component, ICarriableHint
{
	public Player Planter { get; private set; }

	public void Initialize( Player planter )
	{
		Planter = planter;
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

		player.Inventory.Add( new Visualizer() );
		GameObject.Destroy();
	}
}
