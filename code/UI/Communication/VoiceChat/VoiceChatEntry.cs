using Sandbox;
using Sandbox.UI;
using System;

namespace TTT.UI;

public partial class VoiceChatEntry : Panel
{
	public Friend Friend;

	private readonly Connection _client;
	private float _voiceLevel = 0.5f;
	private float _targetVoiceLevel = 0;
	private readonly float _voiceTimeout = 0.1f;

	RealTimeSince _timeSincePlayed;

	public VoiceChatEntry( Connection client )
	{
		_client = client;
		Friend = new Friend( client.SteamId );
	}

	public void Update( float level )
	{
		_timeSincePlayed = 0;
		_targetVoiceLevel = level;
	}

	public override void Tick()
	{
		if ( IsDeleting )
			return;

		var timeoutInv = 1 - (_timeSincePlayed / _voiceTimeout);
		timeoutInv = MathF.Min( timeoutInv * 2.0f, 1.0f );

		if ( timeoutInv <= 0 )
		{
			Delete();
			return;
		}

		_voiceLevel = _voiceLevel.LerpTo( _targetVoiceLevel, Time.Delta * 40.0f );

		var player = Utils.GetPlayersWhere( p => p.Network.Owner == _client ).FirstOrDefault();
		if ( player is null || !player.IsAlive )
			return;

		var renderer = player.Components.Get<SkinnedModelRenderer>();
		if ( renderer is null )
			return;

		var tx = renderer.GetBoneWorldTransform( "head" ) ?? player.WorldTransform;
		var rolePlate = player.Components.Get<RolePlate>( FindMode.InSelf );
		var rolePlateOffset = rolePlate is not null ? 27f : 20f;
		tx.Position += Vector3.Up * rolePlateOffset + (Vector3.Up * _voiceLevel);
		tx.Rotation = Game.ActiveScene?.Camera?.WorldRotation.RotateAroundAxis( Vector3.Up, 180f ) ?? Rotation.Identity;
	}

	protected override int BuildHash()
	{
		var player = Utils.GetPlayersWhere( p => p.Network.Owner == _client ).FirstOrDefault();
		return HashCode.Combine( player?.IsAlive, player?.Role.GetHashCode() );
	}
}
