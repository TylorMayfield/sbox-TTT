using System;

namespace TTT;

public static class KarmaRules
{
	public static float GetDamageFactor( bool enabled, float baseKarma, int startValue )
	{
		if ( !enabled || baseKarma >= startValue )
			return 1f;

		var k = baseKarma - startValue;
		var damageFactor = 1 + (0.0007f * k) + (-0.000002f * (k * k));

		return Math.Clamp( damageFactor, 0.1f, 1f );
	}

	public static float GetSpeedFactor( bool enabled, float damageFactor, float minSpeedScale )
	{
		if ( !enabled )
			return 1f;

		var clampedMinimumScale = Math.Clamp( minSpeedScale, 0.25f, 1f );
		var normalizedDamageFactor = Math.Clamp( (damageFactor - 0.1f) / 0.9f, 0f, 1f );

		return clampedMinimumScale + ((1f - clampedMinimumScale) * normalizedDamageFactor);
	}
}
