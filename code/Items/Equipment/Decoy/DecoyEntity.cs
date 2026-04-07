using Sandbox;

namespace TTT;

public sealed partial class DecoyEntity : Component, ICarriableHint
{
	public Player Planter { get; private set; }

	public void Initialize( Player planter )
	{
		Planter = planter;
	}

	protected override void OnDestroy()
	{
		Planter?.Components.RemoveAny<DecoyComponent>();
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

		player.Inventory.Add( new Decoy() );
		GameObject.Destroy();
	}
}
