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

using AliveCheck = org.herbal3d.basil.protocol.AliveCheck;
using BasilType = org.herbal3d.basil.protocol.BasilType;
using BasilSpaceStream = org.herbal3d.basil.protocol.BasilSpaceStream;

namespace org.herbal3d.BasilTest {
    public class AliveCheckProcessor : MsgProcessor {

        private int _AliveSequenceNumber = 111;

        public AliveCheckProcessor(BasilConnection pConnection) : base(pConnection) {
        }

        public override bool Receive(BasilSpaceStream.SpaceStreamMessage pMsg,
                                        BasilConnection pConnection) {
            bool ret = false;
            if (pMsg.AliveCheckReqMsg != null) {
                ret = true;
                SendResponse<AliveCheck.AliveCheckResp>(
                    ProcAliveCheckReq(pMsg.AliveCheckReqMsg), "AliveCheckResp", pMsg);
            }
            if (pMsg.AliveCheckRespMsg != null) {
                ret = true;
                HandleResponse<AliveCheck.AliveCheckResp>(
                            pMsg.AliveCheckRespMsg, "AliveCheckResp", pMsg);
            }
            return ret;
        }

        public IPromise<AliveCheck.AliveCheckResp> AliveCheck(
                        BasilType.AccessAuthorization pAuth) {
            var req = MakeAliveCheckReq(pAuth);
            return this.SendAndPromiseResponse<AliveCheck.AliveCheckReq,
                                               AliveCheck.AliveCheckResp>(req,
                                               "AliveCheckReq");
        }

        // Send an AliveCheck request without expecting a response
        public void AliveCheckNR(
                        BasilType.AccessAuthorization pAuth) {
            var req = MakeAliveCheckReq(pAuth);
            SendResponse<AliveCheck.AliveCheckReq>(req, "AliveCheckReq", null);
        }

        private AliveCheck.AliveCheckReq MakeAliveCheckReq(
                        BasilType.AccessAuthorization pAuth) {
            return new AliveCheck.AliveCheckReq {
                Auth = pAuth,
                Time = (ulong)DateTime.UtcNow.ToBinary(),
                SequenceNum = _AliveSequenceNumber++
                
            };
        }

        private AliveCheck.AliveCheckResp ProcAliveCheckReq(
                        AliveCheck.AliveCheckReq pReq) {
            return new AliveCheck.AliveCheckResp {
                Time = (ulong)DateTime.UtcNow.ToBinary(),
                SequenceNum = _AliveSequenceNumber++,
                TimeReceived = pReq.Time,
                SequenceNumReceived = pReq.SequenceNum
            };
        }
    }
}
