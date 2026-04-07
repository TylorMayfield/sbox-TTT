using Sandbox;

namespace TTT;

public abstract class Perk : Component
{
	public virtual string SlotText => string.Empty;
	public PerkInfo Info { get; private set; }

	protected override void OnStart()
	{
		Info = GameResource.GetInfo<PerkInfo>( GetType() );
	}
}
