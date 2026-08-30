// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Headers;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using LibReplanetizer.Models.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static LibReplanetizer.DataFunctions;

namespace LibReplanetizer.Parsers
{
    public class EngineParser : RatchetFileParser, IDisposable
    {
        EngineHeader engineHead;

        public EngineParser(string engineFile) : base(engineFile)
        {
            engineHead = new EngineHeader(fileStream);
        }

        public GameType GetGameType()
        {
            return engineHead.game;
        }

        public List<Model> GetMobyModels()
        {
            return GetMobyModels(engineHead.game, engineHead.mobyModelPointer);
        }

        public List<Model> GetTieModels()
        {
            return GetTieModels(engineHead.tieModelPointer, engineHead.tieModelCount);
        }

        public List<Model> GetShrubModels()
        {
            return GetShrubModels(engineHead.shrubModelPointer, engineHead.shrubModelCount);
        }

        public List<Texture> GetTextures()
        {
            return GetTextures(engineHead.texturePointer, engineHead.textureCount);
        }

        public List<Tie> GetTies(List<Model> tieModels)
        {
            return GetTies(tieModels, engineHead.tiePointer, engineHead.tieCount);
        }

        public List<Light> GetLights()
        {
            return GetLights(engineHead.lightPointer, engineHead.lightCount);
        }

        public List<Shrub> GetShrubs(List<Model> shrubModels)
        {
            return GetShrubs(shrubModels, engineHead.shrubPointer, engineHead.shrubCount);
        }

        public Terrain GetTerrainModel()
        {
            return GetTerrainModels(engineHead.terrainPointer, engineHead.game);
        }

        public SkyboxModel GetSkyboxModel()
        {
            return GetSkyboxModel(engineHead.game, engineHead.skyboxPointer);
        }

        public List<UiElement> GetUiElements()
        {
            return GetUiElements(engineHead.uiElementPointer);
        }

        public MobyModel? FindRatchetMoby(List<Model> models)
        {
            if (engineHead.game == GameType.DL)
                return (MobyModel?) models.FirstOrDefault(x => x.id == 9207);

            return (MobyModel?) models.FirstOrDefault();
        }

        public List<Animation> GetPlayerAnimations(MobyModel ratchet)
        {
            if (engineHead.game == GameType.DL)
                return ratchet.animations;

            return GetPlayerAnimations(engineHead.game, engineHead.playerAnimationPointer, ratchet);
        }

        public List<Model> GetGadgets()
        {
            return GetGadgets(engineHead.game, engineHead.gadgetPointer, engineHead.gadgetCount);
        }

        public LightConfig GetLightConfig()
        {
            return GetLightConfig(engineHead.lightConfigPointer);
        }

        public PrecipitationMap? GetPrecipitationMap()
        {
            if (engineHead.precipitationMapPointer == 0)
                return null;

            return new PrecipitationMap(fileStream, engineHead.game, engineHead.precipitationMapPointer);
        }

        public List<int> GetTextureConfigMenu()
        {
            return GetTextureConfigMenu(engineHead.textureConfigMenuPointer, engineHead.textureConfigMenuCount);
        }

        public Collision GetCollisionModel()
        {
            return GetCollisionModel(engineHead.collisionPointer);
        }

        public MobyOcclusion? GetMobyOcclusion()
        {
            if (engineHead.mobyOcclusionPointer == 0)
                return null;

            return new MobyOcclusion(fileStream, engineHead.mobyOcclusionPointer);
        }

        public byte[] GetBillboardBytes()
        {
            if (engineHead.game == GameType.RaC1)
            {
                int endPointer = engineHead.texturePointer;

                // RC1 boot level has no sound config.
                if (engineHead.soundConfigPointer > 0)
                    endPointer = engineHead.soundConfigPointer;

                return ReadArbBytes(engineHead.texture2dPointer, endPointer - engineHead.texture2dPointer);
            }


            if (engineHead.game == GameType.RaC2 || engineHead.game == GameType.RaC3)
                return ReadArbBytes(engineHead.texture2dPointer, engineHead.mobyModelPointer - engineHead.texture2dPointer);

            return ReadArbBytes(engineHead.texture2dPointer, engineHead.skyboxPointer - engineHead.texture2dPointer);
        }

        public byte[] GetSoundConfigBytes()
        {
            if (engineHead.game == GameType.RaC1)
                return ReadArbBytes(engineHead.soundConfigPointer, engineHead.lightPointer - engineHead.soundConfigPointer);

            if (engineHead.game == GameType.RaC2 || engineHead.game == GameType.RaC3)
                return ReadArbBytes(engineHead.soundConfigPointer, engineHead.playerAnimationPointer - engineHead.soundConfigPointer);

            int endPointer = (engineHead.unk9Pointer > 0) ? engineHead.unk9Pointer : engineHead.terrainPointer;

            Utilities.DebugAssert(endPointer != 0, "Failed to determine proper end pointer!");

            return ReadArbBytes(engineHead.soundConfigPointer, endPointer - engineHead.soundConfigPointer);
        }

        public byte[] GetUnk1Bytes()
        {
            if (engineHead.unk1Pointer == 0) { return []; }

            if (engineHead.game == GameType.RaC1 || engineHead.game == GameType.RaC2 || engineHead.game == GameType.RaC3)
            {
                int endPointer = engineHead.collisionPointer;

                if (engineHead.precipitationMapPointer > 0)
                    endPointer = engineHead.precipitationMapPointer;

                if (engineHead.unk2Pointer > 0)
                    endPointer = engineHead.unk2Pointer;

                Utilities.DebugAssert(endPointer != 0, "No valid endPointer was found!");

                return ReadBlock(fileStream, engineHead.unk1Pointer, endPointer - engineHead.unk1Pointer);
            }
            else
            {
                return [];
            }
        }

        public byte[] GetUnk2Bytes()
        {
            if (engineHead.unk2Pointer == 0) { return []; }

            if (engineHead.game == GameType.RaC1 || engineHead.game == GameType.RaC2 || engineHead.game == GameType.RaC3)
            {
                // No order relationship with unk3Pointer is known.
                return ReadBlock(fileStream, engineHead.unk2Pointer, engineHead.collisionPointer - engineHead.unk2Pointer);
            }
            else
            {
                return ReadBlock(fileStream, engineHead.unk2Pointer, engineHead.texturePointer - engineHead.unk2Pointer);
            }
        }


        public byte[] GetUnk4Bytes()
        {
            if (engineHead.unk4Pointer == 0) { return []; }
            return ReadBlock(fileStream, engineHead.unk4Pointer, engineHead.textureConfigMenuPointer - engineHead.unk4Pointer);
        }

        public byte[] GetUnk5Bytes()
        {
            if (engineHead.unk5Pointer == 0) { return []; }
            return ReadBlock(fileStream, engineHead.unk5Pointer, engineHead.tieModelPointer - engineHead.unk5Pointer);
        }

        public byte[] GetUnk8Bytes()
        {
            if (engineHead.unk8Pointer == 0) { return []; }
            byte[] head = ReadBlock(fileStream, engineHead.unk8Pointer, 16);
            int amount = ReadInt(head, 4);
            return ReadBlock(fileStream, engineHead.unk8Pointer, 0x10 + amount);
        }

        public byte[] GetUnk9Bytes()
        {
            if (engineHead.unk9Pointer == 0) { return []; }
            return ReadBlock(fileStream, engineHead.unk9Pointer, engineHead.terrainPointer - engineHead.unk9Pointer);
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
