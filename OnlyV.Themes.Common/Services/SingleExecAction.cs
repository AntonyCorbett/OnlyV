using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyV.Themes.Common.Services
{
    public class SingleExecAction
    {
        private readonly object _locker = new object();
        private readonly TimeSpan _timeout;

        public SingleExecAction(TimeSpan timeout)
        {
            _timeout = timeout;
        }

        public void Execute(Action action)
        {
            lock (_locker)
            {
                Monitor.PulseAll(_locker);
            }
            
            Task.Run(() =>
            {
                lock (_locker)
                {
                    if (!Monitor.Wait(_locker, _timeout))
                    {
                        action();
                    }
                }
            });
        }
    }
}
