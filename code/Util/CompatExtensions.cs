using Sandbox;
using Sandbox.Physics;
using System.Collections.Generic;
using System.Linq;

namespace TTT;

public static class CompatExtensions
{
	private static readonly Dictionary<ulong, Dictionary<string, object>> ConnectionValues = new();

	public static bool IsValid( this Connection connection ) => connection is not null;

	public static T GetValue<T>( this Connection connection, string key )
	{
		if ( connection is null )
			return default;

		if ( ConnectionValues.TryGetValue( connection.SteamId, out var values ) && values.TryGetValue( key, out var value ) && value is T typed )
			return typed;

		return default;
	}

	public static T GetValue<T>( this Connection connection, string key, T defaultValue )
	{
		if ( connection is null )
			return defaultValue;

		if ( ConnectionValues.TryGetValue( connection.SteamId, out var values ) && values.TryGetValue( key, out var value ) && value is T typed )
			return typed;

		return defaultValue;
	}

	public static void SetValue( this Connection connection, string key, object value )
	{
		if ( connection is null )
			return;

		if ( !ConnectionValues.TryGetValue( connection.SteamId, out var values ) )
		{
			values = new();
			ConnectionValues[connection.SteamId] = values;
		}

		values[key] = value;
	}

	public static void SetAnimParameter( this Player player, string name, object value )
	{
		if ( player?.Renderer is null )
			return;

		switch ( value )
		{
			case bool boolValue:
				player.Renderer.Set( name, boolValue );
				break;
			case int intValue:
				player.Renderer.Set( name, intValue );
				break;
			case float floatValue:
				player.Renderer.Set( name, floatValue );
				break;
			case Vector3 vectorValue:
				player.Renderer.Set( name, vectorValue );
				break;
		}
	}

	public static void SendAfkHeartbeat( this Player player )
	{
		// Old chat UI called into this directly; AFK tracking now happens from the player update loop.
	}

	public static DamageInfo WithDamage( this DamageInfo info, float damage )
	{
		info.Damage = damage;
		return info;
	}

	public static DamageInfo WithTag( this DamageInfo info, string tag )
	{
		info.Tags.Add( tag );
		return info;
	}

	public static DamageInfo WithTags( this DamageInfo info, params string[] tags )
	{
		foreach ( var tag in tags )
			info.Tags.Add( tag );

		return info;
	}

	public static bool HasTag( this DamageInfo info, string tag ) => info.Tags.Has( tag );

	public static DamageInfo WithAttacker( this DamageInfo info, GameObject attacker )
	{
		info.Attacker = attacker;
		return info;
	}

	public static DamageInfo WithAttacker( this DamageInfo info, Component attacker )
	{
		info.Attacker = attacker?.GameObject;
		return info;
	}

	public static DamageInfo WithAttacker( this DamageInfo info, Player attacker )
	{
		info.Attacker = attacker?.GameObject;
		return info;
	}

	public static DamageInfo WithWeapon( this DamageInfo info, Component weapon )
	{
		info.Weapon = weapon?.GameObject;
		return info;
	}

	public static DamageInfo WithWeapon( this DamageInfo info, GameObject weapon )
	{
		info.Weapon = weapon;
		return info;
	}

	public static DamageInfo UsingTraceResult( this DamageInfo info, SceneTraceResult trace )
	{
		info.Position = trace.EndPosition;

		if ( trace.Tags is not null )
		{
			foreach ( var tag in trace.Tags )
				info.Tags.Add( tag );
		}

		return info;
	}

	public static Transform? GetAttachment( this ModelRenderer renderer, string name )
	{
		return renderer?.GetAttachmentObject( name )?.WorldTransform;
	}

	public static Transform? GetAttachment( this SkinnedModelRenderer renderer, string name )
	{
		return renderer?.GetAttachmentObject( name )?.WorldTransform;
	}

	public static Transform? GetBoneWorldTransform( this SkinnedModelRenderer renderer, string name )
	{
		return renderer?.GetBoneObject( name )?.WorldTransform;
	}

	public static Transform? GetBoneWorldTransform( this SkinnedModelRenderer renderer, int bone )
	{
		return renderer?.GetBoneObject( bone )?.WorldTransform;
	}

	public static SceneTrace StaticOnly( this SceneTrace trace ) => trace;

	public static void DoBulletImpact( this Surface surface, SceneTraceResult trace )
	{
	}

	public static void DoFootstep( this Surface surface, Player player, SceneTraceResult trace, int foot, float volume )
	{
	}

	public static void RemoveAny<T>( this ComponentList components ) where T : Component
	{
		foreach ( var component in components.GetAll<T>().ToArray() )
			component.Destroy();
	}

	public static void DressEntity( this ClothingContainer clothing, SkinnedModelRenderer renderer )
	{
	}

	public static void DressEntity( this ClothingContainer clothing, Player player )
	{
	}

	public static void ClearEntities( this ClothingContainer clothing )
	{
	}

	public static void Simulate( this Perk perk )
	{
	}

	public static void BroadcastFound( this Corpse corpse, Player finder )
	{
		corpse?.BroadcastCorpseFound( finder );
	}

	public static void SendDamageInfo( this Player player, Connection to )
	{
	}

	public static void Init( this DNA dna, Player killer )
	{
		if ( dna is not null )
			dna.TargetPlayer = killer;
	}

	public static bool IsStatic( this PhysicsBody body ) => body is not null;

	public static void EnableAngularConstraint( this Sandbox.Physics.SpringJoint joint, bool enabled )
	{
	}
}
