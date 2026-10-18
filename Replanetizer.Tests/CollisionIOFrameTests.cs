using System.Collections.Generic;
using LibReplanetizer.Models;
using Replanetizer.Frames;
using Xunit;

namespace Replanetizer.Tests
{
    public class CollisionIOFrameTests
    {
        [Fact]
        public void SelectedChunkZeroResolvesToChunkZero()
        {
            var engineCollision = new Collision();
            var chunkZero = new Collision();
            var chunks = new List<Collision> { chunkZero };

            Assert.Same(chunkZero, CollisionIOFrame.ResolveCollisionTarget(
                engineCollision,
                chunks,
                0));
        }

        [Fact]
        public void ImportToChunkZeroSynchronizesBothSlots()
        {
            var engineCollision = new Collision();
            var chunkZero = new Collision();
            var importedCollision = new Collision();
            var chunks = new List<Collision> { chunkZero };

            Collision updatedEngine = CollisionIOFrame.SetCollisionTarget(
                engineCollision,
                chunks,
                0,
                importedCollision);

            Assert.Same(importedCollision, updatedEngine);
            Assert.Same(importedCollision, chunks[0]);
        }

        [Fact]
        public void ImportToHigherChunkLeavesEngineAndChunkZeroUnchanged()
        {
            var engineCollision = new Collision();
            var chunkZero = new Collision();
            var chunkOne = new Collision();
            var importedCollision = new Collision();
            var chunks = new List<Collision> { chunkZero, chunkOne };

            Collision updatedEngine = CollisionIOFrame.SetCollisionTarget(
                engineCollision,
                chunks,
                1,
                importedCollision);

            Assert.Same(engineCollision, updatedEngine);
            Assert.Same(chunkZero, chunks[0]);
            Assert.Same(importedCollision, chunks[1]);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public void NoChunksUseEngineCollisionRegardlessOfStaleSelection(int selectedChunkIndex)
        {
            var engineCollision = new Collision();
            var importedCollision = new Collision();
            var chunks = new List<Collision>();

            Assert.Same(engineCollision, CollisionIOFrame.ResolveCollisionTarget(engineCollision, chunks, selectedChunkIndex));
            Assert.Same(importedCollision, CollisionIOFrame.SetCollisionTarget(
                engineCollision,
                chunks,
                selectedChunkIndex,
                importedCollision));
        }
    }
}
