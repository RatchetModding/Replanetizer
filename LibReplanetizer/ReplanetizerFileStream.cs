// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

// In DEBUG builds, ReplanetizerFileStream is DebugFileStream, which tracks
// every read/write with its stack trace and byte range for debugging.
// In Release builds it is a plain FileStream with zero overhead.
#if DEBUG
global using ReplanetizerFileStream = LibReplanetizer.DebugFileStream;
#else
global using ReplanetizerFileStream = System.IO.FileStream;
#endif
