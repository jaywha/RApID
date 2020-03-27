using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RApID_Project_WPF.Classes
{
    public class TaskInfo
    {
        private Predicate<object> Predicate;
        private Func<object> Function;
        private Action Action;
        private bool IsRunning;
        private int Id;

        public TaskInfo(Action action, bool isRunning, int id)
        {
            Action = action;
            IsRunning = isRunning;
            Id = id;
        }

        public TaskInfo(Func<object> function, bool isRunning, int id)
        {
            Function = function;
            IsRunning = isRunning;
            Id = id;
        }

        public TaskInfo(Predicate<object> predicate, bool isRunning, int id)
        {
            Predicate = predicate;
            IsRunning = isRunning;
            Id = id;
        }

        public void InvokeAction() => Action.Invoke();

        public override bool Equals(object obj)
        {
            if (obj is Task task)
            {
                return task.Id == this.Id;
            }
            else if (!(obj is TaskInfo))
            {
                return false;
            }
            else { return false; }
        }

        public override int GetHashCode()
        {
            return new Random(68723541).Next();
        }

        public static bool operator ==(TaskInfo left, TaskInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TaskInfo left, TaskInfo right)
        {
            return !(left == right);
        }
    }
}
