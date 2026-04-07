using Sandbox.UI;

namespace TTT;

public partial class Player : ICarriableHint
{
	public float HintDistance => MaxHintDistance;
	public bool ShowGlow => false;

	public bool CanHint( Player player )
	{
		var disguiser = Perks.Find<Disguiser>();
		return !disguiser?.IsActive ?? true;
	}

	Panel ICarriableHint.DisplayHint( Player player )
	{
		return new UI.Nameplate( this );
	}

	void ICarriableHint.Tick( Player player ) { }
}
