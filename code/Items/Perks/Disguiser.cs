using Sandbox;
using System.Threading.Tasks;

namespace TTT;

[Category( "Perks" )]
[ClassName( "ttt_perk_disguiser" )]
[Title( "Disguiser" )]
public partial class Disguiser : Perk
{
	public bool IsActive { get; set; } = false;

	public override string SlotText => IsActive ? "ON" : "OFF";
	private readonly float _lockOutSeconds = 1f;
	private bool _isLocked = false;

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		if ( Input.Down( InputAction.Grenade ) && !_isLocked )
		{
			if ( Networking.IsHost )
			{
				IsActive = !IsActive;
				_isLocked = true;
			}

			_ = DisguiserLockout();
		}
	}

	private async Task DisguiserLockout()
	{
		await GameTask.DelaySeconds( _lockOutSeconds );
		_isLocked = false;
	}
}
