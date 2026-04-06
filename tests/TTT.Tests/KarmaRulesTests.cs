using TTT;
using Xunit;

namespace TTT.Tests;

public class KarmaRulesTests
{
	[Fact]
	public void DamageFactorIsFullAtOrAboveStartKarma()
	{
		var factor = KarmaRules.GetDamageFactor( enabled: true, baseKarma: 1000, startValue: 1000 );

		Assert.Equal( 1f, factor );
	}

	[Fact]
	public void DamageFactorDropsBelowStartKarma()
	{
		var factor = KarmaRules.GetDamageFactor( enabled: true, baseKarma: 800, startValue: 1000 );

		Assert.InRange( factor, 0.1f, 0.99f );
	}

	[Fact]
	public void DamageFactorRespectsDisabledKarma()
	{
		var factor = KarmaRules.GetDamageFactor( enabled: false, baseKarma: 500, startValue: 1000 );

		Assert.Equal( 1f, factor );
	}

	[Fact]
	public void SpeedFactorReturnsFullSpeedWhenDisabled()
	{
		var factor = KarmaRules.GetSpeedFactor( enabled: false, damageFactor: 0.25f, minSpeedScale: 0.85f );

		Assert.Equal( 1f, factor );
	}

	[Fact]
	public void SpeedFactorUsesConfiguredMinimumAtLowestDamage()
	{
		var factor = KarmaRules.GetSpeedFactor( enabled: true, damageFactor: 0.1f, minSpeedScale: 0.85f );

		Assert.Equal( 0.85f, factor );
	}

	[Fact]
	public void SpeedFactorClampsConfiguredMinimum()
	{
		var factor = KarmaRules.GetSpeedFactor( enabled: true, damageFactor: 0.1f, minSpeedScale: 0.05f );

		Assert.Equal( 0.25f, factor );
	}
}
