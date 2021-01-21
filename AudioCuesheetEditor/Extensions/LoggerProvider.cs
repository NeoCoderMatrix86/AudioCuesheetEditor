using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.JSInterop.WebAssembly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Extensions
{
    public class LoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, WebAssemblyConsoleExtensionLogger<object>> _loggers;
        private readonly ConcurrentDictionary<string, List<String>> _logMessages;

        /// <summary>
        /// Creates an instance of <see cref="LoggerProvider"/>.
        /// </summary>
        public LoggerProvider()
        {
            _loggers = new ConcurrentDictionary<string, WebAssemblyConsoleExtensionLogger<object>>();
            _logMessages = new ConcurrentDictionary<string, List<String>>();
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string name)
        {
            var alreadyContained = _loggers.ContainsKey(name);
            var logger = _loggers.GetOrAdd(name, loggerName => new WebAssemblyConsoleExtensionLogger<object>(name));
            if (alreadyContained == false)
            {
                logger.MessageLogged += Logger_MessageLogged;
            }
            return logger;
        }

        private void Logger_MessageLogged(object sender, KeyValuePair<string, string> e)
        {
            _logMessages.GetOrAdd(e.Key, new List<String>()).Add(e.Value);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            var enumerator = _loggers.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.MessageLogged -= Logger_MessageLogged;
            }
            _loggers.Clear();
            _logMessages.Clear();
        }

        public IReadOnlyCollection<String> GetLogMessages(String loggerName = null)
        {
            List<string> list;
            if (String.IsNullOrEmpty(loggerName) == false)
            {
                list = _logMessages.GetValueOrDefault(loggerName);
            }
            else
            {
                list = new List<string>();
                foreach(var messages in _logMessages.Values)
                {
                    list.AddRange(messages);
                }
            }
            if (list != null)
            {
                return list.AsReadOnly();
            }
            else
            {
                return null;
            }
        }
    }
}
