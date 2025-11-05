using ImGuiNET;
using System.Numerics;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Resources;

namespace AvorionLike.Core.UI;

/// <summary>
/// Inventory/Cargo UI system for managing items and resources
/// </summary>
public class InventoryUI
{
    private readonly GameEngine _gameEngine;
    private bool _showInventory = false;
    private Guid? _selectedEntityId = null;
    
    public bool IsOpen => _showInventory;
    
    public InventoryUI(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }
    
    public void Show(Guid? entityId = null)
    {
        _showInventory = true;
        _selectedEntityId = entityId;
    }
    
    public void Hide()
    {
        _showInventory = false;
        _selectedEntityId = null;
    }
    
    public void Toggle()
    {
        if (_showInventory)
            Hide();
        else
            Show();
    }
    
    public void Render()
    {
        if (!_showInventory)
            return;
        
        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(new Vector2(io.DisplaySize.X * 0.5f, io.DisplaySize.Y * 0.5f), ImGuiCond.FirstUseEver, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(700, 500), ImGuiCond.FirstUseEver);
        
        if (ImGui.Begin("Inventory & Cargo", ref _showInventory))
        {
            // Entity selector
            RenderEntitySelector();
            
            ImGui.Separator();
            
            if (_selectedEntityId.HasValue)
            {
                var inventoryComp = _gameEngine.EntityManager.GetComponent<InventoryComponent>(_selectedEntityId.Value);
                if (inventoryComp != null)
                {
                    RenderInventoryContents(inventoryComp);
                }
                else
                {
                    ImGui.TextColored(new Vector4(1.0f, 0.5f, 0.2f, 1.0f), "Selected entity has no inventory.");
                }
            }
            else
            {
                ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), "Select an entity to view its inventory.");
            }
        }
        ImGui.End();
    }
    
    private void RenderEntitySelector()
    {
        ImGui.Text("Select Entity:");
        ImGui.SameLine();
        
        var entities = _gameEngine.EntityManager.GetAllEntities()
            .Where(e => _gameEngine.EntityManager.GetComponent<InventoryComponent>(e.Id) != null)
            .ToList();
        
        if (entities.Count == 0)
        {
            ImGui.TextColored(new Vector4(1.0f, 0.5f, 0.2f, 1.0f), "No entities with inventory found.");
            return;
        }
        
        // Create combo box with entity IDs
        string currentLabel = _selectedEntityId.HasValue 
            ? $"Entity {_selectedEntityId.Value.ToString()[..8]}..." 
            : "Select...";
        
        if (ImGui.BeginCombo("##EntitySelector", currentLabel))
        {
            foreach (var entity in entities)
            {
                bool isSelected = _selectedEntityId == entity.Id;
                string label = $"Entity {entity.Id.ToString()[..8]}...";
                
                if (ImGui.Selectable(label, isSelected))
                {
                    _selectedEntityId = entity.Id;
                }
                
                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
    }
    
    private void RenderInventoryContents(InventoryComponent inventoryComp)
    {
        var inventory = inventoryComp.Inventory;
        
        // Capacity bar
        float capacityPercent = inventory.MaxCapacity > 0 
            ? (float)inventory.CurrentCapacity / inventory.MaxCapacity 
            : 0.0f;
        
        ImGui.Text($"Capacity: {inventory.CurrentCapacity} / {inventory.MaxCapacity}");
        ImGui.ProgressBar(capacityPercent, new Vector2(-1, 0), $"{capacityPercent * 100:F1}%");
        
        ImGui.Dummy(new Vector2(0, 10));
        
        // Resource list
        ImGui.Text("Resources:");
        ImGui.Separator();
        
        if (ImGui.BeginChild("ResourceList", new Vector2(0, -50)))
        {
            var resources = inventory.GetAllResources();
            bool hasResources = false;
            
            // Create a table for better layout
            if (ImGui.BeginTable("ResourceTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                // Headers
                ImGui.TableSetupColumn("Resource", ImGuiTableColumnFlags.WidthFixed, 150);
                ImGui.TableSetupColumn("Amount", ImGuiTableColumnFlags.WidthFixed, 100);
                ImGui.TableSetupColumn("Add", ImGuiTableColumnFlags.WidthFixed, 80);
                ImGui.TableSetupColumn("Remove", ImGuiTableColumnFlags.WidthFixed, 80);
                ImGui.TableHeadersRow();
                
                foreach (var kvp in resources.OrderBy(x => x.Key))
                {
                    if (kvp.Value <= 0)
                        continue;
                    
                    hasResources = true;
                    ImGui.TableNextRow();
                    
                    // Resource name
                    ImGui.TableSetColumnIndex(0);
                    Vector4 resourceColor = GetResourceColor(kvp.Key);
                    ImGui.TextColored(resourceColor, kvp.Key.ToString());
                    
                    // Amount
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(kvp.Value.ToString());
                    
                    // Add button
                    ImGui.TableSetColumnIndex(2);
                    if (ImGui.SmallButton($"+10##add_{kvp.Key}"))
                    {
                        inventory.AddResource(kvp.Key, 10);
                    }
                    
                    // Remove button
                    ImGui.TableSetColumnIndex(3);
                    if (ImGui.SmallButton($"-10##remove_{kvp.Key}") && kvp.Value >= 10)
                    {
                        inventory.RemoveResource(kvp.Key, 10);
                    }
                }
                
                ImGui.EndTable();
            }
            
            if (!hasResources)
            {
                ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), "Inventory is empty.");
            }
        }
        ImGui.EndChild();
        
        ImGui.Separator();
        
        // Bottom buttons
        if (ImGui.Button("Add All Resources (+100)", new Vector2(200, 0)))
        {
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                inventory.AddResource(type, 100);
            }
        }
        
        ImGui.SameLine();
        
        if (ImGui.Button("Clear Inventory", new Vector2(150, 0)))
        {
            inventory.Clear();
        }
    }
    
    private Vector4 GetResourceColor(ResourceType type)
    {
        return type switch
        {
            ResourceType.Iron => new Vector4(0.7f, 0.7f, 0.7f, 1.0f),      // Gray
            ResourceType.Titanium => new Vector4(0.8f, 0.9f, 1.0f, 1.0f),  // Light blue
            ResourceType.Naonite => new Vector4(0.3f, 1.0f, 0.3f, 1.0f),   // Green
            ResourceType.Trinium => new Vector4(0.3f, 0.8f, 0.8f, 1.0f),   // Cyan
            ResourceType.Xanion => new Vector4(1.0f, 0.5f, 1.0f, 1.0f),    // Purple
            ResourceType.Ogonite => new Vector4(1.0f, 0.7f, 0.2f, 1.0f),   // Orange
            ResourceType.Avorion => new Vector4(1.0f, 0.2f, 0.2f, 1.0f),   // Red
            ResourceType.Credits => new Vector4(1.0f, 1.0f, 0.3f, 1.0f),   // Yellow/Gold
            _ => new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
        };
    }
    
    public void HandleInput()
    {
        // Toggle inventory with I key
        if (ImGui.IsKeyPressed(ImGuiKey.I))
        {
            Toggle();
        }
    }
}
