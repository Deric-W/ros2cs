// Copyright 2019-2023 Robotec.ai
// Copyright 2019 Dyno Robotics (by Samuel Lindgren samuel@dynorobotics.se)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using NUnit.Framework;

namespace ROS2.Test
{
    [TestFixture]
    public class ClockTest
    {
        private Clock Clock;

        [SetUp]
        public void SetUp()
        {
            this.Clock = new Clock();
        }

        [TearDown]
        public void TearDown()
        {
            this.Clock.Dispose();
        }

        [Test]
        public void IsDisposed()
        {
            Assert.That(this.Clock.IsDisposed, Is.False);

            this.Clock.Dispose();

            Assert.That(this.Clock.IsDisposed, Is.True);
        }

        [Test]
        public void DoubleDisposal()
        {
            this.Clock.Dispose();
            this.Clock.Dispose();

            Assert.That(this.Clock.IsDisposed, Is.True);
        }

        [Test]
        public void ClockGetNow()
        {
            Assert.That(this.Clock.Now.Seconds, Is.Not.EqualTo(0));

            this.Clock.Dispose();

            Assert.Throws<ObjectDisposedException>(() => { _ = this.Clock.Now; });
        }

        [Test]
        public void RosTimeSeconds()
        {
            RosTime oneSecond = new RosTime(1, 0);
            Assert.That(oneSecond.TotalSeconds, Is.EqualTo(1.0d));

            RosTime twoPointSix = new RosTime(2, 600000000);
            Assert.That(twoPointSix.TotalSeconds, Is.EqualTo(2.6d));
        }
    }
}
