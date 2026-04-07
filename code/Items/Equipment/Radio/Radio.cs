using Sandbox;

namespace TTT;

[Category( "Equipment" )]
[ClassName( "ttt_equipment_radio" )]
[Title( "Radio" )]
public class Radio : Deployable
{
	protected override void OnDeploy()
	{
		var entity = Components.GetOrCreate<RadioEntity>();
		entity.Initialize( PreviousOwner );

		var radioComponent = PreviousOwner.Components.GetOrCreate<RadioComponent>();
		radioComponent.Radio = entity;
	}
}

public partial class RadioComponent : Component
{
	public RadioEntity Radio { get; set; }

	protected override void OnStart()
	{
		if ( Player.Local == Components.Get<Player>( FindMode.InSelf ) )
			UI.InfoFeed.AddEntry( "Radio deployed, access it using the Role Menu." );
	}

	protected override void OnDestroy()
	{
		var player = Components.Get<Player>( FindMode.InSelf );
		if ( Player.Local == player && player?.Inventory.Find<Radio>() is null )
			UI.InfoFeed.AddEntry( "Your radio has been destroyed." );
	}
}
