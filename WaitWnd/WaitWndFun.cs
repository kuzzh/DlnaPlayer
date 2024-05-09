using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace WaitWnd
{
    class ThreadParam
    {
        public Form Parent { get; set; }
        public Action Action { get; set; }

        public ThreadParam(Form parent, Action action)
        {
            Parent = parent;
            Action = action;
        }
    }
    public class WaitWndFun
    {
        WaitForm loadingForm;
        Thread loadthread;
        /// <summary>
        /// 显示等待框
        /// </summary>
        public void Show(Action action)
        {
            LoadingProcessEx(new ThreadParam(null, action));
            //loadthread = new Thread(new ParameterizedThreadStart(LoadingProcessEx));
            //loadthread.Start(new ThreadParam(null, action));
        }
        /// <summary>
        /// 显示等待框
        /// </summary>
        /// <param name="parent">父窗体</param>
        public void Show(Form parent, Action action)
        {
            //loadthread = new Thread(new ParameterizedThreadStart(LoadingProcessEx));
            //loadthread.Start(new ThreadParam(parent, action));

            LoadingProcessEx(new ThreadParam(parent, action));
        }
        private void Close()
        {
            if (loadingForm != null)
            {
                loadingForm.BeginInvoke(new System.Threading.ThreadStart(loadingForm.CloseLoadingForm));
                loadingForm = null;
                loadthread = null;
            }
        }
        private void LoadingProcessEx(object state)
        {
            var threadParam = state as ThreadParam;
            Form Cparent = threadParam.Parent;
            loadingForm = new WaitForm(Cparent);
            loadingForm.Shown += (sender, e) =>
            {
                var task = Task.Factory.StartNew(() => { threadParam.Action(); });
                task.ContinueWith(t => { Close(); });
            };
            loadingForm.ShowDialog(Cparent);
        }
    }
}
