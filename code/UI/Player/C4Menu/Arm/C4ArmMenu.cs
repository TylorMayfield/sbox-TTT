using System;
using Sandbox.UI;

namespace TTT.UI;

public partial class C4ArmMenu : Panel
{
	public int Timer { get; set; } = 45;

	private readonly C4Entity _c4;

	public C4ArmMenu( C4Entity c4 ) => _c4 = c4;

	public void Arm() => C4Entity.ArmC4Cmd( _c4.GameObject.Id.GetHashCode(), Timer );
	public void Pickup() => C4Entity.PickupCmd( _c4.GameObject.Id.GetHashCode() );
	public void Destroy() => C4Entity.DeleteC4Cmd( _c4.GameObject.Id.GetHashCode() );
	protected override int BuildHash() => HashCode.Combine( Timer );
}
