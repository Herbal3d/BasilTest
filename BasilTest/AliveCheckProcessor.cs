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
using BasilMessage = org.herbal3d.basil.protocol.Message;

namespace org.herbal3d.BasilTest {
    public class AliveCheckProcessor : MsgProcessor {

        private int _AliveSequenceNumber = 111;

        public AliveCheckProcessor(BasilConnection pConnection) : base(pConnection) {
            // Add processors for message ops
            BasilConnection.Processors processors = new BasilConnection.Processors {
                { (Int32)BasilMessage.BasilMessageOps.AliveCheckReq, this.ProcAliveCheckReq },
                { (Int32)BasilMessage.BasilMessageOps.AliveCheckResp, this.HandleResponse }
            };
            _basilConnection.AddMessageProcessors(processors);
        }

        public IPromise<BasilMessage.BasilMessage> AliveCheck(
                        BasilType.AccessAuthorization pAuth) {
            BasilMessage.BasilMessage req = MakeAliveCheckReq(pAuth);
            return this.SendAndPromiseResponse(req);
        }

        // Send an AliveCheck request without expecting a response
        public void AliveCheckNR(BasilType.AccessAuthorization pAuth) {
            BasilMessage.BasilMessage req = MakeAliveCheckReq(pAuth);
            this.SendMessage(req, null);
        }

        private BasilMessage.BasilMessage MakeAliveCheckReq(
                        BasilType.AccessAuthorization pAuth) {
            BasilMessage.BasilMessage ret = new BasilMessage.BasilMessage() {
                Auth = pAuth
            };
            ret.OpParameters.Add("time", DateTime.UtcNow.ToString());
            ret.OpParameters.Add("sequenceNum", (_AliveSequenceNumber++).ToString());
            return ret;
        }

        private BasilMessage.BasilMessage ProcAliveCheckReq(
                        BasilMessage.BasilMessage pReq) {
            BasilMessage.BasilMessage ret = new BasilMessage.BasilMessage() {
                Op = _basilConnection.BasilMessageOpByName["AliveCheckResp"]
            };
            ret.OpParameters.Add("time", DateTime.UtcNow.ToString());
            ret.OpParameters.Add("sequenceNum", (_AliveSequenceNumber++).ToString());
            ret.OpParameters.Add("timeReceived", pReq.OpParameters["time"]);
            ret.OpParameters.Add("sequenceNumReceived", pReq.OpParameters["sequenceNum"]);
            return ret;
        }
    }
}
