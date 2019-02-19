using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    class EngineSerializer
    {

        public void Save(Level level, String fileName)
        {
            FileStream fs = File.Open(fileName, FileMode.Create);
            EngineHeader engineHeader = new EngineHeader();

            // Seek past the header, as we don't have the data ready for it yet
            fs.Seek(0x90, SeekOrigin.Begin);

            engineHeader.uiElementPointer = (int)fs.Position;
            WriteUiElements(fs, level.uiElements);


            // Write all the different data types to the file
            SeekPast(fs);
            engineHeader.skyboxPointer = (int)fs.Position;
            byte[] skyboxBytes = level.skybox.Serialize((int)fs.Position);
            fs.Write(skyboxBytes, 0, skyboxBytes.Length);



            // Fix these

            // 0x3c - terrain
            SeekPast(fs);
            engineHeader.terrainPointer = (int)fs.Position;
            fs.Write(level.terrainBytes, 0, level.terrainBytes.Length);

            // 0x04 - renderdef
            SeekPast(fs);
            engineHeader.renderDefPointer = (int)fs.Position;
            fs.Write(level.renderDefBytes, 0, level.renderDefBytes.Length);

            // 0x14 - collision
            SeekPast(fs);
            engineHeader.collisionPointer = (int)fs.Position;
            fs.Write(level.collBytes, 0, level.collBytes.Length);



            SeekPast(fs);
            engineHeader.mobyModelPointer = (int)fs.Position;
            byte[] mobyModelBytes = WriteMobies(level.mobyModels, (int)fs.Position);
            fs.Write(mobyModelBytes, 0, mobyModelBytes.Length);

            SeekPast(fs);
            engineHeader.playerAnimationPointer = (int)fs.Position;
            byte[] playerAnimationBytes = WritePlayerAnimations(level.playerAnimations, (int)fs.Position);
            fs.Write(playerAnimationBytes, 0, playerAnimationBytes.Length);

            SeekPast(fs);
            engineHeader.weaponPointer = (int)fs.Position;
            byte[] weaponModelBytes = WriteWeapons(level.weaponModels, (int)fs.Position);
            fs.Write(weaponModelBytes, 0, weaponModelBytes.Length);

            SeekPast(fs);
            engineHeader.tieModelPointer = (int)fs.Position;
            byte[] tiemodelBytes = WriteTieModels(level.tieModels, (int)fs.Position);
            fs.Write(tiemodelBytes, 0, tiemodelBytes.Length);

            SeekPast(fs);
            engineHeader.tiePointer = (int)fs.Position;
            byte[] tieBytes = WriteTies(level.ties, (int)fs.Position);
            fs.Write(tieBytes, 0, tieBytes.Length);

            SeekPast(fs);
            engineHeader.shrubModelPointer = (int)fs.Position;
            byte[] shrubModelBytes = WriteShrubModels(level.shrubModels, (int)fs.Position);
            fs.Write(shrubModelBytes, 0, shrubModelBytes.Length);

            SeekPast(fs);
            engineHeader.shrubPointer = (int)fs.Position;
            byte[] shrubBytes = WriteShrubs(level.shrubs);
            fs.Write(shrubBytes, 0, shrubBytes.Length);

            SeekPast(fs);
            engineHeader.textureConfigMenuPointer = (int)fs.Position;
            byte[] menuTextureBytes = WriteTextureConfigMenus(level.textureConfigMenus);
            fs.Write(menuTextureBytes, 0, menuTextureBytes.Length);



            // And these

            // 0x70 2dtexturestuff
            SeekPast(fs);
            engineHeader.texture2dPointer = (int)fs.Position;
            fs.Write(level.billboardBytes, 0, level.billboardBytes.Length);

            // 0x48 soundconfigs
            SeekPast(fs);
            engineHeader.soundConfigPointer = (int)fs.Position;
            fs.Write(level.soundConfigBytes, 0, level.soundConfigBytes.Length);
            


            SeekPast(fs);
            engineHeader.lightPointer = (int)fs.Position;
            byte[] lightBytes = WriteLights(level.lights);
            fs.Write(lightBytes, 0, lightBytes.Length);

            SeekPast(fs);
            engineHeader.lightConfigPointer = (int)fs.Position;
            fs.Write(level.lightConfig, 0, level.lightConfig.Length);

            SeekPast(fs);
            engineHeader.texturePointer = (int)fs.Position;
            byte[] textureBytes = WriteTextures(level.textures);
            fs.Write(textureBytes, 0, textureBytes.Length);


            // Counts
            engineHeader.tieModelCount = level.tieModels.Count();
            engineHeader.tieCount = level.ties.Count();
            engineHeader.shrubModelCount = level.shrubModels.Count();
            engineHeader.shrubCount = level.shrubs.Count();
            engineHeader.weaponCount = level.weaponModels.Count();
            engineHeader.textureCount = level.textures.Count();
            engineHeader.lightCount = level.lights.Count();
            engineHeader.textureConfigMenuCount = level.textureConfigMenus.Count();


            // Seek to the beginning and write the header now that we have all the pointers
            byte[] head = engineHeader.Serialize();
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);

            fs.Close();

            //*/
        }

        private void SeekPast(FileStream fs)
        {
            while (fs.Position % 0x10 != 0)
            {
                fs.Seek(4, SeekOrigin.Current);
            }
        }

        private void WriteUiElements(FileStream fs, List<UiElement> uiElements)
        {
            short offset = 0;
            var spriteIds = new List<int>();
            byte[] elemBytes = new byte[uiElements.Count() * 8];
            for(int i = 0; i < uiElements.Count(); i++)
            {
                WriteShort(ref elemBytes, i * 8 + 0x00, uiElements[i].id);
                if (uiElements[i].id == -1) continue;
                WriteShort(ref elemBytes, i * 8 + 0x02, (short)uiElements[i].sprites.Count());
                WriteShort(ref elemBytes, i * 8 + 0x04, offset);

                spriteIds.AddRange(uiElements[i].sprites);

                offset += (short)uiElements[i].sprites.Count();
            }

            byte[] spriteBytes = new byte[spriteIds.Count() * 4];
            for (int i = 0; i < spriteIds.Count(); i++)
            {
                WriteInt(ref spriteBytes, i * 4, spriteIds[i]);
            }


            int headStart = (int)fs.Position;
            fs.Seek(0x10, SeekOrigin.Current);
            int elemStart = (int)fs.Position;
            fs.Write(elemBytes, 0, elemBytes.Length);
            SeekPast(fs);
            int spriteStart = (int)fs.Position;
            fs.Write(spriteBytes, 0, spriteBytes.Length);
            int sectionEnd = (int)fs.Position;

            byte[] headBytes = new byte[0x10];
            WriteShort(ref headBytes, 0x00, (short)uiElements.Count());
            WriteShort(ref headBytes, 0x02, (short)spriteIds.Count());
            WriteInt(ref headBytes, 0x04, elemStart);
            WriteInt(ref headBytes, 0x08, spriteStart);

            fs.Seek(headStart, SeekOrigin.Begin);
            fs.Write(headBytes, 0, headBytes.Length);
            fs.Seek(sectionEnd, SeekOrigin.Begin);
        }

        private byte[] WriteMobies(List<Model> mobyModels, int initOffset)
        {
            int offs = initOffset;
            byte[] headBytes = new byte[mobyModels.Count * 8 + 4];
            WriteInt(ref headBytes, 0, mobyModels.Count);
            offs += GetLength(headBytes.Length);

            List<byte> bodBytes = new List<byte>();

            for (int i = 0; i < mobyModels.Count; i++)
            {
                WriteInt(ref headBytes, 4 + i * 8, mobyModels[i].id);

                MobyModel g = (MobyModel)mobyModels[i];
                if (!g.isModel)
                    continue;

                WriteInt(ref headBytes, 4 + i * 8 + 4, offs);
                byte[] bodyByte = g.Serialize();
                bodBytes.AddRange(bodyByte);
                offs += GetLength(bodyByte.Length);

            }

            byte[] outBuff = new byte[GetLength(headBytes.Length) + bodBytes.Count];
            headBytes.CopyTo(outBuff, 0);
            bodBytes.CopyTo(outBuff, GetLength(headBytes.Length));

            return outBuff;
        }


        private byte[] WriteWeapons(List<Model> weaponModels, int initOffset)
        {
            int offs = initOffset;
            byte[] headBytes = new byte[weaponModels.Count * 0x10];
            int headLength = GetLength(headBytes.Length);
            offs += headLength;

            List<byte> bodBytes = new List<byte>();

            for (int i = 0; i < weaponModels.Count; i++)
            {
                WriteInt(ref headBytes, i * 0x10, weaponModels[i].id);

                MobyModel g = (MobyModel)weaponModels[i];
                if (!g.isModel)
                    continue;

                byte[] bodyByte = g.Serialize();
                WriteInt(ref headBytes, i * 0x10 + 4, offs);
                WriteInt(ref headBytes, i * 0x10 + 8, bodyByte.Length);
                bodBytes.AddRange(bodyByte);
                offs += GetLength(bodyByte.Length);

            }

            byte[] outBuff = new byte[headLength + bodBytes.Count];
            headBytes.CopyTo(outBuff, 0);
            bodBytes.CopyTo(outBuff, headLength);

            return outBuff;
        }

        private byte[] WriteTies(List<Tie> ties, int offset)
        {
            byte[] headBytes = new byte[ties.Count * 0x70];
            List<byte> colorBytes = new List<byte>();
            int initOffset = offset + ties.Count * 0x70;

            for (int i = 0; i < ties.Count; i++)
            {
                ties[i].ToByteArray(initOffset + colorBytes.Count).CopyTo(headBytes, i * 0x70);
                byte[] colByte = ties[i].colorBytes;
                colorBytes.AddRange(colByte);
                while((colorBytes.Count % 0x80) != 0)
                {
                    colorBytes.Add(0);
                }
            }

            byte[] outBytes = new byte[headBytes.Length + colorBytes.Count];
            headBytes.CopyTo(outBytes, 0);
            colorBytes.CopyTo(outBytes, headBytes.Length);

            return outBytes;
        }

        private byte[] WriteShrubs(List<Shrub> shrubs)
        {
            byte[] outBytes = new byte[shrubs.Count * 0x70];
            for (int i = 0; i < shrubs.Count; i++)
            {
                shrubs[i].ToByteArray().CopyTo(outBytes, i * 0x70);
            }

            return outBytes;
        }

        private byte[] WriteLights(List<Light> lights)
        {
            byte[] outBytes = new byte[lights.Count * 0x40];
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].Serialize().CopyTo(outBytes, i * 0x40);
            }

            return outBytes;
        }

        private byte[] WriteTextureConfigMenus(List<int> textureConfigMenus)
        {
            byte[] outBytes = new byte[textureConfigMenus.Count * 0x4];
            for (int i = 0; i < textureConfigMenus.Count; i++)
            {
                WriteInt(ref outBytes, i * 4, textureConfigMenus[i]);
            }

            return outBytes;
        }

        private byte[] WriteTieModels(List<Model> tiemodels, int startOff)
        {
            byte[] headBytes = new byte[tiemodels.Count * 0x40];
            List<byte> bodyBytes = new List<byte>();

            startOff += tiemodels.Count * 0x40;

            for (int i = 0; i < tiemodels.Count; i++)
            {
                TieModel g = (TieModel)tiemodels[i];
                byte[] tieByte = g.SerializeHead(startOff);
                byte[] bodBytes = g.SerializeBody(startOff);
                bodyBytes.AddRange(bodBytes);
                startOff += bodBytes.Length;
                tieByte.CopyTo(headBytes, i * 0x40);
            }

            byte[] outBytes = new byte[headBytes.Length + bodyBytes.Count];
            headBytes.CopyTo(outBytes, 0);
            bodyBytes.CopyTo(outBytes, headBytes.Length);

            return outBytes;
        }

        private byte[] WriteShrubModels(List<Model> shrubmodels, int startOff)
        {
            byte[] headBytes = new byte[shrubmodels.Count * 0x40];
            List<byte> bodyBytes = new List<byte>();

            startOff += shrubmodels.Count * 0x40;

            for (int i = 0; i < shrubmodels.Count; i++)
            {
                ShrubModel g = (ShrubModel)shrubmodels[i];
                byte[] tieByte = g.SerializeHead(startOff);
                byte[] bodBytes = g.SerializeBody(startOff);
                bodyBytes.AddRange(bodBytes);
                startOff += bodBytes.Length;
                tieByte.CopyTo(headBytes, i * 0x40);
            }

            byte[] outBytes = new byte[headBytes.Length + bodyBytes.Count];
            headBytes.CopyTo(outBytes, 0);
            bodyBytes.CopyTo(outBytes, headBytes.Length);

            return outBytes;
        }

        private byte[] WriteTextures(List<Texture> textures)
        {
            byte[] outBytes = new byte[textures.Count * 0x24];
            for (int i = 0; i < textures.Count; i++)
            {
                textures[i].Serialize().CopyTo(outBytes, i * 0x24);
            }

            return outBytes;
        }

        private byte[] WritePlayerAnimations(List<Animation> animations, int animationOffset)
        {
            int offsetListLength = GetLength(animations.Count * 4);
            animationOffset += offsetListLength;

            List<byte> animByteList = new List<byte>();
            List<int> animOffsets = new List<int>();

            foreach (Animation anim in animations)
            {
                if (anim.frames.Count != 0)
                {
                    animOffsets.Add(animationOffset);
                    byte[] anima = anim.Serialize(0);
                    animByteList.AddRange(anima);
                    animationOffset += anima.Length;
                }
                else
                {
                    animOffsets.Add(0);
                }
            }

            byte[] outBytes = new byte[offsetListLength + animByteList.Count];
            for (int i = 0; i < animations.Count; i++)
            {
                WriteInt(ref outBytes, i * 0x04, animOffsets[i]);
            }
            animByteList.CopyTo(outBytes, offsetListLength);

            return outBytes;
        }

        private void WriteSkybox(FileStream fs, List<UiElement> uiElements)
        {

        }
    }
}
