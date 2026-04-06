using System;

namespace TTT;

public enum TribunalResolution
{
	None,
	Guilty,
	NotGuilty,
	NoConsensus,
	InsufficientVotes
}

public readonly record struct TribunalEvaluationResult( TribunalResolution Resolution, string Notes )
{
	public static TribunalEvaluationResult None => new( TribunalResolution.None, string.Empty );
}

public static class TribunalRules
{
	public static TribunalEvaluationResult Evaluate( int guiltyVotes, int notGuiltyVotes, int minimumVotes, float requiredRatio, bool forceTimeoutResolution )
	{
		var totalVotes = guiltyVotes + notGuiltyVotes;
		if ( totalVotes <= 0 )
			return TribunalEvaluationResult.None;

		var dominantVotes = Math.Max( guiltyVotes, notGuiltyVotes );
		var ratio = dominantVotes / (float)totalVotes;

		if ( !forceTimeoutResolution )
		{
			if ( totalVotes < minimumVotes || ratio < requiredRatio )
				return TribunalEvaluationResult.None;
		}

		if ( forceTimeoutResolution && totalVotes < minimumVotes )
		{
			return new TribunalEvaluationResult(
				TribunalResolution.InsufficientVotes,
				$"Tribunal expired without quorum ({totalVotes}/{minimumVotes} votes)."
			);
		}

		if ( guiltyVotes == notGuiltyVotes )
		{
			if ( !forceTimeoutResolution )
				return TribunalEvaluationResult.None;

			return new TribunalEvaluationResult(
				TribunalResolution.NoConsensus,
				$"Tribunal ended in a tie ({guiltyVotes}-{notGuiltyVotes})."
			);
		}

		if ( guiltyVotes > notGuiltyVotes )
		{
			return new TribunalEvaluationResult(
				TribunalResolution.Guilty,
				$"Global tribunal voted guilty ({guiltyVotes}-{notGuiltyVotes})."
			);
		}

		return new TribunalEvaluationResult(
			TribunalResolution.NotGuilty,
			$"Global tribunal voted not guilty ({notGuiltyVotes}-{guiltyVotes})."
		);
	}
}
