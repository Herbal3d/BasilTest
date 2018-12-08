// Copyright 2018 Robert Adams
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

using log4net;

namespace org.herbal3d.BasilTest {

    public abstract class Logger {
        public abstract void SetVerbose(bool val);
        public abstract void Log(string msg, params Object[] args);
        public abstract void DebugFormat(string msg, params Object[] args);
        public abstract void ErrorFormat(string msg, params Object[] args);
    }

    public class LoggerConsole : Logger {
        private static readonly ILog _log = LogManager.GetLogger("BasilTest", "BasilTest");

        private bool _verbose = false;
        public override void SetVerbose(bool value) {
            _verbose = value;
        }

        public override void Log(string msg, params Object[] args) {
            System.Console.WriteLine(msg, args);
        }

        // Output the message if 'Verbose' is true
        public override void DebugFormat(string msg, params Object[] args) {
            if (_verbose) {
                System.Console.WriteLine(msg, args);
            }
        }

        public override void ErrorFormat(string msg, params Object[] args) {
            System.Console.WriteLine(msg, args);
        }
    }

    // Do logging with Log4net
    public class LoggerLog4Net : Logger {
        private static readonly ILog _log = LogManager.GetLogger("BasilTest", "BasilTest");
        private static readonly string _logHeader = "[Logger]";

        private bool _verbose = false;
        public override void SetVerbose(bool value) {
            _verbose = value;
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

        public override void Log(string msg, params Object[] args) {
            _log.InfoFormat(msg, args);
        }

        // Output the message if 'Verbose' is true
        public override void DebugFormat(string msg, params Object[] args) {
            _log.DebugFormat(msg, args);
        }

        public override void ErrorFormat(string msg, params Object[] args) {
            _log.ErrorFormat(msg, args);
        }
    }
}
