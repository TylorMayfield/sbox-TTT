using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TTT.UI;

public partial class InfoFeed : Panel
{
	public static InfoFeed Instance { get; private set; }

	public InfoFeed() => Instance = this;

	private const int MaxMessagesDisplayed = 5;
	private const float DisplayDuration = 2.5f;
	private readonly Queue<InfoFeedEntry> _entryQueue = new();
	private RealTimeUntil _timeUntilNextDisplay = 0f;
	private RealTimeSince _timeSinceLastDisplayed = 0f;
	private RealTimeUntil _timeUntilNextDelete = 0f;

	public void AddToFeed( InfoFeedEntry entry )
	{
		_entryQueue.Enqueue( entry );
	}

	public override void Tick()
	{
		if ( _entryQueue.Any() && _timeUntilNextDisplay && ChildrenCount < MaxMessagesDisplayed )
		{
			var newEntry = _entryQueue.Dequeue();
			AddChild( newEntry );

			_timeUntilNextDisplay = 1f;
			_timeSinceLastDisplayed = 0f;
		}

		if ( Children.Any() && _timeUntilNextDelete && _timeSinceLastDisplayed > DisplayDuration )
		{
			var oldEntry = Children.ElementAt( 0 );
			oldEntry.Delete();
			_timeUntilNextDelete = DisplayDuration;
		}
	}

	public static void AddEntry( string message )
	{
		Instance?.AddToFeed( new InfoFeedEntry( message ) );
	}

	public static void AddEntry( string message, Color color )
	{
		Instance?.AddToFeed( new InfoFeedEntry( message, color ) );
	}

	public static void AddEntry( Player player, string message )
	{
		Instance?.AddToFeed( new InfoFeedEntry( player, message ) );
	}

	public static void AddRoleEntry( RoleInfo roleInfo, string message )
	{
		Instance?.AddToFeed( new InfoFeedEntry( roleInfo, message ) );
	}

	public static void AddPlayerToPlayerEntry( Player left, Player right, string message, string suffix = "" )
	{
		Instance?.AddToFeed( new InfoFeedEntry( left, right, message, suffix ) );
	}

	[TTTEvent.Player.CorpseFound]
	private void OnCorpseFound( Player player )
	{
		AddPlayerToPlayerEntry
		(
			player.Corpse.Finder,
			player,
			"found the body of",
			$"({player.Role.Title})"
		);
	}

	[TTTEvent.Round.Start]
	private void OnRoundStart()
	{
		this.Enabled( true );

		if ( GameManager.Instance?.State.HasStarted == true )
			return;

		var player = Player.Local;
		if ( player is null )
			return;

		AddEntry( "Roles have been assigned and the round has begun..." );
		AddEntry( $"Traitors will receive an additional {GameManager.InProgressSecondsPerDeath} seconds per death." );

		if ( !Karma.Enabled )
			return;

		var karma = MathF.Round( player.BaseKarma );
		var damagePenalty = MathF.Round( 100f - player.DamageFactor * 100f );
		var speedPenalty = MathF.Round( 100f - player.KarmaSpeedScale * 100f );

		if ( damagePenalty <= 0 && speedPenalty <= 0 )
		{
			AddEntry( $"Your karma is {karma}, you'll deal full damage and move at full speed this round." );
			return;
		}

		AddEntry( $"Your karma is {karma}, you'll deal {damagePenalty}% reduced damage and move {speedPenalty}% slower this round." );
	}

	[TTTEvent.Round.End]
	private void OnRoundEnd( Team _, WinType _1 )
	{
		this.Enabled( false );
		DeleteChildren();
	}
}
