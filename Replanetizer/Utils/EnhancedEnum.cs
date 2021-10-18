// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Replanetizer.Utils
{
    public abstract class EnhancedEnum<T> where T : EnhancedEnum<T>
    {
        protected static readonly List<T> ENUM_VALUES = new();
        protected static readonly HashSet<int> KEYS_SET = new();

        public readonly int KEY;
        public readonly string HUMAN_NAME;

        public EnhancedEnum(int key, string humanName)
        {
            if (KEYS_SET.Contains(key))
                throw new ArgumentException($"Enum already contains a key {key}");
            KEYS_SET.Add(key);

            KEY = key;
            HUMAN_NAME = humanName;
            ENUM_VALUES.Add((T) this);
        }

        public static T operator ++(EnhancedEnum<T> a)
        {
            int newIdx = (ENUM_VALUES.IndexOf((T) a) + 1) % ENUM_VALUES.Count;
            return ENUM_VALUES[newIdx];
        }

        public static ReadOnlyCollection<T> GetValues()
        {
            return ENUM_VALUES.AsReadOnly();
        }

        public static T? GetByKey(int key)
        {
            foreach (T t in ENUM_VALUES)
                if (t.KEY == key)
                    return t;
            return null;
        }

        public override string ToString()
        {
            return HUMAN_NAME;
        }
    }
}
