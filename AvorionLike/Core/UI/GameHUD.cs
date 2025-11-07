using System.Numerics;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Physics;

namespace AvorionLike.Core.UI;

/// <summary>
/// Main game HUD rendered with custom OpenGL renderer (not ImGui)
/// ImGui is reserved for debug/console use only
/// </summary>
public class GameHUD
{
    private readonly GameEngine _gameEngine;
    private readonly CustomUIRenderer _renderer;
    private bool _enabled = true;
    private Guid? _playerShipId;
    private float _screenWidth;
    private float _screenHeight;
    
    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }
    
    public Guid? PlayerShipId
    {
        get => _playerShipId;
        set => _playerShipId = value;
    }
    
    public GameHUD(GameEngine gameEngine, CustomUIRenderer renderer, float screenWidth, float screenHeight)
    {
        _gameEngine = gameEngine;
        _renderer = renderer;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }
    
    public void UpdateScreenSize(float width, float height)
    {
        _screenWidth = width;
        _screenHeight = height;
    }
    
    public void Update(float deltaTime)
    {
        _renderer.Update(deltaTime);
    }
    
    public void Render()
    {
        if (!_enabled) return;
        
        _renderer.BeginRender();
        
        // Draw HUD elements
        RenderCrosshair();
        RenderCornerFrames();
        RenderShipStatus();
        RenderRadar();
        
        _renderer.EndRender();
    }
    
    private void RenderCrosshair()
    {
        _renderer.DrawCrosshair();
    }
    
    private void RenderCornerFrames()
    {
        _renderer.DrawCornerFrames();
    }
    
    private void RenderShipStatus()
    {
        if (!_playerShipId.HasValue) return;
        
        // Get player ship components
        var physics = _gameEngine.EntityManager.GetComponent<PhysicsComponent>(_playerShipId.Value);
        if (physics == null) return;
        
        // Draw ship status panel in top-left (below corner frame)
        float panelX = 25f;
        float panelY = 100f;
        float panelWidth = 250f;
        float panelHeight = 120f;
        
        Vector4 panelBgColor = new Vector4(0.0f, 0.1f, 0.15f, 0.6f);
        Vector4 panelBorderColor = new Vector4(0.0f, 0.8f, 1.0f, 0.8f);
        
        // Background
        _renderer.DrawRectFilled(new Vector2(panelX, panelY), new Vector2(panelWidth, panelHeight), panelBgColor);
        
        // Border
        _renderer.DrawRect(new Vector2(panelX, panelY), new Vector2(panelWidth, panelHeight), panelBorderColor, 2f);
        
        // TODO: Add text rendering for ship info
        // For now, just draw placeholder bars
        
        // Hull integrity bar (example)
        float barX = panelX + 15f;
        float barY = panelY + 30f;
        float barWidth = panelWidth - 30f;
        float barHeight = 20f;
        
        Vector4 barBgColor = new Vector4(0.1f, 0.1f, 0.1f, 0.8f);
        Vector4 barFillColor = new Vector4(0.0f, 1.0f, 0.5f, 0.9f);
        
        // Background bar
        _renderer.DrawRectFilled(new Vector2(barX, barY), new Vector2(barWidth, barHeight), barBgColor);
        
        // Fill bar (80% for example)
        float fillPercent = 0.8f;
        _renderer.DrawRectFilled(new Vector2(barX, barY), new Vector2(barWidth * fillPercent, barHeight), barFillColor);
        
        // Bar border
        _renderer.DrawRect(new Vector2(barX, barY), new Vector2(barWidth, barHeight), panelBorderColor, 1.5f);
        
        // Energy bar
        barY += 35f;
        Vector4 energyBarColor = new Vector4(0.2f, 0.6f, 1.0f, 0.9f);
        _renderer.DrawRectFilled(new Vector2(barX, barY), new Vector2(barWidth, barHeight), barBgColor);
        _renderer.DrawRectFilled(new Vector2(barX, barY), new Vector2(barWidth * 0.6f, barHeight), energyBarColor);
        _renderer.DrawRect(new Vector2(barX, barY), new Vector2(barWidth, barHeight), panelBorderColor, 1.5f);
    }
    
    private void RenderRadar()
    {
        // Draw radar in bottom-left corner
        float radarX = 25f;
        float radarY = _screenHeight - 225f;
        float radarSize = 200f;
        float radarRadius = radarSize * 0.4f;
        
        Vector2 radarCenter = new Vector2(radarX + radarSize * 0.5f, radarY + radarSize * 0.5f);
        
        Vector4 radarBgColor = new Vector4(0.0f, 0.1f, 0.15f, 0.6f);
        Vector4 radarBorderColor = new Vector4(0.0f, 0.8f, 1.0f, 0.8f);
        Vector4 radarGridColor = new Vector4(0.0f, 0.6f, 0.8f, 0.4f);
        
        // Background
        _renderer.DrawRectFilled(new Vector2(radarX, radarY), new Vector2(radarSize, radarSize), radarBgColor);
        
        // Radar circle
        _renderer.DrawCircle(radarCenter, radarRadius, radarGridColor, 32, 2f);
        _renderer.DrawCircle(radarCenter, radarRadius * 0.5f, radarGridColor, 32, 1f);
        _renderer.DrawCircle(radarCenter, radarRadius * 0.25f, radarGridColor, 32, 1f);
        
        // Crosshair on radar
        _renderer.DrawLine(
            radarCenter - new Vector2(radarRadius, 0),
            radarCenter + new Vector2(radarRadius, 0),
            radarGridColor, 1f);
        _renderer.DrawLine(
            radarCenter - new Vector2(0, radarRadius),
            radarCenter + new Vector2(0, radarRadius),
            radarGridColor, 1f);
        
        // Center dot (player position)
        Vector4 playerDotColor = new Vector4(0.0f, 1.0f, 0.5f, 1.0f);
        _renderer.DrawCircleFilled(radarCenter, 3f, playerDotColor);
        
        // TODO: Draw other entities on radar
        
        // Border
        _renderer.DrawRect(new Vector2(radarX, radarY), new Vector2(radarSize, radarSize), radarBorderColor, 2f);
    }
}
