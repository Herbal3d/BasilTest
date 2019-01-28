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

        private delegate Task DoATest();

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
        public async void DoTests(MapField<string, string> pParams) {
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
                        pParams.ContainsKey("TestLoaderType") ? pParams["TestLoaderType"] : "GLTF");
            }
            else {
                // No parameters passed in so use known values.
                anAsset.DisplayInfo.Asset.Add("url", "http://files.misterblue.com/BasilTest/convoar/testtest88/unoptimized/testtest88.gltf");
                anAsset.DisplayInfo.Asset.Add("loaderType", "GLTF");
            }

            List<DoATest> tests = new List<DoATest> {
                CreateAndDeleteDisplayableAsync,
                CreateAndDeleteInstanceAsync,
                CreateTenDisplayablesAndDeleteOne,
                Create125InstancesDeleteOneAsync,
                CreateObjectsInDifferentFormatsAsync
            };

            foreach (DoATest test in tests) {
                await test();
            }

            // Task.WaitAll(tests.Select(t => { return Task.Run(t); }).ToArray());

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

        private async Task CreateAndDeleteDisplayableAsync() {
            string testName = "CreateAndDeleteDisplayable";
            string testPhase = "unknown";
            List<BasilType.ObjectIdentifier> createdDisplayables = new List<BasilType.ObjectIdentifier>();
            List<BasilType.InstanceIdentifier> createdInstances = new List<BasilType.InstanceIdentifier>();

            try {
                // Create an displayable
                BasilType.AccessAuthorization auth = null;
                BasilType.AaBoundingBox aabb = null;
                var testAsset = BuildAsset(null);
                BasilMessage.BasilMessage resp;
                testPhase = "Creating displayable";
                resp = await _connection.Client.IdentifyDisplayableObjectAsync(auth, testAsset, aabb);
                BasilTest.log.DebugFormat("{0} {1}: created displayable {2}", _logHeader, testName, resp.ObjectId.Id);

                // Make sure displayable is there by fetching its parameters.
                var createdDisplayableId = resp.ObjectId;
                createdDisplayables.Add(createdDisplayableId);
                BasilTest.log.DebugFormat("{0} {1}: fetching displayable properties", _logHeader, testName);
                testPhase = "Fetching displayable parameters to verify displayable's creation";
                resp = await _connection.Client.RequestObjectPropertiesAsync(auth, createdDisplayableId, "");

                // Forget the displayable.
                BasilTest.log.DebugFormat("{0} {1}: forgetting displayable", _logHeader, testName);
                testPhase = "Forgetting created displayable";
                resp = await _connection.Client.ForgetDisplayableObjectAsync(auth, createdDisplayableId);
                BasilTest.log.DebugFormat("{0} {1}: deleted displayable {2}", _logHeader, testName, createdDisplayableId);

                // Make sure we cannot get its parameters any more.
                BasilTest.log.DebugFormat("{0} {1}: fetch properties of forgotten displayable", _logHeader, testName);
                try {
                    testPhase = "Verifying cannot get fetch parameters of forgotton displayable";
                    resp = await _connection.Client.RequestObjectPropertiesAsync(auth, createdDisplayableId, "");
                    // This should have failed at getting the parameters
                }
                catch (BasilException be) {
                    resp = null;
                    var temp = be;
                }
                if (resp != null) {
                    throw new BasilException("Fetched forgotton displayable parameters");
                }
                BasilTest.log.InfoFormat("{0}: {1}: TEST SUCCESS", _logHeader, testName);
            }
            catch (BasilException be) {
                BasilTest.log.InfoFormat("{0}: {1}: TEST FAILED: {2}: {3}", _logHeader, testName, testPhase, be);
            }
            catch (Exception e) {
                BasilTest.log.ErrorFormat("{0}: {1}: TEST EXCEPTION: {2}: {3}", _logHeader, testName, testPhase, e);
            }
            finally {
                CleanUpTest(createdDisplayables, createdInstances);
            }

            return;
        }

        private async Task<bool> CreateAndDeleteInstanceAsync() {
            string testName = "CreateAndDeleteInstance";
            string testPhase = "unknown";
            List<BasilType.ObjectIdentifier> createdDisplayables = new List<BasilType.ObjectIdentifier>();
            List<BasilType.InstanceIdentifier> createdInstances = new List<BasilType.InstanceIdentifier>();

            try {
                // Create a displayable.
                BasilType.AccessAuthorization auth = null;
                BasilType.AaBoundingBox aabb = null;
                BasilMessage.BasilMessage resp;
                var testAsset = BuildAsset(null);
                testPhase = "Creating displayable";
                resp = await _connection.Client.IdentifyDisplayableObjectAsync(auth, testAsset, aabb);
                var createdDisplayableId = resp.ObjectId;
                createdDisplayables.Add(createdDisplayableId);
                BasilTest.log.DebugFormat("{0} {1}: created displayable {2}", _logHeader, testName, createdDisplayableId.Id);

                // Create an instance of that displayable.
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
                testPhase = "Creating instance of displayable";
                resp = await _connection.Client.CreateObjectInstanceAsync(auth, createdDisplayableId, instancePositionInfo);
                var createdInstanceId = resp.InstanceId;
                createdInstances.Add(createdInstanceId);
                BasilTest.log.DebugFormat("{0} {1}: created instance {2}", _logHeader, testName, createdInstanceId.Id);

                // Verify the instance exists by fetching it's parameters.
                testPhase = "Verifiying instance by fetching parameters";
                resp = await _connection.Client.RequestInstancePropertiesAsync(auth, createdInstanceId, "");

                // Delete the instance.
                testPhase = "Deleting instance";
                resp = await _connection.Client.DeleteObjectInstanceAsync(auth, createdInstanceId);

                // Verify the instance is gone by trying to fetch its parameters.
                testPhase = "Verifying cannot get fetch parameters of deleted instance";
                try {
                    resp = await _connection.Client.RequestInstancePropertiesAsync(auth, createdInstanceId, "");
                }
                catch (BasilException be) {
                    resp = null;
                    var temp = be;
                }
                if (resp != null) {
                    throw new BasilException("Fetched forgotton instance parameters");
                }
                BasilTest.log.InfoFormat("{0}: {1}: TEST SUCCESS", _logHeader, testName);
            }
            catch (BasilException be) {
                BasilTest.log.InfoFormat("{0}: {1}: TEST FAILED: {2}: {3}", _logHeader, testName, testPhase, be);
            }
            catch (Exception e) {
                BasilTest.log.ErrorFormat("{0}: {1}: TEST EXCEPTION: {2}: {3}", _logHeader, testName, testPhase, e);
            }
            finally {
                CleanUpTest(createdDisplayables, createdInstances);
            }

            return false;
        }

        private async Task<bool> CreateTenDisplayablesAndDeleteOne() {
            string testName = "CreateTenDisplayablesAndDeleteOne";
            string testPhase = "unknown";
            List<BasilType.ObjectIdentifier> createdDisplayables = new List<BasilType.ObjectIdentifier>();
            List<BasilType.InstanceIdentifier> createdInstances = new List<BasilType.InstanceIdentifier>();

            int numToCreate = 10;

            try {
                BasilType.AccessAuthorization auth = null;
                BasilType.AaBoundingBox aabb = null;
                BasilMessage.BasilMessage resp;
                // Create 10 displayables
                testPhase = "Creating displayables";
                for (int ii = 0; ii < numToCreate; ii++) {
                    var testAsset = BuildAsset(null);
                    resp = await _connection.Client.IdentifyDisplayableObjectAsync(auth, testAsset, aabb);
                    createdDisplayables.Add(resp.ObjectId);
                }

                // Verify all ten exist by fetching their parameters.
                testPhase = "Verifying displayables created";
                foreach (var objId in createdDisplayables) {
                    resp = await _connection.Client.RequestObjectPropertiesAsync(auth, objId, "");
                }

                // Choose one of the displayables to delete
                var rand = new Random();
                var deletedDisplayableId = createdDisplayables[rand.Next(createdInstances.Count)];

                // Delete the one selected instance
                testPhase = "Deleting displayable";
                resp = await _connection.Client.ForgetDisplayableObjectAsync(auth, deletedDisplayableId);

                // Verify all the displayables are still there except for the one deleted one
                testPhase = "Verifying displayables except for deleted displayable";
                foreach (var disp in createdDisplayables) {
                    bool success = true;
                    try {
                        resp = await _connection.Client.RequestObjectPropertiesAsync(auth, disp, "");
                    }
                    catch (BasilException be) {
                        var temp = be;
                        if (disp.Id != deletedDisplayableId.Id) {
                            success = false;
                        }
                    }
                    if (!success) {
                        throw new BasilException("Other displayable missing: " + disp.Id);
                    }
                }
                // Verify the other nine exist by fetching their parameters. The deleted one should fail.
                BasilTest.log.InfoFormat("{0}: {1}: TEST SUCCESS", _logHeader, testName);
            }
            catch (BasilException be) {
                BasilTest.log.InfoFormat("{0}: {1}: TEST FAILED: {2}: {3}", _logHeader, testName, testPhase, be);
            }
            catch (Exception e) {
                BasilTest.log.ErrorFormat("{0}: {1}: TEST EXCEPTION: {2}: {3}", _logHeader, testName, testPhase, e);
            }
            finally {
                CleanUpTest(createdDisplayables, createdInstances);
            }

            return false;
        }

        private async Task<bool> Create125InstancesDeleteOneAsync() {
            string testName = "Create125InstancesDeleteOne";
            string testPhase = "unknown";
            List<BasilType.ObjectIdentifier> createdDisplayables = new List<BasilType.ObjectIdentifier>();
            List<BasilType.InstanceIdentifier> createdInstances = new List<BasilType.InstanceIdentifier>();

            try {
                BasilTest.log.DebugFormat("{0} Create125InstancesDeleteOne: creating object", _logHeader);
                BasilType.AccessAuthorization auth = null;
                BasilType.AaBoundingBox aabb = null;
                var testAsset = BuildAsset(null);
                testPhase = "Creating displayable";
                var resp = await _connection.Client.IdentifyDisplayableObjectAsync(auth, testAsset, aabb);

                var createdDisplayableId = resp.ObjectId;
                createdDisplayables.Add(createdDisplayableId);

                // Create the 125 instances
                List<int> range = new List<int>() { 0, 1, 2, 3, 5 };
                foreach (int xx in range) {
                    foreach (int yy in range) {
                        foreach (int zz in range) {
                            BasilType.InstancePositionInfo instancePositionInfo = new BasilType.InstancePositionInfo() {
                                Pos = new BasilType.CoordPosition() {
                                    Pos = new BasilType.Vector3() {
                                        X = 100 + 10 * xx,
                                        Y = 100 + 10 * yy,
                                        Z = 100 + 10 * zz
                                    },
                                    PosRef = BasilType.CoordSystem.Wgs86,
                                    RotRef = BasilType.RotationSystem.Worldr
                                }
                            };
                            testPhase = "Creating instance of displayable";
                            resp = await _connection.Client.CreateObjectInstanceAsync(auth,
                                                createdDisplayableId, instancePositionInfo);
                            var createdInstanceId = resp.InstanceId;
                            createdInstances.Add(createdInstanceId);
                            // BasilTest.log.DebugFormat("{0} {1}: created instance {2}",
                            //                     _logHeader, testName, createdInstanceId.Id);
                        }
                    }
                }

                // Verify all the instances are there by getting their parameters
                foreach (var inst in createdInstances) {
                    resp = await _connection.Client.RequestInstancePropertiesAsync(auth, inst, "");
                }

                // Choose one of the instances to delete
                var rand = new Random();
                var deletedInstanceId = createdInstances[rand.Next(createdInstances.Count)];

                // Delete the one selected instance
                testPhase = "Deleting instance";
                resp = await _connection.Client.DeleteObjectInstanceAsync(auth, deletedInstanceId);

                // Verify all the instances are still there except for the one deleted one
                testPhase = "Verifying instances except for deleted instance";
                foreach (var inst in createdInstances) {
                    bool success = true;
                    try {
                        resp = await _connection.Client.RequestInstancePropertiesAsync(auth, inst, "");
                    }
                    catch (BasilException be) {
                        var temp = be;
                        if (inst.Id != deletedInstanceId.Id) {
                            success = false;
                        }
                    }
                    if (!success) {
                        throw new BasilException("Other instance missing: " + inst.Id);
                    }
                }

                BasilTest.log.InfoFormat("{0}: {1}: TEST SUCCESS", _logHeader, testName);
            }
            catch (BasilException be) {
                var temp = be;
                BasilTest.log.InfoFormat("{0}: {1}: TEST FAILED: {2}: {3}", _logHeader, testName, testPhase, be);
            }
            catch (Exception e) {
                BasilTest.log.ErrorFormat("{0}: {1}: TEST EXCEPTION: {2}: {3}", _logHeader, testName, testPhase, e);
            }
            finally {
                CleanUpTest(createdDisplayables, createdInstances);
            }

            return true;
        }

        private async Task<bool> UpdateInstancePositionAsync() {
            string testName = "CreateDisplayablesAndDeleteOne";
            string testPhase = "unknown";
            List<BasilType.ObjectIdentifier> createdDisplayables = new List<BasilType.ObjectIdentifier>();
            List<BasilType.InstanceIdentifier> createdInstances = new List<BasilType.InstanceIdentifier>();

            try {

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

                var createdDisplayableId = resp.ObjectId;

                BasilTest.log.InfoFormat("{0}: {1}: TEST SUCCESS", _logHeader, testName);
            }
            catch (BasilException be) {
                var temp = be;
                BasilTest.log.InfoFormat("{0}: {1}: TEST FAILED: {2}: {3}", _logHeader, testName, testPhase, be);
            }
            catch (Exception e) {
                BasilTest.log.ErrorFormat("{0}: {1}: TEST EXCEPTION: {2}: {3}", _logHeader, testName, testPhase, e);
            }
            finally {
                CleanUpTest(createdDisplayables, createdInstances);
            }

            return true;
        }

        private async Task<bool> CreateObjectsInDifferentFormatsAsync() {
            string testName = "CreateObjectsInDifferentFormats";
            string testPhase = "unknown";
            List<BasilType.ObjectIdentifier> createdDisplayables = new List<BasilType.ObjectIdentifier>();
            List<BasilType.InstanceIdentifier> createdInstances = new List<BasilType.InstanceIdentifier>();

            List<string> urls = new List<string>() {
                "http://files.misterblue.com/BasilTest/gltf/Duck/glTF/Duck.gltf",
                // "http://files.misterblue.com/BasilTest/gltf/Duck/glTF-Binary/Duck.gltf",
                // "http://files.misterblue.com/BasilTest/gltf/Duck/glTF-Draco/Duck.gltf",
                "http://files.misterblue.com/BasilTest/gltf/Duck/glTF-Embedded/Duck.gltf",
                "http://files.misterblue.com/BasilTest/gltf/Duck/glTF-pbrSpecularGlossiness/Duck.gltf"
            };

            try {
                BasilType.AccessAuthorization auth = null;
                BasilMessage.BasilMessage resp;
                foreach (var url in urls) {
                    BasilTest.log.DebugFormat("{0} {1}: creating object", _logHeader, testName);
                    BasilType.AaBoundingBox aabb = null;
                    var testAsset = BuildAsset(url);
                    testPhase = "Creating formated displayable " + url;
                    resp = await _connection.Client.IdentifyDisplayableObjectAsync(auth, testAsset, aabb);
                    BasilTest.log.DebugFormat("{0} {1}: created {2} from {3}",
                                    _logHeader, testName, resp.ObjectId.Id, url);
                    createdDisplayables.Add(resp.ObjectId);
                }

                // Verify everything was created by asking for its properties
                foreach (var objId in createdDisplayables) {
                    testPhase = "Verifying existance of " + objId.Id;
                    resp = await _connection.Client.RequestObjectPropertiesAsync(auth, objId, "");
                }
                BasilTest.log.InfoFormat("{0}: {1}: TEST SUCCESS", _logHeader, testName);
            }
            catch (BasilException be) {
                var temp = be;
                BasilTest.log.InfoFormat("{0}: {1}: TEST FAILED: {2}: {3}", _logHeader, testName, testPhase, be);
            }
            catch (Exception e) {
                BasilTest.log.ErrorFormat("{0}: {1}: TEST EXCEPTION: {2}: {3}", _logHeader, testName, testPhase, e);
            }
            finally {
                CleanUpTest(createdDisplayables, createdInstances);
            }

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
        private async void CleanUpTest(List<BasilType.ObjectIdentifier> pDisplayables, List<BasilType.InstanceIdentifier> pInstances) {
            BasilMessage.BasilMessage resp;
            BasilType.AccessAuthorization auth = null;
            foreach (var objId in pDisplayables) {
                // BasilTest.log.DebugFormat("{0}: CleanupTest: Forgetting displayable {1}", _logHeader, objId.Id);
                try {
                    resp = await _connection.Client.ForgetDisplayableObjectAsync(auth, objId);
                }
                catch (BasilException be) {
                    // Forgetting errors are expected
                    var temp = be;
                }
                catch (Exception e) {
                    BasilTest.log.ErrorFormat("{0}: CleanUpTest: exception deleting displayables: {1}", _logHeader, e);
                }
            }
            foreach (var instId in pInstances) {
                // BasilTest.log.DebugFormat("{0}: CleanupTest: Deleting instance {1}", _logHeader, instId.Id);
                try {
                    resp = await _connection.Client.DeleteObjectInstanceAsync(auth, instId);
                }
                catch (BasilException be) {
                    // Forgetting errors are expected
                    var temp = be;
                }
                catch (Exception e) {
                    BasilTest.log.ErrorFormat("{0}: CleanUpTest: exception deleting instances: {1}", _logHeader, e);
                }
            }
        }

    }
}
