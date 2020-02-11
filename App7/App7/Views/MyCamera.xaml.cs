using App7.DependencyServices;
using App7.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App7.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MyCamera : ContentPage
	{
        BackgroundWorker bgMediaFilesFlush = null;

        public MyCamera ()
		{
			InitializeComponent ();
            BindingContext = new MyCameraViewModel(this);
            RunMediaFilesFlushBackgroundWorker();
        }

        private void RunMediaFilesFlushBackgroundWorker()
        {
            bgMediaFilesFlush = new BackgroundWorker();
            bgMediaFilesFlush.WorkerSupportsCancellation = true;
            bgMediaFilesFlush.DoWork += ClearFiles;
            bgMediaFilesFlush.RunWorkerCompleted += MediaFlushTimerComplete;
            bgMediaFilesFlush.RunWorkerAsync();
        }
        private void ClearFiles(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (bgMediaFilesFlush.CancellationPending)
                {
                    e.Cancel = true;
                }
                else
                {
                    DependencyService.Get<ILocalFileProvider>().DeleteMediaFiles();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while deleting files" + ex);

            }
        }
        private void MediaFlushTimerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            HandleTimeCompletion(e);
        }
        private void HandleTimeCompletion(RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled)
            {
                //do nothing
            }

            if (e.Error != null)
            {
                Console.WriteLine(e.Error.InnerException);
            }
        }
    }
}