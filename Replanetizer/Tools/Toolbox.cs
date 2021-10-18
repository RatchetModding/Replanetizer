// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;

namespace Replanetizer.Tools
{
    public class ToolChangedEventArgs : EventArgs
    {
        public ToolType toolType { get; }

        public ToolChangedEventArgs(ToolType toolType)
        {
            this.toolType = toolType;
        }
    }

    public class Toolbox
    {
        public Tool? tool => _tool;
        public ToolType type => _type;
        public TransformationSpace transformationSpace { get; set; } = TransformationSpace.Global;
        public PivotPositioning pivotPositioning { get; set; } = PivotPositioning.Median;

        public event EventHandler<ToolChangedEventArgs>? ToolChanged;

        private Tool? _tool;
        private ToolType _type = ToolType.None;
        private readonly TranslationTool TRANSLATION_TOOL;
        private readonly RotationTool ROTATION_TOOL;
        private readonly ScalingTool SCALING_TOOL;
        private readonly VertexTranslationTool VERTEX_TRANSLATION_TOOL;

        public Toolbox()
        {
            TRANSLATION_TOOL = new TranslationTool(this);
            ROTATION_TOOL = new RotationTool(this);
            SCALING_TOOL = new ScalingTool(this);
            VERTEX_TRANSLATION_TOOL = new VertexTranslationTool(this);
        }

        public void ChangeTool(ToolType toolType)
        {
            if (toolType == type)
                return;
            _type = toolType;

            if (toolType == ToolType.None)
                _tool = null;
            else if (toolType == ToolType.Translation)
                _tool = TRANSLATION_TOOL;
            else if (toolType == ToolType.Rotation)
                _tool = ROTATION_TOOL;
            else if (toolType == ToolType.Scaling)
                _tool = SCALING_TOOL;
            else if (toolType == ToolType.VertexTranslation)
                _tool = VERTEX_TRANSLATION_TOOL;

            _tool?.Reset();
            OnToolChanged(new ToolChangedEventArgs(toolType));
        }

        private void OnToolChanged(ToolChangedEventArgs e)
        {
            ToolChanged?.Invoke(this, e);
        }
    }
}
