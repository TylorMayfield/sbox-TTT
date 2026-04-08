using Sandbox;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TTT;

/// <summary>
/// Manages <see cref="Perk"/> components on a <see cref="Player"/>.
/// </summary>
public class Perks : IEnumerable<Perk>
{
	public Player Owner { get; private init; }

	public Perk this[int i] => _list[i];

	private readonly List<Perk> _list = new();
	public int Count => _list.Count;

	public Perks( Player player ) => Owner = player;

	public void Add( Perk perk )
	{
		if ( !Networking.IsHost )
			return;

		var typeDescription = TypeLibrary.GetType( perk.GetType() );
		var created = typeDescription is not null
			? Owner.Components.Create( typeDescription ) as Perk
			: null;

		created ??= perk;
		_list.Add( created );
	}

	public void Remove( Perk perk )
	{
		if ( !Networking.IsHost )
			return;

		_list.Remove( perk );
		perk?.Destroy();
	}

	public bool Has<T>()
	{
		return _list.Any( x => x is T );
	}

	public bool Contains( Perk perk ) => _list.Contains( perk );

	public T Find<T>() where T : Perk
	{
		foreach ( var perk in _list )
		{
			if ( perk is T t )
				return t;
		}

		return default;
	}

	public void DeleteContents()
	{
		foreach ( var perk in _list.ToArray() )
		{
			perk?.Destroy();
		}

		_list.Clear();
	}

	public IEnumerator<Perk> GetEnumerator() => _list.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
