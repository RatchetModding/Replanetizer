using System;
using System.Collections.Generic;
using System.IO;
using static RatchetEdit.DataFunctions;
using RatchetEdit.Models.Animations;

namespace RatchetEdit.Models
{
    public class MobyModel : Model
    {
        const int VERTELEMENTSIZE = 0x28;
        const int TEXTUREELEMENTSIZE = 0x10;
        const int MESHHEADERSIZE = 0x20;
        const int HEADERSIZE = 0x48;

        public int null1 { get; set; }

        public byte lpBoneCount { get; set; }            // Low poly bone count
        public byte count3 { get; set; }
        public byte count4 { get; set; }
        public byte lpRenderDist { get; set; }            // Low poly render distance
        public byte count8 { get; set; }

        public int null2 { get; set; }
        public int null3 { get; set; }

        public float unk1 { get; set; }
        public float unk2 { get; set; }
        public float unk3 { get; set; }
        public float unk4 { get; set; }

        public uint color2 { get; set; }               // RGBA color
        public uint unk6 { get; set; }

        public ushort vertexCount2 { get; set; }

        public List<Animation> animations { get; set; } = new List<Animation>();
        public List<ModelSound> modelSounds { get; set; } = new List<ModelSound>();
        public List<Attachment> attachments { get; set; } = new List<Attachment>();
        public List<BoneMatrix> boneMatrices { get; set; } = new List<BoneMatrix>();
        public List<BoneData> boneDatas { get; set; } = new List<BoneData>();

        public bool isModel = true;


        // Unparsed sections
        public byte[] type10Block = {  };                  // Hitbox
        

        public MobyModel() { }

        public MobyModel(FileStream fs, short modelID, int offset)
        {
            id = modelID;
            if (offset == 0x00)
            {
                isModel = false;
                return;
            }


                
            // Header
            byte[] headBlock = ReadBlock(fs, offset, HEADERSIZE);

            int meshPointer = ReadInt(headBlock, 0x00);
            null1 = ReadInt(headBlock, 0x04);

            byte boneCount = headBlock[0x08];
            lpBoneCount = headBlock[0x09];
            count3 = headBlock[0x0A];
            count4 = headBlock[0x0B];
            byte animationCount = headBlock[0x0C];
            byte soundCount = headBlock[0x0D];
            lpRenderDist = headBlock[0x0E];
            count8 = headBlock[0x0F];

            int type10Pointer = ReadInt(headBlock, 0x10);
            int boneMatrixPointer = ReadInt(headBlock, 0x14);
            int boneDataPointer = ReadInt(headBlock, 0x18);
            int attachmentPointer = ReadInt(headBlock, 0x1C);

            null2 = ReadInt(headBlock, 0x20);
            size = ReadFloat(headBlock, 0x24);
            int soundPointer = ReadInt(headBlock, 0x28);
            null3 = ReadInt(headBlock, 0x2C);

            if (null1 != 0 || null2 != 0 || null3 != 0) { Console.WriteLine("Warning: null in model header wan't null"); }

            unk1 = ReadFloat(headBlock, 0x30);
            unk2 = ReadFloat(headBlock, 0x34);
            unk3 = ReadFloat(headBlock, 0x38);
            unk4 = ReadFloat(headBlock, 0x3C);

            color2 = ReadUint(headBlock, 0x40);
            unk6 = ReadUint(headBlock, 0x44);

            // Animation block
            byte[] animationPointerBlock = ReadBlock(fs, offset + 0x48, animationCount * 0x04);

            for (int i = 0; i < animationCount; i++)
            {
                //animations.Add(new Animation());
                animations.Add(new Animation(fs, offset, ReadInt(animationPointerBlock, i * 0x04), boneCount));
            }

            // Type 10 ( has something to do with collision )
            if (type10Pointer > 0)
            {
                byte[] type10Head = ReadBlock(fs, offset + type10Pointer, 0x10);
                int type10Length = ReadInt(type10Head, 0x04) + ReadInt(type10Head, 0x08) + ReadInt(type10Head, 0x0C);
                type10Block = ReadBlock(fs, offset + type10Pointer, 0x10 + type10Length);
            }

            // Bone matrix

            if (boneMatrixPointer > 0)
            {
                byte[] boneMatrixBlock = ReadBlock(fs, offset + boneMatrixPointer, boneCount * 0x40);
                for (int i = 0; i < boneCount; i++)
                {
                    boneMatrices.Add(new BoneMatrix(boneMatrixBlock, i));
                }
            }


            // Bone data

            if (boneDataPointer > 0)
            {
                byte[] boneDataBlock = ReadBlock(fs, offset + boneDataPointer, boneCount * 0x10);
                for (int i = 0; i < boneCount; i++)
                {
                    boneDatas.Add(new BoneData(boneDataBlock, i));
                }
            }



            // Attachments
            if (attachmentPointer > 0)
            {
                int attachmentCount = ReadInt(ReadBlock(fs, offset + attachmentPointer, 4), 0);
                byte[] headerBlock = ReadBlock(fs, offset + attachmentPointer + 4, attachmentCount * 4);
                for (int i = 0; i < attachmentCount; i++)
                {
                    int attachmentOffset = ReadInt(headerBlock, i * 4);
                    attachments.Add(new Attachment(fs, offset + attachmentOffset));
                }
            }


            // Sounds
            if (soundPointer > 0)
            {
                byte[] soundBlock = ReadBlock(fs, offset + soundPointer, soundCount * 0x20);
                for (int i = 0; i < soundCount; i++)
                {
                    modelSounds.Add(new ModelSound(soundBlock, i));
                }
            }

            // Mesh meta
            if (meshPointer > 0)
            {
                byte[] meshHeader = ReadBlock(fs, offset + meshPointer, 0x20);

                int texCount = ReadInt(meshHeader, 0x00);
                int otherCount = ReadInt(meshHeader, 0x04);
                int texBlockPointer = offset + ReadInt(meshHeader, 0x08);
                int otherBlockPointer = offset + ReadInt(meshHeader, 0x0C);
                int vertPointer = offset + ReadInt(meshHeader, 0x10);
                int indexPointer = offset + ReadInt(meshHeader, 0x14);
                ushort vertexCount = ReadUshort(meshHeader, 0x18);

                vertexCount2 = ReadUshort(meshHeader, 0x1C);     //These vertices are not affected by color2

                int faceCount = 0;

                //Texture configuration
                if (texBlockPointer > 0)
                {
                    textureConfig = GetTextureConfigs(fs, texBlockPointer, texCount, TEXTUREELEMENTSIZE);
                    faceCount = GetFaceCount();
                }

                if (vertPointer > 0 && vertexCount > 0)
                {
                    //Get vertex buffer float[vertX, vertY, vertZ, normX, normY, normZ, U, V, reserved, reserved]
                    vertexBuffer = GetVertices(fs, vertPointer, vertexCount, VERTELEMENTSIZE);
                }

                if (indexPointer > 0 && faceCount > 0)
                {
                    //Index buffer
                    indexBuffer = GetIndices(fs, indexPointer, faceCount);
                }
            }
        }




        public byte[] Serialize()
        {
            int stupidOffset = 0;
            if (id == 0) stupidOffset = 0x1c * 4;

            byte[] vertexBytes = SerializeVertices();
            byte[] faceBytes = GetFaceBytes();

            //sounds
            byte[] soundBytes = new byte[modelSounds.Count * 0x20];
            for (int i = 0; i < modelSounds.Count; i++)
            {
                byte[] soundByte = modelSounds[i].Serialize();
                soundByte.CopyTo(soundBytes, i * 0x20);
            }

            //boneMatrix
            byte[] boneMatrixBytes = new byte[boneMatrices.Count * 0x40];
            for (int i = 0; i < boneMatrices.Count; i++)
            {
                byte[] boneMatrixByte = boneMatrices[i].Serialize();
                boneMatrixByte.CopyTo(boneMatrixBytes, i * 0x40);
            }

            //boneData
            byte[] boneDataBytes = new byte[boneDatas.Count * 0x10];
            for (int i = 0; i < boneDatas.Count; i++)
            {
                byte[] boneDataByte = boneDatas[i].Serialize();
                boneDataByte.CopyTo(boneDataBytes, i * 0x10);
            }

            
            int meshDataOffset = GetLength(HEADERSIZE + animations.Count * 4 + stupidOffset);
            int textureConfigOffset = meshDataOffset + 0x20;
            int vertOffset = textureConfigOffset + textureConfig.Count * 0x10;
            int faceOffset = GetLength(vertOffset + vertexBytes.Length);
            int type10Offset = GetLength(faceOffset + faceBytes.Length);
            int soundOffset = GetLength(type10Offset + type10Block.Length);
            int attachmentOffset = GetLength(soundOffset + soundBytes.Length);


            List<byte> attachmentBytes = new List<byte>();
            if (attachments != null)
            {
                byte[] attachmentHead = new byte[4 + attachments.Count * 4];
                WriteInt(attachmentHead, 0, attachments.Count);
                int attOffset = attachmentOffset + 4 + attachments.Count * 4;
                for (int i = 0; i < attachments.Count; i++)
                {
                    WriteInt(attachmentHead, 4 + i * 4, attOffset);
                    byte[] attBytes = attachments[i].Serialize();
                    attachmentBytes.AddRange(attBytes);
                    attOffset += attBytes.Length;
                }
                attachmentBytes.InsertRange(0, attachmentHead);
            }



            int boneMatrixOffset = GetLength(attachmentOffset + attachmentBytes.Count);
            int boneDataOffset = GetLength(boneMatrixOffset + boneMatrixBytes.Length);
            int animationOffset = GetLength(boneDataOffset + boneDataBytes.Length);
            int newAnimationOffset = animationOffset;
            List<byte> animByteList = new List<byte>();

            List<int> animOffsets = new List<int>();

            foreach (Animation anim in animations)
            {
                if (anim.frames.Count != 0)
                {
                    animOffsets.Add(newAnimationOffset);
                    byte[] anima = anim.Serialize(newAnimationOffset);
                    animByteList.AddRange(anima);
                    newAnimationOffset += anima.Length;
                }
                else
                {
                    animOffsets.Add(0);
                }
            }

            int modelLength = GetLength(newAnimationOffset);
            byte[] outbytes = new byte[modelLength];


            // Header
            if(vertexBuffer.Length != 0)
                WriteInt(outbytes, 0x00, meshDataOffset);

            outbytes[0x08] = (byte)(boneMatrices.Count);
            outbytes[0x09] = lpBoneCount;
            outbytes[0x0A] = count3;
            outbytes[0x0B] = count4;
            outbytes[0x0C] = (byte)animations.Count;
            outbytes[0x0D] = (byte)modelSounds.Count;
            outbytes[0x0E] = lpRenderDist;
            outbytes[0x0F] = count8;

            if(type10Block.Length != 0)
                WriteInt(outbytes, 0x10, type10Offset);
            
            if(boneMatrices.Count != 0) { 
                WriteInt(outbytes, 0x14, boneMatrixOffset);
                WriteInt(outbytes, 0x18, boneDataOffset);
            }
            
            if(attachments.Count != 0)
                WriteInt(outbytes, 0x1C, attachmentOffset);
            

            //null
            WriteFloat(outbytes, 0x24, size);
            if(modelSounds.Count != 0)
                WriteInt(outbytes, 0x28, soundOffset);
            
            
            //null

            WriteFloat(outbytes, 0x30, unk1);
            WriteFloat(outbytes, 0x34, unk2);
            WriteFloat(outbytes, 0x38, unk3);
            WriteFloat(outbytes, 0x3C, unk4);

            WriteUint(outbytes, 0x40, color2);
            WriteUint(outbytes, 0x44, unk6);

            for (int i = 0; i < animations.Count; i++)
            {
                WriteInt(outbytes, HEADERSIZE + i * 0x04, animOffsets[i]);
            }

            vertexBytes.CopyTo(outbytes, vertOffset);
            faceBytes.CopyTo(outbytes, faceOffset);
            if(type10Block != null)
            {
                type10Block.CopyTo(outbytes, type10Offset);
            }
            
            soundBytes.CopyTo(outbytes, soundOffset);
            attachmentBytes.CopyTo(outbytes, attachmentOffset);
            boneMatrixBytes.CopyTo(outbytes, boneMatrixOffset);
            boneDataBytes.CopyTo(outbytes, boneDataOffset);
            animByteList.CopyTo(outbytes, animationOffset);


            // Mesh header
            WriteInt(outbytes, meshDataOffset + 0x00, textureConfig.Count);
            //Othercount
            WriteInt(outbytes, meshDataOffset + 0x08, textureConfigOffset);
            //otheroffset
            WriteInt(outbytes, meshDataOffset + 0x10, vertOffset);
            WriteInt(outbytes, meshDataOffset + 0x14, faceOffset);
            WriteShort(outbytes, meshDataOffset + 0x18, (short)(vertexBytes.Length / VERTELEMENTSIZE));
            WriteShort(outbytes, meshDataOffset + 0x1C, (short)(vertexCount2));


            for (int i = 0; i < textureConfig.Count; i++)
            {
                WriteInt(outbytes, textureConfigOffset + i * 0x10 + 0x04, textureConfig[i].start);
                WriteInt(outbytes, textureConfigOffset + i * 0x10 + 0x08, textureConfig[i].size);
                WriteInt(outbytes, textureConfigOffset + i * 0x10 + 0x0C, textureConfig[i].mode);
                WriteInt(outbytes, textureConfigOffset + i * 0x10 + 0x00, textureConfig[i].ID);
            }
            
            return outbytes;
        }
    }
}
