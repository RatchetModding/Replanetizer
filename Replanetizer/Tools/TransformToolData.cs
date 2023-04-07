// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using OpenTK.Mathematics;
using Replanetizer.Utils;

namespace Replanetizer.Tools
{
    public class TransformToolData
    {
        public Vector3 cameraPos;
        public Vector3 mousePrevDir;
        public Vector3 mouseCurrDir;
        private bool _mouseDiffDirComputed = false;
        private Vector3 _mouseDiffDir;
        public Vector3 mouseDiffDir
        {
            get
            {
                if (_mouseDiffDirComputed) return _mouseDiffDir;
                _mouseDiffDir = mouseCurrDir - mousePrevDir;
                _mouseDiffDirComputed = true;
                return _mouseDiffDir;
            }
        }
        private bool _vecComputed = false;
        private Vector3 _vec;
        public Vector3 vec
        {
            get
            {
                if (_vecComputed) return _vec;
                _vec = axisDir * mouseDiffDir * 50.0f;
                _vecComputed = true;
                return _vec;
            }
        }
        public Vector3 axisDir;


        public TransformToolData(Camera camera, Vector3 mousePrevDir, Vector3 mouseCurrDir, Vector3 axisDir)
        {
            this.cameraPos = camera.position;
            this.mousePrevDir = mousePrevDir;
            this.mouseCurrDir = mouseCurrDir;
            this.axisDir = axisDir;
        }
    }
}
