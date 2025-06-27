using System.Text.Json;

/// <summary>
/// Represents a user with an ID and a balance.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Account balance associated with the user.
    /// </summary>
    public int Balance { get; set; }

    /// <summary>
    /// Serializes the user object into a string in the form "(Id,Balance)".
    /// This is used as input to Merkle hashing to match BIP340-style leaf format.
    /// </summary>
    /// <returns>String representation of the user</returns>
    public override string ToString() => $"({Id},{Balance})"; // BIP340-compliant serialization
}

/// <summary>
/// Utility class for reading user data from a JSON file.
/// </summary>
public static class FileUtils
{
    /// <summary>
    /// Loads a list of users from a JSON file.
    /// </summary>
    /// <param name="filePath">The absolute or relative path to the JSON file.</param>
    /// <returns>A list of User objects parsed from the JSON content.</returns>
    public static List<User> LoadUsersFromFile(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<User>>(json);
    }
}
