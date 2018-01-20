namespace System.Diagnostics
{
    internal class Stopwatch
    {
        private bool _Running;
        private DateTime _StartTime;
        private long _ElapsedMilliseconds;

        public long ElapsedMilliseconds
        {
            get
            {
                if (_Running)
                {
                    return _ElapsedMilliseconds + (long)(DateTime.Now - _StartTime).TotalMilliseconds;
                }
                else
                {
                    return _ElapsedMilliseconds;
                }
            }
        }

        public Stopwatch()
        {
            _Running = false;
            _ElapsedMilliseconds = 0;
        }

        public void Reset()
        {
            _Running = false;
            _ElapsedMilliseconds = 0;
        }

        public void Start()
        {
            _StartTime = DateTime.Now;
            _Running = true;
        }

        public void Restart()
        {
            Reset();
            Start();
        }

        public void Stop()
        {
            _ElapsedMilliseconds = _ElapsedMilliseconds + (long)(DateTime.Now - _StartTime).TotalMilliseconds;
            _Running = false;
        }

    }
}
