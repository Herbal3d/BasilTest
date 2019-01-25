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
using System.Threading.Tasks;

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

        // Function to do a test and return a success value
        private delegate Task<bool> DoATest();

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

            if (await CreateAndDeleteObjectAsync()) {
                BasilTest.log.InfoFormat("{0} SUCCESS CreateAndDeleteObject", _logHeader);
            }
            else {
                BasilTest.log.InfoFormat("{0} FAILURE CreateAndDeleteObject", _logHeader);
            }
            if (await CreateAndDeleteInstanceAsync()) {
                BasilTest.log.InfoFormat("{0} SUCCESS CreateAndDeleteInstance", _logHeader);
            }
            else {
                BasilTest.log.InfoFormat("{0} FAILURE CreateAndDeleteObject", _logHeader);
            }
            /*
            if ( await CreateTenObjectsDeleteOneAsync()) {
                BasilTest.log.InfoFormat("{0} SUCCESS CreateTenObjectsDeleteOne", _logHeader);
            }
            else {
                BasilTest.log.InfoFormat("{0} FAILURE CreateTenObjectsDeleteOne", _logHeader);
            }
            if ( await Create125InstancesDeleteOneAsync()) {
                BasilTest.log.InfoFormat("{0} SUCCESS Create125InstancesDeleteOne", _logHeader);
            }
            else {
                BasilTest.log.InfoFormat("{0} FAILURE Create125InstancesDeleteOne", _logHeader);
            }
            */
            if ( await CreateObjectsInDifferentFormatsAsync()) {
                BasilTest.log.InfoFormat("{0} SUCCESS CreateObjectsInDifferentFormats", _logHeader);
            }
            else {
                BasilTest.log.InfoFormat("{0} FAILURE CreateObjectsInDifferentFormats", _logHeader);
            }
            /*
            BasilType.AccessAuthorization auth = null;
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
            */
        }

        private async Task<bool> CreateAndDeleteObjectAsync() {
            List<BasilType.ObjectIdentifier> createdObjects = new List<BasilType.ObjectIdentifier>();
            List<BasilType.InstanceIdentifier> createdInstances = new List<BasilType.InstanceIdentifier>();

            // Create an object
            BasilType.AccessAuthorization auth = null;
            BasilType.AaBoundingBox aabb = null;
            var testAsset = BuildAsset(null);
            BasilMessage.BasilMessage resp;
            resp = await _connection.Client.IdentifyDisplayableObjectAsync(auth, testAsset, aabb);
            if (resp.Exception != null) {
                BasilTest.log.ErrorFormat("{0} CreateAndDeleteObject: failure creating Object: {1}",
                            _logHeader, resp.Exception.Reason);
                return false;
            }
            BasilTest.log.DebugFormat("{0} CreateAndDeleteObject: created object {1}",
                            _logHeader, resp.ObjectId.Id);

            // Make sure object is there by fetching its parameters.
            var createdObjectId = resp.ObjectId;
            createdObjects.Add(createdObjectId);
            BasilTest.log.DebugFormat("{0} CreateAndDeleteObject: fetching object properties", _logHeader);
            resp = await _connection.Client.RequestObjectPropertiesAsync(auth, createdObjectId, "");
            if (resp.Exception != null) {
                BasilTest.log.ErrorFormat("{0} CreateAndDeleteObject: failure fetching object properties: {1}",
                            _logHeader, resp.Exception.Reason);
                return false;
            }

            // Forget the object.
            BasilTest.log.DebugFormat("{0} CreateAndDeleteObject: forgetting object", _logHeader);
            resp = await _connection.Client.ForgetDisplayableObjectAsync(auth, createdObjectId);
            if (resp.Exception != null) {
                BasilTest.log.ErrorFormat("{0} CreateAndDeleteObject: failure forgetting object: {1}",
                            _logHeader, resp.Exception.Reason);
                return false;
            }
            BasilTest.log.DebugFormat("{0} CreateAndDeleteObject: deleted object {1}",
                            _logHeader, createdObjectId);
            
            // Make sure we cannot get its parameters any more.
            BasilTest.log.DebugFormat("{0} CreateAndDeleteObject: fetch properties of forgotten object", _logHeader);
            resp = await _connection.Client.RequestObjectPropertiesAsync(auth, createdObjectId, "");
            if (resp.Exception == null) {
                BasilTest.log.ErrorFormat("{0} CreateAndDeleteObject: got parameters for forgotten object: {1}",
                            _logHeader, resp.Exception.Reason);
                return false;
            }

            CleanUpTest(createdObjects, createdInstances);
            return true;
        }

        private async Task<bool> CreateAndDeleteInstanceAsync() {
            List<BasilType.ObjectIdentifier> createdObjects = new List<BasilType.ObjectIdentifier>();
            List<BasilType.InstanceIdentifier> createdInstances = new List<BasilType.InstanceIdentifier>();

            // Create an object.
            BasilType.AccessAuthorization auth = null;
            BasilType.AaBoundingBox aabb = null;
            BasilMessage.BasilMessage resp;
            var testAsset = BuildAsset(null);
            resp = await _connection.Client.IdentifyDisplayableObjectAsync(auth, testAsset, aabb);
            if (resp.Exception != null) {
                BasilTest.log.ErrorFormat("{0} CreateAndDeleteInstance: failure creating Object: {1}",
                                    _logHeader, resp.Exception.Reason);
                return false;
            }
            var createdObjectId = resp.ObjectId;
            createdObjects.Add(createdObjectId);
            BasilTest.log.DebugFormat("{0} CreateAndDeleteInstance: created object {1}",
                                    _logHeader, createdObjectId.Id);

            // Create an instance of that object.
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
            resp = await _connection.Client.CreateObjectInstanceAsync(auth, createdObjectId, instancePositionInfo);
            var createdInstanceId = resp.InstanceId;
            createdInstances.Add(createdInstanceId);
            BasilTest.log.DebugFormat("{0} CreateAndDeleteInstance: created instance {1}",
                                    _logHeader, createdInstanceId.Id);

            // Verify the instance exists by fetching it's parameters.
            resp = await _connection.Client.RequestInstancePropertiesAsync(auth, createdInstanceId, "");
            if (resp.Exception != null) {
                BasilTest.log.ErrorFormat("{0} CreateAndDeleteInstance: failure fetching properties of instance {1}: {2}",
                                    _logHeader, createdInstanceId.Id, resp.Exception.Reason);
                return false;
            }

            // Delete the instance.
            resp = await _connection.Client.DeleteObjectInstanceAsync(auth, createdInstanceId);
            if (resp.Exception != null) {
                BasilTest.log.ErrorFormat("{0} CreateAndDeleteInstance: failure deleting instance {1}: {2}",
                                    _logHeader, createdInstanceId.Id, resp.Exception.Reason);
                return false;
            }

            // Verify the instance is gone by trying to fetch its parameters.
            resp = await _connection.Client.RequestInstancePropertiesAsync(auth, createdInstanceId, "");
            if (resp.Exception == null) {
                BasilTest.log.ErrorFormat("{0} CreateAndDeleteInstance: instance {1} returned parameters after delete",
                                    _logHeader, createdInstanceId.Id);
                return false;
            }

            CleanUpTest(createdObjects, createdInstances);
            return false;
        }

        private async Task<bool> CreateTenObjectsDeleteOneAsync() {
            List<BasilType.ObjectIdentifier> createdObjects = new List<BasilType.ObjectIdentifier>();
            List<BasilType.InstanceIdentifier> createdInstances = new List<BasilType.InstanceIdentifier>();

            int numToCreate = 10;

            // Create 10 objects
            for (int ii = 0; ii < numToCreate; ii++) {
                BasilType.AccessAuthorization auth = null;
                BasilType.AaBoundingBox aabb = null;
                var testAsset = BuildAsset(null);
                var resp = await _connection.Client.IdentifyDisplayableObjectAsync(auth, testAsset, aabb);
                if (resp.Exception != null) {
                    BasilTest.log.ErrorFormat("{0} CreateTenObjectsDeleteOne: failure creating Object: {1}",
                                    _logHeader, resp.Exception.Reason);
                    return false;
                }
                BasilTest.log.DebugFormat("{0} CreateTenObjectsDeleteOne: created object {1}",
                                    _logHeader, resp.ObjectId.Id);
                createdObjects.Add(resp.ObjectId);
            }

            // Verify all ten exist by fetching their parameters.
            foreach (var objId in createdObjects) {
            }

            // Delete one of the objects.

            // Verify the other nine exist by fetching their parameters. The deleted one should fail.

            CleanUpTest(createdObjects, createdInstances);
            return false;
        }

        private async Task<bool> Create125InstancesDeleteOneAsync() {
            List<BasilType.ObjectIdentifier> createdObjects = new List<BasilType.ObjectIdentifier>();
            List<BasilType.InstanceIdentifier> createdInstances = new List<BasilType.InstanceIdentifier>();

            BasilTest.log.DebugFormat("{0} Create125InstancesDeleteOne: creating object", _logHeader);
            BasilType.AccessAuthorization auth = null;
            BasilType.AaBoundingBox aabb = null;
            var testAsset = BuildAsset(null);
            var resp = await _connection.Client.IdentifyDisplayableObjectAsync(auth, testAsset, aabb);
            if (resp.Exception != null) {
                BasilTest.log.ErrorFormat("{0} Create125InstancesDeleteOne: failure creating Object: {1}",
                                _logHeader, resp.Exception.Reason);
                return false;
            }

            var createdObjectId = resp.ObjectId;

            CleanUpTest(createdObjects, createdInstances);
            return true;
        }

        private async Task<bool> UpdateInstancePositionAsync() {
            List<BasilType.ObjectIdentifier> createdObjects = new List<BasilType.ObjectIdentifier>();
            List<BasilType.InstanceIdentifier> createdInstances = new List<BasilType.InstanceIdentifier>();

            BasilTest.log.DebugFormat("{0} UpdateInstancePosition: creating object", _logHeader);
            BasilType.AccessAuthorization auth = null;
            BasilType.AaBoundingBox aabb = null;
            var testAsset = BuildAsset(null);
            var resp = await _connection.Client.IdentifyDisplayableObjectAsync(auth, testAsset, aabb);
            if (resp.Exception != null) {
                BasilTest.log.ErrorFormat("{0} UpdateInstancePosition: failure creating Object: {1}",
                                _logHeader, resp.Exception.Reason);
                return false;
            }

            var createdObjectId = resp.ObjectId;

            CleanUpTest(createdObjects, createdInstances);
            return true;
        }

        private async Task<bool> CreateObjectsInDifferentFormatsAsync() {
            List<BasilType.ObjectIdentifier> createdObjects = new List<BasilType.ObjectIdentifier>();
            List<BasilType.InstanceIdentifier> createdInstances = new List<BasilType.InstanceIdentifier>();

            List<string> urls = new List<string>() {
                "http://files.misterblue.com/BasilTest/gltf/Duck/glTF/Duck.gltf",
                // "http://files.misterblue.com/BasilTest/gltf/Duck/glTF-Binary/Duck.gltf",
                // "http://files.misterblue.com/BasilTest/gltf/Duck/glTF-Draco/Duck.gltf",
                "http://files.misterblue.com/BasilTest/gltf/Duck/glTF-Embedded/Duck.gltf",
                "http://files.misterblue.com/BasilTest/gltf/Duck/glTF-pbrSpecularGlossiness/Duck.gltf"
            };

            BasilType.AccessAuthorization auth = null;
            BasilMessage.BasilMessage resp;
            foreach (var url in urls) {
                BasilTest.log.DebugFormat("{0} CreateObjectsInDifferentFormats: creating object", _logHeader);
                BasilType.AaBoundingBox aabb = null;
                var testAsset = BuildAsset(url);
                resp = await _connection.Client.IdentifyDisplayableObjectAsync(auth, testAsset, aabb);
                if (resp.Exception != null) {
                    BasilTest.log.ErrorFormat("{0} CreateObjectsInDifferentFormats: failure creating Object {1}: {2}",
                                    _logHeader, url, resp.Exception.Reason);
                    return false;
                }
                BasilTest.log.DebugFormat("{0} CreateObjectsInDifferentFormats: created {1} from {2}",
                                _logHeader, resp.ObjectId.Id, url);
                createdObjects.Add(resp.ObjectId);
            }

            foreach (var objId in createdObjects) {
                resp = await _connection.Client.RequestObjectPropertiesAsync(auth, objId, "");
                if (resp.Exception != null) {
                    BasilTest.log.ErrorFormat("{0} CreateObjectsInDifferentFormats: failure fetching properties of {1}",
                                    _logHeader, objId.Id);
                    return false;
                }
            }

            CleanUpTest(createdObjects, createdInstances);
            return true;
        }

        // Build an AssetInformation based around the passed GLTF url.
        // If 'url' is null, use a default, test duck.
        private BasilType.AssetInformation BuildAsset(string url) {
            var testAsset = new BasilType.AssetInformation() {
                DisplayInfo = new BasilType.DisplayableInfo() {
                    DisplayableType = "meshset",
                }
            };
            if (String.IsNullOrEmpty(url)) {
                testAsset.DisplayInfo.Asset.Add("url", "http://files.misterblue.com/BasilTest/gltf/Duck/glTF/Duck.gltf");
            }
            else {
                testAsset.DisplayInfo.Asset.Add("url", url);
            }
            testAsset.DisplayInfo.Asset.Add("loaderType", "GLTF");
            return testAsset;
        }

        // Try to remove the things created by a test.
        // This does not wait for any errors.
        private async void CleanUpTest(List<BasilType.ObjectIdentifier> pObjects, List<BasilType.InstanceIdentifier> pInstances) {
            BasilMessage.BasilMessage resp;
            BasilType.AccessAuthorization auth = null;
            foreach (var objId in pObjects) {
                resp = await _connection.Client.ForgetDisplayableObjectAsync(auth, objId);
            }
            foreach (var instId in pInstances) {
                resp = await _connection.Client.DeleteObjectInstanceAsync(auth, instId);
            }
        }

    }
}
