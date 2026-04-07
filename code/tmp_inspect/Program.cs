using System;
using System.IO;
using System.Linq;
using System.Reflection;

var outputDir = Path.GetFullPath(@"C:\Program Files (x86)\Steam\steamapps\common\sbox\.vs\output");
var names = new[] { "Event", "GameEvent", "Rpc", "NetPermission", "NetFlags", "Slider2D", "Slider", "CitizenAnimation", "CitizenAnimationHelper", "PhysicsJoint", "Panel", "WorldPanel", "TextEntry", "IClient", "Client", "MenuSystem", "GameMenu", "ILoadingScreenPanel", "NavHostPanel", "IGameMenuPanel", "Menu" };
foreach (var path in Directory.EnumerateFiles(outputDir, "*.dll"))
{
    try
    {
        var assembly = Assembly.LoadFrom(path);
        var types = assembly.GetTypes();
        foreach (var name in names)
        {
            var found = types.Where(t => t.Name == name || (t.FullName != null && t.FullName.EndsWith("." + name))).ToArray();
            if (found.Any())
            {
                Console.WriteLine($"{Path.GetFileName(path)} contains {name}: {found.Length}");
                foreach (var type in found.Take(10))
                    Console.WriteLine("  " + type.FullName);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed load {Path.GetFileName(path)}: {ex.Message}");
    }
}

var enginePath = Path.Combine(outputDir, "Sandbox.Engine.dll");
var engineAsm = Assembly.LoadFrom(enginePath);
var netFlagsType = engineAsm.GetType("Sandbox.NetFlags");
if (netFlagsType != null)
{
    Console.WriteLine($"Sandbox.NetFlags members:");
    foreach (var field in netFlagsType.GetFields(BindingFlags.Public | BindingFlags.Static))
        Console.WriteLine($"  Field: {field.Name} {field.FieldType}");
}

var baseLibPath = Path.Combine(outputDir, "Base Library.dll");
var baseLibAsm = Assembly.LoadFrom(baseLibPath);
var uiTypes = baseLibAsm.GetTypes().Where(t => t.Namespace == "Sandbox.UI" && t.Name.Contains("Slider")).ToArray();
Console.WriteLine($"Sandbox.UI Slider types: {uiTypes.Length}");
foreach (var t in uiTypes)
{
    Console.WriteLine($"  {t.Name} (IsPublic={t.IsPublic})");
}
