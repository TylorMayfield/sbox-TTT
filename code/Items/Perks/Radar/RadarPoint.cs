using System;
using Sandbox.UI;

namespace TTT.UI;

public partial class RadarPoint : Sandbox.UI.WorldPanel
{
	private readonly RadarPointData _radarData;
	private string _distance;
	private Vector3 _screenPos;

	public RadarPoint( RadarPointData data )
		: base( Sandbox.Game.ActiveScene.SceneWorld )
	{
		if ( WorldPoints.Instance is null )
			return;

		_radarData = data;
		WorldPoints.Instance.AddChild( this );
	}

	protected override int BuildHash() => HashCode.Combine( _distance, _screenPos );
}
