using Sandbox.UI;

namespace TTT.UI;

public partial class RoundSummaryPage : Panel
{
	protected override void OnAfterTreeRender( bool firstTime )
	{
		if ( !firstTime )
			return;

		RoleSummary.RequestLatestSummary();
	}
}
