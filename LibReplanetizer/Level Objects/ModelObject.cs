// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Models;
using System.ComponentModel;

namespace LibReplanetizer.LevelObjects
{
    public abstract class ModelObject : LevelObject, IRenderable
    {

        [Category("Attributes"), DisplayName("Model ID")]
        public int modelID { get; set; }


        [Category("Attributes"), TypeConverter(typeof(ExpandableObjectConverter)), DisplayName("Model")]
        public Model model { get; set; }

        public ushort[] GetIndices()
        {
            return model.GetIndices();
        }

        public float[] GetVertices()
        {
            return model.GetVertices();
        }

        public byte[] GetAmbientRGBAs()
        {
            if (this is Tie)
            {
                Tie tie = (Tie) this;
                return tie.colorBytes;
            }
            else
            {
                return model.rgbas;
            }
        }

        public bool IsDynamic()
        {
            return model.IsDynamic();
        }

    }
}
