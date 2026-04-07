namespace TTT;

// WIP, not currently added to any shop.
[Category( "Equipment" )]
[ClassName( "ttt_equipment_visualizer" )]
[Title( "Visualizer" )]
public class Visualizer : Deployable
{
	protected override bool CanPlant => false;

	protected override void OnDeploy()
	{
		var entity = Components.GetOrCreate<VisualizerEntity>();
		entity.Initialize( PreviousOwner );
	}
}
