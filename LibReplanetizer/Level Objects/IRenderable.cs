namespace LibReplanetizer.LevelObjects
{
    public interface IRenderable
    {
        ushort[] GetIndices();
        float[] GetVertices();

        bool IsDynamic();
    }
}