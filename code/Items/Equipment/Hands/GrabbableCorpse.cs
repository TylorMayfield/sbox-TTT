using Sandbox;
namespace TTT;

public class GrabbableCorpse : IGrabbable
{
	public string PrimaryAttackHint => !IsHolding ? "Pickup" : AttachmentHint;
	private string AttachmentHint => !_corpse.Ropes.IsNullOrEmpty() ? "Detach" : _owner.Role.CanAttachCorpses ? "Hang" : string.Empty;
	public string SecondaryAttackHint => IsHolding ? "Drop" : string.Empty;
	public bool IsHolding => _corpse.IsValid();

	private readonly Player _owner;
	private readonly Corpse _corpse;

	public GrabbableCorpse( Player player, Corpse corpse )
	{
		_owner = player;
		_corpse = corpse;
	}

	public GameObject Drop()
	{
		return _corpse.GameObject;
	}

	public void Update( Player player )
	{
		if ( !_corpse.IsValid() )
			return;

		var renderer = player.Components.Get<SkinnedModelRenderer>();
		var attachment = renderer?.GetAttachment( Hands.MiddleHandsAttachment );
		if ( attachment.HasValue )
		{
			_corpse.WorldPosition = attachment.Value.Position;
			_corpse.WorldRotation = attachment.Value.Rotation;
		}
	}

	public void SecondaryAction()
	{
		_corpse.RemoveRopeAttachments();
		Drop();
	}
}
