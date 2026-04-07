using Sandbox;
using System.Threading.Tasks;

namespace TTT;

/// <summary>
/// If this component is present on a prop, any "PhysicsImpact" damage dealt to a player will be ignored.
/// Added briefly after a prop is thrown to prevent self-damage from the toss.
/// </summary>
public sealed class IgnoreDamage : Component
{
	protected override void OnStart()
	{
		if ( !Networking.IsHost )
			return;

		Tags.Add( DamageTags.IgnoreDamage );
		_ = RemoveAfterDelay();
	}

	protected override void OnDestroy()
	{
		if ( Networking.IsHost )
			Tags.Remove( DamageTags.IgnoreDamage );
	}

	private async Task RemoveAfterDelay()
	{
		await GameTask.DelaySeconds( 0.5f );

		if ( IsValid )
			Components.RemoveAny<IgnoreDamage>();
	}
}
