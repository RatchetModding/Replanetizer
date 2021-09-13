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
            public float Duration { get; set; }
            public int Repetitions { get; set;  }
            public bool IsFiring { get; set; }

            public void Reset()
            {
                Duration = 0f;
                Repetitions = 0;
                IsFiring = false;
            }
        }

        /// <summary>
        /// Wait for this many seconds before beginning to fire repeat actions
        /// </summary>
        public float HoldDelay { get; set; } = 0.5f;

        /// <summary>
        /// Delay between firing repeat actions
        /// </summary>
        public float RepeatDelay { get; set; } = 0.1f;

        /// <summary>
        /// Keys to watch for being held
        /// </summary>
        public ObservableCollection<Keys> WatchedKeys { get; }

        private readonly Dictionary<Keys, KeyHeldInfo> keysHeld = new();

        public KeyHeldHandler()
        {
            WatchedKeys = new ObservableCollection<Keys>();
            WatchedKeys.CollectionChanged += WatchedKeysOnCollectionChanged;
        }

        private void WatchedKeysOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (Keys item in e.OldItems)
                {
                    keysHeld.Remove(item);
                }
            if (e.NewItems != null)
                foreach (Keys item in e.NewItems)
                {
                    keysHeld.Add(item, new KeyHeldInfo());
                }
        }

        /// <summary>
        /// Update key held states. Call this method each ImGui tick.
        /// </summary>
        /// <param name="keyboardState">the window's keyboard state</param>
        /// <param name="deltaTime">the delta time since the last update</param>
        public void Update(KeyboardState keyboardState, float deltaTime)
        {
            foreach (var key in WatchedKeys)
            {
                var info = keysHeld[key];
                if (keyboardState.IsKeyDown(key))
                    UpdateKeyHeldInfo(info, deltaTime);
                else if (keyboardState.IsKeyReleased(key))
                    info.Reset();
            }
        }

        private void UpdateKeyHeldInfo(KeyHeldInfo info, float deltaTime)
        {
            info.IsFiring = false;

            info.Duration += deltaTime;
            if (info.Duration < HoldDelay)
            {
                // We're not repeating yet
                if (info.Repetitions == 0)
                {
                    // Fire for the initial press
                    info.Repetitions = 1;
                    info.IsFiring = true;
                }
                return;
            }

            var time = info.Duration - HoldDelay;
            var newRepetitions = (int) (time / RepeatDelay);
            if (newRepetitions != info.Repetitions)
            {
                // Fire once for this new repetition
                info.Repetitions = newRepetitions;
                info.IsFiring = true;
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
            return keysHeld.TryGetValue(key, out var info) && info.IsFiring;
        }
    }
}
