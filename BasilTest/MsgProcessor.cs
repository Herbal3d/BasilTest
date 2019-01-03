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

        private struct SentRPC<RESP> {
            public UInt32 session;
            public MsgProcessor context;
            public UInt64 timeRPCCreated;
            public Action<RESP> resolver;
            public Action<Exception> rejector;
            public string requestName;
        };
        private Random _randomNumbers = new Random();
        private Dictionary<UInt32, Object> _outstandingRPC = new Dictionary<UInt32, Object>();
        protected BasilConnection _basilConnection;

        public MsgProcessor(BasilConnection pConnection) {
            _basilConnection = pConnection;
        }

        protected IPromise<RESP> SendAndPromiseResponse<REQ,RESP>(REQ req, string pReqName) {
            UInt32 thisSession = (UInt32)_randomNumbers.Next();
#pragma warning disable IDE0017 // Simplify object initialization
            BasilSpaceStream.BasilStreamMessage msg = new BasilSpaceStream.BasilStreamMessage();
#pragma warning restore IDE0017 // Simplify object initialization
            msg.ResponseReq = new BasilType.BResponseRequest() {
                    ResponseSession = thisSession
            };
            BasilSpaceStream.BasilStreamMessage.Descriptor.FindFieldByName(pReqName).Accessor.SetValue(msg, req);
            return new Promise<RESP>((resolve, reject) => {
                lock (_outstandingRPC) {
                    _outstandingRPC.Add(thisSession, new SentRPC<RESP> {
                        session = thisSession,
                        context = this,
                        timeRPCCreated = (ulong)DateTime.UtcNow.ToBinary(),
                        resolver = resolve,
                        rejector = reject,
                        requestName = pReqName
                    });
                }
                _basilConnection.Send(msg.ToByteArray());
            });
        }

        // Received a response type message.
        // Find the matching RPC call info and call the process waiting for the response.
        protected void HandleResponse<RESP>(RESP pResponseMsg, string pResponseMsgName,
                                BasilSpaceStream.SpaceStreamMessage pEnclosing) {
            if (pEnclosing.ResponseReq != null) {
                if (pEnclosing.ResponseReq.ResponseSession != 0) {
                    UInt32 sessionIndex = pEnclosing.ResponseReq.ResponseSession;
                    Object session = null;
                    lock (_outstandingRPC) {
                        if (_outstandingRPC.ContainsKey(sessionIndex)) {
                            session = _outstandingRPC[sessionIndex];
                            _outstandingRPC.Remove(sessionIndex);
                        }
                    }
                    try {
                        // TODO: figure out how to make this 'await'
                        session.GetType().GetMethod("resolver").Invoke(session,
                                        new object[] { pResponseMsg, pResponseMsgName });
                    }
                    catch (Exception e) {
                        BasilTest.log.ErrorFormat("{0} Exception processing message: {1}",
                                        _logHeader, e);
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
            BasilSpaceStream.BasilStreamMessage msg = new BasilSpaceStream.BasilStreamMessage();
            if (pEnclosing != null && pEnclosing.ResponseReq != null) {
                msg.ResponseReq = pEnclosing.ResponseReq;
            }
            BasilSpaceStream.BasilStreamMessage.Descriptor.FindFieldByName(pResponseMsgName).Accessor.SetValue(msg, pResponseMsg);
            _basilConnection.Send(msg.ToByteArray());
        }

        public virtual bool Receive(BasilSpaceStream.SpaceStreamMessage pMsg, BasilConnection pConnection) {
            return false;
        }

    }
}
