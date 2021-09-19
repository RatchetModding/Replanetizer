using LibReplanetizer.Headers;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using LibReplanetizer.Models.Animations;
using System;
using System.Collections.Generic;
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

        public List<Animation> GetPlayerAnimations(MobyModel ratchet)
        {
            if (engineHead.game.num == 4) return new List<Animation>();

            return GetPlayerAnimations(engineHead.playerAnimationPointer, ratchet);
        }

        public List<Model> GetGadgets()
        {
            return GetGadgets(engineHead.game, engineHead.gadgetPointer, engineHead.gadgetCount);
        }

        public LightConfig GetLightConfig()
        {
            return GetLightConfig(engineHead.lightConfigPointer);
        }

        public List<int> GetTextureConfigMenu()
        {
            return GetTextureConfigMenu(engineHead.textureConfigMenuPointer, engineHead.textureConfigMenuCount);
        }

        public Model GetCollisionModel()
        {
            return GetCollisionModel(engineHead.collisionPointer);
        }

        public byte[] GetRenderDefBytes()
        {
            if (engineHead.renderDefPointer > 0)
            {
                return ReadArbBytes(engineHead.renderDefPointer, engineHead.collisionPointer - engineHead.renderDefPointer);
            }
            else
            {
                return new byte[0];
            }
        }

        public byte[] GetCollisionBytes()
        {
            if (engineHead.collisionPointer > 0)
            {
                if (engineHead.game.num == 1)
                {
                    return ReadBlock(fileStream, engineHead.collisionPointer, engineHead.mobyModelPointer - engineHead.collisionPointer);
                }
                else
                {
                    return ReadBlock(fileStream, engineHead.collisionPointer, engineHead.tieModelPointer - engineHead.collisionPointer);
                }
            }
            else
            {
                return null;
            }
        }

        public byte[] GetBillboardBytes()
        {
            switch (engineHead.game.num)
            {
                case 1:
                    return ReadArbBytes(engineHead.texture2dPointer, engineHead.soundConfigPointer - engineHead.texture2dPointer);
                case 2:
                case 3:
                case 4:
                default:
                    return ReadArbBytes(engineHead.texture2dPointer, engineHead.mobyModelPointer - engineHead.texture2dPointer);
            }
        }

        public byte[] GetSoundConfigBytes()
        {
            switch (engineHead.game.num)
            {
                case 1:
                    return ReadArbBytes(engineHead.soundConfigPointer, engineHead.lightPointer - engineHead.soundConfigPointer);
                case 2:
                case 3:
                case 4:
                default:
                    return ReadArbBytes(engineHead.soundConfigPointer, engineHead.playerAnimationPointer - engineHead.soundConfigPointer);
            }
        }

        public byte[] GetUnk3Bytes()
        {
            if (engineHead.unk3Pointer == 0) { return null; }
            return ReadBlock(fileStream, engineHead.unk3Pointer, engineHead.texturePointer - engineHead.unk3Pointer);
        }

        public byte[] GetUnk4Bytes()
        {
            if (engineHead.unk4Pointer == 0) { return null; }
            return ReadBlock(fileStream, engineHead.unk4Pointer, engineHead.textureConfigMenuPointer - engineHead.unk4Pointer);
        }

        public byte[] GetUnk5Bytes()
        {
            if (engineHead.unk5Pointer == 0) { return null; }
            return ReadBlock(fileStream, engineHead.unk5Pointer, engineHead.tieModelPointer - engineHead.unk5Pointer);
        }

        public byte[] GetUnk8Bytes()
        {
            if (engineHead.unk8Pointer == 0) { return null; }
            byte[] head = ReadBlock(fileStream, engineHead.unk8Pointer, 16);
            int amount = ReadInt(head, 4);
            return ReadBlock(fileStream, engineHead.unk8Pointer, 0x10 + amount);
        }

        public byte[] GetUnk9Bytes()
        {
            if (engineHead.unk9Pointer == 0) { return null; }
            return ReadBlock(fileStream, engineHead.unk9Pointer, engineHead.terrainPointer - engineHead.unk9Pointer);
        }

        public void Dispose()
        {
            fileStream.Close();
        }
    }
}
