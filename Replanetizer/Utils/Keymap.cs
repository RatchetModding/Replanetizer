// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Replanetizer.Utils
{
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

        // Tools
        ToolNone,
        ToolTranslate,
        ToolRotation,
        ToolScaling,
        ToolVertexTranslator,

        // Misc
        Delete,
        MultiSelectModifier,
    }

    public readonly struct KeyCombo
    {
        public Keys[] modifiers { get; }
        public Keys key { get; }

        public KeyCombo(Keys key, params Keys[] modifiers)
        {
            this.key = key;
            this.modifiers = modifiers;
        }

        public bool IsDown(Window wnd)
        {
            foreach (Keys modifier in modifiers)
                if (!wnd.IsKeyDown(modifier))
                    return false;
            return wnd.IsKeyDown(key);
        }

        public bool IsPressed(Window wnd)
        {
            foreach (Keys modifier in modifiers)
                if (!wnd.IsKeyDown(modifier) && !wnd.IsKeyPressed(modifier))
                    return false;
            return wnd.IsKeyPressed(key);
        }

        public bool IsReleased(Window wnd)
        {
            foreach (Keys modifier in modifiers)
                if (!wnd.IsKeyDown(modifier) && !wnd.IsKeyReleased(modifier))
                    return false;
            return wnd.IsKeyReleased(key);
        }
    }

    public class Keymap
    {
        public Window wnd { get; set; }
        public IDictionary<Keybinds, KeyCombo[]> keymap { get; set; }

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
                // Tools
                { Keybinds.ToolNone, new[] { new KeyCombo(Keys.D5) } },
                { Keybinds.ToolTranslate, new[] { new KeyCombo(Keys.D1) } },
                { Keybinds.ToolRotation, new[] { new KeyCombo(Keys.D2) } },
                { Keybinds.ToolScaling, new[] { new KeyCombo(Keys.D3) } },
                { Keybinds.ToolVertexTranslator, new[] { new KeyCombo(Keys.D4) } },
                // Misc
                { Keybinds.Delete, new[] { new KeyCombo(Keys.Delete) } },
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

        public bool IsKeybindDown(Keybinds keybind)
        {
            foreach (KeyCombo keyCombo in keymap[keybind])
            {
                if (keyCombo.IsDown(wnd))
                    return true;
            }

            return false;
        }

        public bool IsKeybindPressed(Keybinds keybind)
        {
            foreach (KeyCombo keyCombo in keymap[keybind])
            {
                if (keyCombo.IsPressed(wnd))
                    return true;
            }

            return false;
        }

        public bool IsKeybindReleased(Keybinds keybind)
        {
            foreach (KeyCombo keyCombo in keymap[keybind])
            {
                if (keyCombo.IsReleased(wnd))
                    return true;
            }

            return false;
        }
    }
}
