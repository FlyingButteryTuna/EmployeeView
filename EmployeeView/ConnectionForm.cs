using Microsoft.Data.SqlClient;

namespace EmployeeViewer.Ui
{
    public class ConnectionForm : Form
    {
        private TextBox txtConn;
        private Button btnTest;
        private Button btnOk;
        private Button btnCancel;

        public string ConnectionString => txtConn.Text;

        public ConnectionForm(string initial)
        {
            Text = "Подключение к БД";
            Width = 700; Height = 200;
            StartPosition = FormStartPosition.CenterScreen;

            txtConn = new TextBox { Left = 10, Top = 10, Width = 660, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            if (!string.IsNullOrWhiteSpace(initial)) txtConn.Text = initial;

            btnTest = new Button { Left = 10, Top = 50, Width = 120, Text = "Проверить" };
            btnTest.Click += (s, e) =>
            {
                try
                {
                    using (var conn = new SqlConnection(txtConn.Text))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = "SELECT 1";
                        var ok = Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                        MessageBox.Show(ok ? "Успешно!" : "Ошибка проверки");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            };

            btnOk = new Button { Left = 550, Top = 100, Width = 120, Text = "OK", Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtConn.Text))
                {
                    MessageBox.Show("Строка подключения не может быть пустой.");
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            };

            btnCancel = new Button { Left = 420, Top = 100, Width = 120, Text = "Отмена", Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(txtConn);
            Controls.Add(btnTest);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }
    }
}