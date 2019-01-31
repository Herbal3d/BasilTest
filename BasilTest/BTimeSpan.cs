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

namespace org.herbal3d.BasilTest {
    // Very simple class for measuring the time of some operation.
    // Usage:
    //          using (new BTimeSpan( t => { SomeOperation(t); } ) ) {
    //              statementsToMeasure;
    //          }
    // 'SomeOperation' is usually a debug print.
    // The 't' that is passed is a 'TimeSpan' between the start and end of the block.
    public class BTimeSpan : IDisposable {

        // A global flag that turns on and off the time measurements
        public static bool Enable = true;

        private readonly Action<TimeSpan> _DoneOperation;
        private readonly DateTime _startTime;

        public BTimeSpan(Action<TimeSpan> pDoneOp) {
            _DoneOperation = pDoneOp;
            _startTime = DateTime.UtcNow;
        }

        public void Dispose() {
            if (BTimeSpan.Enable) {
                TimeSpan duration = DateTime.UtcNow - _startTime;
                _DoneOperation(duration);
            }
        }
    }
}
