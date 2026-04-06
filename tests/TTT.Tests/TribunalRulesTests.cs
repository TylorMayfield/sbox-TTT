using TTT;
using Xunit;

namespace TTT.Tests;

public class TribunalRulesTests
{
	[Fact]
	public void EarlyResolutionRequiresQuorumAndRatio()
	{
		var result = TribunalRules.Evaluate( guiltyVotes: 2, notGuiltyVotes: 0, minimumVotes: 3, requiredRatio: 0.6f, forceTimeoutResolution: false );

		Assert.Equal( TribunalResolution.None, result.Resolution );
	}

	[Fact]
	public void EarlyGuiltyResolutionPassesWithEnoughVotes()
	{
		var result = TribunalRules.Evaluate( guiltyVotes: 3, notGuiltyVotes: 1, minimumVotes: 3, requiredRatio: 0.6f, forceTimeoutResolution: false );

		Assert.Equal( TribunalResolution.Guilty, result.Resolution );
		Assert.Contains( "guilty", result.Notes );
	}

	[Fact]
	public void EarlyNotGuiltyResolutionPassesWithEnoughVotes()
	{
		var result = TribunalRules.Evaluate( guiltyVotes: 1, notGuiltyVotes: 3, minimumVotes: 3, requiredRatio: 0.6f, forceTimeoutResolution: false );

		Assert.Equal( TribunalResolution.NotGuilty, result.Resolution );
		Assert.Contains( "not guilty", result.Notes );
	}

	[Fact]
	public void TimeoutWithoutQuorumBecomesInsufficientVotes()
	{
		var result = TribunalRules.Evaluate( guiltyVotes: 1, notGuiltyVotes: 0, minimumVotes: 3, requiredRatio: 0.6f, forceTimeoutResolution: true );

		Assert.Equal( TribunalResolution.InsufficientVotes, result.Resolution );
		Assert.Contains( "without quorum", result.Notes );
	}

	[Fact]
	public void TimeoutTieBecomesNoConsensus()
	{
		var result = TribunalRules.Evaluate( guiltyVotes: 2, notGuiltyVotes: 2, minimumVotes: 3, requiredRatio: 0.6f, forceTimeoutResolution: true );

		Assert.Equal( TribunalResolution.NoConsensus, result.Resolution );
		Assert.Contains( "tie", result.Notes );
	}
}
