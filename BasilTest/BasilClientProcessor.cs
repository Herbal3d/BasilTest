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

using BasilMessage = org.herbal3d.basil.protocol.Message;

namespace org.herbal3d.BasilTest {
    public class BasilClientProcessor : MsgProcessor {
        public BasilClientProcessor(BasilConnection pConnection) : base(pConnection) {
            // Add processors for message ops
            var processors = new BasilConnection.Processors {
                { (Int32)BasilMessage.BasilMessageOps.IdentifyDisplayableObjectResp, this.HandleResponse },
                { (Int32)BasilMessage.BasilMessageOps.ForgetDisplayableObjectResp, this.HandleResponse },
                { (Int32)BasilMessage.BasilMessageOps.CreateObjectInstanceResp, this.HandleResponse },
                { (Int32)BasilMessage.BasilMessageOps.DeleteObjectInstanceResp, this.HandleResponse },
                { (Int32)BasilMessage.BasilMessageOps.UpdateObjectPropertyResp, this.HandleResponse },
                { (Int32)BasilMessage.BasilMessageOps.UpdateInstancePropertyResp, this.HandleResponse },
                { (Int32)BasilMessage.BasilMessageOps.RequestObjectPropertiesResp, this.HandleResponse },
                { (Int32)BasilMessage.BasilMessageOps.RequestInstancePropertiesResp, this.HandleResponse },
                { (Int32)BasilMessage.BasilMessageOps.CloseSessionResp, this.HandleResponse },
                { (Int32)BasilMessage.BasilMessageOps.MakeConnectionResp, this.HandleResponse }
            };
            _basilConnection.AddMessageProcessors(processors);
        }
    }
}
