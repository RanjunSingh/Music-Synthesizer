using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/**
 * 
 * 
 * Total available points:  100

__X____	55	Tutorial completed
__X___	20	Duration Fix
__X____	25	Attack/Release
__100____	Total (please add the points and include the total here)
 */
namespace Synthie
{
    public partial class MainForm : Form
    {
        SynthieView view = new SynthieView();

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Helper function to handle file output and/or automatic playback when gneration is done
        /// </summary>
        private void OnPostGeneration()
        {
            if (fileOutputItem.Checked)
            {
                if (saveFileDlg.ShowDialog() == DialogResult.OK)
                {
                    view.Save(saveFileDlg.FileName, saveFileDlg.FilterIndex);
                }
                saveFileDlg.Dispose();
            }
            if (audioOutputItem.Checked)
            {
                view.Play();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            view.OnPaint(e.Graphics);
        }
        #region Menu Handlers
        private void audioOutputItem_Click(object sender, EventArgs e)
        {

            audioOutputItem.Checked = !audioOutputItem.Checked;
        }

        private void exitItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void fileOutputItem_Click(object sender, EventArgs e)
        {
            fileOutputItem.Checked = !fileOutputItem.Checked;
        }

        private void hz1000Item_Click(object sender, EventArgs e)
        {
            view.OnGenerate1000hztone();
            OnPostGeneration();

        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            view.Play();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            view.Stop();
        }
        #endregion

        private void synthesizerItem_Click(object sender, EventArgs e)
        {
            view.Generate();
            OnPostGeneration();
        }

        private void openScoreItem_Click(object sender, EventArgs e)
        {
            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                view.OpenScore(openFileDlg.FileName);
            }
        }
    }
}
