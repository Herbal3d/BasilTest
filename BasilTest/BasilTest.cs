// Copyright (c) 2019 Robert Adams
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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Fleck;

using System.Security.Cryptography.X509Certificates;

namespace org.herbal3d.BasilTest {
    public class BasilTest {
        // Globals for some things that just are global
        static public Params parms;
        static public Logger log;
        static public Statistics stats;
        // Global flag used to let everyone know when to stop processing
        static public bool KeepRunning = false;

        static public string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        // A command is added to the pre-build events that generates BuildDate resource:
        //        echo %date% %time% > "$(ProjectDir)\Resources\BuildDate.txt"
        static public string buildDate = Properties.Resources.BuildDate.Trim();
        // A command is added to the pre-build events that generates last commit resource:
        //        git rev-parse HEAD > "$(ProjectDir)\Resources\GitCommit.txt"
        static public string gitCommit = Properties.Resources.GitCommit.Trim();

        private static readonly string _logHeader = "[BasilTest]";

        private List<TransportConnection> _transports = new List<TransportConnection>();

        private string Invocation() {
            StringBuilder buff = new StringBuilder();
            buff.AppendLine("Invocation: BasilTest <parameters>");
            buff.AppendLine("   Possible parameters are (negate bool parameters by prepending 'no'):");
            string[] paramDescs = BasilTest.parms.ParameterDefinitions.Select(pp => { return pp.ToString(); }).ToArray();
            buff.AppendLine(String.Join(Environment.NewLine, paramDescs));
            return buff.ToString();
        }

        static void Main(string[] args) {
            BasilTest.parms = new Params();
            BasilTest.log = new LoggerConsole();
            BasilTest.stats = new Statistics();

            BasilTest basilTest = new BasilTest();
            basilTest.Start(args);
            return;
        }

        public void Start(string[] args) {
            // A single parameter of '--help' outputs the invocation parameters
            if (args.Length > 0 && args[0] == "--help") {
                System.Console.Write(Invocation());
                return;
            }

            // 'Params' initializes to default values.
            // Override default values with command line parameters.
            try {
                // Note that trailing parameters will be put into "Extras" parameter
                BasilTest.parms.MergeCommandLine(args, null, "Extras");
            }
            catch (Exception e) {
                BasilTest.log.ErrorFormat("ERROR: bad parameters: " + e.Message);
                BasilTest.log.ErrorFormat(Invocation());
                return;
            }

            if (BasilTest.parms.P<bool>("Verbose")) {
                BasilTest.log.SetVerbose(BasilTest.parms.P<bool>("Verbose"));
            }

            if (!BasilTest.parms.P<bool>("Quiet")) {
                System.Console.WriteLine("Convoar v" + BasilTest.version
                            + " built " + BasilTest.buildDate
                            + " commit " + BasilTest.gitCommit
                            );
            }

            BasilTest.KeepRunning = true;

            FleckLog.Level = LogLevel.Warn;
            List<TransportConnection> allClientConnections = new List<TransportConnection>();

            // For debugging, it is possible to set up a non-encrypted connection
            WebSocketServer server = null;
            if (BasilTest.parms.P<bool>("IsSecure")) {
                BasilTest.log.DebugFormat("{0} Creating secure server", _logHeader);
                server = new WebSocketServer(BasilTest.parms.P<string>("SecureConnectionURL")) {
                    Certificate = new X509Certificate2(BasilTest.parms.P<string>("Certificate")),
                    EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
                };
            }
            else {
                BasilTest.log.DebugFormat("{0} Creating insecure server", _logHeader);
                server = new WebSocketServer(BasilTest.parms.P<string>("ConnectionURL"));
            }

            // Disable the ACK delay for better responsiveness
            if (BasilTest.parms.P<bool>("DisableNaglesAlgorithm")) {
                server.ListenerSocket.NoDelay = true;
            }

            // Loop around waiting for connections
            using (server) {
                server.Start(socket => {
                    BasilTest.log.DebugFormat("{0} Received WebSocket connection", _logHeader);
                    lock (_transports) {
                        TransportConnection transportConnection = new TransportConnection(socket);
                        transportConnection.OnDisconnect += client => {
                            lock (_transports) {
                                BasilTest.log.InfoFormat("{0} client disconnected", _logHeader);
                                _transports.Remove(client);
                            }
                        };
                        _transports.Add(transportConnection);
                    };

                });
                while (KeepRunning) {
                    Thread.Sleep(250);
                }
            }

        }
    }
}


