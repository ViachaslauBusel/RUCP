using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RUCP
{
    internal class TaskPipeline
    {
        private Task m_previousTask;
        private volatile int m_taskCount = 0;
        private SemaphoreSlim m_concurrencySemaphore;
        private object m_locker = new object();

        public TaskPipeline(SemaphoreSlim concurrencySemaphore)
        {
            m_concurrencySemaphore = concurrencySemaphore;
        }

        internal void Insert(Task task)
        {
            lock (m_locker)
            {
                if (m_taskCount++ == 0)
                {
                   // Console.WriteLine("Lock thread");
                    m_concurrencySemaphore.Wait();
                }

                Task continueTask = task.ContinueWith(x => Release());

                if (m_previousTask != null) { m_previousTask.ContinueWith(p => task.Start()); }
                else { task.Start(); }

                m_previousTask = continueTask;
            }
        }

        private void Release()
        {
            lock (m_locker)
            {
                if(--m_taskCount == 0)
                {
                   // Console.WriteLine("Unlock thread");
                    m_concurrencySemaphore.Release();
                }
            }
        }
    }
    internal class TaskPool : IDisposable
    {
        private SemaphoreSlim m_concurrencySemaphore;


        public TaskPool(int maxParallelism)
        {
            m_concurrencySemaphore = new SemaphoreSlim(maxParallelism);
        }

        internal TaskPipeline CreatePipeline()
        {
            return new TaskPipeline(m_concurrencySemaphore);
        }

       

        public void Dispose()
        {

          //  m_concurrencySemaphore.Dispose();
        }
    }
}
