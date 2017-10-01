using Microsoft.Extensions.Logging;

namespace MapleLib.Helper
{
    public static class LogManager
    {
        private static ILogger _log;

        public static void ConfigureLogger(ILoggerFactory factory)
        {
            factory.AddDebug(LogLevel.Debug);
            factory.AddConsole(LogLevel.Debug);
        }

        public static ILogger Log
        {
            get
            {
				if (_log == null)
				{
					var factory = new LoggerFactory()
						.AddConsole()
						.AddDebug();

					_log = factory.CreateLogger("MapleLib");
				}

				return _log;
            }
        }
    }
}