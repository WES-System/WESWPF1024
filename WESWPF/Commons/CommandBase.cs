using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WES.Commons
{
    public class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// 判断是否执行的逻辑
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            //return true;
            return DoCanExecute?.Invoke(parameter) == true;
        }

        /// <summary>
        /// 执行命令逻辑
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            DoExcute?.Invoke(parameter);
        }

        public Action<object> DoExcute { get; set; }

        public Func<object, bool> DoCanExecute { get; set; }
    }
}
