﻿// Copyright (C) 2018-2021, The Replanetizer Contributors.
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
        public int Location;
        public string Name;
        public int Size;
        public ActiveUniformType Type;
    }

    class Shader
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public readonly string Name;
        public int Program { get; private set; }
        private readonly Dictionary<string, int> UniformToLocation = new Dictionary<string, int>();
        private bool Initialized = false;

        private readonly (ShaderType Type, string Path)[] Files;

        public Shader(string name, string vertexShader, string fragmentShader)
        {
            Name = name;
            Files = new[]{
                (ShaderType.VertexShader, vertexShader),
                (ShaderType.FragmentShader, fragmentShader),
            };
            Program = CreateProgram(name, Files);
        }
        public void UseShader()
        {
            GL.UseProgram(Program);
        }

        public void Dispose()
        {
            if (Initialized)
            {
                GL.DeleteProgram(Program);
                Initialized = false;
            }
        }

        public UniformFieldInfo[] GetUniforms()
        {
            GL.GetProgram(Program, GetProgramParameterName.ActiveUniforms, out int UnifromCount);

            UniformFieldInfo[] Uniforms = new UniformFieldInfo[UnifromCount];

            for (int i = 0; i < UnifromCount; i++)
            {
                string Name = GL.GetActiveUniform(Program, i, out int Size, out ActiveUniformType Type);

                UniformFieldInfo FieldInfo;
                FieldInfo.Location = GetUniformLocation(Name);
                FieldInfo.Name = Name;
                FieldInfo.Size = Size;
                FieldInfo.Type = Type;

                Uniforms[i] = FieldInfo;
            }

            return Uniforms;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetUniformLocation(string uniform)
        {
            if (UniformToLocation.TryGetValue(uniform, out int location) == false)
            {
                location = GL.GetUniformLocation(Program, uniform);
                UniformToLocation.Add(uniform, location);

                if (location == -1)
                {
                    Debug.Print($"The uniform '{uniform}' does not exist in the shader '{Name}'!");
                }
            }

            return location;
        }

        private int CreateProgram(string name, params (ShaderType Type, string source)[] shaderPaths)
        {
            Util.CreateProgram(name, out int Program);

            int[] Shaders = new int[shaderPaths.Length];
            for (int i = 0; i < shaderPaths.Length; i++)
            {
                Shaders[i] = CompileShader(name, shaderPaths[i].Type, shaderPaths[i].source);
            }

            foreach (var shader in Shaders)
                GL.AttachShader(Program, shader);

            GL.LinkProgram(Program);

            GL.GetProgram(Program, GetProgramParameterName.LinkStatus, out int Success);
            if (Success == 0)
            {
                string Info = GL.GetProgramInfoLog(Program);
                Logger.Debug("GL.LinkProgram had info log [{0}]:\n{1}", name, Info);
            }

            foreach (var Shader in Shaders)
            {
                GL.DetachShader(Program, Shader);
                GL.DeleteShader(Shader);
            }

            Initialized = true;

            return Program;
        }

        private int CompileShader(string name, ShaderType type, string source)
        {
            Util.CreateShader(type, name, out int Shader);
            GL.ShaderSource(Shader, source);
            GL.CompileShader(Shader);

            GL.GetShader(Shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string Info = GL.GetShaderInfoLog(Shader);
                Logger.Debug("GL.CompileShader for shader '{0}' [{1}] had info log:\n{2}", name, type, Info);
            }

            return Shader;
        }
    }
}
