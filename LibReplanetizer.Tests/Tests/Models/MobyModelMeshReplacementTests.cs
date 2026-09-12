using System.Collections.Generic;
using LibReplanetizer.Models;
using LibReplanetizer.Models.Animations;
using Xunit;

namespace LibReplanetizer.Tests.Models
{
    public class MobyModelMeshReplacementTests
    {
        [Fact]
        public void ReplaceMeshData_CopiesRegularAndMetalMeshDataWithoutAliasing()
        {
            MobyModel source = new MobyModel
            {
                vertexBuffer = new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 0.1f, 0.2f },
                indexBuffer = new ushort[] { 0, 1, 2 },
                vertexBoneWeights = new uint[] { 0x01020304 },
                vertexBoneIds = new uint[] { 0x05060708 },
                rgbas = new byte[] { 10, 20, 30, 40 },
                metalVertexBuffer = new float[] { 7.0f, 8.0f, 9.0f, 10.0f, 11.0f, 12.0f, 0.3f, 0.4f },
                metalIndexBuffer = new ushort[] { 2, 1, 0 },
                metalVertexBoneWeights = new uint[] { 0x11121314 },
                metalVertexBoneIds = new uint[] { 0x15161718 },
                vertexCount2 = 4,
                textureConfig = new List<TextureConfig>
                {
                    new TextureConfig { id = 3, start = 2, size = 4, mode = 5, unk1 = 6, unk2 = 7 }
                },
                metalTextureConfig = new List<TextureConfig>
                {
                    new TextureConfig { id = 8, start = 9, size = 10, mode = 11, unk1 = 12, unk2 = 13 }
                }
            };
            MobyModel target = new MobyModel();
            uint originalMeshDataVersion = target.meshDataVersion;

            target.ReplaceMeshData(source);

            Assert.Equal(originalMeshDataVersion + 1, target.meshDataVersion);
            Assert.Equal(0u, source.meshDataVersion);
            Assert.Equal(source.vertexBuffer, target.vertexBuffer);
            Assert.Equal(source.indexBuffer, target.indexBuffer);
            Assert.Equal(source.vertexBoneWeights, target.vertexBoneWeights);
            Assert.Equal(source.vertexBoneIds, target.vertexBoneIds);
            Assert.Equal(source.rgbas, target.rgbas);
            Assert.Equal(source.metalVertexBuffer, target.metalVertexBuffer);
            Assert.Equal(source.metalIndexBuffer, target.metalIndexBuffer);
            Assert.Equal(source.metalVertexBoneWeights, target.metalVertexBoneWeights);
            Assert.Equal(source.metalVertexBoneIds, target.metalVertexBoneIds);
            Assert.Equal(source.vertexCount2, target.vertexCount2);

            Assert.NotSame(source.vertexBuffer, target.vertexBuffer);
            Assert.NotSame(source.indexBuffer, target.indexBuffer);
            Assert.NotSame(source.textureConfig, target.textureConfig);
            Assert.NotSame(source.textureConfig[0], target.textureConfig[0]);
            Assert.NotSame(source.metalTextureConfig, target.metalTextureConfig);
            Assert.NotSame(source.metalTextureConfig[0], target.metalTextureConfig[0]);
            Assert.Equal(source.textureConfig[0].id, target.textureConfig[0].id);
            Assert.Equal(source.textureConfig[0].unk2, target.textureConfig[0].unk2);
            Assert.Equal(source.metalTextureConfig[0].mode, target.metalTextureConfig[0].mode);

            target.vertexBuffer[0] = 99.0f;
            target.textureConfig[0].id = 100;
            target.metalTextureConfig[0].mode = 101;

            Assert.Equal(1.0f, source.vertexBuffer[0]);
            Assert.Equal(3, source.textureConfig[0].id);
            Assert.Equal(11, source.metalTextureConfig[0].mode);
        }

        [Fact]
        public void ReplaceMeshData_PreservesTargetMetadata()
        {
            MobyModel source = new MobyModel
            {
                vertexBuffer = new float[] { 1.0f },
                indexBuffer = new ushort[] { 0 },
                vertexBoneWeights = new uint[] { 0 },
                vertexBoneIds = new uint[] { 0 }
            };
            MobyModel target = new MobyModel
            {
                id = 42,
                size = 2.5f,
                cullingX = 1.0f,
                cullingY = 2.0f,
                cullingZ = 3.0f,
                cullingRadius = 4.0f,
                color2 = 0xAABBCCDD,
                animations = new List<LibReplanetizer.Models.Animations.Animation>(),
                boneMatrices = new List<BoneMatrix>(),
                boneDatas = new List<BoneData>(),
                attachments = new List<Attachment>(),
                collisionData = new MobyModelCollision()
            };
            List<LibReplanetizer.Models.Animations.Animation> animations = target.animations;
            List<BoneMatrix> boneMatrices = target.boneMatrices;
            List<BoneData> boneDatas = target.boneDatas;
            List<Attachment> attachments = target.attachments;
            MobyModelCollision collisionData = target.collisionData;

            target.ReplaceMeshData(source);

            Assert.Equal(42, target.id);
            Assert.Equal(2.5f, target.size);
            Assert.Equal(1.0f, target.cullingX);
            Assert.Equal(2.0f, target.cullingY);
            Assert.Equal(3.0f, target.cullingZ);
            Assert.Equal(4.0f, target.cullingRadius);
            Assert.Equal(0xAABBCCDDu, target.color2);
            Assert.Same(animations, target.animations);
            Assert.Same(boneMatrices, target.boneMatrices);
            Assert.Same(boneDatas, target.boneDatas);
            Assert.Same(attachments, target.attachments);
            Assert.Same(collisionData, target.collisionData);
        }
    }
}
