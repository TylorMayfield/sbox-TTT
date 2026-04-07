using Sandbox;

namespace TTT;

/// <summary>
/// A scene component that can be placed in maps to wire up TTT round events.
/// Replaces the old Hammer entity approach.
/// </summary>
public class MapSettings : Component
{
	protected override void OnStart()
	{
		// Wire up round events when the scene starts
		if ( Networking.IsHost )
			FireSettingsSpawn();
	}

	public void FireSettingsSpawn()
	{
		// No-op in new API - events are handled via TTTEvent system
	}
}
