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
using System.IO;

using RSG;

using BasilType = org.herbal3d.basil.protocol.BasilType;
using BasilServer = org.herbal3d.basil.protocol.BasilServer;

namespace org.herbal3d.BasilTest {
    public class BasilClient : MsgProcessor {

        public BasilClient(BasilConnection pBasilConnection) : base(pBasilConnection) {
        }

        public IPromise<BasilServer.IdentifyDisplayableObjectResp> IdentifyDisplayableObject(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.AssetInformation pAsset,
                        BasilType.AaBoundingBox pAabb) {
            var req = new BasilServer.IdentifyDisplayableObjectReq {
                Auth = pAuth,
                AssetInfo = pAsset,
                Aabb = pAabb
            };
            return this.SendAndPromiseResponse<BasilServer.IdentifyDisplayableObjectReq,
                                               BasilServer.IdentifyDisplayableObjectResp>(req,
                                               "IdentifyDisplayableObjectReq");
        }

        public IPromise<BasilServer.ForgetDisplayableObjectResp> ForgetDisplayableObject(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.ObjectIdentifier pId) {
            var req = new BasilServer.ForgetDisplayableObjectReq {
                Auth = pAuth,
                ObjectId = pId
            };
            return this.SendAndPromiseResponse<BasilServer.ForgetDisplayableObjectReq,
                                               BasilServer.ForgetDisplayableObjectResp>(req,
                                               "ForgetDisplayableObjectReq");
        }

        public IPromise<BasilServer.CreateObjectInstanceResp> CreateObjectInstance(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.ObjectIdentifier pId,
                        BasilType.InstancePositionInfo pInstancePositionInfo) {
            Dictionary<string, string> propertyList = null;
            return CreateObjectInstance(pAuth, pId, pInstancePositionInfo, propertyList);
        }

        public IPromise<BasilServer.CreateObjectInstanceResp> CreateObjectInstance(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.ObjectIdentifier pId,
                        BasilType.InstancePositionInfo pInstancePositionInfo,
                        Dictionary<string,string> pPropertyList) {
            var req = new BasilServer.CreateObjectInstanceReq {
                Auth = pAuth,
                ObjectId = pId,
                Pos = pInstancePositionInfo,
            };
            req.PropertiesToSet.Add(pPropertyList);
            return this.SendAndPromiseResponse<BasilServer.CreateObjectInstanceReq,
                                               BasilServer.CreateObjectInstanceResp>(req,
                                               "CreateObjectInstanceReq");
        }

        public IPromise<BasilServer.DeleteObjectInstanceResp> DeleteObjectInstance(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.InstanceIdentifier pId) {
            var req = new BasilServer.DeleteObjectInstanceReq {
                Auth = pAuth,
                InstanceId = pId
            };
            return this.SendAndPromiseResponse<BasilServer.DeleteObjectInstanceReq,
                                               BasilServer.DeleteObjectInstanceResp>(req,
                                               "DeleteObjectInstanceReq");
        }

        public IPromise<BasilServer.UpdateObjectPropertyResp> UpdateObjectProperty(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.ObjectIdentifier pId,
                        Dictionary<string,string> pPropertyList) {
            var req = new BasilServer.UpdateObjectPropertyReq {
                Auth = pAuth,
                ObjectId = pId
            };
            req.Props.Add(pPropertyList);
            return this.SendAndPromiseResponse<BasilServer.UpdateObjectPropertyReq,
                                               BasilServer.UpdateObjectPropertyResp>(req,
                                               "UpdateObjectPropertyReq");
        }

        public IPromise<BasilServer.UpdateInstancePropertyResp> UpdateInstanceProperty(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.InstanceIdentifier pId,
                        Dictionary<string,string> pPropertyList) {
            var req = new BasilServer.UpdateInstancePropertyReq {
                Auth = pAuth,
                InstanceId = pId
            };
            req.Props.Add(pPropertyList);
            return this.SendAndPromiseResponse<BasilServer.UpdateInstancePropertyReq,
                                               BasilServer.UpdateInstancePropertyResp>(req,
                                               "UpdateInstancePropertyReq");
        }
        public IPromise<BasilServer.UpdateInstancePositionResp> UpdateInstancePosition(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.InstanceIdentifier pId,
                        BasilType.InstancePositionInfo pInstancePositionInfo) {
            var req = new BasilServer.UpdateInstancePositionReq {
                Auth = pAuth,
                InstanceId = pId,
                Pos = pInstancePositionInfo
            };
            return this.SendAndPromiseResponse<BasilServer.UpdateInstancePositionReq,
                                               BasilServer.UpdateInstancePositionResp>(req,
                                               "UpdateInstancePositionReq");
        }
        public IPromise<BasilServer.RequestObjectPropertiesResp> RequestObjectProperties(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.ObjectIdentifier pId,
                        string pPropertyMatch) {
            var req = new BasilServer.RequestObjectPropertiesReq {
                Auth = pAuth,
                ObjectId = pId,
                PropertyMatch = pPropertyMatch
            };
            return this.SendAndPromiseResponse<BasilServer.RequestObjectPropertiesReq,
                                               BasilServer.RequestObjectPropertiesResp>(req,
                                               "RequestObjectPropertiesReq");
        }

        public IPromise<BasilServer.RequestInstancePropertiesResp> RequestInstanceProperties(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.InstanceIdentifier pId,
                        string pPropertyMatch) {
            var req = new BasilServer.RequestInstancePropertiesReq {
                Auth = pAuth,
                InstanceId = pId,
                PropertyMatch = pPropertyMatch
            };
            return this.SendAndPromiseResponse<BasilServer.RequestInstancePropertiesReq,
                                               BasilServer.RequestInstancePropertiesResp>(req,
                                               "RequestInstancePropertiesReq");
        }

        // RESOURCE MANAGEMENT =========================================

        // CONNECTION MANAGEMENT =======================================

        public IPromise<BasilServer.CloseSessionResp> CloseSession(
                        BasilType.AccessAuthorization pAuth,
                        string pReason) {
            var req = new BasilServer.CloseSessionReq {
                Auth = pAuth,
                Reason = pReason
            };
            return this.SendAndPromiseResponse<BasilServer.CloseSessionReq,
                                               BasilServer.CloseSessionResp>(req,
                                               "CloseSessionReq");
        }

        public IPromise<BasilServer.MakeConnectionResp> MakeConnection(
                        BasilType.AccessAuthorization pAuth,
                        Dictionary<string,string> pConnectionParams) {
            var req = new BasilServer.MakeConnectionReq {
                Auth = pAuth,
            };
            req.ConnectionParams.Add(pConnectionParams);
            return this.SendAndPromiseResponse<BasilServer.MakeConnectionReq,
                                               BasilServer.MakeConnectionResp>(req,
                                               "MakeConnectionReq");
        }

        public IPromise<BasilServer.AliveCheckResp> AliveCheck(
                        BasilType.AccessAuthorization pAuth,
                        UInt64 pTime,
                        Int32 pSequenceNum) {
            var req = new BasilServer.AliveCheckReq {
                Auth = pAuth,
                Time = pTime,
                SequenceNum = pSequenceNum
            };
            return this.SendAndPromiseResponse<BasilServer.AliveCheckReq,
                                               BasilServer.AliveCheckResp>(req,
                                               "AliveCheckReq");
        }
    }
}
