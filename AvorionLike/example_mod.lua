-- Sample Lua mod script for AvorionLike
-- This demonstrates the scripting API capabilities

log("=== Sample Mod Loaded ===")
log("This is a demonstration of Lua scripting in AvorionLike")

-- Define a custom function
function calculateDamage(baseDamage, multiplier)
    local damage = baseDamage * multiplier
    log("Calculated damage: " .. damage)
    return damage
end

-- Define a ship creation helper
function createCustomShip(name, blockCount)
    log("Creating custom ship: " .. name)
    log("  Blocks: " .. blockCount)
    
    -- You can access the game engine here
    -- Example: Engine:CreateEntity(name)
    
    return {
        name = name,
        blocks = blockCount,
        health = blockCount * 100
    }
end

-- Test the functions
calculateDamage(100, 1.5)
local ship = createCustomShip("Custom Fighter", 50)
log("Ship created with health: " .. ship.health)

log("=== Mod Initialization Complete ===")
