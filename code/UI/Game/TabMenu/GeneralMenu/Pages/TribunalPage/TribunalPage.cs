using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Text.Json;
using global::TTT;

namespace TTT.UI;

public partial class TribunalPage : Panel
{
	public static TribunalPage Instance { get; private set; }

	private static readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	internal static List<RdmReport> Reports { get; private set; } = new();
	internal static DateTime LastUpdated { get; private set; }

	public TribunalPage()
	{
		Instance = this;
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		if ( !firstTime )
			return;

		GameManager.RequestTribunalSnapshot();
	}

	public static void ReceiveSnapshot( string reportsJson )
	{
		Reports = JsonSerializer.Deserialize<List<RdmReport>>( reportsJson ?? "[]", _jsonOptions ) ?? new();
		LastUpdated = DateTime.Now;
		Instance?.StateHasChanged();
	}
}
