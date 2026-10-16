using LibReplanetizer.LevelObjects;
using OpenTK.Mathematics;
using Replanetizer.Utils;
using Xunit;

namespace Replanetizer.Tests
{
    public class CameraSplineFocusTests
    {
        [Fact]
        public void MoveBehindUsesCurrentSplineVertexBuffer()
        {
            var spline = new Spline(1, new[] { 1.0f, 2.0f, 3.0f });
            spline.splineVerticesList.Add(new Vector3(-100.0f, -100.0f, -100.0f));
            spline.SetVertex(0, new Vector3(10.0f, 20.0f, 30.0f));

            var camera = new Camera();
            camera.MoveBehind(spline);

            Assert.Equal(5.0f, camera.position.X, 4);
            Assert.Equal(20.0f, camera.position.Y, 4);
            Assert.Equal(32.5f, camera.position.Z, 4);
        }

        [Fact]
        public void MoveBehindEmptySplineFallsBackToObjectPosition()
        {
            var spline = new Spline(1, System.Array.Empty<float>())
            {
                position = new Vector3(1.0f, 2.0f, 3.0f)
            };

            var camera = new Camera();
            camera.MoveBehind(spline);

            Assert.Equal(-4.0f, camera.position.X, 4);
            Assert.Equal(2.0f, camera.position.Y, 4);
            Assert.Equal(5.5f, camera.position.Z, 4);
        }
    }
}
