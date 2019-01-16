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
using System.Threading.Tasks;

using BasilType = org.herbal3d.basil.protocol.BasilType;
using SpaceServer = org.herbal3d.basil.protocol.SpaceServer;
using BasilSpaceStream = org.herbal3d.basil.protocol.BasilSpaceStream;

namespace org.herbal3d.BasilTest {
    public class SpaceServerProcessor : MsgProcessor {
        private static readonly string _logHeader = "[SpaceServerProcessor]";

        public SpaceServerProcessor(BasilConnection pConnection) : base(pConnection) {
        }

        public override bool Receive(BasilSpaceStream.SpaceStreamMessage pMsg, BasilConnection pConnection) {
            bool ret = false;
            if (pMsg.CameraViewReqMsg != null) {
                ret = true;
                SendResponse<SpaceServer.CameraViewResp>(
                    ProcCameraViewReq(pMsg.CameraViewReqMsg), "CameraViewResp", pMsg);
            }
            if (pMsg.OpenSessionReqMsg != null) {
                ret = true;
                SendResponse<SpaceServer.OpenSessionResp>(
                    ProcOpenSessionReq(pMsg.OpenSessionReqMsg), "OpenSessionResp", pMsg);
            }
            if (pMsg.CloseSessionReqMsg != null) {
                ret = true;
                SendResponse<SpaceServer.CloseSessionResp>(
                    ProcCloseSessionReq(pMsg.CloseSessionReqMsg), "CloseSessionResp", pMsg);
            }
            return ret;
        }

        private SpaceServer.CameraViewResp ProcCameraViewReq(
                        SpaceServer.CameraViewReq pReq) {
            BasilTest.log.DebugFormat("{0} CameraViewReq", _logHeader);
            return new SpaceServer.CameraViewResp {
            };

        }
        private SpaceServer.OpenSessionResp ProcOpenSessionReq(
                        SpaceServer.OpenSessionReq pReq) {
            BasilTest.log.DebugFormat("{0} OpenSessionReq", _logHeader);
            var tester = new BasilTester(_basilConnection);
            Task.Run(() => {
                tester.DoTests(pReq.Features);
            });
            return new SpaceServer.OpenSessionResp {
            };
        }
        private SpaceServer.CloseSessionResp ProcCloseSessionReq(
                        SpaceServer.CloseSessionReq pReq) {
            BasilTest.log.DebugFormat("{0} CloseSessionReq", _logHeader);
            return new SpaceServer.CloseSessionResp {
            };
        }


    }
}
