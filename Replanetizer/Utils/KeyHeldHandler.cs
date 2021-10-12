// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

#nullable enable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Replanetizer.Utils
{
    public class KeyHeldHandler
    {
        private class KeyHeldInfo
        {
            public float duration { get; set; }
            public int repetitions { get; set; }
            public bool isFiring { get; set; }

            public void Reset()
            {
                duration = 0f;
                repetitions = 0;
                isFiring = false;
            }
        }

        /// <summary>
        /// Wait for this many seconds before beginning to fire repeat actions
        /// </summary>
        public float holdDelay { get; set; } = 0.5f;

        /// <summary>
        /// Delay between firing repeat actions
        /// </summary>
        public float repeatDelay { get; set; } = 0.1f;

        /// <summary>
        /// Keys to watch for being held
        /// </summary>
        public ObservableCollection<Keys> watchedKeys { get; }

        private readonly Dictionary<Keys, KeyHeldInfo> KEYS_HELD = new();

        public KeyHeldHandler()
        {
            watchedKeys = new ObservableCollection<Keys>();
            watchedKeys.CollectionChanged += WatchedKeysOnCollectionChanged;
        }

        private void WatchedKeysOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (Keys item in e.OldItems)
                {
                    KEYS_HELD.Remove(item);
                }
            if (e.NewItems != null)
                foreach (Keys item in e.NewItems)
                {
                    KEYS_HELD.Add(item, new KeyHeldInfo());
                }
        }

        /// <summary>
        /// Update key held states. Call this method each ImGui tick.
        /// </summary>
        /// <param name="keyboardState">the window's keyboard state</param>
        /// <param name="deltaTime">the delta time since the last update</param>
        public void Update(KeyboardState keyboardState, float deltaTime)
        {
            foreach (var key in watchedKeys)
            {
                var info = KEYS_HELD[key];
                if (keyboardState.IsKeyDown(key))
                    UpdateKeyHeldInfo(info, deltaTime);
                else if (keyboardState.IsKeyReleased(key))
                    info.Reset();
            }
        }

        private void UpdateKeyHeldInfo(KeyHeldInfo info, float deltaTime)
        {
            info.isFiring = false;

            info.duration += deltaTime;
            if (info.duration < holdDelay)
            {
                // We're not repeating yet
                if (info.repetitions == 0)
                {
                    // Fire for the initial press
                    info.repetitions = 1;
                    info.isFiring = true;
                }
                return;
            }

            var time = info.duration - holdDelay;
            var newRepetitions = (int) (time / repeatDelay);
            if (newRepetitions != info.repetitions)
            {
                // Fire once for this new repetition
                info.repetitions = newRepetitions;
                info.isFiring = true;
            }
        }

        /// <summary>
        /// Whether this key is being held. Will fire once on the first press,
        /// then repetitively fires after HoldDelay every RepeatDelay seconds.
        /// </summary>
        /// <param name="key">the key to test</param>
        /// <returns></returns>
        public bool IsKeyHeld(Keys key)
        {
            return KEYS_HELD.TryGetValue(key, out var info) && info.isFiring;
        }
    }
}
