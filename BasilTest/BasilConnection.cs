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
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using Google.Protobuf;

using BasilMessage = org.herbal3d.basil.protocol.Message;

namespace org.herbal3d.BasilTest {
    // A connection to a SpaceServer from a Basil Viewer.
    // Accept the OpenConnection from client then start the processing of messages.
    public class BasilConnection  : IDisposable {
        private static readonly string _logHeader = "[BasilConnection]";

        // Public connections to the outside world: transport connection and calls to server
        public readonly TransportConnection Transport;
        public readonly BasilClient Client;

        // Mapping of BasilMessage op's to and from code to string operation name
        public Dictionary<Int32, String> BasilMessageNameByOp = new Dictionary<int, string>();
        public Dictionary<string, Int32> BasilMessageOpByName = new Dictionary<string, int>();

        // The registered processors for received operation codes
        public delegate BasilMessage.BasilMessage ProcessMessage(BasilMessage.BasilMessage pMsg);
        public class Processors : Dictionary<Int32, ProcessMessage> {
        };
        // The processors for received op codes. Added to by the *Processor classes.
        private Processors _MsgProcessors = new Processors();

        // Handles to the various message processors. Held on to for later disposal.
        readonly AliveCheckProcessor _aliveCheckProcessor;
        readonly SpaceServerProcessor _spaceServerProcessor;
        readonly BasilClientProcessor _basilClientProcessor;

        // Per Basil connection RPC information.
        // One is kept for each outstanding RPC request. A later response will find this and call 'resolver'.
        public Dictionary<UInt32, SentRPC> OutstandingRPC = new Dictionary<UInt32, SentRPC>();
        public class SentRPC {
            public UInt32 session;
            public MsgProcessor context;
            public UInt64 timeRPCCreated;
            public Action<BasilMessage.BasilMessage> resolver;
            public Action<Exception> rejector;
            public string requestName;
        };

        // A socket connection has been made to a Basil Server.
        // Initialize message receivers and senders.
        public BasilConnection(TransportConnection pConnection) {
            Transport = pConnection;
            // Build the tables of ops to names based on enum in Protobuf definitions
            this.BuildBasilMessageOps();

            // Processors for received messages
            _aliveCheckProcessor = new AliveCheckProcessor(this);
            _spaceServerProcessor = new SpaceServerProcessor(this);
            _basilClientProcessor = new BasilClientProcessor(this);

            // Routines for sending messages.
            Client = new BasilClient(this);
        }

        public void Dispose() {
            throw new NotImplementedException();
        }

        // Add some message processors for received op codes.
        public void AddMessageProcessors(Processors pProcessors) {
            foreach (var processor in pProcessors) {
                _MsgProcessors.Add(processor.Key, processor.Value);
            }
        }

        // This process shouldn't be receiving text message over the WebSocket
        public void Receive(string pMsg) {
            BasilTest.log.ErrorFormat("{0} Receive: received a text message: {1}", _logHeader, pMsg);
        }

        // Received a binary message. Find the processor and execute it.
        public void Receive(byte[] pMsg) {
            BasilMessage.BasilMessage rcvdMsg = BasilMessage.BasilMessage.Parser.ParseFrom(pMsg);
            if (_MsgProcessors.ContainsKey(rcvdMsg.Op)) {
                try {
                    BasilMessage.BasilMessage reply = _MsgProcessors[rcvdMsg.Op](rcvdMsg);
                    if (reply != null) {
                        this.Send(reply.ToByteArray());
                    }
                }
                catch (Exception e) {
                    BasilTest.log.ErrorFormat("{0} Exception processing received message: {1}, e={2}",
                            _logHeader, BasilMessageNameByOp[rcvdMsg.Op], e);
                }
            }
            else {
                BasilTest.log.ErrorFormat("{0} Receive: received an unknown message op: {1}", _logHeader, rcvdMsg);
            }
        }

        // Send the binary message!!
        public void Send(byte[] pMsg) {
            Transport.Send(pMsg);
        }

        // 
        public void AbortConnection() {
        }

        // Loop over the Protobuf op enum and build a name to op and an op to name map.
        private void BuildBasilMessageOps() {
            Type enumType = typeof(BasilMessage.BasilMessageOps);
            foreach (BasilMessage.BasilMessageOps op in (BasilMessage.BasilMessageOps[])Enum.GetValues(enumType)) {
                string opName = Enum.GetName(enumType, op);
                this.BasilMessageOpByName.Add(opName, (Int32)op);
                this.BasilMessageNameByOp.Add((Int32)op, opName);
            }
        }
    }
}
