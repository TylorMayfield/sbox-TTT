using Sandbox.UI;

namespace TTT.UI;

public partial class VoiceChatDisplay : Panel
{
	public static VoiceChatDisplay Instance { get; private set; }

	public VoiceChatDisplay() => Instance = this;

	public void OnVoicePlayed() { }
}
