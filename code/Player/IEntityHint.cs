using Sandbox.UI;

namespace TTT;

/// <summary>
/// Interface for components that can display a hint when looked at.
/// Replaces the old IEntityHint and IUse interfaces.
/// </summary>
public interface ICarriableHint
{
	/// <summary>
	/// The max viewable distance of the hint.
	/// </summary>
	float HintDistance => Player.UseDistance;

	/// <summary>
	/// Whether or not the entity should show a glow outline.
	/// </summary>
	bool ShowGlow => true;

	/// <summary>
	/// Whether or not we can show the UI hint.
	/// </summary>
	bool CanHint( Player player ) => true;

	/// <summary>
	/// The hint panel to display.
	/// </summary>
	Panel DisplayHint( Player player )
	{
		return new UI.Hint() { HintText = "Use" };
	}

	/// <summary>
	/// Called each tick while the hint is active.
	/// </summary>
	void Tick( Player player ) { }
}
