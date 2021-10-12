// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;

namespace Replanetizer.Utils
{
    struct UniformFieldInfo
    {
        public int location;
        public string name;
        public int size;
        public ActiveUniformType type;
    }

    class Shader
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
        public void UseShader()
        {
            GL.UseProgram(program);
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
            Util.CreateProgram(name, out int program);

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
            Util.CreateShader(type, name, out int shader);
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
    }
}
