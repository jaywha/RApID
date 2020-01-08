using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RApID_Project_WPF.Classes
{
    /// <summary>
    /// Used to buffer calls to a WinForms <see cref="BackgroundWorker"/>
    /// </summary>
    /// <typeparam name="T">Argument type passed to the <see cref="BackgroundWorker"/></typeparam>
    public class WorkBuffer<T> {
        private readonly BackgroundWorker Worker = null;

        public List<T> Jobs { get; } = new List<T>();

        public bool IsEmpty => Jobs.Count == 0;

        public WorkBuffer(BackgroundWorker worker = null) {
            Worker = worker ?? new BackgroundWorker();
        }

        public T this[int index] => Jobs[index];

        public void Add(T job) => Jobs.Add(job);

        public T Next() {
            if (Jobs.Count > 1)
            {
                var job = Jobs[1];
                Jobs.RemoveAt(0);
                return job;
            }
            else
            {
                return default;
            }
        }
    }
}
