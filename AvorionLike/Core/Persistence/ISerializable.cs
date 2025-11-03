namespace AvorionLike.Core.Persistence;

/// <summary>
/// Interface for objects that can be serialized to/from save files
/// </summary>
public interface ISerializable
{
    /// <summary>
    /// Serialize the object to a dictionary of key-value pairs
    /// </summary>
    Dictionary<string, object> Serialize();
    
    /// <summary>
    /// Deserialize the object from a dictionary of key-value pairs
    /// </summary>
    void Deserialize(Dictionary<string, object> data);
}
