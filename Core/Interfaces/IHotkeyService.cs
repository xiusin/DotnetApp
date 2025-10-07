using System;
using System.Collections.Generic;

namespace ConfigButtonDisplay.Core.Interfaces;

public interface IHotkeyService
{
    void RegisterHotkey(string name, Action callback, HotkeyConfig config);
    void UnregisterHotkey(string name);
    bool IsHotkeyConflict(HotkeyConfig config);
}

public class HotkeyConfig
{
    public string Type { get; set; } = "DoubleShift";  // DoubleShift, Combination
    public List<string> Modifiers { get; set; } = new();
    public string Key { get; set; } = "";
    public int DoubleKeyInterval { get; set; } = 500;
}
