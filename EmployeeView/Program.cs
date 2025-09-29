using System.Configuration;

namespace EmployeeViewer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string conn = null;
            bool useConfig = string.Equals(
                ConfigurationManager.AppSettings["UseConfigConnectionString"], "true", StringComparison.OrdinalIgnoreCase);
            if (useConfig)
            {
                var cs = ConfigurationManager.ConnectionStrings["Db"];
                conn = cs != null ? cs.ConnectionString : null;
            }

            using (var connForm = new Ui.ConnectionForm(conn))
            {
                if (string.IsNullOrWhiteSpace(conn))
                {
                    var dlg = connForm.ShowDialog();
                    if (dlg != DialogResult.OK) return;
                }
                Application.Run(new Ui.MainForm(connForm.ConnectionString));
            }
        }
    }
}