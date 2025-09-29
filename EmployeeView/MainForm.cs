using EmployeeViewer.Data;
using EmployeeViewer.Models;
using System.ComponentModel;
namespace EmployeeViewer.Ui
{
    public class MainForm : Form
    {
        private readonly string _connStr;
        private Db _db;

        private ComboBox cbStatus, cbDep, cbPost;
        private TextBox tbLastName;
        private Button btnSearch, btnReset, btnStats;
        private DataGridView grid;
        private BindingList<PersonRow> data;
        private Label legend;

        private ComboBox cbStatStatus;
        private DateTimePicker dpFrom, dpTo;
        private RadioButton rbHired, rbFired;
        private Button btnBuild;
        private DataGridView gridStats;

        private BindingSource gridSource;

        private CancellationTokenSource _listCts;
        private CancellationTokenSource _statsCts;

        private string currentSortColumn = "last_name";
        private string currentSortDir = "ASC";

        public MainForm(string connectionString)
        {
            _connStr = connectionString;
            Text = "Просмотр сотрудников и статистика";
            Width = 1100; Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            var tabs = new TabControl { Dock = DockStyle.Fill };
            var tabList = new TabPage("Сотрудники");
            var tabStats = new TabPage("Статистика");
            tabs.TabPages.Add(tabList);
            tabs.TabPages.Add(tabStats);
            Controls.Add(tabs);

            // Employees tab
            cbStatus = new ComboBox { Left = 10, Top = 10, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cbDep = new ComboBox { Left = 220, Top = 10, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cbPost = new ComboBox { Left = 430, Top = 10, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            tbLastName = new TextBox { Left = 640, Top = 10, Width = 200, PlaceholderText = "Фильтр по фамилии" };
            btnSearch = new Button { Left = 850, Top = 8, Width = 100, Text = "Найти" };
            btnReset = new Button { Left = 960, Top = 8, Width = 100, Text = "Сброс" };

            legend = new Label { Left = 10, Top = 40, Width = 600, Text = "Легенда: ФИО у уволенных выделено цветом.", ForeColor = Color.DarkRed };

            grid = new DataGridView
            {
                Left = 10,
                Top = 70,
                Width = 1050,
                Height = 550,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ScrollBars = ScrollBars.Vertical,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "FioShort", 
                HeaderText = "ФИО", 
                Name = "col_fio", 
                SortMode = DataGridViewColumnSortMode.Programmatic, 
                Width = 220 
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "StatusName", 
                HeaderText = "Статус", 
                Name = "col_status",
                SortMode = DataGridViewColumnSortMode.Programmatic, 
                Width = 150 
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "DepName", 
                HeaderText = "Отдел", 
                Name = "col_dep", 
                SortMode = DataGridViewColumnSortMode.Programmatic, Width = 200 
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "PostName", 
                HeaderText = "Должность", 
                Name = "col_post", 
                SortMode = DataGridViewColumnSortMode.Programmatic, 
                Width = 200 
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "DateEmploy", 
                HeaderText = "Принят",
                Name = "col_date_employ", 
                SortMode = DataGridViewColumnSortMode.Programmatic, 
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" } 
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DateUneploy",
                HeaderText = "Уволен",
                Name = "col_date_uneploy",
                SortMode = DataGridViewColumnSortMode.Programmatic,
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" }
            });

            grid.CellFormatting += Grid_CellFormatting;
            grid.ColumnHeaderMouseClick += Grid_ColumnHeaderMouseClick;

            gridSource = new BindingSource();
            grid.DataSource = gridSource;

            grid.Height = tabList.ClientSize.Height - grid.Top - 10;
            grid.Width = tabList.ClientSize.Width - 20;

            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null,
                grid,
                new object[] { true }
            );

            btnSearch.Click += BtnSearch_Click;
            btnReset.Click += BtnReset_Click;

            tabList.Controls.Add(cbStatus);
            tabList.Controls.Add(cbDep);
            tabList.Controls.Add(cbPost);
            tabList.Controls.Add(tbLastName);
            tabList.Controls.Add(btnSearch);
            tabList.Controls.Add(btnReset);
            tabList.Controls.Add(legend);
            tabList.Controls.Add(grid);

            // Stats tab
            cbStatStatus = new ComboBox { Left = 10, Top = 10, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            dpFrom = new DateTimePicker { Left = 220, Top = 10, Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) };
            dpTo = new DateTimePicker { Left = 380, Top = 10, Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            
            rbHired = new RadioButton { Left = 540, Top = 12, Text = "Приняты", Checked = true };
            rbFired = new RadioButton { Left = 650, Top = 12, Text = "Уволенные" };
            
            btnBuild = new Button { Left = 760, Top = 8, Width = 120, Text = "Построить" };
            btnBuild.Click  += BtnBuild_Click;

            gridStats = new DataGridView 
            { 
                Left = 10, 
                Top = 50, 
                Width = 1050, 
                Height = 570, 
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom, 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                AutoGenerateColumns = false 
            };
            gridStats.Columns.Add(new DataGridViewTextBoxColumn 
            {
                DataPropertyName = "Day", 
                HeaderText = "Дата", 
                Width = 160, 
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" } 
            });
            gridStats.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "Count", 
                HeaderText = "Количество", 
                Width = 160 
            });

            gridStats.Height = tabList.ClientSize.Height - grid.Top - 10;
            gridStats.Width = tabList.ClientSize.Width - 20;

            tabStats.Controls.Add(cbStatStatus);
            tabStats.Controls.Add(dpFrom);
            tabStats.Controls.Add(dpTo);
            tabStats.Controls.Add(rbHired);
            tabStats.Controls.Add(rbFired);
            tabStats.Controls.Add(btnBuild);
            tabStats.Controls.Add(gridStats);

            Load += MainForm_LoadAsync;
            FormClosed += (s, e) => { _listCts?.Cancel(); _statsCts?.Cancel(); };
        }

        private async void MainForm_LoadAsync(object sender, EventArgs e)
        {
            try
            {
                _db = new Db(_connStr);

                ToggleTopInputs(false);
                var statusesTask = _db.GetStatusesAsync();
                var depsTask = _db.GetDepartmentsAsync();
                var postsTask = _db.GetPostsAsync();

                var statuses = await statusesTask;
                var deps = await depsTask;
                var posts = await postsTask;

                statuses.Insert(0, new LookupItem { Id = -1, Name = "Все статусы" });
                deps.Insert(0, new LookupItem { Id = -1, Name = "Все отделы" });
                posts.Insert(0, new LookupItem { Id = -1, Name = "Все должности" });

                cbStatus.DataSource = statuses; cbStatus.DisplayMember = "Name"; cbStatus.ValueMember = "Id";
                cbDep.DataSource = deps; cbDep.DisplayMember = "Name"; cbDep.ValueMember = "Id";
                cbPost.DataSource = posts; cbPost.DisplayMember = "Name"; cbPost.ValueMember = "Id";

                cbStatStatus.DataSource = await _db.GetStatusesAsync();
                cbStatStatus.DisplayMember = "Name";
                cbStatStatus.ValueMember = "Id";

                await ReloadListAsync();
                await ReloadStatsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке: " + ex.Message);
            }
            finally
            {
                ToggleTopInputs(true);
            }
        }

        private void ToggleTopInputs(bool enabled)
        {
            cbStatus.Enabled = enabled;
            cbDep.Enabled = enabled;
            cbPost.Enabled = enabled;
            tbLastName.Enabled = enabled;
            btnSearch.Enabled = enabled;
            btnReset.Enabled = enabled;
        }


        private async Task ReloadListAsync()
        {
            _listCts?.Cancel();
            _listCts = new CancellationTokenSource();
            var ct = _listCts.Token;

            try
            {
                int? statusId = (cbStatus.SelectedItem as LookupItem)?.Id;
                if (statusId == -1) statusId = null;
                int? depId = (cbDep.SelectedItem as LookupItem)?.Id;
                if (depId == -1) depId = null;
                int? postId = (cbPost.SelectedItem as LookupItem)?.Id;
                if (postId == -1) postId = null;

                var people = await _db.GetPersonsAsync(statusId, depId, postId, tbLastName.Text, currentSortColumn, currentSortDir, ct);

                gridSource.DataSource = new BindingList<PersonRow>(people);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении данных: " + ex.Message);
            }
            finally
            {
                ToggleTopInputs(true);
            }
        }

        private async Task ReloadStatsAsync()
        {
            _statsCts?.Cancel();
            _statsCts = new CancellationTokenSource();
            var ct = _statsCts.Token;
            try
            {
                if (dpFrom.Value.Date > dpTo.Value.Date)
                {
                    MessageBox.Show("Дата 'с' должна быть не позже даты 'по'.");
                    return;
                }
                if (cbStatStatus.SelectedValue == null)
                {
                    MessageBox.Show("В базе данных отсутсвуют статусы");
                    return;
                } 
                
                btnBuild.Enabled = false;

                int statusId = (int)cbStatStatus.SelectedValue;
                bool hired = rbHired.Checked;
                var points = await _db.GetStatsAsync(statusId, dpFrom.Value.Date, dpTo.Value.Date, hired, ct);
                gridStats.DataSource = new BindingList<StatPoint>(points);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении статистики: " + ex.Message);
            }
            finally
            {
                btnBuild.Enabled = true;
            }
        }

        private async void BtnSearch_Click(object sender, EventArgs e) => await ReloadListAsync();
        private async void BtnReset_Click(object sender, EventArgs e)
        {
            cbStatus.SelectedIndex = 0;
            cbDep.SelectedIndex = 0;
            cbPost.SelectedIndex = 0;
            tbLastName.Text = "";
            currentSortColumn = "last_name"; currentSortDir = "ASC";
            await ReloadListAsync();
        }
        private async void BtnBuild_Click(object sender, EventArgs e) => await ReloadStatsAsync();

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (grid.Columns[e.ColumnIndex].Name == "col_fio")
            {
                var row = grid.Rows[e.RowIndex].DataBoundItem as PersonRow;
                if (row != null && row.IsFired)
                {
                    e.CellStyle.BackColor = Color.MistyRose;
                    e.CellStyle.ForeColor = Color.Maroon;
                    e.CellStyle.Font = new Font(grid.Font, FontStyle.Bold);
                }
            }
        }

        private async void Grid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var col = grid.Columns[e.ColumnIndex];
            string sortCol = currentSortColumn;
            switch (col.Name)
            {
                case "col_fio": sortCol = "last_name"; break;
                case "col_status": sortCol = "status_name"; break;
                case "col_dep": sortCol = "dep_name"; break;
                case "col_post": sortCol = "post_name"; break;
                case "col_date_employ": sortCol = "date_employ"; break;
                case "col_date_uneploy": sortCol = "date_uneploy"; break;
                default: sortCol = "last_name"; break;
            }
            if (currentSortColumn == sortCol) currentSortDir = currentSortDir == "ASC" ? "DESC" : "ASC";
            else { currentSortColumn = sortCol; currentSortDir = "ASC"; }

            grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection =
                currentSortDir == "ASC" ? SortOrder.Ascending : SortOrder.Descending;

            await ReloadListAsync();
        }
    }
}