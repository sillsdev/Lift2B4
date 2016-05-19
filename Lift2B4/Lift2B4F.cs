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
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        private void convert_Click(object sender, EventArgs e)
        {
            _deleteFolder = !ModifierKeys.HasFlag(Keys.Shift);
            var liftConvert = new LiftConvert(textBox1.Text);
            var dateStamp = DateTime.Now.ToString("o");
            var langFolder = liftConvert.LangFolder();
            if (Directory.Exists(langFolder))
            {
                const bool recursive = true;
                Directory.Delete(langFolder, recursive);
            }
            var units = liftConvert.Units();
            foreach (string unit in units)
            {
                liftConvert.SetUnit(unit);
                OutputCategories(liftConvert.Lessons(unit), liftConvert, langFolder, dateStamp);
            }
            //liftConvert.SetUnit("Other");
            //OutputCategories(liftConvert.Categories(null), liftConvert, langFolder, dateStamp);
            close.Focus();
        }

        private bool _deleteFolder = true;
        private void OutputCategories(IList<string> lessons, LiftConvert liftConvert, string langFolder,
            string dateStamp)
        {
            if (lessons == null) throw new ArgumentNullException("lessons");
            foreach (string lesson in lessons)
            {
                log.Items.Add(lesson);
                log.SelectedIndex = log.Items.Count - 1;
                log.Refresh();
                liftConvert.Convert(lesson, langFolder, dateStamp);
                //liftConvert.CopySchema();
                liftConvert.CopyAudio();
                liftConvert.Package(_deleteFolder);
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void close_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
