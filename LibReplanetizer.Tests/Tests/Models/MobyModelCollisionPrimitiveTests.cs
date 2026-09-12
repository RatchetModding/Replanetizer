using LibReplanetizer.Models;
using Xunit;

namespace LibReplanetizer.Tests.Models
{
    public class MobyModelCollisionPrimitiveTests
    {
        [Fact]
        public void IndexedCapsuleAliasesReadPackedIndicesAndRadius()
        {
            var primitive = new MobyModelCollisionPrimitive
            {
                value0 = unchecked((int) 0x12345678),
                value2 = 2560.0f
            };

            Assert.Equal(0x1234, primitive.indexedCapsuleVertex0);
            Assert.Equal(0x5678, primitive.indexedCapsuleVertex1);
            Assert.Equal(2.5f, primitive.indexedCapsuleRadius);
        }

        [Fact]
        public void ShapeAliasesUseVerifiedSlots()
        {
            var primitive = new MobyModelCollisionPrimitive
            {
                value0 = 7,
                value1 = 1.0f,
                value2 = 2048.0f,
                value3 = 3072.0f,
                value4 = 4096.0f,
                value5 = 5120.0f,
                value6 = 6144.0f
            };

            Assert.Equal(7, primitive.indexedSphereVertex);
            Assert.Equal(3.0f, primitive.indexedSphereOffsetX);
            Assert.Equal(4.0f, primitive.indexedSphereOffsetY);
            Assert.Equal(5.0f, primitive.indexedSphereOffsetZ);
            Assert.Equal(2.0f, primitive.indexedSphereRadius);
            Assert.Equal(3.0f, primitive.sphereCenterX);
            Assert.Equal(6.0f, primitive.sphereRadius);
            Assert.Equal(3.0f, primitive.capsuleCenterX);
            Assert.Equal(6.0f, primitive.capsuleRadius);
        }

        [Fact]
        public void CapsuleLengthUsesFloatValue0AndFixedPointScale()
        {
            var primitive = new MobyModelCollisionPrimitive
            {
                value0 = System.BitConverter.SingleToInt32Bits(1280.0f)
            };

            Assert.Equal(1.25f, primitive.capsuleLength);
            Assert.Equal(0.625f, primitive.capsuleHalfLength);

            primitive.capsuleHalfLength = 2.5f;
            Assert.Equal(5.0f, primitive.capsuleLength);
        }
    }
}
