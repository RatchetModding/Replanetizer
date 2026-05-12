// Copyright (C) 2026, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibReplanetizer.LevelObjects;
using Replanetizer.Utils;

namespace Replanetizer.Renderer
{
    public class GrindPathRenderer : Renderer
    {
        private BillboardRenderer grindPathsRenderer;
        private WireframeRenderer splinesRenderer;

        public GrindPathRenderer(ShaderTable shaderTable)
        {
            grindPathsRenderer = new BillboardRenderer(shaderTable);
            splinesRenderer = new WireframeRenderer(shaderTable);
        }
        public override void Include<T>(T obj)
        {
            if (obj is GrindPath grindPath)
            {
                grindPathsRenderer.Include(grindPath);
                splinesRenderer.Include(grindPath.spline);

                return;
            }

            throw new NotImplementedException();
        }

        public override void Include<T>(List<T> list)
        {
            if (list is List<GrindPath> grindPaths)
            {
                foreach (GrindPath gp in grindPaths)
                {
                    Include(gp);
                }

                return;
            }

            throw new NotImplementedException();
        }

        public override void Render(RendererPayload payload)
        {
            // Objects need to be in "payload.selection" to be rendered
            // using the alternate color. Add the "dummy" Spline object
            // which correspond to every selected GrindPath to this list
            // such that both the billboard and the wireframe render in
            // alternate color (or none if unselected).
            List<Spline> selectedGPSplines = new List<Spline>();

            foreach (var selected in payload.selection)
            {
                if (selected is GrindPath gp)
                {
                    selectedGPSplines.Add(gp.spline);
                }
            }
            payload.selection.Add(selectedGPSplines);

            grindPathsRenderer.Render(payload);
            splinesRenderer.Render(payload);

            // Remove the dummy Spline objects from "payload.selection"
            // after rendering is done; for every purpose other than
            // rendering, they are NOT selected.
            payload.selection.Remove(selectedGPSplines);
        }

        public override void Dispose()
        {
            grindPathsRenderer?.Dispose();
            splinesRenderer?.Dispose();
        }
    }
}
