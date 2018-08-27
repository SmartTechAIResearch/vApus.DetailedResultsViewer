/*
 * 2013 Sizing Servers Lab, affiliated with IT bachelor degree NMCT
 * University College of West-Flanders, Department GKG (www.sizingservers.be, www.nmct.be, www.howest.be/en)
 * 
 * Author(s):
 *    Dieter Vandroemme
 */
using System;
using vApus.Results;
using WeifenLuo.WinFormsUI.Docking;

namespace vApus.DetailedResultsViewer {
    public partial class ResultsPanel : DockablePanel {

        public event EventHandler ResultsDeleted;
        
        private ResultsHelper _resultsHelper;

        public ResultsHelper ResultsHelper {
            get { return _resultsHelper; }
            set { _resultsHelper = value; }
        }
        /// <summary>
        /// Don't forget to set ResultsHelper.
        /// </summary>
        public ResultsPanel() {
            InitializeComponent();
        }
        public void ClearResults() {
            this.Enabled = false;
            detailedResultsControl.ClearResults();
            this.Enabled = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stressTestId">0 for all</param>
        public void RefreshResults(int stressTestId) {
            this.Enabled = false;
            if (stressTestId == 0) detailedResultsControl.RefreshResults(_resultsHelper); else detailedResultsControl.RefreshResults(_resultsHelper, stressTestId);
            this.Enabled = true;
        }

        private void detailedResultsControl_ResultsDeleted(object sender, System.EventArgs e) {
            if (ResultsDeleted != null) ResultsDeleted(this, null);
        }
    }
}
