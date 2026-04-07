using Sandbox;

namespace TTT;

public partial class Player
{
	public Carriable Using { get; protected set; }

	/// <summary>
	/// The carriable we're currently looking at.
	/// </summary>
	public Carriable HoveredCarriable { get; private set; }

	/// <summary>
	/// The player we're currently looking at (for hints).
	/// </summary>
	public Player HoveredPlayer { get; private set; }

	public const float UseDistance = 80f;
	private float _traceDistance;

	public bool CanUse( Carriable carriable )
	{
		if ( carriable is null )
			return false;

		if ( !carriable.IsUsable( this ) )
			return false;

		if ( _traceDistance > UseDistance && FindUsablePoint( carriable ) is null )
			return false;

		return true;
	}

	public bool CanContinueUsing( Carriable carriable )
	{
		if ( HoveredCarriable != carriable )
			return false;

		if ( _traceDistance > UseDistance && FindUsablePoint( carriable ) is null )
			return false;

		if ( carriable.OnUse( this ) )
			return true;

		return false;
	}

	public void StartUsing( Carriable carriable )
	{
		Using = carriable;
	}

	protected void StopUsing()
	{
		Using = null;
	}

	protected void PlayerUse()
	{
		HoveredCarriable = FindHoveredCarriable();

		if ( Input.Pressed( InputAction.Use ) )
		{
			if ( CanUse( HoveredCarriable ) )
				StartUsing( HoveredCarriable );
		}

		if ( !Input.Down( InputAction.Use ) )
		{
			StopUsing();
			return;
		}

		if ( !Using.IsValid() )
			return;

		if ( !CanContinueUsing( Using ) )
			StopUsing();
	}

	protected GameObject FindHoveredGameObject()
	{
		var pos = EyePosition;
		var forward = pos + EyeRotation.Forward * MaxHintDistance;

		var trace = Scene.Trace.Ray( pos, forward )
			.WithAnyTags( "solid", "interactable" )
			.UseHitboxes()
			.IgnoreGameObject( GameObject )
			.Run();

		if ( !trace.Hit )
			return null;

		_traceDistance = trace.Distance;
		return trace.GameObject;
	}

	private Carriable FindHoveredCarriable()
	{
		var go = FindHoveredGameObject();
		if ( go is null )
			return null;

		return go.Components.TryGet<Carriable>( out var c ) ? c : null;
	}

	private Vector3? FindUsablePoint( Carriable carriable )
	{
		if ( carriable is null )
			return null;

		var bounds = carriable.GameObject.GetBounds();
		var closestPoint = bounds.ClosestPoint( EyePosition );

		if ( EyePosition.Distance( closestPoint ) <= UseDistance )
			return closestPoint;

		return null;
	}
}
