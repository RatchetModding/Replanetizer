// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.IO;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;

namespace Replanetizer.Renderer
{
    static class GLUtil
    {
        [Pure]
        public static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }

        [Conditional("DEBUG")]
        public static void CheckGlError(string title, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string caller = "")
        {
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                string fileName = Path.GetFileName(caller);
                Debug.Print($"[{fileName}:{lineNumber}] {title}: {error}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LabelObject(ObjectLabelIdentifier objLabelIdent, int glObject, string name)
        {
            GL.ObjectLabel(objLabelIdent, glObject, name.Length, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateTexture(TextureTarget target, string name, out int texture)
        {
            GL.CreateTextures(target, 1, out texture);
            LabelObject(ObjectLabelIdentifier.Texture, texture, $"Texture: {name}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateProgram(string name, out int program)
        {
            program = GL.CreateProgram();
            LabelObject(ObjectLabelIdentifier.Program, program, $"Program: {name}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateShader(ShaderType type, string name, out int shader)
        {
            shader = GL.CreateShader(type);
            LabelObject(ObjectLabelIdentifier.Shader, shader, $"Shader: {type}: {name}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateBuffer(string name, out int buffer)
        {
            GL.CreateBuffers(1, out buffer);
            LabelObject(ObjectLabelIdentifier.Buffer, buffer, $"Buffer: {name}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateVertexBuffer(string name, out int buffer) => CreateBuffer($"VBO: {name}", out buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateElementBuffer(string name, out int buffer) => CreateBuffer($"EBO: {name}", out buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateVertexArray(string name, out int vao)
        {
            GL.CreateVertexArrays(1, out vao);
            LabelObject(ObjectLabelIdentifier.VertexArray, vao, $"VAO: {name}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ActivateNumberOfVertexAttribArrays(int num)
        {
            for (int i = 0; i < num; i++)
            {
                GL.EnableVertexAttribArray(i);
            }
        }
    }
}
