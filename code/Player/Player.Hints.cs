using Sandbox.UI;

namespace TTT;

public partial class Player
{
	public const float MaxHintDistance = 5000f;

	private static Panel _currentHintPanel;
	private static ICarriableHint _currentHint;

	private void DisplayEntityHints()
	{
		var hovered = FindHoveredHint();

		if ( hovered is null || _traceDistance > MaxHintDistance || !hovered.CanHint( UI.Hud.DisplayedPlayer ) )
		{
			DeleteHint();
			return;
		}

		if ( hovered == _currentHint )
		{
			hovered.Tick( UI.Hud.DisplayedPlayer );
			return;
		}

		DeleteHint();

		_currentHintPanel = hovered.DisplayHint( UI.Hud.DisplayedPlayer );
		if ( _currentHintPanel is not null )
		{
			_currentHintPanel.Parent = UI.HintDisplay.Instance;
			_currentHintPanel.Enabled( true );
		}

		_currentHint = hovered;
	}

	private static void DeleteHint()
	{
		_currentHintPanel?.Delete( true );
		_currentHintPanel = null;
		UI.FullScreenHintMenu.Instance?.Close();
		_currentHint = null;
	}

	private ICarriableHint FindHoveredHint()
	{
		var hovered = FindHoveredGameObject();
		if ( hovered is null )
			return null;

		return hovered.Components.TryGet<ICarriableHint>( out var hint ) ? hint : null;
	}
}
