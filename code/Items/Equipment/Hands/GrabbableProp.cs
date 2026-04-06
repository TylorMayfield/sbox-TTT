using Sandbox;
using Sandbox.Physics;

namespace TTT;

public class GrabbableProp : IGrabbable
{
	private ModelEntity GrabbedEntity { get; set; }
	public string PrimaryAttackHint => GrabbedEntity.IsValid() ? "Throw" : string.Empty;
	public string SecondaryAttackHint => GrabbedEntity.IsValid() ? "Drop" : string.Empty;
	public bool IsHolding => GrabbedEntity.IsValid() || _isThrowing;

	private readonly Player _owner;
	private bool _isThrowing = false;
	private readonly bool _isInteractable = false;
	private PhysicsBody _handPhysicsBody;
	private PhysicsJoint _joint;

	public GrabbableProp( Player owner, ModelEntity grabbedEntity )
	{
		_owner = owner;

		// We want to be able to shoot whatever entity the player is holding.
		// Let's give it a tag that interacts with bullets and doesn't collide with players.
		_isInteractable = grabbedEntity.Tags.Has( "interactable" );
		if ( !_isInteractable )
			grabbedEntity.Tags.Add( "interactable" );

		GrabbedEntity = grabbedEntity;
		GrabbedEntity.EnableTouch = false;
		GrabbedEntity.EnableHideInFirstPerson = false;

		if ( GrabbedEntity.PhysicsBody.IsValid() )
		{
			_handPhysicsBody = new PhysicsBody( Game.PhysicsWorld )
			{
				BodyType = PhysicsBodyType.Keyframed
			};

			var attachment = owner.GetAttachment( Hands.MiddleHandsAttachment )!.Value;
			_handPhysicsBody.Position = attachment.Position;
			_handPhysicsBody.Rotation = attachment.Rotation;

			_joint = PhysicsJoint.CreateFixed( _handPhysicsBody, GrabbedEntity.PhysicsBody );
		}
	}

	public void Update( Player player )
	{
		// Incase someone walks up and picks up the carriable from the player's hands
		// we just need to reset "EnableHideInFirstPerson".
		var carriableHasOwner = GrabbedEntity is Carriable && GrabbedEntity.Owner.IsValid();
		if ( carriableHasOwner )
		{
			GrabbedEntity.EnableHideInFirstPerson = true;
			GrabbedEntity = null;
		}

		if ( !GrabbedEntity.IsValid() || !_owner.IsValid() )
		{
			Drop();
			return;
		}

		if ( _handPhysicsBody is null )
			return;

		if ( Vector3.DistanceBetween( GrabbedEntity.Position, _owner.EyePosition ) > Player.UseDistance * 1.75f )
		{
			Drop();
			return;
		}

		var attachment = player.GetAttachment( Hands.MiddleHandsAttachment )!.Value;
		_handPhysicsBody.Position = attachment.Position;
		_handPhysicsBody.Rotation = attachment.Rotation;
	}

	public Entity Drop()
	{
		var grabbedEntity = GrabbedEntity;

		if ( _joint.IsValid() )
			_joint.Remove();

		_joint = null;
		_handPhysicsBody = null;

		if ( grabbedEntity.IsValid() )
		{
			if ( !_isInteractable )
			{
				grabbedEntity.Tags.Remove( "interactable" );
				grabbedEntity.Components.GetOrCreate<IgnoreDamage>();
			}

			grabbedEntity.LastAttacker = _owner;
			grabbedEntity.EnableHideInFirstPerson = true;
			grabbedEntity.EnableTouch = true;
			grabbedEntity.SetParent( null );

			if ( grabbedEntity is Carriable carriable )
				carriable.OnCarryDrop( _owner );
		}

		GrabbedEntity = null;
		return grabbedEntity;
	}

	public void SecondaryAction()
	{
		_isThrowing = true;

		var droppedEntity = Drop();
		if ( droppedEntity.IsValid() )
			droppedEntity.Velocity = _owner.GetDropVelocity();

		_owner.SetAnimParameter( "b_attack", true );
		Utils.DelayAction( 0.6f, () => _isThrowing = false );
	}
}
