using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Text.Json;
using global::TTT;

namespace TTT.UI;

public partial class AdminPage : Panel
{
	public static AdminPage Instance { get; private set; }

	private static readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	internal static List<RdmReport> Reports { get; private set; } = new();
	internal static List<ModerationLogEntry> Logs { get; private set; } = new();
	internal static List<ModerationPlayerInfo> Players { get; private set; } = new();
	internal static DateTime LastUpdated { get; private set; }

	public AdminPage()
	{
		Instance = this;
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		if ( !firstTime )
			return;

		GameManager.RequestModerationSnapshot();
	}

	public static void ReceiveSnapshot( string reportsJson, string logsJson, string playersJson )
	{
		Reports = JsonSerializer.Deserialize<List<RdmReport>>( reportsJson ?? "[]", _jsonOptions ) ?? new();
		Logs = JsonSerializer.Deserialize<List<ModerationLogEntry>>( logsJson ?? "[]", _jsonOptions ) ?? new();
		Players = JsonSerializer.Deserialize<List<ModerationPlayerInfo>>( playersJson ?? "[]", _jsonOptions ) ?? new();
		LastUpdated = DateTime.Now;

		Instance?.StateHasChanged();
	}
}
