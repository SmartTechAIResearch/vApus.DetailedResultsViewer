/*
 * 2012 Sizing Servers Lab, affiliated with IT bachelor degree NMCT
 * University College of West-Flanders, Department GKG (www.sizingservers.be, www.nmct.be, www.howest.be/en)
 * 
 * Author(s):
 *    Dieter Vandroemme
 */
using System.Windows.Forms;

namespace vApus.DetailedResultsViewer {
    public partial class MySQLServerDialog : Form {
        public bool Connected { get { return savingResultsPanel.Connected; } }
        public string ConnectionString {
            get {
                try {
                    return savingResultsPanel.ConnectionString;
                } catch {
                    //Handled later.
                }
                return null;
            }
        }
        public void GetCurrentConnectionString(out string user, out string host, out int port, out string password) {
            savingResultsPanel.GetCurrentConnectionString(out user, out host, out port, out password);
        }
        public MySQLServerDialog() {
            InitializeComponent();
        }
    }
}
