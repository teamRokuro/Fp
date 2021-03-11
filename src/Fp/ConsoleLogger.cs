using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Fp
{
    /// <summary>
    /// Provides basic formatted logging to console output.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        /// <summary>
        /// Stores configuration properties for <see cref="ConsoleLogger"/>.
        /// </summary>
        public class Config
        {
            /// <summary>
            /// Enabled log levels.
            /// </summary>
            public HashSet<LogLevel> EnabledLevels { get; set; }

            internal static readonly HashSet<LogLevel> _knownLogLevels = new(new[]
            {
                LogLevel.Critical, LogLevel.Debug, LogLevel.Error, LogLevel.Information, LogLevel.None,
                LogLevel.Trace, LogLevel.Warning
            });

            /// <summary>
            /// Creates a new instance of <see cref="Config"/>.
            /// </summary>
            public Config()
            {
                EnabledLevels = new HashSet<LogLevel>(_knownLogLevels);
            }

            /// <summary>
            /// Creates a new instance of <see cref="Config"/> with the specified log levels.
            /// </summary>
            /// <param name="enabledLevels">Enabled log levels.</param>
            public Config(IEnumerable<LogLevel> enabledLevels)
            {
                EnabledLevels = new HashSet<LogLevel>(enabledLevels);
            }
        }


        private readonly Config _config;

        /// <summary>
        /// Creates an instance of <see cref="ConsoleLogger"/> with the specified configuration.
        /// </summary>
        /// <param name="config">Log configuration.</param>
        public ConsoleLogger(Config config)
        {
            _config = config;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            Console.WriteLine(formatter(state, exception));
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) =>
            !Config._knownLogLevels.Contains(logLevel) || _config.EnabledLevels.Contains(logLevel);

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => default!;
    }

    /// <summary>
    /// Creates console logger.
    /// </summary>
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ConsoleLogger.Config _config;

        /// <summary>
        /// Creates a new instance of <see cref="ConsoleLoggerProvider"/>.
        /// </summary>
        /// <param name="config">Logger configuration.</param>
        public ConsoleLoggerProvider(ConsoleLogger.Config config)
        {
            _config = config;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName) => new ConsoleLogger(_config);
    }
}
