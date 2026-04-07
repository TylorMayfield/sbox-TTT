using Sandbox;

namespace TTT;

#if DEBUG
// DebugBot requires the s&box Bot API which has changed significantly.
// Stubbed pending a full rewrite for the scene/component system.
public partial class DebugBot
{
	[ConCmd( "bot_add" )]
	private static void AddBot()
	{
		Log.Warning( "DebugBot: bot_add is not yet implemented in the scene/component system." );
	}

	[ConCmd( "bot_add_multiple" )]
	private static void AddMultipleBots( int count = 1 )
	{
		Log.Warning( "DebugBot: bot_add_multiple is not yet implemented in the scene/component system." );
	}
}
#endif
