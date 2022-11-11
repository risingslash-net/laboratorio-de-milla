using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using BepInEx;
using System.Collections.Generic;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public class CustomControls : MonoBehaviour
{
    public static Dictionary<ConfigEntry<string>, KeyMapping> DictHotkeyPrefToKeyMappings;
    public static ManualLogSource LocalLog;
    public void Start()
    {
        LocalLog = BepInEx.Logging.Logger.CreateLogSource("RisingSlashCustomControls");
        if (DictHotkeyPrefToKeyMappings == null)
        {
            DictHotkeyPrefToKeyMappings = new Dictionary<ConfigEntry<string>, KeyMapping>();
        }
    }

    public static void Add(ConfigEntry<string> ce)
    {
        if (DictHotkeyPrefToKeyMappings == null)
        {
            DictHotkeyPrefToKeyMappings = new Dictionary<ConfigEntry<string>, KeyMapping>();
        }
        
        DictHotkeyPrefToKeyMappings.Add(ce, InputControl.setKey(ce.Value, KeyboardInputFromString(ce.Value)));
    }
    
    public static bool GetButtonDown(ConfigEntry<string> ce)
    {
        if (ce == null)
        {
            LocalLog.LogInfo(String.Format("ce appears to be null: {0}", ce));
            return false;
        }
        else if (ce.Value == null)
        {
            LocalLog.LogInfo(String.Format("ce exists and is set, but value appears to be null: {0} -> {1}", ce.Definition.Key, ce.Value));
            return false;
        }

        return InputControl.GetButtonDown(DictHotkeyPrefToKeyMappings[ce], true);
    }
    
    public static bool GetButton(ConfigEntry<string> ce)
    {
        if (ce == null)
        {
            LocalLog.LogInfo(String.Format("ce appears to be null: {0}", ce));
            return false;
        }
        else if (ce.Value == null)
        {
            LocalLog.LogInfo(String.Format("ce exists and  is set, but value appears to be null: {0} -> {1}", ce.Definition.Key, ce.Value));
            return false;
        }

        return InputControl.GetButton(DictHotkeyPrefToKeyMappings[ce], true);
    }

    public static KeyboardInput KeyboardInputFromString(string value)
    {
        if (value == null)
        {
            return null;
        }


        var modifiers = ModifiersFromString(value);
        try
        {
            String baseInput = value;
            if (modifiers != KeyModifier.NoModifier)
            {
                baseInput = value.Substring(value.LastIndexOf("+") + 1);
                LocalLog.LogInfo(String.Format("Input base interpreted as {0} -> {1}\n", value, baseInput));
            }

             
            return new KeyboardInput(KeyCodeFromString(baseInput), modifiers);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void OnLevelWasLoaded(int level)
    {
        try
        {
            
        }
        catch (Exception e)
        {
            LocalLog.LogInfo(e.Message + e.StackTrace);
        }
    }

    public static KeyCode KeyCodeFromString(string strKeyCode)
    {
        return (KeyCode) Enum.Parse(typeof(KeyCode), strKeyCode);
    }

    public static KeyModifier ModifiersFromString(string value)
    {
        KeyModifier keyMod = KeyModifier.NoModifier;

        if (value == null)
        {
            return keyMod;
        }

        int maxModifiers = 7;
        var strCtrlP = "Ctrl+";
        var strAltP = "Alt+";
        var strShiftP = "Shift+";
        
        for (int i = 0; i < maxModifiers; i++)
        {
            if (value.Contains(strCtrlP))
            {
                value = value.Replace(strCtrlP, "");
                keyMod |= KeyModifier.Ctrl;
                continue;
            }
            if (value.Contains(strAltP))
            {
                value = value.Replace(strAltP, "");
                keyMod |= KeyModifier.Alt;
                continue;
            }
            if (value.Contains(strShiftP))
            {
                value = value.Replace(strShiftP, "");
                keyMod |= KeyModifier.Shift;
                continue;
            }
            break;
        }

        return keyMod;
    }

    public static string GetBindingString()
    {
        string strControlListing = "Current Hotkey Bindings:\n";
        
        // Optimization option: Possible optimization by caching a List version of these pairs instead of using the dictionary.
        foreach (var configBinding in DictHotkeyPrefToKeyMappings.Keys)
        {
            strControlListing += String.Format("{0} -> {1}\n", configBinding.Value, configBinding.Definition.Key);
        }

        return strControlListing;
    }
    
    public static string GetBindingString(int start, int end)
    {
        string strControlListing = "Current Hotkey Bindings:\n";
        int lineCount = 1;
        // Optimization option: Possible optimization by caching a List version of these pairs instead of using the dictionary.
        foreach (var configBinding in DictHotkeyPrefToKeyMappings.Keys)
        {
            if (lineCount >= start && lineCount <= end)
            {
                strControlListing += String.Format("{1} -> {0}\n", configBinding.Value, configBinding.Definition.Key);
            }

            lineCount++;
        }

        return strControlListing;
    }
    
    public static ConfigEntry<string> CreateEntryAndBindHotkey(string identifier,
        string default_value, ConfigFile conf)
    {
        var configHotkey = conf.Bind("Keybinds",      // The section under which the option is shown
            identifier,  // The key of the configuration option in the configuration file
            default_value, // The default value
            $"A custom input binding for {identifier}"); // Description of the option to show in the config file
        //FP2TrainerCustomHotkeys.Add(melonPrefEntry);
        CustomControls.Add(configHotkey);
        return configHotkey;
    }
}