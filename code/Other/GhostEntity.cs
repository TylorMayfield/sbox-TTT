using Sandbox;

namespace TTT;

/// <summary>
/// A translucent ghost of a model used for placement preview.
/// </summary>
public sealed class GhostEntity : Component
{
	public ModelRenderer Renderer { get; private set; }

	protected override void OnStart()
	{
		Renderer = Components.Get<ModelRenderer>( FindMode.InSelf );
		Tags.Add( "interactable" );
	}

	public void SetEntity( ModelRenderer source )
	{
		Renderer ??= Components.Get<ModelRenderer>( FindMode.InSelf );
		if ( Renderer is not null )
		{
			Renderer.Model = source.Model;
			Renderer.Tint = Renderer.Tint.WithAlpha( 0.5f );
		}
	}

	public void ShowValid()
	{
		if ( Renderer is not null )
			Renderer.Tint = Color.Green.WithAlpha( 0.5f );
	}

	public void ShowInvalid()
	{
		if ( Renderer is not null )
			Renderer.Tint = Color.Red.WithAlpha( 0.5f );
	}

	public bool IsPlacementValid( ref SceneTraceResult trace )
	{
		var position = trace.EndPosition;
		var bounds = Renderer?.Model?.PhysicsBounds ?? BBox.FromPositionAndSize( position, 16f );
		bounds = bounds.Translate( position );

		// Check for overlapping scene objects
		var hit = Scene.Trace.Box( bounds, position, position )
			.WithAnyTags( "solid" )
			.IgnoreGameObject( GameObject )
			.Run();

		return !hit.Hit;
	}
}
