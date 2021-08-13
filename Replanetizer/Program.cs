namespace Replanetizer
{
    class Program
    {
        [System.STAThread]
        static void Main(string[] args)
        {
            Window wnd = new Window(args);
            wnd.Run();
        }
    }
}
