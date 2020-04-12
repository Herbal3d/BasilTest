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
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

using org.herbal3d.OSAuth;

using org.herbal3d.cs.CommonEntitiesUtil;

using BM = org.herbal3d.basil.protocol.Message;
using BT = org.herbal3d.basil.protocol.BasilType;
using HT = org.herbal3d.transport;

namespace org.herbal3d.BasilTest {
    public class SpaceServerTester  : HT.SpaceServerBase {
        private static readonly string _logHeader = "[SpaceServerTester]";

        // Creation of an instance for a specific client.
        // Note: this canceller is for the individual session.
        public SpaceServerTester(CancellationTokenSource pCanceller,
                            HT.BasilConnection pBasilConnection) 
                    : base(pCanceller, pBasilConnection) {
        }

        // I don't have anything special do do for Shutdown
        protected override void DoShutdownWork() {
            ClientConnection.Context.Log.DebugFormat("{0} DoShutdownWork: ", _logHeader);
            return;
        }

        /// <summary>
        ///  The client does an OpenSession with 'login' information. Authorize the
        ///  logged in user.
        /// </summary>
        /// <param name="pUserToken">UserAuth token sent from the client making the OpenSession
        ///     which authenticates the access.</param>
        /// <returns>"true" if the user is authorized</returns>
        protected override bool VerifyClientAuthentication(OSAuthToken pUserToken) {
            ClientConnection.Context.Log.DebugFormat("{0} VerifyClientAuthentication: accepting token '{1}'",
                        _logHeader, pUserToken);
            return true;
        }

        protected override void DoOpenSessionWork(HT.BasilConnection pConnection, HT.BasilComm pClient, Dictionary<string,string> pParms) {
            ClientConnection.Context.Log.DebugFormat("{0} DoOpenSessionWork: starting tester", _logHeader);
            BasilTester tester = new BasilTester(Client, ClientConnection);
            Task.Run(async () => {
                await tester.DoTests(pParms);
            });
        }

        // I don't have anything to do for a CloseSession
        protected override void DoCloseSessionWork() {
            ClientConnection.Context.Log.DebugFormat("{0} DoCloseSessionWork: ", _logHeader);
            return;
        }

    }
}
