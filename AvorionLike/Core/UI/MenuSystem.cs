using ImGuiNET;
using System.Numerics;

namespace AvorionLike.Core.UI;

/// <summary>
/// Menu system for managing game menus (main menu, pause menu, settings, etc.)
/// </summary>
public class MenuSystem
{
    private readonly GameEngine _gameEngine;
    private MenuState _currentMenu = MenuState.None;
    private bool _showMainMenu = false;
    private bool _showPauseMenu = false;
    private bool _showSettingsMenu = false;
    
    // Settings values
    private float _masterVolume = 1.0f;
    private float _musicVolume = 0.8f;
    private float _sfxVolume = 0.9f;
    private int _targetFPS = 60;
    private bool _vsync = true;
    private int _selectedResolution = 0;
    private readonly string[] _resolutions = new[] { "1280x720", "1920x1080", "2560x1440", "3840x2160" };
    
    public bool IsMenuOpen => _currentMenu != MenuState.None;
    
    public enum MenuState
    {
        None,
        MainMenu,
        PauseMenu,
        Settings
    }
    
    public MenuSystem(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }
    
    public void ShowMainMenu()
    {
        _currentMenu = MenuState.MainMenu;
        _showMainMenu = true;
    }
    
    public void ShowPauseMenu()
    {
        _currentMenu = MenuState.PauseMenu;
        _showPauseMenu = true;
    }
    
    public void HideMenu()
    {
        _currentMenu = MenuState.None;
        _showMainMenu = false;
        _showPauseMenu = false;
        _showSettingsMenu = false;
    }
    
    public void Render()
    {
        if (_showMainMenu)
            RenderMainMenu();
        
        if (_showPauseMenu)
            RenderPauseMenu();
        
        if (_showSettingsMenu)
            RenderSettingsMenu();
    }
    
    private void RenderMainMenu()
    {
        // Center the menu window
        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(new Vector2(io.DisplaySize.X * 0.5f, io.DisplaySize.Y * 0.5f), ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(400, 500), ImGuiCond.FirstUseEver);
        
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoResize | 
                                       ImGuiWindowFlags.NoCollapse |
                                       ImGuiWindowFlags.NoMove;
        
        if (ImGui.Begin("AvorionLike - Main Menu", ref _showMainMenu, windowFlags))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(10, 10));
            
            ImGui.Dummy(new Vector2(0, 20));
            
            // Title
            var titleText = "AVORION-LIKE";
            var titleSize = ImGui.CalcTextSize(titleText);
            ImGui.SetCursorPosX((ImGui.GetWindowWidth() - titleSize.X) * 0.5f);
            ImGui.TextColored(new Vector4(0.3f, 0.7f, 1.0f, 1.0f), titleText);
            
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 20));
            
            // Menu buttons
            Vector2 buttonSize = new Vector2(ImGui.GetWindowWidth() - 40, 40);
            
            ImGui.SetCursorPosX(20);
            if (ImGui.Button("New Game", buttonSize))
            {
                Console.WriteLine("Starting new game...");
                HideMenu();
            }
            
            ImGui.SetCursorPosX(20);
            if (ImGui.Button("Continue", buttonSize))
            {
                Console.WriteLine("Continuing game...");
                HideMenu();
            }
            
            ImGui.SetCursorPosX(20);
            if (ImGui.Button("Load Game", buttonSize))
            {
                Console.WriteLine("Loading game...");
            }
            
            ImGui.SetCursorPosX(20);
            if (ImGui.Button("Settings", buttonSize))
            {
                _showMainMenu = false;
                _showSettingsMenu = true;
                _currentMenu = MenuState.Settings;
            }
            
            ImGui.Dummy(new Vector2(0, 20));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 10));
            
            ImGui.SetCursorPosX(20);
            if (ImGui.Button("Exit", buttonSize))
            {
                Console.WriteLine("Exiting game...");
                Environment.Exit(0);
            }
            
            ImGui.PopStyleVar();
        }
        ImGui.End();
        
        if (!_showMainMenu)
            HideMenu();
    }
    
    private void RenderPauseMenu()
    {
        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(new Vector2(io.DisplaySize.X * 0.5f, io.DisplaySize.Y * 0.5f), ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(350, 400), ImGuiCond.FirstUseEver);
        
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoResize | 
                                       ImGuiWindowFlags.NoCollapse |
                                       ImGuiWindowFlags.NoMove;
        
        if (ImGui.Begin("Game Paused", ref _showPauseMenu, windowFlags))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(10, 10));
            
            ImGui.Dummy(new Vector2(0, 20));
            
            var titleText = "PAUSED";
            var titleSize = ImGui.CalcTextSize(titleText);
            ImGui.SetCursorPosX((ImGui.GetWindowWidth() - titleSize.X) * 0.5f);
            ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.2f, 1.0f), titleText);
            
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 20));
            
            Vector2 buttonSize = new Vector2(ImGui.GetWindowWidth() - 40, 40);
            
            ImGui.SetCursorPosX(20);
            if (ImGui.Button("Resume", buttonSize))
            {
                HideMenu();
            }
            
            ImGui.SetCursorPosX(20);
            if (ImGui.Button("Settings", buttonSize))
            {
                _showPauseMenu = false;
                _showSettingsMenu = true;
                _currentMenu = MenuState.Settings;
            }
            
            ImGui.SetCursorPosX(20);
            if (ImGui.Button("Save Game", buttonSize))
            {
                Console.WriteLine("Saving game...");
                // TODO: Implement save functionality
            }
            
            ImGui.Dummy(new Vector2(0, 20));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 10));
            
            ImGui.SetCursorPosX(20);
            if (ImGui.Button("Main Menu", buttonSize))
            {
                _showPauseMenu = false;
                _showMainMenu = true;
                _currentMenu = MenuState.MainMenu;
            }
            
            ImGui.PopStyleVar();
        }
        ImGui.End();
        
        if (!_showPauseMenu && _currentMenu == MenuState.PauseMenu)
            HideMenu();
    }
    
    private void RenderSettingsMenu()
    {
        var io = ImGui.GetIO();
        ImGui.SetNextWindowPos(new Vector2(io.DisplaySize.X * 0.5f, io.DisplaySize.Y * 0.5f), ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(500, 600), ImGuiCond.FirstUseEver);
        
        if (ImGui.Begin("Settings", ref _showSettingsMenu))
        {
            if (ImGui.BeginTabBar("SettingsTabs"))
            {
                if (ImGui.BeginTabItem("Graphics"))
                {
                    ImGui.Dummy(new Vector2(0, 10));
                    
                    ImGui.Text("Resolution:");
                    ImGui.Combo("##Resolution", ref _selectedResolution, _resolutions, _resolutions.Length);
                    
                    ImGui.Dummy(new Vector2(0, 10));
                    
                    ImGui.Checkbox("VSync", ref _vsync);
                    
                    ImGui.Dummy(new Vector2(0, 10));
                    
                    ImGui.Text($"Target FPS: {_targetFPS}");
                    ImGui.SliderInt("##TargetFPS", ref _targetFPS, 30, 144);
                    
                    ImGui.Dummy(new Vector2(0, 20));
                    ImGui.Separator();
                    ImGui.Dummy(new Vector2(0, 10));
                    
                    ImGui.TextWrapped("Note: Some settings require a restart to take effect.");
                    
                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Audio"))
                {
                    ImGui.Dummy(new Vector2(0, 10));
                    
                    ImGui.Text($"Master Volume: {_masterVolume * 100:F0}%");
                    ImGui.SliderFloat("##MasterVolume", ref _masterVolume, 0.0f, 1.0f);
                    
                    ImGui.Dummy(new Vector2(0, 10));
                    
                    ImGui.Text($"Music Volume: {_musicVolume * 100:F0}%");
                    ImGui.SliderFloat("##MusicVolume", ref _musicVolume, 0.0f, 1.0f);
                    
                    ImGui.Dummy(new Vector2(0, 10));
                    
                    ImGui.Text($"SFX Volume: {_sfxVolume * 100:F0}%");
                    ImGui.SliderFloat("##SFXVolume", ref _sfxVolume, 0.0f, 1.0f);
                    
                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Controls"))
                {
                    ImGui.Dummy(new Vector2(0, 10));
                    
                    ImGui.Text("Camera Controls:");
                    ImGui.BulletText("WASD - Move horizontally");
                    ImGui.BulletText("Space - Move up");
                    ImGui.BulletText("Shift - Move down");
                    ImGui.BulletText("Mouse - Look around");
                    
                    ImGui.Dummy(new Vector2(0, 10));
                    
                    ImGui.Text("UI Controls:");
                    ImGui.BulletText("F1 - Toggle Debug Info");
                    ImGui.BulletText("F2 - Toggle Entity List");
                    ImGui.BulletText("F3 - Toggle Resources");
                    ImGui.BulletText("ESC - Pause Menu");
                    
                    ImGui.EndTabItem();
                }
                
                ImGui.EndTabBar();
            }
            
            ImGui.Dummy(new Vector2(0, 20));
            
            // Bottom buttons
            Vector2 buttonSize = new Vector2(150, 30);
            float buttonSpacing = 10;
            float totalWidth = buttonSize.X * 2 + buttonSpacing;
            ImGui.SetCursorPosX((ImGui.GetWindowWidth() - totalWidth) * 0.5f);
            
            if (ImGui.Button("Apply", buttonSize))
            {
                ApplySettings();
            }
            
            ImGui.SameLine(0, buttonSpacing);
            
            if (ImGui.Button("Back", buttonSize))
            {
                _showSettingsMenu = false;
                if (_currentMenu == MenuState.PauseMenu)
                    _showPauseMenu = true;
                else
                    _showMainMenu = true;
            }
        }
        ImGui.End();
        
        if (!_showSettingsMenu && _currentMenu == MenuState.Settings)
        {
            _currentMenu = MenuState.None;
        }
    }
    
    private void ApplySettings()
    {
        Console.WriteLine("Applying settings...");
        Console.WriteLine($"  Resolution: {_resolutions[_selectedResolution]}");
        Console.WriteLine($"  VSync: {_vsync}");
        Console.WriteLine($"  Target FPS: {_targetFPS}");
        Console.WriteLine($"  Master Volume: {_masterVolume * 100:F0}%");
        Console.WriteLine($"  Music Volume: {_musicVolume * 100:F0}%");
        Console.WriteLine($"  SFX Volume: {_sfxVolume * 100:F0}%");
        
        // TODO: Apply settings to game engine
    }
    
    public void HandleInput()
    {
        // Toggle pause menu with ESC (only if not in main menu)
        if (ImGui.IsKeyPressed(ImGuiKey.Escape) && _currentMenu != MenuState.MainMenu)
        {
            if (_currentMenu == MenuState.PauseMenu)
                HideMenu();
            else if (_currentMenu == MenuState.None)
                ShowPauseMenu();
        }
    }
}
