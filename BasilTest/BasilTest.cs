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

using org.herbal3d.transport;
using org.herbal3d.cs.CommonEntitiesUtil;

using BasilMessage = org.herbal3d.basil.protocol.Message;

using Fleck;

using System.Security.Cryptography.X509Certificates;
using org.herbal3d.basil.protocol.SpaceServer;

namespace org.herbal3d.BasilTest {
    public class BasilTest {
        // Globals for some things that just are global
        static public Params parms;
        static public BLogger log;
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
                System.Console.WriteLine("BasilTest v" + BasilTest.version
                            + " built " + BasilTest.buildDate
                            + " commit " + BasilTest.gitCommit
                            );
            }

            var spaceServerTester  = new SpaceServerTester();
            HerbalTransport transport = new HerbalTransport(spaceServerTester, BasilTest.parms, BasilTest.log);

            var canceller = new CancellationTokenSource();
            transport.Start(canceller);

            while (!canceller.IsCancellationRequested) {
                Thread.Sleep(100);
            }
        }
    }

    public class SpaceServerTester : ISpaceServer {

        private static readonly string _logHeader = "[SpaceServerTester]";

        public SpaceServerTester() {
        }

        private BasilClient _client;

        void ISpaceServer.SetClientConnection(BasilClient pClient) {
            _client = pClient;
        }

        OpenSessionResp ISpaceServer.OpenSession(OpenSessionReq pReq) {
            var resp = new OpenSessionResp();
            BasilTest.log.DebugFormat("{0} OpenSession", _logHeader);

            if (_client != null) {
                // Start the tester.
                Dictionary<string, string> parms = pReq.Features.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                BasilTest.log.DebugFormat("{0} Received OpenSession. Starting Tester", _logHeader);
                foreach (var kvp in parms) {
                    BasilTest.log.DebugFormat("{0}       {1}: {2}", _logHeader, kvp.Key, kvp.Value);
                }
                BasilTester tester = new BasilTester(_client);
                Task.Run(async () => {
                    await tester.DoTests(parms);
                });
            }
            else {
                resp.Exception = new basil.protocol.BasilType.BasilException() {
                    Reason = "Connection not initialized"
                };
            }
            return resp;
        }

        CloseSessionResp ISpaceServer.CloseSession(CloseSessionReq pReq) {
            return new CloseSessionResp();
        }

        CameraViewResp ISpaceServer.CameraView(CameraViewReq pReq) {
            return new CameraViewResp();
        }
    }
}


