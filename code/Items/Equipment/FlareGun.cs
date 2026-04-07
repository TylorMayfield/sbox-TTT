using Sandbox;

namespace TTT;

[Category( "Equipment" )]
[ClassName( "ttt_equipment_flaregun" )]
[Title( "Flare Gun" )]
public class FlareGun : Weapon
{
	public override string SlotText => AmmoClip.ToString();

	public override void SimulateAnimator( SkinnedModelRenderer renderer )
	{
		base.SimulateAnimator( renderer );
		renderer.Set( "holdtype_handedness", 1 ); // Right hand
	}

	protected override void OnHit( SceneTraceResult trace )
	{
		base.OnHit( trace );

		// TODO: Use proper burning once FP implements it.
		var burnDamage = new DamageInfo()
			.WithDamage( 25 )
			.WithAttacker( Owner.GameObject )
			.WithTag( DamageTags.Burn )
			.WithWeapon( GameObject );

		var hitPlayer = trace.GameObject?.Components.Get<Player>( FindMode.InAncestors );
		hitPlayer?.TakeDamage( burnDamage );

		var corpse = trace.GameObject?.Components.Get<Corpse>( FindMode.InSelf );
		corpse?.GameObject.Destroy();
	}
}
