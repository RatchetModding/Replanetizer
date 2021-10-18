// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Replanetizer.Frames;

namespace Replanetizer.Tools
{
    public abstract class Tool
    {
        public float transformMultiplier { get; set; } = 50f;
        public TransformationSpace transformationSpace { get; set; } = TransformationSpace.Global;
        public PivotPositioning pivotPositioning { get; set; } = PivotPositioning.Median;

        protected int vbo;
        protected float[] vb;
        private const float SCREEN_SPACE_SCALE = 0.06f;

        protected void GetVbo()
        {
            if (vbo == 0)
            {
                GL.GenBuffers(1, out vbo);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, vb.Length * sizeof(float), vb, BufferUsageHint.StaticDraw);
            }
            else
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            }

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        }

        /// <summary>
        /// Get the model matrix, scaled by camera distance
        /// </summary>
        protected Matrix4 GetModelMatrix(Vector3 position, LevelFrame frame)
        {
            float camDist = (frame.camera.position - position).LengthFast;
            return Matrix4.CreateScale(camDist * SCREEN_SPACE_SCALE) * Matrix4.CreateTranslation(position);
        }

        public abstract ToolType toolType { get; }
        public abstract void Render(Vector3 position, LevelFrame frame);

        protected virtual Vector3 ProcessVec(Vector3 direction, Vector3 magnitude)
        {
            return direction * magnitude * transformMultiplier;
        }

        public virtual void Reset()
        {
        }
    }
}
