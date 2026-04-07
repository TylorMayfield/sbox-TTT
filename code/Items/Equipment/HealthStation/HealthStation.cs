namespace TTT;

[Category( "Equipment" )]
[ClassName( "ttt_equipment_healthstation" )]
[Title( "Health Station" )]
public class HealthStation : Deployable
{
	protected override bool CanPlant => false;

	protected override void OnDeploy()
	{
		var entity = Components.GetOrCreate<HealthStationEntity>();
		entity.Initialize( PreviousOwner );
	}
}
