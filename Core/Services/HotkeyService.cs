using System;
using System.Collections.Generic;
using ConfigButtonDisplay.Core.Interfaces;

namespace ConfigButtonDisplay.Core.Services;

public class HotkeyService : IHotkeyService
{
    private readonly Dictionary<string, (Action Callback, HotkeyConfig Config)> _registeredHotkeys = new();

    public void RegisterHotkey(string name, Action callback, HotkeyConfig config)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Hotkey name cannot be null or empty", nameof(name));

        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        if (config == null)
            throw new ArgumentNullException(nameof(config));

        // Check for conflicts before registering
        if (IsHotkeyConflict(config))
        {
            throw new InvalidOperationException($"Hotkey configuration conflicts with an existing hotkey");
        }

        _registeredHotkeys[name] = (callback, config);
    }

    public void UnregisterHotkey(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        _registeredHotkeys.Remove(name);
    }

    public bool IsHotkeyConflict(HotkeyConfig config)
    {
        if (config == null)
            return false;

        foreach (var (_, (_, existingConfig)) in _registeredHotkeys)
        {
            if (AreConfigsEqual(config, existingConfig))
                return true;
        }

        return false;
    }

    private bool AreConfigsEqual(HotkeyConfig config1, HotkeyConfig config2)
    {
        if (config1.Type != config2.Type)
            return false;

        if (config1.Type == "DoubleShift")
        {
            // For double-shift, they're equal if both are double-shift type
            return true;
        }

        if (config1.Type == "Combination")
        {
            // Compare modifiers and key
            if (config1.Key != config2.Key)
                return false;

            if (config1.Modifiers.Count != config2.Modifiers.Count)
                return false;

            var modifiers1 = new HashSet<string>(config1.Modifiers);
            var modifiers2 = new HashSet<string>(config2.Modifiers);

            return modifiers1.SetEquals(modifiers2);
        }

        return false;
    }

    public bool TryGetHotkey(string name, out Action? callback, out HotkeyConfig? config)
    {
        if (_registeredHotkeys.TryGetValue(name, out var hotkey))
        {
            callback = hotkey.Callback;
            config = hotkey.Config;
            return true;
        }

        callback = null;
        config = null;
        return false;
    }

    public IEnumerable<string> GetRegisteredHotkeyNames()
    {
        return _registeredHotkeys.Keys;
    }
}
