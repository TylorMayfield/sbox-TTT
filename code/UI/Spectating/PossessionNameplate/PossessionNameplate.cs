using Sandbox;
using Sandbox.UI;

namespace TTT.UI;

public partial class PossessionNameplate : Sandbox.UI.WorldPanel
{
	private readonly Prop _prop;

	public PossessionNameplate( Prop prop )
		: base( Game.ActiveScene.SceneWorld )
	{
		_prop = prop;
	}

	public override void Tick()
	{
		var tx = Transform;
		tx.Position = _prop.WorldPosition + Vector3.Up * 48f;
		tx.Rotation = (Game.ActiveScene?.Camera?.WorldRotation ?? Rotation.Identity).RotateAroundAxis( Vector3.Up, 180f );

		Transform = tx;
	}
}
