using System;

namespace OctoGames.TestTask.Gameplay.Units
{
    public sealed class RefreshThrottle
    {
        private float _nextAllowedRefreshTime;

        public bool IsDirty { get; private set; }

        public void RequestRefresh(float time, float interval, Action refresh)
        {
            if (time >= _nextAllowedRefreshTime)
            {
                Refresh(time, interval, refresh);
                return;
            }

            IsDirty = true;
        }

        public void TryRefresh(float time, float interval, Action refresh)
        {
            if (!IsDirty || time < _nextAllowedRefreshTime)
            {
                return;
            }

            Refresh(time, interval, refresh);
        }

        public void Reset()
        {
            _nextAllowedRefreshTime = 0f;
            IsDirty = false;
        }

        private void Refresh(float time, float interval, Action refresh)
        {
            refresh?.Invoke();
            _nextAllowedRefreshTime = time + Math.Max(0f, interval);
            IsDirty = false;
        }
    }
}
