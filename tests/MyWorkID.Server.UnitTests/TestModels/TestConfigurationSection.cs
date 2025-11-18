using Xunit.Abstractions;

namespace MyWorkID.Server.UnitTests.TestModels;

/// <summary>
/// Serializable test helper that describes configuration sections as simple key/value pairs.
/// </summary>
public class TestConfigurationSection : IXunitSerializable
{
    private List<KeyValuePair<string, string?>> _entries = new();

    /// <summary>
    /// Produces a shallow clone and applies the provided changes for fluent customization.
    /// </summary>
    public TestConfigurationSection CloneWith(Action<TestConfigurationSection> changes)
    {
        var clone = Create(_entries.Select(entry => (entry.Key, entry.Value)).ToArray());
        changes(clone);
        return clone;
    }

    /// <summary>
    /// Adds a new key/value entry to the section.
    /// </summary>
    public TestConfigurationSection Add(string key, string? value)
    {
        _entries.Add(new KeyValuePair<string, string?>(key, value));
        return this;
    }

    /// <summary>
    /// Creates a section with the provided entries. Call without arguments for an empty section.
    /// </summary>
    public static TestConfigurationSection Create(params (string Key, string? Value)[] entries)
    {
        var section = new TestConfigurationSection();
        foreach (var (key, value) in entries)
        {
            section._entries.Add(new KeyValuePair<string, string?>(key, value));
        }

        return section;
    }

    /// <summary>
    /// Converts the section to the format expected by the configuration helper.
    /// </summary>
    public KeyValuePair<string, string?>[] ToKeyValuePairs() => _entries.ToArray();

    public void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue("Keys", _entries.Select(entry => entry.Key).ToArray());
        info.AddValue("Values", _entries.Select(entry => entry.Value).ToArray());
    }

    public void Deserialize(IXunitSerializationInfo info)
    {
        var keys = info.GetValue<string[]>("Keys");
        var values = info.GetValue<string[]>("Values");
        _entries = keys.Select((key, index) => new KeyValuePair<string, string?>(key, values[index])).ToList();
    }
}
