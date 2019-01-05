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

using BasilServer = org.herbal3d.basil.protocol.BasilServer;
using BasilSpaceStream = org.herbal3d.basil.protocol.BasilSpaceStream;

namespace org.herbal3d.BasilTest {
    public class BasilClientProcessor : MsgProcessor {
        public BasilClientProcessor(BasilConnection pConnection) : base(pConnection) {
        }

        public override bool Receive(BasilSpaceStream.SpaceStreamMessage pMsg, BasilConnection pConnection) {
            bool ret = false;
            if (pMsg.IdentifyDisplayableObjectRespMsg != null) {
                ret = true;
                HandleResponse<BasilServer.IdentifyDisplayableObjectResp>(
                            pMsg.IdentifyDisplayableObjectRespMsg, "IdentifyDisplayableObjectResp", pMsg);
            }
            if (pMsg.ForgetDisplayableObjectRespMsg != null) {
                ret = true;
                HandleResponse<BasilServer.ForgetDisplayableObjectResp>(
                            pMsg.ForgetDisplayableObjectRespMsg, "ForgetDisplayableObjectResp", pMsg);
            }
            if (pMsg.CreateObjectInstanceRespMsg != null) {
                ret = true;
                HandleResponse<BasilServer.CreateObjectInstanceResp>(
                            pMsg.CreateObjectInstanceRespMsg, "CreateObjectInstanceResp", pMsg);
            }
            if (pMsg.DeleteObjectInstanceRespMsg != null) {
                ret = true;
                HandleResponse<BasilServer.DeleteObjectInstanceResp>(
                            pMsg.DeleteObjectInstanceRespMsg, "DeleteObjectInstanceResp", pMsg);
            }
            if (pMsg.UpdateObjectPropertyRespMsg != null) {
                ret = true;
                HandleResponse<BasilServer.UpdateObjectPropertyResp>(
                            pMsg.UpdateObjectPropertyRespMsg, "UpdateObjectPropertyResp", pMsg);
            }
            if (pMsg.UpdateInstancePropertyRespMsg != null) {
                ret = true;
                HandleResponse<BasilServer.UpdateInstancePropertyResp>(
                            pMsg.UpdateInstancePropertyRespMsg, "UpdateInstancePropertyResp", pMsg);
            }
            if (pMsg.RequestObjectPropertiesRespMsg != null) {
                ret = true;
                HandleResponse<BasilServer.RequestObjectPropertiesResp>(
                            pMsg.RequestObjectPropertiesRespMsg, "RequestObjectPropertiesResp", pMsg);
            }
            if (pMsg.RequestInstancePropertiesRespMsg != null) {
                ret = true;
                HandleResponse<BasilServer.RequestInstancePropertiesResp>(
                            pMsg.RequestInstancePropertiesRespMsg, "RequestInstancePropertiesResp", pMsg);
            }
            if (pMsg.CloseSessionRespMsg != null) {
                ret = true;
                HandleResponse<BasilServer.CloseSessionResp>(
                            pMsg.CloseSessionRespMsg, "CloseSessionResp", pMsg);
            }
            if (pMsg.MakeConnectionRespMsg != null) {
                ret = true;
                HandleResponse<BasilServer.MakeConnectionResp>(
                            pMsg.MakeConnectionRespMsg, "MakeConnectionResp", pMsg);
            }
            return ret;
        }
    }
}
