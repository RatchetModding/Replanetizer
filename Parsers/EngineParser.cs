using System.Collections.Generic;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public class EngineParser : RatchetFileParser
    {
        EngineHeader engineHead;

        public EngineParser(string engineFile) : base(engineFile)
        {
            engineHead = new EngineHeader(fileStream);
        }

        public List<Model> GetMobyModels()
        {
            return GetMobyModels(engineHead.mobyModelPointer);
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

        public List<TerrainModel> GetTerrainModels()
        {
            return GetTerrainModels(engineHead.terrainPointer);
        }

        public SkyboxModel GetSkyboxModel()
        {
            return GetSkyboxModel(engineHead.skyboxPointer);
        }

        public List<UiElement> GetUiElements()
        {
            return GetUiElements(engineHead.uiElementPointer);
        }

        public List<Animation> GetPlayerAnimations( MobyModel ratchet)
        {
            return GetPlayerAnimations(engineHead.playerAnimationPointer, ratchet);
        }

        public List<Model> GetWeapons()
        {
            return GetWeapons(engineHead.weaponPointer, engineHead.weaponCount);
        }

        public byte[] GetLightConfig()
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




        public byte[] GetTerrainBytes()
        {
            return ReadArbBytes(engineHead.terrainPointer, engineHead.renderDefPointer - engineHead.terrainPointer);
        }

        public byte[] GetRenderDefBytes()
        {
            return ReadArbBytes(engineHead.renderDefPointer, engineHead.collisionPointer - engineHead.renderDefPointer);
        }

        public byte[] GetCollisionBytes()
        {
            return ReadArbBytes(engineHead.collisionPointer, engineHead.mobyModelPointer - engineHead.collisionPointer);
        }

        public byte[] GetBillboardBytes()
        {
            return ReadArbBytes(engineHead.texture2dPointer, engineHead.soundConfigPointer - engineHead.texture2dPointer);
        }

        public byte[] GetSoundConfigBytes()
        {
            return ReadArbBytes(engineHead.soundConfigPointer, engineHead.lightPointer - engineHead.soundConfigPointer);
        }



        /*
        /// <summary>
        /// Test
        /// </summary>
        /// <returns>stuff</returns>
        public byte[] GetMobyModelBytes()
        {
            return ReadArbBytes(engineHead.mobyModelPointer, engineHead.playerAnimationPointer - engineHead.mobyModelPointer);
        }

        public byte[] GetPlayerAnimBytes()
        {
            return ReadArbBytes(engineHead.playerAnimationPointer, engineHead.weaponPointer - engineHead.playerAnimationPointer);
        }

        public byte[] GetWeaponModelBytes()
        {
            return ReadArbBytes(engineHead.weaponPointer, engineHead.tieModelPointer - engineHead.weaponPointer);
        }

        public byte[] GetTieModelBytes()
        {
            return ReadArbBytes(engineHead.tieModelPointer, engineHead.tiePointer - engineHead.tieModelPointer);
        }

        public byte[] GetTieBytes()
        {
            return ReadArbBytes(engineHead.tiePointer, engineHead.shrubModelPointer - engineHead.tiePointer);
        }

        public byte[] GetShrubModelBytes()
        {
            return ReadArbBytes(engineHead.shrubModelPointer, engineHead.shrubPointer - engineHead.shrubModelPointer);
        }

        public byte[] GetShrubBytes()
        {
            return ReadArbBytes(engineHead.shrubPointer, engineHead.textureConfigMenuPointer - engineHead.shrubPointer);
        }

        public byte[] GetMenuTextureBytes()
        {
            return ReadArbBytes(engineHead.textureConfigMenuPointer, engineHead.texture2dPointer - engineHead.textureConfigMenuPointer);
        }

        public byte[] GetLightBytes()
        {
            return ReadArbBytes(engineHead.lightPointer, engineHead.lightConfigPointer - engineHead.lightPointer);
        }

        public byte[] GetLightConfigBytes()
        {
            return ReadArbBytes(engineHead.lightConfigPointer, engineHead.texturePointer - engineHead.lightConfigPointer);
        }
        
        */




        public GameType DetectGame()
        {
            return DetectGame(0xA0);
        }
    }
}
