using Editor;

namespace TTT;

[Category( "Ammo" )]
[ClassName( "ttt_ammo_shotgun" )]
[EditorModel( "models/ammo/ammo_shotgun/ammo_shotgun.vmdl" )]
[Title( "Shotgun Ammo" )]
public class ShotgunAmmo : Ammo
{
	protected override AmmoType Type => AmmoType.Shotgun;
	protected override int DefaultAmmoCount => 5;
	protected override string WorldModelPath => "models/ammo/ammo_shotgun/ammo_shotgun.vmdl";
}
