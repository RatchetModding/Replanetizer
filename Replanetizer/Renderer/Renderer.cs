// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using LibReplanetizer.LevelObjects;
using Replanetizer.Tools;
using Replanetizer.Utils;

namespace Replanetizer.Renderer
{
    public abstract class Renderer : IDisposable
    {
        /// <summary>
        /// Signals the renderer to include the specified object.
        ///
        /// Note that whether this adds the object to an internal list of objects
        /// to render or whether it replaces the currently included object is
        /// implementation dependent.
        /// </summary>
        public abstract void Include<T>(T obj);
        public abstract void Include<T>(List<T> list);
        public abstract void Render(RendererPayload payload);

        public abstract void Dispose();
    }
}
