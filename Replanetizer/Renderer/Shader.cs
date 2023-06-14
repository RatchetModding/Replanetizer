// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Replanetizer.Renderer
{
    public struct UniformFieldInfo
    {
        public int location;
        public string name;
        public int size;
        public ActiveUniformType type;
    }

    public class Shader
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public readonly string NAME;
        public int program { get; private set; }
        private readonly Dictionary<string, int> UNIFORM_TO_LOCATION = new Dictionary<string, int>();
        private bool initialized = false;

        private readonly (ShaderType Type, string Path)[] FILES;

        public Shader(string name, string vertexShader, string fragmentShader)
        {
            NAME = name;
            FILES = new[]{
                (ShaderType.VertexShader, vertexShader),
                (ShaderType.FragmentShader, fragmentShader),
            };
            program = CreateProgram(name, FILES);
        }

        public static Shader GetShaderFromFiles(string name, string pathVS, string pathFS)
        {
            Shader shader;
            using (StreamReader vs = new StreamReader(pathVS))
            {
                using (StreamReader fs = new StreamReader(pathFS))
                {
                    shader = new Shader(name, vs.ReadToEnd(), fs.ReadToEnd());
                }
            }

            return shader;
        }

        public void UseShader()
        {
            GLState.UseProgram(program);
        }

        public void Dispose()
        {
            if (initialized)
            {
                GL.DeleteProgram(program);
                initialized = false;
            }
        }

        public UniformFieldInfo[] GetUniforms()
        {
            GL.GetProgram(program, GetProgramParameterName.ActiveUniforms, out int uniformCount);

            UniformFieldInfo[] uniforms = new UniformFieldInfo[uniformCount];

            for (int i = 0; i < uniformCount; i++)
            {
                string name = GL.GetActiveUniform(program, i, out int size, out ActiveUniformType type);

                UniformFieldInfo fieldInfo;
                fieldInfo.location = GetUniformLocation(name);
                fieldInfo.name = name;
                fieldInfo.size = size;
                fieldInfo.type = type;

                uniforms[i] = fieldInfo;
            }

            return uniforms;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetUniformLocation(string uniform)
        {
            if (UNIFORM_TO_LOCATION.TryGetValue(uniform, out int location) == false)
            {
                location = GL.GetUniformLocation(program, uniform);
                UNIFORM_TO_LOCATION.Add(uniform, location);

                if (location == -1)
                {
                    Debug.Print($"The uniform '{uniform}' does not exist in the shader '{NAME}'!");
                }
            }

            return location;
        }

        private int CreateProgram(string name, params (ShaderType Type, string source)[] shaderPaths)
        {
            GLUtil.CreateProgram(name, out int program);

            int[] shaders = new int[shaderPaths.Length];
            for (int i = 0; i < shaderPaths.Length; i++)
            {
                shaders[i] = CompileShader(name, shaderPaths[i].Type, shaderPaths[i].source);
            }

            foreach (var shader in shaders)
                GL.AttachShader(program, shader);

            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetProgramInfoLog(program);
                LOGGER.Debug("GL.LinkProgram had info log [{0}]:\n{1}", name, info);
            }

            foreach (var shader in shaders)
            {
                GL.DetachShader(program, shader);
                GL.DeleteShader(shader);
            }

            initialized = true;

            return program;
        }

        private int CompileShader(string name, ShaderType type, string source)
        {
            GLUtil.CreateShader(type, name, out int shader);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetShaderInfoLog(shader);
                LOGGER.Debug("GL.CompileShader for shader '{0}' [{1}] had info log:\n{2}", name, type, info);
            }

            return shader;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUniformMatrix4(string uniform, bool transpose, ref Matrix4 mat)
        {
            int location = GetUniformLocation(uniform);

            if (location != -1)
            {
                GL.UniformMatrix4(location, transpose, ref mat);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUniformMatrix4(string uniform, int count, bool transpose, float[] mat)
        {
            int location = GetUniformLocation(uniform);

            if (location != -1)
            {
                GL.UniformMatrix4(location, count, transpose, mat);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUniformMatrix4(string uniform, int count, bool transpose, ref float mat)
        {
            int location = GetUniformLocation(uniform);

            if (location != -1)
            {
                GL.UniformMatrix4(location, count, transpose, ref mat);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUniform1(string uniform, float v)
        {
            int location = GetUniformLocation(uniform);

            if (location != -1)
            {
                GL.Uniform1(location, v);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUniform1(string uniform, int v)
        {
            int location = GetUniformLocation(uniform);

            if (location != -1)
            {
                GL.Uniform1(location, v);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUniform3(string uniform, float v0, float v1, float v2)
        {
            int location = GetUniformLocation(uniform);

            if (location != -1)
            {
                GL.Uniform3(location, v0, v1, v2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUniform4(string uniform, Color color)
        {
            int location = GetUniformLocation(uniform);

            if (location != -1)
            {
                GL.Uniform4(location, color);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUniform4(string uniform, float v0, float v1, float v2, float v3)
        {
            int location = GetUniformLocation(uniform);

            if (location != -1)
            {
                GL.Uniform4(location, v0, v1, v2, v3);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUniform4(string uniform, Vector4 v)
        {
            SetUniform4(uniform, v.X, v.Y, v.Z, v.W);
        }
    }
}
