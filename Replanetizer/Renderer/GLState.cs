// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;

namespace Replanetizer.Renderer
{
    static class GLState
    {
        private static int activeProgram = -1;
        private static int numEnabledVertexAttribArrays = 0;

        public static void UseProgram(int program)
        {
            if (program != activeProgram)
            {
                GL.UseProgram(program);
                activeProgram = program;
            }
        }

        public static void ChangeNumberOfVertexAttribArrays(int num)
        {
            if (num != numEnabledVertexAttribArrays && num >= 0)
            {
                if (num > numEnabledVertexAttribArrays)
                {
                    for (int i = numEnabledVertexAttribArrays; i < num; i++)
                    {
                        GL.EnableVertexAttribArray(i);
                    }
                }
                else
                {
                    for (int i = numEnabledVertexAttribArrays - 1; i >= num; i--)
                    {
                        GL.DisableVertexAttribArray(i);
                    }
                }

                numEnabledVertexAttribArrays = num;
            }
        }
    }
}
