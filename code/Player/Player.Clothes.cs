using Sandbox;
using System.Collections.Generic;
using System.Linq;

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
		ClothingContainer = new();

		foreach ( var clothing in _currentPreset ?? Enumerable.Empty<Clothing>() )
			ClothingContainer.Add( clothing );

		ClothingContainer.DressEntity( Renderer );
	}

	public static void ChangeClothingPreset()
	{
		if ( Networking.IsHost )
			_currentPreset = Game.Random.FromList( ClothingPresets );
	}
}
