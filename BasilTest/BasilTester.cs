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

using RSG;

using BasilType = org.herbal3d.basil.protocol.BasilType;

namespace org.herbal3d.BasilTest {
    public class BasilTester : IDisposable {

        private static readonly string _logHeader = "[BasilTest]";

        private readonly BasilConnection _connection;

        public BasilTester(BasilConnection pConnection) {
            _connection = pConnection;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public BasilClient Client { get; }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BHttpServer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        public void DoTests() {
            BasilType.AccessAuthorization auth = null;
            var anAsset = new BasilType.AssetInformation() {
                DisplayInfo = new BasilType.DisplayableInfo() {
                    DisplayableType = "meshset",
                }
            };
            anAsset.DisplayInfo.Asset.Add("url", "http://files.misterblue.com/BasilTest/convoar/testtest88/unoptimized/testtest88.gltf");
            anAsset.DisplayInfo.Asset.Add("loaderType", "GLTF");
            BasilType.AaBoundingBox aabb = null;
            _connection.Client.IdentifyDisplayableObject(auth, anAsset, aabb)
            .Then(resp => {
                if (resp.Exception != null) {
                }
                BasilTest.log.InfoFormat("{0} created displayable object {1}", _logHeader, resp.ObjectId.Id);
                BasilType.ObjectIdentifier displayableId = resp.ObjectId;
                BasilType.InstancePositionInfo instancePositionInfo = new BasilType.InstancePositionInfo() {
                    Pos = new BasilType.CoordPosition() {
                        Pos = new BasilType.Vector3() {
                            X = 100,
                            Y = 101,
                            Z = 102
                        },
                        PosRef = BasilType.CoordSystem.Wgs86,
                        RotRef = BasilType.RotationSystem.Worldr
                    }
                };
                _connection.Client.CreateObjectInstance(auth, displayableId, instancePositionInfo)
                .Then(resp2 => {
                    BasilTest.log.InfoFormat("{0} created object instance {1}", _logHeader, resp2.InstanceId.Id);
                    BasilType.InstanceIdentifier instanceIdentifier = resp2.InstanceId;
                    _connection.Client.RequestInstanceProperties(auth, instanceIdentifier, "")
                    .Then(resp3 => {
                        foreach (var key in resp3.Properties.Keys) {
                            BasilTest.log.InfoFormat("{0}     {1} = {2}", _logHeader, key, resp3.Properties[key]);
                        }
                    });
                });
            });
        }
    }
}
