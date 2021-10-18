// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using Replanetizer.Utils;

namespace Replanetizer.Tools
{
    public class PivotPositioning : EnhancedEnum<PivotPositioning>
    {
        public static readonly PivotPositioning Median = new(0, "Median");
        public static readonly PivotPositioning IndividualOrigins = new(1, "Individual Origins");

        public PivotPositioning(int key, string humanName) : base(key, humanName)
        {
        }
    }
}
