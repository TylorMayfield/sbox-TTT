using Sandbox;

namespace TTT;

public sealed class RoleButton : Component
{
	[Property]
	public string RoleName { get; set; } = "Traitor";

	[Property]
	public string Description { get; set; }

	[Property]
	public int Radius { get; set; } = 100;

	[Property]
	public int Delay { get; set; } = 1;

	[Property]
	public bool RemoveOnPress { get; set; } = false;

	[Sync] public bool Locked { get; set; } = false;
	[Sync] public TimeUntil NextUse { get; private set; }
	[Sync] public bool IsRemoved { get; private set; }

	public bool IsDisabled => !NextUse || Locked || IsRemoved;

	public bool CanUse( Player player )
	{
		if ( IsDisabled )
			return false;

		return RoleName == "All" || player.Role == RoleName;
	}

	public void Press( Player player )
	{
		if ( !CanUse( player ) )
			return;

		if ( RemoveOnPress )
		{
			IsRemoved = true;
			return;
		}

		NextUse = Delay;
	}

	public void Lock() => Locked = true;
	public void Unlock() => Locked = false;
	public void Toggle() => Locked = !Locked;
}
