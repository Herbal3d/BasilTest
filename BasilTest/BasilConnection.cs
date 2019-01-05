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

using AliveCheck = org.herbal3d.basil.protocol.AliveCheck;
using BasilSpaceStream = org.herbal3d.basil.protocol.BasilSpaceStream;

namespace org.herbal3d.BasilTest {
    // A connection to a SpaceServer from a Basil Viewer.
    // Accept the OpenConnection from client then start the processing of messages.
    public class BasilConnection  {
        private static readonly string _logHeader = "[BasilConnection]";

        public readonly BasilClient Client;
        public readonly TransportConnection Transport;
        private List<MsgProcessor> _MsgProcessors = new List<MsgProcessor>();

        // Per Basil connection RPC information
        public Dictionary<UInt32, Object> OutstandingRPC = new Dictionary<UInt32, Object>();
        public struct SentRPC<RESP> {
            public UInt32 session;
            public MsgProcessor context;
            public UInt64 timeRPCCreated;
            public Action<RESP> resolver;
            public Action<Exception> rejector;
            public string requestName;
        };

        // A socket connection has been made to a Basil Server.
        // Initialize message receivers and senders.
        public BasilConnection(TransportConnection pConnection) {
            Transport = pConnection;
            // Processors for received messages
            _MsgProcessors.Add(new AliveCheckProcessor(this));
            _MsgProcessors.Add(new SpaceServerProcessor(this));
            _MsgProcessors.Add(new BasilClientProcessor(this));
            // Routines for sending messages.
            Client = new BasilClient(this);
        }

        // This process shouldn't be receiving text message over the WebSocket
        public void Receive(string pMsg) {
            BasilTest.log.ErrorFormat("{0} Receive: received a text message: {1}", _logHeader, pMsg);
        }

        // Received a binary message. Find the processor and execute it.
        public void Receive(byte[] pMsg) {
            BasilSpaceStream.SpaceStreamMessage rcvdMsg = BasilSpaceStream.SpaceStreamMessage.Parser.ParseFrom(pMsg);
            foreach (MsgProcessor processor in _MsgProcessors) {
                if (processor.Receive(rcvdMsg, this)) {
                    break;
                }
            }
        }

        // Send the binary message!!
        public void Send(byte[] pMsg) {
            Transport.Send(pMsg);
        }

        // 
        public void AbortConnection() {
        }
    }
}
