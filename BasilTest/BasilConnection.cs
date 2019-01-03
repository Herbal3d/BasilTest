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

using AliveCheck = org.herbal3d.basil.protocol.AliveCheck;
using BasilSpaceStream = org.herbal3d.basil.protocol.BasilSpaceStream;

namespace org.herbal3d.BasilTest {
    public class BasilConnection  {

        private List<MsgProcessor> _MsgProcessors = new List<MsgProcessor>();

        // A socket connection has been made to a Basil Server.
        // Initialize message receivers and senders.
        public BasilConnection(ClientConnection pConnection) {
            _MsgProcessors.Add(new AliveCheckProcessor(this));
            _MsgProcessors.Add(new SpaceServerProcessor(this));
            _MsgProcessors.Add(new BasilClientProcessor(this));
        }

        public void Receive(string pMsg) {
        }

        public void Receive(byte[] pMsg) {
            BasilSpaceStream.SpaceStreamMessage rcvdMsg = BasilSpaceStream.SpaceStreamMessage.Parser.ParseFrom(pMsg);
            foreach (MsgProcessor processor in _MsgProcessors) {
                if (processor.Receive(rcvdMsg, this)) {
                    break;
                }
            }

            switch (rcvdMsg.SpaceMessageCase) {
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.AliveCheckReqMsg:
                    
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.AliveCheckRespMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.CameraViewReqMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.CloseSessionReqMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.CloseSessionRespMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.CreateObjectInstanceRespMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.DeleteObjectInstanceRespMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.ForgetDisplayableObjectRespMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.IdentifyDisplayableObjectRespMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.MakeConnectionRespMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.OpenSessionReqMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.RequestInstancePropertiesRespMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.RequestObjectPropertiesRespMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.UpdateInstancePositionRespMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.UpdateInstancePropertyRespMsg:
                    break;
                case BasilSpaceStream.SpaceStreamMessage.SpaceMessageOneofCase.UpdateObjectPropertyRespMsg:
                    break;
                default:
                    break;
            }
        }

        public void Send(byte[] pMsg) {
        }

        public void AbortConnection() {
        }

        private void ProcAliveCheckReq(AliveCheck.AliveCheckReq pReq) {
        }
    }
}
