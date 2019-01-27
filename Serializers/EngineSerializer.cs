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

            fs.Seek(0x90, SeekOrigin.Begin);

            engineHeader.uiElementPointer = (int)fs.Position;
            WriteUiElements(fs, level.uiElements);

            SeekPast(fs);
            byte[] skyboxBytes = level.skybox.Serialize((int)fs.Position);
            fs.Write(skyboxBytes, 0, skyboxBytes.Length);



            //Counts
            engineHeader.tieModelCount = level.tieModels.Count();
            engineHeader.tieCount = level.ties.Count();
            engineHeader.shrubModelCount = level.shrubModels.Count();
            engineHeader.shrubCount = level.shrubs.Count();
            //engineHeader.weaponCount = level.weapons.Count();
            engineHeader.textureCount = level.textures.Count();
            //engineHeader.lightCount = level.lights.Count();
            //engineHeader.textureConfigMenuCount = level.textureConfigMenu.Count();

            byte[] head = engineHeader.Serialize();
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(head, 0, head.Length);

            fs.Close();
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

        private void WriteSkybox(FileStream fs, List<UiElement> uiElements)
        {

        }
    }
}
