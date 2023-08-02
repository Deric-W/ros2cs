using System;
using System.Linq;
using NUnit.Framework;

namespace ROS2.Test
{
    [TestFixture]
    public class QualityOfServiceProfileTest
    {
        [Test]
        public void Histroy()
        {
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            foreach (HistoryPolicy history in Enum.GetValues<HistoryPolicy>())
            {
                qos.History = history;
                Assert.That(qos.History, Is.EqualTo(history));
            }
        }

        [Test]
        public void Depth()
        {
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            foreach (ulong depth in Enumerable.Range(0, 10).Select(Convert.ToUInt64))
            {
                qos.Depth = depth;
                Assert.That(qos.Depth, Is.EqualTo(depth));
            }
        }

        [Test]
        public void Reliability()
        {
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            foreach (ReliabilityPolicy reliability in Enum.GetValues<ReliabilityPolicy>())
            {
                qos.Reliability = reliability;
                Assert.That(qos.Reliability, Is.EqualTo(reliability));
            }
        }

        [Test]
        public void Durability()
        {
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            foreach (DurabilityPolicy durability in Enum.GetValues<DurabilityPolicy>())
            {
                qos.Durability = durability;
                Assert.That(qos.Durability, Is.EqualTo(durability));
            }
        }

        [Test]
        public void ServiceProfile()
        {
            QualityOfServiceProfile qos = new QualityOfServiceProfile(QosPresetProfile.SERVICES_DEFAULT);
            Assert.That(qos.Reliability, Is.EqualTo(ReliabilityPolicy.QOS_POLICY_RELIABILITY_RELIABLE));
            Assert.That(qos.Durability, Is.EqualTo(DurabilityPolicy.QOS_POLICY_DURABILITY_VOLATILE));
        }
    }
}