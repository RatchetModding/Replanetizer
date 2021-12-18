// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Replanetizer.Utils
{
    /// <summary>
    /// Keybind actions for this application
    /// </summary>
    public enum Keybinds
    {
        // Camera movement
        MoveForward,
        MoveBackward,
        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,
        MoveFastModifier,
        RotateRight,
        RotateLeft,
        RotateUp,
        RotateDown,

        // Tools
        ToolNone,
        ToolTranslate,
        ToolRotation,
        ToolScaling,
        ToolVertexTranslator,

        // Misc
        DeleteObject,
        MultiSelectModifier
    }

    /// <summary>
    /// Represents a combination of modifier keys and a base key (e.g. "W",
    /// "Ctrl+Z", "Ctrl+Shift+P")
    /// </summary>
    public readonly struct KeyCombo
    {
        public Keys[] modifiers { get; }
        public Keys key { get; }

        /// <summary>
        /// A KeyCombo with only one key
        /// </summary>
        public KeyCombo(Keys key)
        {
            this.key = key;
            modifiers = Array.Empty<Keys>();
        }

        /// <summary>
        /// A KeyCombo with one base key and one or more modifier keys (Ctrl,
        /// Shift, Alt)
        /// </summary>
        /// <param name="modifiersAndKey">
        /// a list of multiple modifier keys and one base key
        /// </param>
        public KeyCombo(params Keys[] modifiersAndKey)
        {
            key = modifiersAndKey[^1];
            modifiers = modifiersAndKey[..^1];
        }

        /// <summary>
        /// Format a key into a human-readable form
        /// </summary>
        private static string FormatKey(Keys key)
        {
            if (key is Keys.LeftControl)
                return "Ctrl";
            if (key is Keys.RightControl)
                return "RCtrl";
            if (key is Keys.LeftShift)
                return "Shift";
            if (key is Keys.RightShift)
                return "RShift";
            if (key is Keys.LeftAlt)
                return "Alt";
            if (key is Keys.RightAlt)
                return "RAlt";

            char keyChar = (char) key;
            if (keyChar >= 0x21 && keyChar <= 0x7e)
                return keyChar.ToString().ToUpper();

            return key.ToString();
        }

        /// <summary>
        /// Format this KeyCombo into a human-readable string (e.g. "Ctrl+P")
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var mod in modifiers)
            {
                sb.Append(FormatKey(mod));
                sb.Append('+');
            }
            return sb.Append(FormatKey(key)).ToString();
        }

        /// <summary>
        /// Get whether this KeyCombo is currently held down
        /// </summary>
        public bool IsDown(Window wnd)
        {
            foreach (Keys modifier in modifiers)
                if (!wnd.IsKeyDown(modifier))
                    return false;
            return wnd.IsKeyDown(key);
        }

        /// <summary>
        /// Get whether this KeyCombo just started being pressed
        /// </summary>
        public bool IsPressed(Window wnd)
        {
            foreach (Keys modifier in modifiers)
                if (!wnd.IsKeyDown(modifier) && !wnd.IsKeyPressed(modifier))
                    return false;
            return wnd.IsKeyPressed(key);
        }

        /// <summary>
        /// Get whether this KeyCombo just finished being pressed
        /// </summary>
        public bool IsReleased(Window wnd)
        {
            foreach (Keys modifier in modifiers)
                if (!wnd.IsKeyDown(modifier) && !wnd.IsKeyReleased(modifier))
                    return false;
            return wnd.IsKeyReleased(key);
        }
    }

    /// <summary>
    /// A class which handles the mapping of keypresses to keybinds. This allows
    /// you to define actions in a key-agnostic way, and makes it easier to
    /// change the keys associated with a given action.
    /// </summary>
    public class Keymap
    {
        public Window wnd { get; set; }
        public IDictionary<Keybinds, KeyCombo[]> keymap { get; set; }

        /// <summary>
        /// The default keymap for this application
        /// </summary>
        public static readonly ImmutableDictionary<Keybinds, KeyCombo[]> DEFAULT_KEYMAP =
            new Dictionary<Keybinds, KeyCombo[]>
            {
                // Camera movement
                { Keybinds.MoveForward, new[] { new KeyCombo(Keys.W) } },
                { Keybinds.MoveBackward, new[] { new KeyCombo(Keys.S) } },
                { Keybinds.MoveLeft, new[] { new KeyCombo(Keys.A) } },
                { Keybinds.MoveRight, new[] { new KeyCombo(Keys.D) } },
                { Keybinds.MoveUp, new[] { new KeyCombo(Keys.E) } },
                { Keybinds.MoveDown, new[] { new KeyCombo(Keys.Q) } },
                { Keybinds.MoveFastModifier, new[] { new KeyCombo(Keys.LeftShift) } },
                { Keybinds.RotateRight, new[] { new KeyCombo(Keys.Right) } },
                { Keybinds.RotateLeft, new[] { new KeyCombo(Keys.Left) } },
                { Keybinds.RotateUp, new[] { new KeyCombo(Keys.Up) } },
                { Keybinds.RotateDown, new[] { new KeyCombo(Keys.Down) } },
                // Tools
                { Keybinds.ToolNone, new[] { new KeyCombo(Keys.D5) } },
                { Keybinds.ToolTranslate, new[] { new KeyCombo(Keys.D1) } },
                { Keybinds.ToolRotation, new[] { new KeyCombo(Keys.D2) } },
                { Keybinds.ToolScaling, new[] { new KeyCombo(Keys.D3) } },
                { Keybinds.ToolVertexTranslator, new[] { new KeyCombo(Keys.D4) } },
                // Misc
                { Keybinds.DeleteObject, new[] { new KeyCombo(Keys.Delete) } },
                {
                    Keybinds.MultiSelectModifier,
                    new[] { new KeyCombo(Keys.LeftShift), new KeyCombo(Keys.RightShift) }
                }
            }.ToImmutableDictionary();

        public Keymap(Window wnd, IDictionary<Keybinds, KeyCombo[]> keymap)
        {
            this.wnd = wnd;
            this.keymap = keymap;
        }

        /// <summary>
        /// Get whether <param name="keybind"/> is currently held down
        /// </summary>
        public bool IsDown(Keybinds keybind)
        {
            foreach (KeyCombo keyCombo in keymap[keybind])
            {
                if (keyCombo.IsDown(wnd))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get whether <paramref name="keybind"/> just started being pressed
        /// </summary>
        public bool IsPressed(Keybinds keybind)
        {
            foreach (KeyCombo keyCombo in keymap[keybind])
            {
                if (keyCombo.IsPressed(wnd))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get whether <paramref name="keybind"/> just finished being pressed
        /// </summary>
        public bool IsReleased(Keybinds keybind)
        {
            foreach (KeyCombo keyCombo in keymap[keybind])
            {
                if (keyCombo.IsReleased(wnd))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get the human-readable name of <paramref name="keybind"/> under the
        /// current keymap (e.g. "Ctrl+Y / Ctrl+Shift+Z")
        /// </summary>
        public string NameOf(Keybinds keybind)
        {
            if (!keymap.TryGetValue(keybind, out var keyCombos))
                return "ERR_NO_KEY";
            return new StringBuilder().AppendJoin(" / ", keyCombos).ToString();
        }
    }
}
