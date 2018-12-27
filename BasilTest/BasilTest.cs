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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using RSG;

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

        // The tasks that are being created for the client
        List<Task> tasks = new List<Task>();

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
                System.Console.WriteLine("Convoar v" + BasilTest.version
                            + " built " + BasilTest.buildDate
                            + " commit " + BasilTest.gitCommit
                            );
            }

            BasilTest.KeepRunning = true;

            using (var httpServer = new BHttpServer(BasilTest.parms.P<int>("ListenPort"))) {
                while (BasilTest.KeepRunning) {
                    httpServer.AcceptConnection()
                        .Then(handle => {
                            // Test that it is a WebSocket connection
                            // Create a transport for the client logic
                            using (var transport = new BTransportWS(handle)) {
                                tasks.Add(Task.Run(() => {
                                    using (var client = new BasilClient(transport)) {
                                        var tester = new BasilTester(client);
                                        tester.DoTests();
                                    }
                                }));
                            }
                        })
                        .Catch(e => {
                        });
                };
            };
        }
    }
}
