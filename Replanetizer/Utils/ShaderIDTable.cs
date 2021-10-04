namespace Replanetizer.Utils
{
    public class ShaderIDTable
    {
        public int ShaderMain { get; set; }
        public int ShaderColor { get; set; }
        public int ShaderCollision { get; set; }
        public int UniformColor { get; set; }
        public int UniformWorldToViewMatrix { get; set; }
        public int UniformModelToWorldMatrix { get; set; }
        public int UniformColorWorldToViewMatrix { get; set; }
        public int UniformColorModelToWorldMatrix { get; set; }
        public int UniformCollisionWorldToViewMatrix { get; set; }
        public int UniformUseFog { get; set; }
        public int UniformFogColor { get; set; }
        public int UniformFogFarDist { get; set; }
        public int UniformFogNearDist { get; set; }
        public int UniformFogFarIntensity { get; set; }
        public int UniformFogNearIntensity { get; set; }
        public int UniformLevelObjectType { get; set; }
        public int UniformLevelObjectNumber { get; set; }
        public int UniformColorLevelObjectType { get; set; }
        public int UniformColorLevelObjectNumber { get; set; }
        public int UniformAmbientColor { get; set; }
    }
}