using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace TTT;

public static class Utils
{
	public static List<Player> GetPlayersWhere( Func<Player, bool> predicate )
	{
		if ( Game.ActiveScene is null )
			return new();

		return Game.ActiveScene.GetAllComponents<Player>()
			.Where( predicate )
			.ToList();
	}

	public static List<Connection> GetClientsWhere( Func<Player, bool> predicate )
	{
		return GetPlayersWhere( predicate )
			.Select( p => p.Network.Owner )
			.Where( c => c != null )
			.ToList();
	}

	public static async void DelayAction( float seconds, Action callback )
	{
		await GameTask.DelaySeconds( seconds );
		callback?.Invoke();
	}

	public static byte[] Serialize<T>( this T data ) => JsonSerializer.SerializeToUtf8Bytes( data );
	public static T Deserialize<T>( this byte[] bytes ) => JsonSerializer.Deserialize<T>( bytes );
}
