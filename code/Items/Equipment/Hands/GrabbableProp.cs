using Sandbox;
using Sandbox.Physics;

namespace TTT;

public class GrabbableProp : IGrabbable
{
	private GameObject GrabbedGo { get; set; }
	public string PrimaryAttackHint => GrabbedGo.IsValid() ? "Throw" : string.Empty;
	public string SecondaryAttackHint => GrabbedGo.IsValid() ? "Drop" : string.Empty;
	public bool IsHolding => GrabbedGo.IsValid() || _isThrowing;

	private readonly Player _owner;
	private bool _isThrowing = false;
	private readonly bool _isInteractable = false;
	private PhysicsBody _handPhysicsBody;
	private PhysicsJoint _joint;

	public GrabbableProp( Player owner, GameObject grabbedGo )
	{
		_owner = owner;

		_isInteractable = grabbedGo.Tags.Has( "interactable" );
		if ( !_isInteractable )
			grabbedGo.Tags.Add( "interactable" );

		GrabbedGo = grabbedGo;

		var rb = GrabbedGo.Components.Get<Rigidbody>();
		if ( rb?.PhysicsBody is not null )
		{
			_handPhysicsBody = new PhysicsBody( Game.PhysicsWorld )
			{
				BodyType = PhysicsBodyType.Keyframed
			};

			var renderer = owner.Components.Get<SkinnedModelRenderer>();
			var attachment = renderer?.GetAttachment( Hands.MiddleHandsAttachment );
			if ( attachment.HasValue )
			{
				_handPhysicsBody.Position = attachment.Value.Position;
				_handPhysicsBody.Rotation = attachment.Value.Rotation;
			}

			_joint = PhysicsJoint.CreateFixed( _handPhysicsBody, rb.PhysicsBody );
		}
	}

	public void Update( Player player )
	{
		// If a carriable was picked up by someone else while held
		var carriable = GrabbedGo?.Components.Get<Carriable>();
		if ( carriable?.Owner is not null )
		{
			GrabbedGo = null;
			return;
		}

		if ( !GrabbedGo.IsValid() || !_owner.IsValid() )
		{
			Drop();
			return;
		}

		if ( _handPhysicsBody is null )
			return;

		if ( Vector3.DistanceBetween( GrabbedGo.WorldPosition, _owner.EyePosition ) > Player.UseDistance * 1.75f )
		{
			Drop();
			return;
		}

		var renderer = player.Components.Get<SkinnedModelRenderer>();
		var attachment = renderer?.GetAttachment( Hands.MiddleHandsAttachment );
		if ( attachment.HasValue )
		{
			_handPhysicsBody.Position = attachment.Value.Position;
			_handPhysicsBody.Rotation = attachment.Value.Rotation;
		}
	}

	public GameObject Drop()
	{
		var droppedGo = GrabbedGo;

		if ( _joint.IsValid() )
			_joint.Remove();

		_joint = null;
		_handPhysicsBody = null;

		if ( droppedGo.IsValid() )
		{
			if ( !_isInteractable )
			{
				droppedGo.Tags.Remove( "interactable" );
				droppedGo.Components.GetOrCreate<IgnoreDamage>();
			}

			droppedGo.Parent = null;

			var carriable = droppedGo.Components.Get<Carriable>();
			if ( carriable is not null )
				carriable.OnCarryDrop( _owner );
		}

		GrabbedGo = null;
		return droppedGo;
	}

	public void SecondaryAction()
	{
		_isThrowing = true;

		var droppedGo = Drop();
		if ( droppedGo.IsValid() )
		{
			var rb = droppedGo.Components.Get<Rigidbody>();
			if ( rb?.PhysicsBody is not null )
				rb.PhysicsBody.Velocity = _owner.EyeRotation.Forward * 500f + _owner.CharController.Velocity;
		}

		_owner.SetAnimParameter( "b_attack", true );
		Utils.DelayAction( 0.6f, () => _isThrowing = false );
	}
}
