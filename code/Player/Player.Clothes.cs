using Sandbox;
using System.Collections.Generic;

namespace TTT;

public partial class Player
{
	public static List<List<Clothing>> ClothingPresets { get; private set; } = new();

	/// <summary>
	/// The current preset from <see cref="ClothingPresets"/>.
	/// </summary>
	private static List<Clothing> _currentPreset;

	public void DressPlayer()
	{
		if ( _currentPreset is not null )
			ClothingContainer.Clothing = _currentPreset;

		ClothingContainer.DressEntity( Renderer );
	}

	public static void ChangeClothingPreset()
	{
		if ( Networking.IsHost )
			_currentPreset = Game.Random.FromList( ClothingPresets );
	}
}
