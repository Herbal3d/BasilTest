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

using BasilType = org.herbal3d.basil.protocol.BasilType;
using BasilServer = org.herbal3d.basil.protocol.BasilServer;

using Google.Protobuf.Collections;

namespace org.herbal3d.BasilTest {
    public class BasilClient {

        private struct SentRPC<RESP> {
            public UInt64 session;
            public BasilClient context;
            public UInt64 timeRPCCreated;
            public Action<RESP> resolver;
            public Action<Exception> rejector;
            public string requestName;
        };
        private BTransport xport;
        private Dictionary<UInt64, Object> outstandingRPC = new Dictionary<ulong, Object>();
        private UInt64 rpcSession = 11;

        public BasilClient(BTransport pXport) {
            xport = pXport;
        }

        private IPromise<RESP> SendAndPromiseResponse<REQ,RESP>(REQ req) {
            var thisSession = this.rpcSession++;
            return new Promise<RESP>((resolve, reject) => {
                outstandingRPC.Add(thisSession, new SentRPC<RESP> {
                    session = thisSession,
                    context = this,
                    timeRPCCreated = (ulong)DateTime.UtcNow.ToBinary(),
                    resolver = resolve,
                    rejector = reject
                });
                

            });
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
                                               BasilServer.IdentifyDisplayableObjectResp>(req);
        }

        public IPromise<BasilServer.ForgetDisplayableObjectResp> ForgetDisplayableObject(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.ObjectIdentifier pId) {
            var req = new BasilServer.ForgetDisplayableObjectReq {
                Auth = pAuth,
                Identifier = pId
            };
            return this.SendAndPromiseResponse<BasilServer.ForgetDisplayableObjectReq,
                                               BasilServer.ForgetDisplayableObjectResp>(req);
        }

        public IPromise<BasilServer.CreateObjectInstanceResp> CreateObjectInstance(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.ObjectIdentifier pId,
                        BasilType.InstancePositionInfo pInstancePositionInfo,
                        Dictionary<string,string> pPropertyList) {
            var req = new BasilServer.CreateObjectInstanceReq {
                Auth = pAuth,
                Identifier = pId,
                Pos = pInstancePositionInfo,
            };
            req.PropertiesToSet.Add(pPropertyList);
            return this.SendAndPromiseResponse<BasilServer.CreateObjectInstanceReq,
                                               BasilServer.CreateObjectInstanceResp>(req);
        }

        public IPromise<BasilServer.DeleteObjectInstanceResp> DeleteObjectInstance(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.InstanceIdentifier pId) {
            var req = new BasilServer.DeleteObjectInstanceReq {
                Auth = pAuth,
                InstanceId = pId
            };
            return this.SendAndPromiseResponse<BasilServer.DeleteObjectInstanceReq,
                                               BasilServer.DeleteObjectInstanceResp>(req);
        }

        public IPromise<BasilServer.UpdateObjectPropertyResp> UpdateObjectProperty(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.ObjectIdentifier pId,
                        Dictionary<string,string> pPropertyList) {
            var req = new BasilServer.UpdateObjectPropertyReq {
                Auth = pAuth,
                Identifier = pId
            };
            req.Props.Add(pPropertyList);
            return this.SendAndPromiseResponse<BasilServer.UpdateObjectPropertyReq,
                                               BasilServer.UpdateObjectPropertyResp>(req);
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
                                               BasilServer.UpdateInstancePropertyResp>(req);
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
                                               BasilServer.UpdateInstancePositionResp>(req);
        }
        public IPromise<BasilServer.RequestObjectPropertiesResp> RequestObjectProperties(
                        BasilType.AccessAuthorization pAuth,
                        BasilType.ObjectIdentifier pId,
                        string pPropertyMatch) {
            var req = new BasilServer.RequestObjectPropertiesReq {
                Auth = pAuth,
                Identifier = pId,
                PropertyMatch = pPropertyMatch
            };
            return this.SendAndPromiseResponse<BasilServer.RequestObjectPropertiesReq,
                                               BasilServer.RequestObjectPropertiesResp>(req);
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
                                               BasilServer.RequestInstancePropertiesResp>(req);
        }

        // RESOURCE MANAGEMENT =========================================

        // CONNECTION MANAGEMENT =======================================

        public IPromise<BasilServer.OpenSessionResp> OpenSession(
                        BasilType.AccessAuthorization pAuth,
                        Dictionary<string,string> pFeatures) {
            var req = new BasilServer.OpenSessionReq {
                Auth = pAuth,
            };
            req.Features.Add(pFeatures);
            return this.SendAndPromiseResponse<BasilServer.OpenSessionReq,
                                               BasilServer.OpenSessionResp>(req);
        }

        public IPromise<BasilServer.CloseSessionResp> CloseSession(
                        BasilType.AccessAuthorization pAuth,
                        string pReason) {
            var req = new BasilServer.CloseSessionReq {
                Auth = pAuth,
                Reason = pReason
            };
            return this.SendAndPromiseResponse<BasilServer.CloseSessionReq,
                                               BasilServer.CloseSessionResp>(req);
        }

        public IPromise<BasilServer.MakeConnectionResp> MakeConnection(
                        BasilType.AccessAuthorization pAuth,
                        Dictionary<string,string> pConnectionParams) {
            var req = new BasilServer.MakeConnectionReq {
                Auth = pAuth,
            };
            req.ConnectionParams.Add(pConnectionParams);
            return this.SendAndPromiseResponse<BasilServer.MakeConnectionReq,
                                               BasilServer.MakeConnectionResp>(req);
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
                                               BasilServer.AliveCheckResp>(req);
        }
    }
}
