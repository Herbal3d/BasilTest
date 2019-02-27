// Copyright (c) 2019 Robert Adams
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
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
using BasilMessage = org.herbal3d.basil.protocol.Message;

namespace org.herbal3d.BasilTest {
    public class SpaceServer : MsgProcessor {
        private static readonly string _logHeader = "[SpaceServer]";

        public SpaceServer(BasilConnection pConnection) : base(pConnection) {
        }

        public BasilMessage.BasilMessage OpenSession(BasilMessage.BasilMessage pReq) {
            BasilTest.log.DebugFormat("{0} OpenSessionReq", _logHeader);

            // For the moment, just start the testing sequence when session is opened
            BasilTester tester = new BasilTester(_basilConnection);
            Task.Run(() => {
                tester.DoTests(pReq.Properties);
            });

            BasilMessage.BasilMessage respMsg = new BasilMessage.BasilMessage {
                Op = _basilConnection.BasilMessageOpByName["OpenSessionResp"]
            };
            MakeMessageAResponse(ref respMsg, pReq);
            return respMsg;
        }

        public BasilMessage.BasilMessage CloseSession(BasilMessage.BasilMessage pReq) {
            BasilTest.log.DebugFormat("{0} CloseSessionReq", _logHeader);
            BasilMessage.BasilMessage respMsg = new BasilMessage.BasilMessage {
                Op = _basilConnection.BasilMessageOpByName["CloseSessionResp"]
            };
            MakeMessageAResponse(ref respMsg, pReq);
            return respMsg;
        }

        public BasilMessage.BasilMessage CameraView(BasilMessage.BasilMessage pReq) {
            BasilTest.log.DebugFormat("{0} CameraViewReq", _logHeader);
            BasilMessage.BasilMessage respMsg = new BasilMessage.BasilMessage {
                Op = _basilConnection.BasilMessageOpByName["CameraViewResp"]
            };
            MakeMessageAResponse(ref respMsg, pReq);
            return respMsg;
        }
    }
}
