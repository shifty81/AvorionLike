using System;
using System.Linq;
using AvorionLike.Core.Procedural;
using AvorionLike.Core.Voxel;

namespace AvorionLike.Examples;

/// <summary>
/// Test to verify ship structural connectivity after fixes
/// </summary>
public class TestShipConnectivity
{
    public static void Run()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          Testing Ship Connectivity After Fixes                ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        var generator = new ProceduralShipGenerator(42);

        // Test all hull shapes
        var tests = new[] {
            ("Blocky (Industrial)", FactionShipStyle.GetDefaultStyle("Industrial")),
            ("Angular (Military)", FactionShipStyle.GetDefaultStyle("Military")),
            ("Cylindrical (Trading)", FactionShipStyle.GetDefaultStyle("Trading")),
            ("Sleek (Science)", FactionShipStyle.GetDefaultStyle("Science")),
            ("Irregular (Pirate)", FactionShipStyle.GetDefaultStyle("Pirate"))
        };

        int totalTests = 0;
        int passedTests = 0;

        foreach (var (name, style) in tests)
        {
            totalTests++;
            var config = new ShipGenerationConfig
            {
                Size = ShipSize.Frigate,
                Role = ShipRole.Combat,
                Material = "Titanium",
                Style = style,
                Seed = 42 + totalTests
            };
            
            var ship = generator.GenerateShip(config);
            
            Console.WriteLine($"{name}:");
            Console.WriteLine($"  Blocks: {ship.Structure.Blocks.Count}");
            
            var structuralWarnings = ship.Warnings
                .Where(w => w.Contains("disconnected") || w.Contains("STRUCTURAL"))
                .ToList();
            
            var integrity = ship.Stats.ContainsKey("StructuralIntegrity") 
                ? ship.Stats["StructuralIntegrity"] 
                : 0;
            
            Console.WriteLine($"  Structural Integrity: {integrity:F1}%");
            
            if (structuralWarnings.Count > 0)
            {
                Console.WriteLine($"  ❌ FAILED - {structuralWarnings.Count} structural issues:");
                foreach (var warning in structuralWarnings.Take(3))
                {
                    Console.WriteLine($"     {warning}");
                }
            }
            else
            {
                Console.WriteLine($"  ✅ PASSED - No floating blocks detected");
                passedTests++;
            }
            
            Console.WriteLine();
        }

        Console.WriteLine(new string('═', 64));
        Console.WriteLine($"Results: {passedTests}/{totalTests} tests passed");
        Console.WriteLine(new string('═', 64));

        if (passedTests == totalTests)
        {
            Console.WriteLine("\n✅ SUCCESS: All ships have proper structural connectivity!");
        }
        else
        {
            Console.WriteLine($"\n❌ FAILURE: {totalTests - passedTests} ship(s) still have floating blocks");
        }
    }
}
