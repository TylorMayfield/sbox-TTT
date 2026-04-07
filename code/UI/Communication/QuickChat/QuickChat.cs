using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace TTT.UI;

public partial class QuickChat : Panel
{
	public static QuickChat Instance { get; private set; }

	private const string NoTarget = "nobody";
	private string _target;
	private bool _isShowing = false;
	private RealTimeSince _timeWithNoTarget;
	private RealTimeSince _timeSinceLastMessage;
	private readonly List<Label> _labels = new();
	private static readonly List<string> _messages = new()
	{
		"I'm with {0}.",
		"I see {0}.",
		"Yes.",
		"No.",
		"{0} is a Traitor!",
		"{0} acts suspicious.",
		"{0} is Innocent.",
		"Help!",
		"Anyone still alive?"
	};

	public QuickChat() => Instance = this;

	protected override void OnAfterTreeRender( bool firstTime )
	{
		if ( !firstTime )
			return;

		_labels.Clear();
		var i = 0;

		foreach ( var child in Children )
		{
			var label = (Label)child.GetChild( 1 );

			if ( !label.HasClass( "message" ) )
				continue;

			_labels.Add( label );
			label.Text = _messages[i++];
		}
	}

	public override void Tick()
	{
		var player = Player.Local;
		if ( player is null )
			return;

		if ( Input.Pressed( InputAction.Zoom ) )
			_isShowing = !_isShowing;

		this.Enabled( player.IsAlive && _isShowing );

		var newTarget = GetTarget();

		if ( newTarget != NoTarget )
			_timeWithNoTarget = 0;
		else if ( _timeWithNoTarget <= 3 )
			return;

		if ( newTarget == _target )
			return;

		_target = newTarget;
		for ( var i = 0; i <= 6; i++ )
		{
			if ( i == 2 || i == 3 )
				continue;

			_labels[i].Text = string.Format( _messages[i], _target );

			if ( i < 4 )
				continue;

			if ( !ShouldCapitalize( _target ) )
				continue;

			_labels[i].Text = _labels[i].Text.FirstCharToUpper();
		}
	}

	public static string GetTarget()
	{
		var localPlayer = Player.Local;
		if ( localPlayer is null )
			return null;

		var trace = Game.ActiveScene?.Trace
			.Ray( localPlayer.EyePosition, localPlayer.EyePosition + localPlayer.EyeRotation.Forward * Player.MaxHintDistance )
			.UseHitboxes()
			.IgnoreGameObject( localPlayer.GameObject )
			.Run();

		if ( trace is null || !trace.Value.Hit || trace.Value.GameObject is null )
			return NoTarget;

		if ( trace.Value.GameObject.Components.TryGet<Corpse>( out var corpse ) )
		{
			return corpse.Player is null ? "an unidentified body" : $"{corpse.Player.SteamName}'s corpse";
		}

		if ( trace.Value.GameObject.Components.TryGet<Player>( out var player ) )
		{
			return player.CanHint( localPlayer ) ? player.SteamName : "someone in disguise";
		}

		return NoTarget;
	}

	private static bool ShouldCapitalize( string target )
	{
		return target == NoTarget || target == "an unidentified body" || target == "someone in disguise";
	}

	public override void OnButtonEvent( ButtonEvent e )
	{
		if ( !this.IsEnabled() )
			return;

		var keyboardIndexPressed = InventorySelection.GetKeyboardNumberPressed();

		if ( keyboardIndexPressed <= 0 )
			return;

		if ( _timeSinceLastMessage > 1 )
		{
			TextChat.SendChat( _labels[keyboardIndexPressed - 1].Text );
			_timeSinceLastMessage = 0;
		}

		_isShowing = false;
	}
}
