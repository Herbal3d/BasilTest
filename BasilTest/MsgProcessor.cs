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
using System.Reflection;

using RSG;
using Google.Protobuf;

using BasilType = org.herbal3d.basil.protocol.BasilType;
using BasilMessage = org.herbal3d.basil.protocol.Message;

namespace org.herbal3d.BasilTest {
    public abstract class MsgProcessor {

        private static readonly string _logHeader = "[MsgProcessor]";

        private Random _randomNumbers = new Random();
        protected BasilConnection _basilConnection;

        public MsgProcessor(BasilConnection pConnection) {
            _basilConnection = pConnection;
        }

        // Send a message and expect a RPC type response.
        protected IPromise<BasilMessage.BasilMessage> SendAndPromiseResponse(BasilMessage.BasilMessage pReq) {
            return new Promise<BasilMessage.BasilMessage>((resolve, reject) => {
                UInt32 thisSession = (UInt32)_randomNumbers.Next();
                pReq.Response = new BasilType.BResponseRequest() {
                    ResponseSession = thisSession
                };
                BasilTest.log.DebugFormat("{0} SendAndPromiseResponse. Adding RPC session {1}", _logHeader, thisSession);
                lock (_basilConnection.OutstandingRPC) {
                    _basilConnection.OutstandingRPC.Add(thisSession, new BasilConnection.SentRPC() {
                        session = thisSession,
                        context = this,
                        timeRPCCreated = (ulong)DateTime.UtcNow.ToBinary(),
                        resolver = resolve,
                        rejector = reject,
                        requestName = _basilConnection.BasilMessageNameByOp[pReq.Op]
                    });
                };
                _basilConnection.Send(pReq.ToByteArray());
            });
        }

        // Construct enclosing stream message to send back to the Basil viewer.
        // Called with a constructed response message and the stream message with the request.
        // Add the response information to the response message so other side can match
        //     the response to the request.
        protected void SendMessage(BasilMessage.BasilMessage pResponseMsg, BasilMessage.BasilMessage pReqMsg) {
            string responseMsgName = _basilConnection.BasilMessageNameByOp[pResponseMsg.Op];
            BasilTest.log.DebugFormat("{0} SendResponse: {1}", _logHeader, responseMsgName);

            BasilMessage.BasilMessage msg = new BasilMessage.BasilMessage();
            if (pReqMsg != null && pReqMsg.Response != null) {
                msg.Response = pReqMsg.Response;
            }
            _basilConnection.Send(msg.ToByteArray());
        }

        // Given a request messsage and a partial response message, add the response tagging formation
        //    to the response so the sender of the request can match the messages.
        protected void MakeMessageAResponse(ref BasilMessage.BasilMessage pResponseMsg,
                    BasilMessage.BasilMessage pRequestMsg) {
            if (pRequestMsg.Response != null) {
                pResponseMsg.Response = pRequestMsg.Response;
            }
        }

        // Received a response type message.
        // Find the matching RPC call info and call the process waiting for the response.
        protected BasilMessage.BasilMessage HandleResponse(BasilMessage.BasilMessage pResponseMsg) {
            if (pResponseMsg.Response != null) {
                if (pResponseMsg.Response.ResponseSession != 0) {
                    UInt32 sessionIndex = pResponseMsg.Response.ResponseSession;
                    BasilConnection.SentRPC session;
                    Action<BasilMessage.BasilMessage> processor = null;
                    lock (_basilConnection.OutstandingRPC) {
                        if (_basilConnection.OutstandingRPC.ContainsKey(sessionIndex)) {
                            session = (BasilConnection.SentRPC)_basilConnection.OutstandingRPC[sessionIndex];
                            _basilConnection.OutstandingRPC.Remove(sessionIndex);
                            processor = session.resolver;
                        }
                        else {
                            BasilTest.log.ErrorFormat("{0} missing RCP response key: {1}", _logHeader, sessionIndex);
                        }
                    }
                    if (processor != null) {
                        try {
                            // TODO: figure out how to make this 'await'
                            processor(pResponseMsg);
                        }
                        catch (Exception e) {
                            BasilTest.log.ErrorFormat("{0} Exception processing message: {1}",
                                            _logHeader, e);
                        }
                    }
                }
                else {
                    BasilTest.log.ErrorFormat("{0} ResponseReq.ResponseSession missing. Type={1}",
                                    _logHeader, _basilConnection.BasilMessageNameByOp[pResponseMsg.Op]);
                }
            }
            else {
                BasilTest.log.ErrorFormat("{0} Response without ResponseReq. Type={1}",
                                _logHeader, _basilConnection.BasilMessageNameByOp[pResponseMsg.Op]);
            }
            return null;    // responses don't have a response
        }
    }
}
