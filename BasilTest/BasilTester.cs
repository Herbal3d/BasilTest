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

using Google.Protobuf.Collections;

using BasilType = org.herbal3d.basil.protocol.BasilType;
using BasilMessage = org.herbal3d.basil.protocol.Message;

namespace org.herbal3d.BasilTest {
    public class BasilTester : IDisposable {

        private static readonly string _logHeader = "[BasilTester]";

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

        // Do the tests.
        // Parameters are passed from the 'properties' of the OpenSession request.
        //    These parameters can specify the tests to do and parameters for same.
        public async void DoTests(MapField<string,string> pParams) {
            BasilType.AccessAuthorization auth = null;
            var anAsset = new BasilType.AssetInformation() {
                DisplayInfo = new BasilType.DisplayableInfo() {
                    DisplayableType = "meshset",
                }
            };
            // Check for passed parameters specifying a test session and parameters for same
            if (pParams.ContainsKey("TestConnection")
                        && Boolean.Parse(pParams["TestConnection"])
                        && pParams.ContainsKey("TestURL")) {
                anAsset.DisplayInfo.Asset.Add("url", pParams["TestURL"]);
                anAsset.DisplayInfo.Asset.Add("loaderType", 
                        pParams.ContainsKey("TestLoaderType") ? pParams["TestLoaderType"] : "GLTF" );
            }
            else {
                // No parameters passed in so use known values.
                anAsset.DisplayInfo.Asset.Add("url", "http://files.misterblue.com/BasilTest/convoar/testtest88/unoptimized/testtest88.gltf");
                anAsset.DisplayInfo.Asset.Add("loaderType", "GLTF");
            }

            try {
                // Create an Object using the asset information.
                BasilType.AaBoundingBox aabb = null;
                BasilMessage.BasilMessage resp = await _connection.Client.IdentifyDisplayableObjectAsync(auth, anAsset, aabb);
                if (resp.Exception != null) {
                }
                BasilTest.log.InfoFormat("{0} created displayable object {1}", _logHeader, resp.ObjectId.Id);

                // Create an Instance of the Object in the viewer
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
                BasilMessage.BasilMessage resp2 = await _connection.Client.CreateObjectInstanceAsync(auth, displayableId, instancePositionInfo);
                BasilTest.log.InfoFormat("{0} created object instance {1}", _logHeader, resp2.InstanceId.Id);

                // Ask the instance for all its properties and print them out
                BasilType.InstanceIdentifier instanceIdentifier = resp2.InstanceId;
                BasilMessage.BasilMessage resp3 = await _connection.Client.RequestInstancePropertiesAsync(auth, instanceIdentifier, "");
                foreach (var key in resp3.Properties.Keys) {
                    BasilTest.log.InfoFormat("{0}     {1} = {2}", _logHeader, key, resp3.Properties[key]);
                }
            }
            catch (Exception e) {
                BasilTest.log.DebugFormat("{0} DoTests: exception: {1}", _logHeader, e);
            }
        }
    }
}
