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
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using BT = org.herbal3d.basil.protocol.BasilType;
using BasilMessage = org.herbal3d.basil.protocol.Message;

using org.herbal3d.cs.CommonEntitiesUtil;

using org.herbal3d.transport;

namespace org.herbal3d.BasilTest {
    public class BasilTester : IDisposable {

        private static readonly string _logHeader = "[BasilTester]";

        private delegate Task DoATest();

        public readonly BasilComm Client;
        public readonly BasilConnection ClientConnection;

        public BasilTester(BasilComm pClient, BasilConnection pConnection) {
            Client = pClient;
            ClientConnection = pConnection;
        }

        #region Dispose functionality
        private bool disposedValue = false; // To detect redundant calls
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
        public async Task DoTests(Dictionary<string, string> pParams) {

            List<DoATest> tests = new List<DoATest> {
                CreateAndDeleteDisplayableAsync,
                CreateAndDeleteInstanceAsync,
                CreateTenDisplayablesAndDeleteOne,
                Create125InstancesDeleteOneAsync,
                UpdateInstancePositionAsync,
                // CreateObjectsInDifferentFormatsAsync
            };

            foreach (DoATest test in tests) {
                await test();
            }
            // await Task.WhenAll(tests.Select(async t => { await t(); }).ToArray());
        }

        // TEST: Create one Displayable, verify created, delete it, verify deleted
        private async Task CreateAndDeleteDisplayableAsync() {
            string testName = "CreateAndDeleteDisplayable";
            string testPhase = "unknown";
            List<BT.ItemId> createdItems = new List<BT.ItemId>();

            try {
                // Create an item that has a displayable
                testPhase = "Creating displayable";
                BT.ItemId createdItemId = await CreateTestDisplayable();
                createdItems.Add(createdItemId);

                // Verify the item has been created with a displayable by asking for it's parameters
                testPhase = "verifying created item has been created";
                BT.Props resp = await Client.RequestPropertiesAsync(createdItemId, null);
                if (!resp.ContainsKey("DisplayableType")) {
                    throw new BasilException("Created item did not have Displayable properties");
                };

                // Delete the item
                testPhase = "Deleting the created item";
                resp = await Client.DeleteItemAsync(createdItemId);

                // Make sure we cannot get its parameters any more.
                bool success = false;
                try {
                    testPhase = "Verifying cannot get fetch parameters of forgotton displayable";
                    resp = await Client.RequestPropertiesAsync(createdItemId, null);
                    // This should throw an exception as the item is not there
                    success = false;
                }
                catch (BasilException) {
                    success = true;
                }
                if (!success) {
                    throw new BasilException("Fetched deleted instance parameters");
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
                CleanUpTest(createdItems);
            }

            return;
        }

        // TEST: Create displayable, verify created, create instance of displayable,
        //     verify created, delete instance, verify deleted
        private async Task<bool> CreateAndDeleteInstanceAsync() {
            string testName = "CreateAndDeleteInstance";
            string testPhase = "unknown";
            List<BT.ItemId> createdItems = new List<BT.ItemId>();

            try {
                // Create an item that has a displayable
                testPhase = "Creating displayable";
                BT.ItemId createdDisplayableId = await CreateTestDisplayable();
                createdItems.Add(createdDisplayableId);

                // Verify the item has been created with a displayable by asking for it's parameters
                testPhase = "verifying created item has been created";
                BT.Props resp = await Client.RequestPropertiesAsync(createdDisplayableId, null);
                if (!resp.ContainsKey("DisplayableType")) {
                    throw new BasilException("Created item did not have Displayable properties");
                };

                // Add AbilityInstance to the item to put it in the world
                testPhase = "Adding AbilityInstance to displayable item";
                resp = await CreateInstanceAt(createdDisplayableId, 10, 11, 12);
                BT.ItemId createdInstanceId = new BT.ItemId(resp["ItemId"]);
                createdItems.Add(createdInstanceId);

                // Verify the instance exists by fetching it's parameters.
                testPhase = "Verifiying instance by fetching parameters";
                resp = await Client.RequestPropertiesAsync(createdInstanceId, null);
                if (!resp.ContainsKey("Pos")) {
                    throw new BasilException("After adding AbilityInstance, property 'Pos' not present");
                }

                // Delete the instance.
                testPhase = "Deleting instance";
                resp = await Client.DeleteItemAsync(createdInstanceId);
                createdItems.Remove(createdInstanceId);

                // Make sure the item is deleted
                testPhase = "Verifying cannot get fetch parameters of deleted instance";
                bool success = false;
                try {
                    resp = await Client.RequestPropertiesAsync(createdInstanceId, null);
                }
                catch (BasilException be) {
                    success = true;
                    var temp = be;  // suppress non-use warning
                }
                if (!success) {
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
                CleanUpTest(createdItems);
            }

            return false;
        }

        // TEST: create 10 displayables, verify they exist, delete one random displayable,
        //    verify all displayables are there except for the one deleted
        private async Task<bool> CreateTenDisplayablesAndDeleteOne() {
            string testName = "CreateTenDisplayablesAndDeleteOne";
            string testPhase = "unknown";
            List<BT.ItemId> createdItems = new List<BT.ItemId>();

            int numToCreate = 10;

            try {
                // Create displayables
                testPhase = "Creating displayables";
                for (int ii = 0; ii < numToCreate; ii++) {
                    BT.ItemId createdItemId = await CreateTestDisplayable();
                    createdItems.Add(createdItemId);
                }

                // Verify all ten exist by fetching their parameters.
                testPhase = "Verifying displayables created";
                foreach (var item in createdItems) {
                    BT.Props resp2 = await Client.RequestPropertiesAsync(item, null);
                }

                // Choose one of the instances to delete
                var rand = new Random();
                BT.ItemId deletedDisplayableId = createdItems[rand.Next(createdItems.Count)];

                // Delete the one selected instance
                testPhase = "Deleting displayable";
                BT.Props resp3 = await Client.DeleteItemAsync(deletedDisplayableId);

                // Verify all the displayables are still there except for the one deleted one
                testPhase = "Verifying displayables except for deleted displayable";
                foreach (var item in createdItems) {
                    bool success = true;
                    try {
                        BT.Props resp4 = await Client.RequestPropertiesAsync(item, null);
                    }
                    catch (BasilException) {
                        if (item.Id != deletedDisplayableId.Id) {
                            success = false;
                        }
                    }
                    if (!success) {
                        throw new BasilException("Other instance missing: " + item.Id);
                    }
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
                CleanUpTest(createdItems);
            }

            return false;
        }

        // TEST: create displayable, verify created, create 125 (5**3) instances
        //    of displayable, verify all created, delete one random instance,
        //    verify all instances still there except for one deleted.
        private async Task<bool> Create125InstancesDeleteOneAsync() {
            string testName = "Create125InstancesDeleteOne";
            string testPhase = "unknown";
            List<BT.ItemId> createdItems = new List<BT.ItemId>();

            // Collect and output timing information
            BTimeSpan.Enable = true;
            // The dimension of the cube
            int rangeMax = BasilTest.parms.P<int>("125BlockSize");  // default is 5

            try {
                // Create the item of a displayable
                testPhase = "Creating displayable";
                BT.ItemId createdDisplayableId = await CreateTestDisplayable();
                createdItems.Add(createdDisplayableId);

                // Start up all the creation of all the instances
                IEnumerable<int> range = Enumerable.Range(0, rangeMax);

                // Create the instances
                testPhase = "Creating instances";
                using (new BTimeSpan(span => {
                    var msPerOp = (float)(span.TotalMilliseconds / createdItems.Count);
                    BasilTest.log.DebugFormat("{0} {1}: {2}s {3}ms/req to create {4} instances",
                                    _logHeader, testName, span.TotalSeconds, msPerOp, rangeMax*rangeMax*rangeMax);
                })) {
                    foreach (int xx in range) {
                        foreach (int yy in range) {
                            foreach (int zz in range) {
                                testPhase = "Creating instance of displayable";
                                BT.Props resp2 = await CreateInstanceAt(createdDisplayableId,
                                                                        100.0 + (10.0 * xx),
                                                                        100.0 + (10.0 * yy),
                                                                        100.0 + (10.0 * zz) );
                                BT.ItemId createdInstanceId = new BT.ItemId(resp2["ItemId"]);
                                createdItems.Add(createdInstanceId);
                            }
                        }
                    }
                }

                // Verify all the instances are there by getting their parameters
                BasilTest.log.DebugFormat("{0} {1}: verifying instances created", _logHeader, testName);
                testPhase = "Verifying all instances were created";
                using (new BTimeSpan(span => {
                    var msPerOp = (float)(span.TotalMilliseconds / createdItems.Count);
                    BasilTest.log.DebugFormat("{0} {1}: {2}s {3}ms/req to verify {4} instances",
                                    _logHeader, testName, span.TotalSeconds, msPerOp, createdItems.Count);
                })) {
                    foreach (var item in createdItems) {
                        BT.Props resp3 = await Client.RequestPropertiesAsync(item, null);
                    }
                }

                // Choose one of the instances to delete
                var rand = new Random();
                var deletedInstanceId = createdItems[rand.Next(createdItems.Count)];

                // Delete the one selected instance
                BasilTest.log.DebugFormat("{0} {1}: deleting one instance", _logHeader, testName);
                testPhase = "Deleting instance";
                BT.Props resp5 = await Client.DeleteItemAsync(deletedInstanceId);

                // Let them be in the world for a second.
                Thread.Sleep(1000);

                // Verify all the instances are still there except for the one deleted one
                BasilTest.log.DebugFormat("{0} {1}: verifying non-deleted instances exist", _logHeader, testName);
                testPhase = "Verifying instances except for deleted instance";
                using (new BTimeSpan(span => {
                    var msPerOp = (float)(span.TotalMilliseconds / createdItems.Count);
                    BasilTest.log.DebugFormat("{0} {1}: {2}s {3}ms/req to verify non-deleted instances",
                                            _logHeader, testName, span.TotalSeconds, msPerOp);
                })) {
                    foreach (var item in createdItems) {
                        bool success = true;
                        try {
                            BT.Props resp6 = await Client.RequestPropertiesAsync(item, null);
                        }
                        catch (BasilException be) {
                            var temp = be;
                            if (item.Id != deletedInstanceId.Id) {
                                success = false;
                            }
                        }
                        if (!success) {
                            throw new BasilException("Other instance missing: " + item.Id);
                        }
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
                CleanUpTest(createdItems);
            }

            return true;
        }

        private async Task<bool> UpdateInstancePositionAsync() {
            string testName = "UpdateInstancePosition";
            string testPhase = "unknown";
            List<BT.ItemId> createdItems = new List<BT.ItemId>();

            double baseX = 10.0;
            double baseY = 10.0;
            double baseZ = 10.0;

            try {
                testPhase = "Creating displayable";
                BT.ItemId displayableId = await CreateTestDisplayable();
                createdItems.Add(displayableId);

                // Place it in the world somewhere
                testPhase = "Placing instance in the world";
                BT.Props resp = await CreateInstanceAt(displayableId, baseX, baseY, baseZ);
                BT.ItemId instanceId = new BT.ItemId(resp["ItemId"]);
                createdItems.Add(instanceId);

                // Move the object slowly so the viewer can see it
                testPhase = "Moving instance in the world";
                for (int ii = 0; ii < 100; ii += 10) {
                    Thread.Sleep(500);
                    BT.Props props = new BT.Props {
                        {
                            "Pos",
                            BT.AbilityBase.VectorToString(new double[] {
                                    baseX + ii, baseY + ii, baseZ + ii }
                    )
                        }
                    };
                    BT.Props resp2 = await Client.UpdatePropertiesAsync(instanceId, props);
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
                CleanUpTest(createdItems);
            }

            return true;
        }

        private async Task<bool> CreateObjectsInDifferentFormatsAsync() {
            string testName = "CreateObjectsInDifferentFormats";
            string testPhase = "unknown";
            List<BT.ItemId> createdItems = new List<BT.ItemId>();
            List<BT.ItemId> createdDisplayables = new List<BT.ItemId>();

            List<string> urls = new List<string>() {
                "https://files.misterblue.com/BasilTest/gltf/Duck/glTF/Duck.gltf",
                // "http://files.misterblue.com/BasilTest/gltf/Duck/glTF-Binary/Duck.gltf",
                // "http://files.misterblue.com/BasilTest/gltf/Duck/glTF-Draco/Duck.gltf",
                "https://files.misterblue.com/BasilTest/gltf/Duck/glTF-Embedded/Duck.gltf",
                "https://files.misterblue.com/BasilTest/gltf/Duck/glTF-pbrSpecularGlossiness/Duck.gltf"
            };

            double baseX = 10.0;
            double baseY = 10.0;
            double baseZ = 10.0;

            try {
                // Create displayables for each of the display types
                foreach (var url in urls) {
                    testPhase = "Creating formated displayable " + url;
                    BT.ItemId createdDisplayable = await CreateTestDisplayable(url);
                    createdItems.Add(createdDisplayable);
                    createdDisplayables.Add(createdDisplayable);
                }

                // Place the items in world for everyone to enjoy
                testPhase = "Placing displayables in world";
                double displace = 0.0;
                foreach (BT.ItemId item in createdDisplayables) {
                    BT.Props resp2 = await CreateInstanceAt(item, baseX + displace, baseY, baseZ);
                    createdItems.Add(new BT.ItemId(resp2["ItemId"]) );
                    displace += 10.0;
                }

                // Verify everything was created by asking for its properties
                foreach (var item in createdItems) {
                    testPhase = "Verifying existance of " + item.Id;
                    BT.Props resp3 = await Client.RequestPropertiesAsync(item, null);
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
                CleanUpTest(createdItems);
            }

            return true;
        }

        // Create an Item that is a displayable of the test subject.
        // If not Url is passed for the displayable, a default test Url is used.
        private async Task<BT.ItemId> CreateTestDisplayable() {
            return await CreateTestDisplayable("https://files.misterblue.com/BasilTest/gltf/Duck/glTF/Duck.gltf");
        }
        private async Task<BT.ItemId> CreateTestDisplayable(string pUrl) {
            BT.Props props = new BT.Props();
            BT.AbilityList abilities = new BT.AbilityList {
                new BT.AbilityDisplayable() {
                    DisplayableUrl = pUrl,
                    DisplayableType = "meshset",
                    LoaderType = "GLTF"
                }
            };
            BT.Props resp = await Client.CreateItemAsync(props, abilities);
            return new BT.ItemId(resp["ItemId"]);
        }

        // Utility routine to place an instance at location in the world.
        // Returns an awaitable thingy that returns the properties from the response.
        private Task<BT.Props> CreateInstanceAt(BT.ItemId dispId, double xx, double yy, double zz) {
            BT.Props props = new BT.Props();
            BT.AbilityList abilities = new BT.AbilityList {
                new BT.AbilityInstance() {
                    DisplayableItemId = dispId,
                    Pos = new double[] { xx, yy, zz }
                }
            };
            return Client.CreateItemAsync(props, abilities);
        }

        // Try to remove the things created by a test.
        // This does not wait for any errors.
        private async void CleanUpTest(List<BT.ItemId> pItems) {
            if (pItems.Count > 0) {
                using (new BTimeSpan(span => {
                    var msPerOp = (float)(span.TotalMilliseconds / pItems.Count);
                    BasilTest.log.DebugFormat("{0} CleanupTest: {1}s {2}ms/req to delete {3} instances",
                                            _logHeader, span.TotalSeconds, msPerOp, pItems.Count);
                })) {
                    foreach (var item in pItems) {
                        try {
                            BT.Props resp = await Client.DeleteItemAsync(item);
                        }
                        catch (BasilException) {
                            // exceptions are expected for non-existant items
                        }
                    }
                }
            }
        }

    }
}
