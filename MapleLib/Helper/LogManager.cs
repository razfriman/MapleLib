using Microsoft.Extensions.Logging;

namespace MapleLib.Helper
{
    public static class LogManager
    {
        private static ILogger _log;

        public static ILogger Log
        {
            get
            {
                if (_log == null)
                {
                    var factory = new LoggerFactory();
                    _log = factory.CreateLogger("MapleLib");
                }

                return _log;
            }
        }
    }
}