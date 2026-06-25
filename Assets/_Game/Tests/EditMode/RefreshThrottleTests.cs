using NUnit.Framework;
using OctoGames.TestTask.Gameplay.Units;

namespace OctoGames.TestTask.Tests
{
    public sealed class RefreshThrottleTests
    {
        [Test]
        public void BurstRequests_AreCoalescedUntilIntervalPasses()
        {
            RefreshThrottle throttle = new RefreshThrottle();
            int refreshCount = 0;

            throttle.RequestRefresh(0f, 0.25f, () => refreshCount++);
            for (int i = 0; i < 20; i++)
            {
                throttle.RequestRefresh(0.1f, 0.25f, () => refreshCount++);
            }

            throttle.TryRefresh(0.2f, 0.25f, () => refreshCount++);
            Assert.AreEqual(1, refreshCount);
            Assert.IsTrue(throttle.IsDirty);

            throttle.TryRefresh(0.25f, 0.25f, () => refreshCount++);
            Assert.AreEqual(2, refreshCount);
            Assert.IsFalse(throttle.IsDirty);
        }
    }
}
