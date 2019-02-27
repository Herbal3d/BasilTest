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
using System.Threading.Tasks;

using BasilType = org.herbal3d.basil.protocol.BasilType;
using BasilMessage = org.herbal3d.basil.protocol.Message;

namespace org.herbal3d.BasilTest {
    // Message Basil might send to us as a SpaceServer.
    public class SpaceServerProcessor : MsgProcessor {
        // private static readonly string _logHeader = "[SpaceServerProcessor]";
        public readonly SpaceServer Server;

        public SpaceServerProcessor(BasilConnection pConnection) : base(pConnection) {
            // Add processors for message ops
            Server = new SpaceServer(pConnection);
            BasilConnection.Processors processors = new BasilConnection.Processors {
                { (Int32)BasilMessage.BasilMessageOps.OpenSessionReq, Server.OpenSession },
                { (Int32)BasilMessage.BasilMessageOps.CloseSessionReq, Server.CloseSession },
                { (Int32)BasilMessage.BasilMessageOps.CameraViewReq, Server.CameraView }
            };
            _basilConnection.AddMessageProcessors(processors);
        }
    }
}
