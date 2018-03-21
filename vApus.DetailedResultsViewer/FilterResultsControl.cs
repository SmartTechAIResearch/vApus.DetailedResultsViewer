/*
 * Copyright 2013 (c) Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 * Author(s):
 *    Dieter Vandroemme
 */
using SizingServers.Util.WinForms;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using vApus.Util;

namespace vApus.DetailedResultsViewer {
    public partial class FilterResultsControl : UserControl {
        public event EventHandler FilterChanged;

        private System.Windows.Forms.Timer _filterChangedDelayedTimer = new System.Windows.Forms.Timer() { Interval = 1000 };

        public string Filter { get { return txtFilter.Text.Trim(); } }

        public FilterResultsControl() {
            InitializeComponent();
            ClearAvailableTags();
            _filterChangedDelayedTimer.Tick += _filterChangedDelayedTimer_Tick;
        }

        /// <summary>
        /// Duplicate tags are ignored.
        /// </summary>
        /// <param name="tags"></param>
        public void SetAvailableTags(DatabaseActions databaseActions, string[] readyDbs) {
            Cursor = Cursors.WaitCursor;
            try {
                var bag = new ConcurrentDictionary<string, string>();

                Parallel.ForEach(readyDbs, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, (db) => {
                    try {
                        using (var dba = new DatabaseActions() { ConnectionString = databaseActions.ConnectionString, CommandTimeout = 600 })
                            foreach (DataRow row in dba.GetDataTable("Select Tag from " + db + ".tags;").Rows) {
                                string tag = (row.ItemArray[0] as string).Trim().ToLowerInvariant();
                                if (tag.Length != 0) bag.TryAdd(tag, "");
                            }
                    }
                    catch {
                        //corrupt db
                    }
                });
                SetAvailableTags(bag.Keys.OrderBy(x => x).ToArray());
            }
            catch {
                //Ignore.
            }
            try { if (!Disposing && !IsDisposed) Cursor = Cursors.Arrow; } catch { }
        }
        private void SetAvailableTags(string[] tags) {
            flpTags.AutoScroll = false;
            flpTags.SuspendLayout();
            ClearAvailableTags();
            foreach (string tag in tags) {
                var kvpTag = new KeyValuePairControl(tag, string.Empty) { BackColor = SystemColors.Control };
                kvpTag.Tooltip = "Click to add this tag to the filter.";
                kvpTag.Cursor = Cursors.Hand;
                kvpTag.MouseDown += kvpTag_MouseDown;
                flpTags.Controls.Add(kvpTag);
            }
            flpTags.ResumeLayout();
            flpTags.AutoScroll = true;
        }
        private void kvpTag_MouseDown(object sender, MouseEventArgs e) {
            var kvpTag = sender as KeyValuePairControl;
            string tag = "\\b" + Regex.Escape(kvpTag.Key) + "\\b";

            if (!Regex.IsMatch(txtFilter.Text, tag, RegexOptions.IgnoreCase)) {
                if (txtFilter.Text.Length != 0) txtFilter.Text += " ";
                txtFilter.Text += kvpTag.Key;
                txtFilter.Focus();
                txtFilter.Select(txtFilter.Text.Length, 0);
            }
        }
        public void ClearAvailableTags() { flpTags.Controls.Clear(); }
        private void txtFilter_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) e.Handled = true; }
        private void txtFilter_KeyPress(object sender, KeyPressEventArgs e) { if (e.KeyChar == '\r') e.Handled = true; }

        private void txtFilter_TextChanged(object sender, EventArgs e) {
            _filterChangedDelayedTimer.Stop();
            _filterChangedDelayedTimer.Start();
        }
        private void _filterChangedDelayedTimer_Tick(object sender, EventArgs e) {
            _filterChangedDelayedTimer.Stop();
            InvokeFilterChanged();
        }
        private void InvokeFilterChanged() {
            if (FilterChanged != null) {
                int caretPosition = txtFilter.SelectionStart;
                FilterChanged(this, null);
                txtFilter.Focus();
                txtFilter.Select(caretPosition, 0);
            }
        }

        private void FilterResults_Resize(object sender, EventArgs e) {
            flpTags.PerformLayout();
        }

        private void flpTags_Scroll(object sender, ScrollEventArgs e) {
            flpTags.PerformLayout();
        }
    }
}
