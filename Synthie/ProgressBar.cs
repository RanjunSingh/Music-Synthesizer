using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Synthie
{
    public partial class ProgressBar : Form
    {
        private int progress = 0;
        public ProgressBar()
        {
            InitializeComponent();
        }

        //starts up the progress bar
        public void Runworker()
        {
            backgroundWorker1.RunWorkerAsync();
            Show();
        }

        //Allow external locations to set the progress to a specified percent
        public void UpdateProgress(double percent)
        {
            progress = (int)(percent * 100);
            ProgressChanged(this, new ProgressChangedEventArgs(progress, null));
        }

        //update the prograss bar
        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Change the value of the ProgressBar to the BackgroundWorker progress.
            progressBar1.Value = e.ProgressPercentage;
            // Set the text.
            Text = "Progress " + e.ProgressPercentage.ToString() + "%";
        }


        //when done, close the window
        private void WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }
    }
}
