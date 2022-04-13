using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTK
{
    public static class Program
    {
        private static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1024, 768),
                Title = "OpenGL Red Heart",
                //Для корректной работы на Mac OS
                Flags = ContextFlags.ForwardCompatible,
            };

            using var window = new Window(GameWindowSettings.Default, nativeWindowSettings);
            window.Run();
        }
    }
}
