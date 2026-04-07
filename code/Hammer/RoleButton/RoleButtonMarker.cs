using Sandbox;
using Sandbox.UI;
using System;

namespace TTT.UI;

public partial class RoleButtonMarker : Panel
{
	private const int CenterPercent = 50;
	private readonly float _minViewDistance = 512;
	private readonly float _maxViewDistance = 1024;
	private readonly float _focusSize = 2f;

	private readonly RoleButton _roleButton;
	private readonly RoleInfo _roleInfo;
	private Vector3 _screenPos;

	public RoleButtonMarker( RoleButton roleButton )
	{
		_roleButton = roleButton;
		_roleInfo = GameResource.GetInfo<RoleInfo>( _roleButton.RoleName );
		_maxViewDistance = roleButton.Radius;
		_minViewDistance = Math.Min( _minViewDistance, _maxViewDistance / 2 );
	}

	public override void Tick()
	{
		var player = Player.Local;
		if ( player is null )
			return;

		if ( !_roleButton.IsValid() )
		{
			Delete();
			return;
		}

		if ( !this.IsEnabled() )
		{
			if ( Player.FocusedButton == _roleButton )
				Player.FocusedButton = null;
			return;
		}

		_screenPos = _roleButton.WorldPosition.ToScreen();
		Style.Opacity = Math.Clamp( 1f - (player.WorldPosition.Distance( _roleButton.WorldPosition ) - _minViewDistance) / (_maxViewDistance - _minViewDistance), 0f, 1f );

		if ( IsLookingAtRoleButton() && player.WorldPosition.Distance( _roleButton.WorldPosition ) <= _maxViewDistance )
			Player.FocusedButton ??= _roleButton;
		else if ( Player.FocusedButton == _roleButton )
			Player.FocusedButton = null;
	}

	public bool IsLookingAtRoleButton()
	{
		if ( Style.Left is null || Style.Top is null )
			return false;

		return Style.Left.Value.Value > CenterPercent - _focusSize && Style.Left.Value.Value < CenterPercent + _focusSize
			&& Style.Top.Value.Value > CenterPercent - _focusSize * Screen.Aspect && Style.Top.Value.Value < CenterPercent + _focusSize * Screen.Aspect;
	}

	protected override int BuildHash() => HashCode.Combine( _screenPos, _roleButton.IsDisabled );
}
