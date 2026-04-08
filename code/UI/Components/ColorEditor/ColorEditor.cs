using Sandbox;
using Sandbox.UI;

namespace TTT.UI;

public class ColorEditor : Panel
{
	private ColorHsv _value;

	[Property]
	public ColorHsv Value
	{
		get => _value;
		set
		{
			_value = value;
			CreateValueEvent( "value", _value.ToColor() );
		}
	}
}
