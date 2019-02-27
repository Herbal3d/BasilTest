// Copyright (c) 2019 Robert Adams
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

using log4net;

namespace org.herbal3d.BasilTest {

    public abstract class Logger {
        public abstract void SetVerbose(bool pValue);
        public abstract void Log(string pMsg, params Object[] pArgs);
        public abstract void InfoFormat(string pMsg, params Object[] pArgs);
        public abstract void DebugFormat(string pMsg, params Object[] pArgs);
        public abstract void ErrorFormat(string pMsg, params Object[] pArgs);
    }

    public class LoggerConsole : Logger {
        private static readonly ILog _log = LogManager.GetLogger("BasilTest", "BasilTest");

        private bool _verbose = false;
        public override void SetVerbose(bool pValue) {
            _verbose = pValue;
        }

        public override void Log(string pMsg, params Object[] pArgs) {
            System.Console.WriteLine(pMsg, pArgs);
        }

        public override void InfoFormat(string pMsg, params Object[] pArgs) {
            System.Console.WriteLine(pMsg, pArgs);
        }

        // Output the message if 'Verbose' is true
        public override void DebugFormat(string pMsg, params Object[] pArgs) {
            if (_verbose) {
                System.Console.WriteLine(pMsg, pArgs);
            }
        }

        public override void ErrorFormat(string pMsg, params Object[] pArgs) {
            System.Console.WriteLine(pMsg, pArgs);
        }
    }

    // Do logging with Log4net
    public class LoggerLog4Net : Logger {
        private static readonly ILog _log = LogManager.GetLogger("BasilTest", "BasilTest");
        private static readonly string _logHeader = "[Logger]";

        private bool _verbose = false;
        public override void SetVerbose(bool pValue) {
            _verbose = pValue;
            bool alreadyDebug = (LogManager.GetRepository("BasilTest").Threshold == log4net.Core.Level.Debug);
            if (_verbose && !alreadyDebug) {
                // turning Verbose on
                _log.InfoFormat("{0} SetVerbose: Setting logging threshold to DEBUG", _logHeader);
                // LogManager.GetRepository().Threshold = log4net.Core.Level.Debug;
                var logHeir = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository("BasilTest");
                logHeir.Root.Level = log4net.Core.Level.Debug;
                logHeir.RaiseConfigurationChanged(EventArgs.Empty);
            }
        }

        public override void Log(string pMsg, params Object[] pArgs) {
            _log.InfoFormat(pMsg, pArgs);
        }

        public override void InfoFormat(string pMsg, params Object[] pArgs) {
            _log.InfoFormat(pMsg, pArgs);
        }

        // Output the message if 'Verbose' is true
        public override void DebugFormat(string pMsg, params Object[] pArgs) {
            _log.DebugFormat(pMsg, pArgs);
        }

        public override void ErrorFormat(string pMsg, params Object[] pArgs) {
            _log.ErrorFormat(pMsg, pArgs);
        }
    }
}
