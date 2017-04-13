using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RApID_Project_WPF.VMB
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        protected ViewModelBase() { }

        public virtual string DisplayName { get; protected set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if(handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        protected virtual void NotifyPropertyChangedAll(object inObject)
        {
            foreach(PropertyInfo pi in inObject.GetType().GetProperties())
            {
                NotifyPropertyChanged(pi.Name);
            }
        }

        public virtual void Refresh()
        {
            NotifyPropertyChangedAll(this);
        }

        public static PropertyInfo PropertyOf<T>(Expression<Func<T>> expression)
        {
            var memberExpr = expression.Body as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException("Expression \"" + expression + "\" is not a valid member expression.");
            var property = memberExpr.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException("Expression \"" + expression + "\" does not reference a property.");
            return property;
        }

        protected void NotifyPropertyChanged<T>(Expression<Func<T>> expression)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;
            var propertyName = PropertyOf(expression).Name;
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void NotifyPropertyChanged()
        {
            var methodName = new StackFrame(1).GetMethod().Name;
            if (!methodName.StartsWith("set_"))
                throw new Exception("This overload of the NotifyPropertyChanged" +
                          "method must be called from a property setter.");
            NotifyPropertyChanged(methodName.Substring("set_".Length));
        }

        public void Dispose()
        {
            this.OnDispose();
        }

        protected virtual void OnDispose() { }
    }
}
