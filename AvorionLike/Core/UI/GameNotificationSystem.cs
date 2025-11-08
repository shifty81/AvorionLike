using System.Numerics;
using ImGuiNET;

namespace AvorionLike.Core.UI;

/// <summary>
/// Simple notification system for in-game events
/// </summary>
public class GameNotificationSystem
{
    private readonly List<Notification> _notifications = new();
    private readonly int _maxNotifications = 5;
    private readonly float _notificationDuration = 5f; // seconds
    
    public void AddNotification(string message, NotificationType type = NotificationType.Info)
    {
        _notifications.Add(new Notification
        {
            Message = message,
            Type = type,
            TimeRemaining = _notificationDuration
        });
        
        // Keep only recent notifications
        while (_notifications.Count > _maxNotifications)
        {
            _notifications.RemoveAt(0);
        }
    }
    
    public void Update(float deltaTime)
    {
        // Update timers and remove expired notifications
        for (int i = _notifications.Count - 1; i >= 0; i--)
        {
            _notifications[i].TimeRemaining -= deltaTime;
            if (_notifications[i].TimeRemaining <= 0)
            {
                _notifications.RemoveAt(i);
            }
        }
    }
    
    public void Render(float screenWidth, float screenHeight)
    {
        if (_notifications.Count == 0) return;
        
        float notificationX = screenWidth / 2 - 200f;
        float notificationY = 80f;
        float notificationWidth = 400f;
        float notificationHeight = 40f;
        float spacing = 5f;
        
        for (int i = 0; i < _notifications.Count; i++)
        {
            var notification = _notifications[i];
            float y = notificationY + (notificationHeight + spacing) * i;
            
            // Calculate fade effect based on remaining time
            float alpha = Math.Min(1f, notification.TimeRemaining / 1f);
            
            ImGui.SetNextWindowPos(new Vector2(notificationX, y));
            ImGui.SetNextWindowSize(new Vector2(notificationWidth, notificationHeight));
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.1f, 0.15f, 0.8f * alpha));
            ImGui.PushStyleColor(ImGuiCol.Border, GetNotificationColor(notification.Type, alpha));
            
            string windowId = $"##Notification{i}";
            if (ImGui.Begin(windowId, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | 
                            ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, GetNotificationTextColor(notification.Type, alpha));
                
                // Center text vertically
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 8f);
                ImGui.Text(notification.Message);
                
                ImGui.PopStyleColor();
            }
            
            ImGui.End();
            ImGui.PopStyleColor(2);
        }
    }
    
    private Vector4 GetNotificationColor(NotificationType type, float alpha)
    {
        return type switch
        {
            NotificationType.Success => new Vector4(0.0f, 0.8f, 0.2f, 0.8f * alpha),
            NotificationType.Warning => new Vector4(1.0f, 0.6f, 0.0f, 0.8f * alpha),
            NotificationType.Error => new Vector4(1.0f, 0.2f, 0.2f, 0.8f * alpha),
            _ => new Vector4(0.0f, 0.8f, 1.0f, 0.8f * alpha) // Info
        };
    }
    
    private Vector4 GetNotificationTextColor(NotificationType type, float alpha)
    {
        return type switch
        {
            NotificationType.Success => new Vector4(0.8f, 1.0f, 0.8f, alpha),
            NotificationType.Warning => new Vector4(1.0f, 1.0f, 0.7f, alpha),
            NotificationType.Error => new Vector4(1.0f, 0.8f, 0.8f, alpha),
            _ => new Vector4(0.9f, 0.9f, 0.9f, alpha) // Info
        };
    }
    
    private class Notification
    {
        public string Message { get; set; } = "";
        public NotificationType Type { get; set; }
        public float TimeRemaining { get; set; }
    }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}
