// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Mathematics;
using LibReplanetizer.Models;
using System.ComponentModel;

namespace LibReplanetizer.LevelObjects
{
    public abstract class ModelObject : LevelObject, IRenderable
    {

        [Category("Attributes"), DisplayName("Model ID")]
        public int modelID { get; set; }


        [Category("Attributes"), TypeConverter(typeof(ExpandableObjectConverter)), DisplayName("Model")]
        public Model? model { get; set; }

        public ushort[] GetIndices()
        {
            if (model != null)
            {
                return model.GetIndices();
            }
            else
            {
                return new ushort[0];
            }
        }

        public float[] GetVertices()
        {
            if (model != null)
            {
                return model.GetVertices();
            }
            else
            {
                return new float[0];
            }
        }

        /// <summary>
        /// Attempts to load the correct model after the modelID was changed. Only works for mobies and shrubs.
        /// Ties store data per vertex which is troublesome if you change the model.
        /// </summary>
        public void TryChangeModel(Level level)
        {
            Model? newModel = null;

            if (this is Moby)
            {
                newModel = level.mobyModels.Find(mobyModel => mobyModel.id == modelID);
            }
            else if (this is Shrub)
            {
                newModel = level.shrubModels.Find(shrubModel => shrubModel.id == modelID);
            }

            if (newModel != null)
            {
                model = newModel;
            }
        }

        public override void SetFromMatrix(Matrix4 mat)
        {
            position = mat.ExtractTranslation();
            rotation = mat.ExtractRotation();
            scale = mat.ExtractScale();
            if (model != null)
                scale = scale * (1.0f / model.size);
            modelMatrix = mat;
        }

        public byte[] GetAmbientRgbas()
        {
            if (model == null) return new byte[0];

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
            return (model == null) ? false : model.IsDynamic();
        }

    }
}
