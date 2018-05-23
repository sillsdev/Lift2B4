// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2015, SIL International. All Rights Reserved.
// <copyright from='2015' to='2015' company='SIL International'>
//		Copyright (c) 2015, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: Lift2B4F.cs
// Responsibility: Greg Trihus
// ---------------------------------------------------------------------------------------------
using System;
using System.Windows.Forms;

namespace Lift2B4
{
    public partial class Lift2B4F : Form
    {
        public Lift2B4F()
        {
            InitializeComponent();
        }

        private void browse_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog { DefaultExt = ".lift", Filter = "Lift Export|*.lift" };
            dlg.ShowDialog();
            textBox1.Text = dlg.FileName;
        }

        private void ok_Click(object sender, EventArgs e)
        {
            var liftConvert = new LiftConvert(textBox1.Text);
            var lang1 = liftConvert.Slide1Lang().ToUpper();
            var lang2 = liftConvert.Slide2Lang().ToUpper();
            var dateStamp = DateTime.Now.ToString("s");
            var categories = liftConvert.Categories();
            foreach (string category in categories)
            {
                log.Items.Add(category);
                liftConvert.Convert(category, lang1, lang2, dateStamp);
                liftConvert.CopySchema();
                liftConvert.CopyAudio();
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
