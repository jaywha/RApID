using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RApID_Project_WPF.Classes
{
    public class ManagedThread : Task
    {
        private TaskInfo Info;
        private static int _nextId = 0;
        private int NextId
        {
            get => ++_nextId;
        }

        public ManagedThread(Action action, string Name="Default_Thread") 
            : base(action) 
            => Info = new TaskInfo(action, false, NextId)
            {};

        public void Run()
        {
            try
            {
                Info.InvokeAction();
            } catch(Exception e)
            {
                Console.WriteLine("Issue occured on " + Info.ToString());
            }
        }
    }
}
