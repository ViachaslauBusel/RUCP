using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RUCP
{
    internal sealed class TaskPipeline
    {
        private Task _previousTask;
        private volatile int _taskCount = 0;
        private SemaphoreSlim _concurrencySemaphore;
        private Client _client;
        private object _locker = new object();

        public TaskPipeline(SemaphoreSlim concurrencySemaphore, Client client)
        {
            _concurrencySemaphore = concurrencySemaphore;
            _client = client;
        }

        internal void Insert(Task task)
        {
            lock (_locker)
            {
                if (_taskCount++ == 0)
                {
                   // Console.WriteLine("Lock thread");
                    _concurrencySemaphore.Wait();
                }

                Task continueTask = task.ContinueWith(x => Release());

                if (_previousTask != null) { _previousTask.ContinueWith(p => task.Start()); }
                else { task.Start(); }

                _previousTask = continueTask;
            }
        }

        private void Release()
        {
            lock (_locker)
            {
                if(--_taskCount == 0)
                {
                    // Console.WriteLine("Unlock thread");
                    _client.Stream.ForceFlushToSocket();
                    _concurrencySemaphore.Release();
                }
            }
        }
    }


    internal sealed class TaskPool : IDisposable
    {
        private SemaphoreSlim m_concurrencySemaphore;

        public TaskPool(int maxParallelism)
        {
            m_concurrencySemaphore = new SemaphoreSlim(maxParallelism);
        }

        internal TaskPipeline CreatePipeline(Client owner)
        {
            return new TaskPipeline(m_concurrencySemaphore, owner);
        }

        public void Dispose()
        {
            m_concurrencySemaphore.AvailableWaitHandle.WaitOne(5_000);
            m_concurrencySemaphore.Dispose();
        }
    }
}
