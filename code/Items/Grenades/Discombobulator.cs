using Editor;
using Sandbox;
using System;

namespace TTT;

[Category( "Grenades" )]
[ClassName( "ttt_grenade_discombobulator" )]
[EditorModel( "models/weapons/w_frag.vmdl" )]
[Title( "Discombobulator" )]
public class Discombobulator : Grenade
{
	private const string ExplodeSound = "discombobulator_explode-1";
	private const string Particle = "particles/discombobulator/explode.vpcf";

	protected override void OnExplode()
	{
		base.OnExplode();

		Sound.Play( ExplodeSound, WorldPosition );

		var radius = 400f;
		var pushForce = 1024f;

		// Push all players and rigidbodies in range
		foreach ( var player in Utils.GetPlayersWhere( p => p.IsAlive ) )
		{
			var targetPos = player.WorldPosition + Vector3.Up * 63;
			var dist = Vector3.DistanceBetween( WorldPosition, targetPos );
			if ( dist > radius )
				continue;

			var trace = Scene.Trace.Ray( WorldPosition, targetPos )
				.IgnoreGameObject( GameObject )
				.StaticOnly()
				.Run();

			if ( trace.Fraction < 0.98f )
				continue;

			var distanceMul = 1.0f - Math.Clamp( dist / radius, 0.0f, 1.0f );
			var force = pushForce * distanceMul;
			var forceDir = (targetPos - WorldPosition).Normal;

			player.CharController.Punch( force * forceDir );
		}

		// Push physics props in range
		foreach ( var rb in Scene.GetAllComponents<Rigidbody>() )
		{
			if ( rb.GameObject == GameObject )
				continue;

			var targetPos = rb.PhysicsBody?.MassCenter ?? rb.WorldPosition;
			var dist = Vector3.DistanceBetween( WorldPosition, targetPos );
			if ( dist > radius )
				continue;

			if ( !rb.PhysicsBody.IsValid() )
				continue;

			var trace = Scene.Trace.Ray( WorldPosition, targetPos )
				.IgnoreGameObject( GameObject )
				.StaticOnly()
				.Run();

			if ( trace.Fraction < 0.98f )
				continue;

			var distanceMul = 1.0f - Math.Clamp( dist / radius, 0.0f, 1.0f );
			var force = pushForce * distanceMul;
			var forceDir = (targetPos - WorldPosition).Normal;

			rb.PhysicsBody.ApplyForceAt( rb.PhysicsBody.MassCenter, forceDir * force );
		}
	}
}
