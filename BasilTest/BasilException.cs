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
    public class BasilException : Exception {
        private readonly string _message;
        private readonly Dictionary<string, string> _reasonHints;

        public BasilException(string reason) {
            _message = reason;
            _reasonHints = null;
        }

        public BasilException(string reason, Dictionary<string, string> reasonHints) {
            _message = reason;
            _reasonHints = reasonHints;
        }

        public override string Message => _message;

        public override string ToString() {
            StringBuilder buff = new StringBuilder();
            buff.Append(_message);
            if (_reasonHints != null) {
                foreach (var (k, v) in _reasonHints) {
                    buff.Append(String.Format(" {0}:{1}", k, v));
                }
            }
            return buff.ToString();
        }
    }
}
