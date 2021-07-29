namespace Replanetizer.Frames
{
    public abstract class Frame
    {
        protected Window wnd;
        protected abstract string frameName { get; set; }
        public bool isOpen = true;

        public Frame(Window wnd)
        {
            this.wnd = wnd;
            frameName += " ## " + this.GetHashCode().ToString();
        }

        public abstract void Render(float deltaTime);
    }
}