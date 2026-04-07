using System;
using Sandbox;
using Sandbox.UI;

namespace TTT.UI;

public partial class C4DefuseMenu : Panel
{
	private readonly C4Entity _c4;
	private string _timer;
	private bool _isOwner => Player.Local == _c4.Planter;

	public C4DefuseMenu( C4Entity c4 ) => _c4 = c4;

	public override void Tick()
	{
		if ( _c4.IsArmed )
			_timer = TimeSpan.FromSeconds( Math.Max( 0f, (float)_c4.TimeUntilExplode ) ).ToString( "mm':'ss" );
	}

	public void Pickup() => C4Entity.PickupCmd( _c4.GameObject.Id.GetHashCode() );
	public void Destroy() => C4Entity.DeleteC4Cmd( _c4.GameObject.Id.GetHashCode() );

	protected override int BuildHash() => HashCode.Combine( _timer, _c4.IsArmed, _isOwner );
}
