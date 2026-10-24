using System.Numerics;

namespace LibReplanetizer.Models
{
    public static class CollisionMaterialColors
    {
        public const int MaterialCount = 32;

        private static readonly Vector3[] colors =
        {
            new Vector3(0.00f, 0.00f, 1.00f),   // 0x00
            new Vector3(1.00f, 0.00f, 0.00f),   // 0x01
            new Vector3(0.25f, 0.25f, 0.25f),   // 0x02
            new Vector3(0.50f, 0.50f, 0.25f),   // 0x03
            new Vector3(0.00f, 1.00f, 1.00f),   // 0x04
            new Vector3(1.00f, 0.50f, 0.00f),   // 0x05
            new Vector3(0.50f, 0.25f, 0.25f),   // 0x06
            new Vector3(1.00f, 1.00f, 1.00f),   // 0x07
            new Vector3(0.00f, 1.00f, 0.00f),   // 0x08
            new Vector3(0.50f, 0.25f, 0.75f),   // 0x09
            new Vector3(0.50f, 0.00f, 0.50f),   // 0x0A
            new Vector3(0.75f, 0.25f, 0.25f),   // 0x0B
            new Vector3(0.25f, 0.75f, 0.50f),   // 0x0C
            new Vector3(0.75f, 0.75f, 1.00f),   // 0x0D
            new Vector3(0.00f, 0.00f, 0.50f),   // 0x0E
            new Vector3(0.25f, 0.50f, 0.25f),   // 0x0F
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x10
            new Vector3(0.50f, 0.00f, 0.75f),   // 0x11
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x12
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x13
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x14
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x15
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x16
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x17
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x18
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x19
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x1A
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x1B
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x1C
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x1D
            new Vector3(0.00f, 0.00f, 0.00f),   // 0x1E
            new Vector3(1.00f, 1.00f, 0.00f)    // 0x1F
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
