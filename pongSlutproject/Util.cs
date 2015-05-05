using System.Reflection;
using System.IO;

namespace pongSlutproject
{
    public static class Util
    {
        public static Stream GetStream(string filename)
        {
            //..................:)........................finds wear the porject is and its path....................nameSpace.sound.wav
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("pongSlutproject." + filename);
        }
    }
}