using System.Numerics;

namespace LibReplanetizer.Models
{
    public static class CollisionMaterialColors
    {
        public const int MaterialCount = 32;

        private static readonly Vector3[] colors =
        {
            new Vector3(0.00f, 0.00f, 1.00f),
            new Vector3(1.00f, 0.00f, 0.00f),
            new Vector3(0.25f, 0.25f, 0.25f),
            new Vector3(0.50f, 0.50f, 0.25f),
            new Vector3(0.00f, 1.00f, 1.00f),
            new Vector3(1.00f, 0.50f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(1.00f, 1.00f, 1.00f),
            new Vector3(0.00f, 1.00f, 0.00f),
            new Vector3(0.50f, 0.25f, 0.75f),
            new Vector3(0.50f, 0.00f, 0.50f),
            new Vector3(0.75f, 0.25f, 0.25f),
            new Vector3(0.25f, 0.75f, 0.50f),
            new Vector3(0.75f, 0.75f, 1.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(0.00f, 0.00f, 0.00f),
            new Vector3(1.00f, 1.00f, 0.00f)
        };

        public static Vector3 GetColor(int materialId)
        {
            return colors[materialId & 0x1F];
        }

        public static float[] GetShaderValues()
        {
            float[] values = new float[MaterialCount * 3];
            for (int i = 0; i < colors.Length; i++)
            {
                values[i * 3] = colors[i].X;
                values[i * 3 + 1] = colors[i].Y;
                values[i * 3 + 2] = colors[i].Z;
            }

            return values;
        }
    }
}
