namespace Replanetizer.Frames
{
    public abstract class LevelSubFrame : Frame
    {
        protected LevelFrame levelFrame;
        
        public LevelSubFrame(Window wnd, LevelFrame levelFrame) : base(wnd)
        {
            this.levelFrame = levelFrame;
        }
    }
}