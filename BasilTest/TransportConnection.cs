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
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

using Fleck;

namespace org.herbal3d.BasilTest {
    // Wraps the socket connection and manages socket specific operations.
    public class TransportConnection {
        private static readonly string _logHeader = "[TransportConnection]";

        private IWebSocketConnection _connection = null;
        private BasilConnection _basilConnection = null;
        public readonly string Id;

        private BlockingCollection<byte[]> _receiveQueue;
        private BlockingCollection<byte[]> _sendQueue;

        public event Action<TransportConnection> OnDisconnect;
        public enum ConnectionStates {
            INITIALIZING,
            OPEN,
            CLOSING,
            ERROR,
            CLOSED
        };
        public ConnectionStates ConnectionState;
        public bool IsConnected {
            get {
                return (ConnectionState == ConnectionStates.OPEN && _basilConnection != null);
            }
        }
        public string ConnectionName = "UNKNOWN";

        public TransportConnection(IWebSocketConnection pConnection) {
            _connection = pConnection;
            Id = _connection.ConnectionInfo.Id.ToString();
            ConnectionName = _connection.ConnectionInfo.ClientIpAddress.ToString()
                            + ":"
                            + _connection.ConnectionInfo.ClientPort.ToString();
            ConnectionState = ConnectionStates.INITIALIZING;

            _receiveQueue = new BlockingCollection<byte[]>(new ConcurrentQueue<byte[]>());
            _sendQueue = new BlockingCollection<byte[]>(new ConcurrentQueue<byte[]>());

            // Tasks to push and pull from the input and output queues
            Task.Run(() => {
                while (BasilTest.KeepRunning) {
                    byte[] msg = _receiveQueue.Take();
                    _basilConnection.Receive(msg);
                }
            });
            Task.Run(() => {
                while (BasilTest.KeepRunning) {
                    byte[] msg = _sendQueue.Take();
                    _connection.Send(msg);
                }
            });

            _connection.OnOpen = () => { Connection_OnOpen(); };
            _connection.OnClose = () => { Connection_OnClose(); };
            _connection.OnMessage = msg => { Connection_OnMessage(msg); };
            _connection.OnBinary = msg => { Connection_OnBinary(msg); };
            _connection.OnError = except => { Connection_OnError(except); };
        }

        public void Disconnect() {
            if (IsConnected) {
                ConnectionState = ConnectionStates.CLOSING;
                this.TriggerDisconnect();
                _connection.Close();
            }
        }

        // A WebSocket connection has been made.
        // Initialized the message processors.
        private void Connection_OnOpen() {
            if (ConnectionState == ConnectionStates.INITIALIZING) {
                ConnectionState = ConnectionStates.OPEN;
                // Get the processor for the messages
                _basilConnection = new BasilConnection(this);
            }
            else {
                ConnectionState = ConnectionStates.ERROR;
                BasilTest.log.ErrorFormat("{0} OnOpen event on {1} when connection not initializing",
                        _logHeader, ConnectionName);
            }
        }

        // The WebSocket connection is closed. Any application state is out-of-luck
        private void Connection_OnClose() {
            ConnectionState = ConnectionStates.CLOSED;
            TriggerDisconnect();
            if (_basilConnection != null) {
                _basilConnection.AbortConnection();
            }
        }

        private void Connection_OnMessage(string pMsg) {
            if (IsConnected) {
                _basilConnection.Receive(pMsg);
            }
        }

        private void Connection_OnBinary(byte [] pMsg) {
            if (IsConnected) {
                _receiveQueue.Add(pMsg);
            }
        }

        private void Connection_OnError(Exception pExcept) {
            ConnectionState = ConnectionStates.ERROR;
            BasilTest.log.ErrorFormat("{0} OnError event on {1}: {2}", _logHeader, ConnectionName, pExcept);
        }

        // The WebSocket connection is disconnected. Tell the listeners.
        private void TriggerDisconnect() {
            Action<TransportConnection> actions = OnDisconnect;
            if (actions != null) {
                foreach (Action<TransportConnection> action in actions.GetInvocationList()) {
                    action(this);
                }
            }
        }

        public void Send(byte[] pMsg) {
            if (IsConnected) {
                _sendQueue.Add(pMsg);
            }
        }
    }
}
