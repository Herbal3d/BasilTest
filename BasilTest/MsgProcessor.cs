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

using RSG;
using Google.Protobuf;

using BasilType = org.herbal3d.basil.protocol.BasilType;
using BasilServer = org.herbal3d.basil.protocol.BasilServer;
using BasilSpaceStream = org.herbal3d.basil.protocol.BasilSpaceStream;

namespace org.herbal3d.BasilTest {
    public class MsgProcessor {

        private static readonly string _logHeader = "[MsgProcessor]";

        private Random _randomNumbers = new Random();
        protected BasilConnection _basilConnection;

        public MsgProcessor(BasilConnection pConnection) {
            _basilConnection = pConnection;
        }

        protected IPromise<RESP> SendAndPromiseResponse<REQ,RESP>(REQ pReq, string pReqName) {
            UInt32 thisSession = (UInt32)_randomNumbers.Next();
            BasilSpaceStream.BasilStreamMessage msg = new BasilSpaceStream.BasilStreamMessage() {
                ResponseReq = new BasilType.BResponseRequest() {
                    ResponseSession = thisSession
                }
            };
            var field = BasilSpaceStream.BasilStreamMessage.Descriptor.FindFieldByName(pReqName + "Msg");
            if (field != null) {
                field.Accessor.SetValue(msg, pReq);
            }
            else {
                BasilTest.log.ErrorFormat("{0} SendAndPromiseResponse. Sending unknown response field: {1}",
                            _logHeader, pReqName);
            }
            return new Promise<RESP>((resolve, reject) => {
                BasilTest.log.DebugFormat("{0} SendAndPromiseResponse. Adding RPC session {1}", _logHeader, thisSession);
                lock (_basilConnection.OutstandingRPC) {
                    _basilConnection.OutstandingRPC.Add(thisSession, new BasilConnection.SentRPC<RESP> {
                        session = thisSession,
                        context = this,
                        timeRPCCreated = (ulong)DateTime.UtcNow.ToBinary(),
                        resolver = resolve,
                        rejector = reject,
                        requestName = pReqName
                    });
                };
                _basilConnection.Send(msg.ToByteArray());
            });
        }

        // Received a response type message.
        // Find the matching RPC call info and call the process waiting for the response.
        protected void HandleResponse<RESP>(Object pResponseMsg, string pResponseMsgName,
                                BasilSpaceStream.SpaceStreamMessage pEnclosing) {
            if (pEnclosing.ResponseReq != null) {
                if (pEnclosing.ResponseReq.ResponseSession != 0) {
                    UInt32 sessionIndex = pEnclosing.ResponseReq.ResponseSession;
                    BasilTest.log.DebugFormat("{0} HandleResponse. Received RPC session {1}", _logHeader, sessionIndex);
                    Object session = null;
                    lock (_basilConnection.OutstandingRPC) {
                        if (_basilConnection.OutstandingRPC.ContainsKey(sessionIndex)) {
                            session = _basilConnection.OutstandingRPC[sessionIndex];
                            _basilConnection.OutstandingRPC.Remove(sessionIndex);
                        }
                        else {
                            BasilTest.log.ErrorFormat("{0} missing RCP response key: {1}", _logHeader, sessionIndex);
                        }
                    }
                    if (session != null) {
                        try {
                            // TODO: figure out how to make this 'await'
                            session.GetType().GetMethod("resolver").Invoke(session,
                                            new object[] { pResponseMsg });
                            /*
                            session.GetType().GetMember("resolver");
                            */
                        }
                        catch (Exception e) {
                            BasilTest.log.ErrorFormat("{0} Exception processing message: {1}",
                                            _logHeader, e);
                        }
                    }
                }
                else {
                    BasilTest.log.ErrorFormat("{0} ResponseReq.ResponseSession missing. Type={1}",
                                    _logHeader, pResponseMsgName);
                }
            }
            else {
                BasilTest.log.ErrorFormat("{0} Response without ResponseReq. Type={1}",
                                _logHeader, pResponseMsgName);
            }
        }

        // Construct enclosing stream message to send back to the Basil viewer.
        // Called with a constructed response message and the stream message with the request.
        // Add the response information to the response message so other side can match
        //     the response to the request.
        protected void SendResponse<RESP>(RESP pResponseMsg, string pResponseMsgName,
                                BasilSpaceStream.SpaceStreamMessage pEnclosing) {
            BasilTest.log.DebugFormat("{0} SendResponse: {1}", _logHeader, pResponseMsgName);
            BasilSpaceStream.BasilStreamMessage msg = new BasilSpaceStream.BasilStreamMessage();
            if (pEnclosing != null && pEnclosing.ResponseReq != null) {
                msg.ResponseReq = pEnclosing.ResponseReq;
            }
            var field = BasilSpaceStream.BasilStreamMessage.Descriptor.FindFieldByName(pResponseMsgName + "Msg");
            if (field != null) {
                field.Accessor.SetValue(msg, pResponseMsg);
            }
            else {
                BasilTest.log.ErrorFormat("{0} SendResponse. Sending unknown response field: {1}",
                            _logHeader, pResponseMsgName);
            }
            _basilConnection.Send(msg.ToByteArray());
        }

        // Overwrite function for when data is received.
        public virtual bool Receive(BasilSpaceStream.SpaceStreamMessage pMsg, BasilConnection pConnection) {
            return false;
        }

    }
}
