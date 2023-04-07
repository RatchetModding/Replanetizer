// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Replanetizer.Frames;
using Replanetizer.Utils;

namespace Replanetizer.Tools
{
    public abstract class Tool
    {
        public float transformMultiplier { get; set; } = 50f;

        protected Toolbox toolbox { get; set; }

        protected int vbo;
        protected float[] vb = new[]{
                0.0f,    0.0f,    0.0f,
                0.0f,    0.0f,    0.0f,
                0.0f,    0.0f,    0.0f,
                0.0f,    0.0f,    0.0f,
                0.0f,    0.0f,    0.0f,
                0.0f,    0.0f,    0.0f,
            };
        private const float SCREEN_SPACE_SCALE = 0.06f;

        public Tool(Toolbox toolbox)
        {
            this.toolbox = toolbox;
        }

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
        protected static Matrix4 GetModelMatrix(Vector3 position, LevelFrame frame)
        {
            float camDist = (frame.camera.position - position).LengthFast;
            return Matrix4.CreateScale(camDist * SCREEN_SPACE_SCALE) * Matrix4.CreateTranslation(position);
        }

        /// <summary>
        /// Get the model matrix, scaled by camera distance
        /// </summary>
        protected static Matrix4 GetModelMatrix(Vector3 position, Quaternion rotation, LevelFrame frame)
        {
            float camDist = (frame.camera.position - position).LengthFast;
            return
                Matrix4.CreateScale(camDist * SCREEN_SPACE_SCALE) *
                Matrix4.CreateFromQuaternion(rotation) *
                Matrix4.CreateTranslation(position);
        }

        public abstract ToolType toolType { get; }
        public abstract void Render(Matrix4 mat, LevelFrame frame);

        public void Render(Vector3 position, LevelFrame frame)
        {
            var mat = GetModelMatrix(position, frame);
            Render(mat, frame);
        }

        public void Render(Vector3 position, Quaternion rotation, LevelFrame frame)
        {
            var mat = GetModelMatrix(position, rotation, frame);
            Render(mat, frame);
        }

        public void Render(Selection selection, LevelFrame frame)
        {
            if (toolbox.transformSpace == TransformSpace.Global)
            {
                Render(selection.mean, frame);
            }
            else if (toolbox.transformSpace == TransformSpace.Local)
            {
                if (selection.newestObject != null)
                    Render(selection.mean, selection.newestObject.rotation, frame);
                else
                    Render(selection.mean, frame);
            }
        }

        protected virtual Vector3 ProcessVec(Vector3 direction, Vector3 magnitude)
        {
            return direction * magnitude * transformMultiplier;
        }

        /// <summary>
        /// Computes the intersection of the lines x + a * dx and y + b * dy. The returned float f is such that x + f * dx is the intersection.
        /// If no intersection is given then the corresponding value for the closest approach is returned.
        /// Note: That last one is a happy coincidence, I did not verify this but it seems to work. :)
        /// </summary>
        protected float getLineIntersectionDist(Vector3 x, Vector3 dx, Vector3 y, Vector3 dy)
        {
            Vector3 g = y - x;
            Vector3 h = Vector3.Cross(dy, g);
            Vector3 k = Vector3.Cross(dy, dx);

            float ha = h.Length;
            float ka = k.Length;

            if (ha == 0.0f || ka == 0.0f)
            {
                return 0.0f;
            }

            float sign = (Vector3.Dot(h, k) >= 0.0f) ? 1.0f : -1.0f;

            return (ha / ka) * sign;
        }

        public virtual void Reset()
        {
        }
    }
}
