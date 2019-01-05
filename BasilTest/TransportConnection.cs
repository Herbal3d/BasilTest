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

using Fleck;

namespace org.herbal3d.BasilTest {
    public class TransportConnection {
        private static readonly string _logHeader = "[ClientConnection]";

        private IWebSocketConnection _connection = null;
        private BasilConnection _basilConnection = null;
        public readonly string Id;

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

            _connection.OnOpen = () => { Connection_OnOpen(); };
            _connection.OnClose = () => { Connection_OnClose(); };
            _connection.OnMessage = msg => { Connection_OnMessage(msg); };
            _connection.OnBinary = msg => { Connection_OnBinary(msg); };
            _connection.OnError = except => { Connection_OnError(except); };
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
                _basilConnection.Receive(pMsg);
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
                _connection.Send(pMsg);
            }
        }

        private void AbortAppConnection() {
        }
    }
}
