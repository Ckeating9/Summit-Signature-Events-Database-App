using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SummitSignatureEventsPart2
{
    public class frmClientEventsMain : Form
    {
        private readonly ComboBox cboClient = new ComboBox();
        private readonly TextBox txtFirstName = new TextBox();
        private readonly TextBox txtLastName = new TextBox();
        private readonly TextBox txtEmail = new TextBox();
        private readonly TextBox txtPhone = new TextBox();
        private readonly TextBox txtCompanyName = new TextBox();
        private readonly DataGridView dgvEvents = new DataGridView();
        private readonly Label lblConnection = new Label();
        private readonly Button btnSaveClient = new Button();
        private readonly Button btnRefreshEvents = new Button();
        private readonly Button btnSaveEvents = new Button();
        private readonly Button btnClose = new Button();

        private SqlDataAdapter eventAdapter;
        private DataTable eventTable;
        private string activeConnectionString = "";

        private const string eventSql =
            @"SELECT EventID, VenueID, EventTypeID, LeadPlannerEmployeeID, EventName, EventDate, GuestCount, Budget, Status
              FROM dbo.Event
              WHERE ClientID = @ClientID
              ORDER BY EventDate, EventID";

        public frmClientEventsMain()
        {
            InitializeForm();
            Load += frmClientEventsMain_Load;
        }

        private void InitializeForm()
        {
            Text = "Client Main Form with Event Subform";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1120, 650);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            Label title = new Label();
            title.Text = "Client Main Form with Event Subform";
            title.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            title.AutoSize = true;
            title.Location = new Point(20, 18);
            Controls.Add(title);

            lblConnection.AutoSize = true;
            lblConnection.Location = new Point(22, 52);
            Controls.Add(lblConnection);

            GroupBox grpClient = new GroupBox();
            grpClient.Text = "Client";
            grpClient.Location = new Point(20, 82);
            grpClient.Size = new Size(1080, 190);
            Controls.Add(grpClient);

            TableLayoutPanel table = new TableLayoutPanel();
            table.Location = new Point(16, 28);
            table.Size = new Size(1048, 120);
            table.ColumnCount = 4;
            table.RowCount = 3;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340F));
            grpClient.Controls.Add(table);

            cboClient.DropDownStyle = ComboBoxStyle.DropDownList;
            AddField(table, 0, 0, "Select Client", cboClient);
            AddField(table, 0, 1, "First Name", txtFirstName);
            AddField(table, 0, 2, "Last Name", txtLastName);
            AddField(table, 2, 0, "Email", txtEmail);
            AddField(table, 2, 1, "Phone", txtPhone);
            AddField(table, 2, 2, "Company Name", txtCompanyName);

            FlowLayoutPanel topButtons = new FlowLayoutPanel();
            topButtons.Location = new Point(16, 150);
            topButtons.Size = new Size(500, 30);
            BuildButton(btnSaveClient, "Save Client", btnSaveClient_Click);
            BuildButton(btnRefreshEvents, "Refresh Events", btnRefreshEvents_Click);
            topButtons.Controls.Add(btnSaveClient);
            topButtons.Controls.Add(btnRefreshEvents);
            grpClient.Controls.Add(topButtons);

            GroupBox grpEvents = new GroupBox();
            grpEvents.Text = "Related Events";
            grpEvents.Location = new Point(20, 290);
            grpEvents.Size = new Size(1080, 320);
            Controls.Add(grpEvents);

            dgvEvents.Location = new Point(16, 30);
            dgvEvents.Size = new Size(1048, 240);
            dgvEvents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvEvents.AllowUserToAddRows = false;
            dgvEvents.AllowUserToDeleteRows = false;
            dgvEvents.DataError += dgvEvents_DataError;
            grpEvents.Controls.Add(dgvEvents);

            FlowLayoutPanel bottomButtons = new FlowLayoutPanel();
            bottomButtons.Location = new Point(16, 278);
            bottomButtons.Size = new Size(400, 30);
            BuildButton(btnSaveEvents, "Save Event Changes", btnSaveEvents_Click);
            BuildButton(btnClose, "Close", btnClose_Click);
            bottomButtons.Controls.Add(btnSaveEvents);
            bottomButtons.Controls.Add(btnClose);
            grpEvents.Controls.Add(bottomButtons);

            cboClient.SelectedIndexChanged += cboClient_SelectedIndexChanged;
        }

        private void BuildButton(Button button, string text, EventHandler handler)
        {
            button.Text = text;
            button.AutoSize = true;
            button.Padding = new Padding(8, 4, 8, 4);
            button.Click += handler;
        }

        private void AddField(TableLayoutPanel table, int column, int row, string labelText, Control control)
        {
            Label label = new Label();
            label.Text = labelText;
            label.AutoSize = true;
            label.Anchor = AnchorStyles.Left;
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(3, 3, 12, 12);
            table.Controls.Add(label, column, row);
            table.Controls.Add(control, column + 1, row);
        }

        private void frmClientEventsMain_Load(object sender, EventArgs e)
        {
            lblConnection.Text = "Connection: " + Db.DataSourceUsed;
            BindClients();
        }

        private void BindClients()
        {
            DataTable dt = Db.GetTable(
                "SELECT ClientID, FirstName + ' ' + LastName AS FullName FROM dbo.Client ORDER BY LastName, FirstName");
            cboClient.DataSource = dt;
            cboClient.DisplayMember = "FullName";
            cboClient.ValueMember = "ClientID";
        }

        private void cboClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboClient.SelectedValue == null)
            {
                return;
            }

            if (cboClient.SelectedValue is DataRowView)
            {
                return;
            }

            LoadClientDetails();
            LoadEvents();
        }

        private void LoadClientDetails()
        {
            DataTable dt = Db.GetTable(
                "SELECT FirstName, LastName, Email, Phone, CompanyName FROM dbo.Client WHERE ClientID = @ClientID",
                new SqlParameter("@ClientID", Convert.ToInt32(cboClient.SelectedValue)));

            if (dt.Rows.Count == 0)
            {
                return;
            }

            DataRow row = dt.Rows[0];
            txtFirstName.Text = row["FirstName"].ToString();
            txtLastName.Text = row["LastName"].ToString();
            txtEmail.Text = row["Email"].ToString();
            txtPhone.Text = row["Phone"].ToString();
            txtCompanyName.Text = row["CompanyName"] == DBNull.Value ? string.Empty : row["CompanyName"].ToString();
        }

        private void LoadEvents()
        {
            using (SqlConnection conn = Db.GetConnection())
            {
                activeConnectionString = conn.ConnectionString;

                eventAdapter = new SqlDataAdapter(eventSql, conn);
                eventAdapter.SelectCommand.Parameters.AddWithValue("@ClientID", Convert.ToInt32(cboClient.SelectedValue));
                eventAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                SqlCommandBuilder builder = new SqlCommandBuilder(eventAdapter);

                eventTable = new DataTable();
                eventAdapter.Fill(eventTable);
                dgvEvents.DataSource = eventTable;

                if (dgvEvents.Columns.Contains("EventID"))
                {
                    dgvEvents.Columns["EventID"].ReadOnly = true;
                }
            }
        }

        private void btnSaveClient_Click(object sender, EventArgs e)
        {
            Db.ExecuteNonQuery(
                @"UPDATE dbo.Client
                  SET FirstName = @FirstName,
                      LastName = @LastName,
                      Email = @Email,
                      Phone = @Phone,
                      CompanyName = @CompanyName
                  WHERE ClientID = @ClientID",
                new SqlParameter("@FirstName", txtFirstName.Text.Trim()),
                new SqlParameter("@LastName", txtLastName.Text.Trim()),
                new SqlParameter("@Email", txtEmail.Text.Trim()),
                new SqlParameter("@Phone", txtPhone.Text.Trim()),
                new SqlParameter("@CompanyName", string.IsNullOrWhiteSpace(txtCompanyName.Text) ? (object)DBNull.Value : txtCompanyName.Text.Trim()),
                new SqlParameter("@ClientID", Convert.ToInt32(cboClient.SelectedValue)));

            int selectedClientId = Convert.ToInt32(cboClient.SelectedValue);
            BindClients();
            cboClient.SelectedValue = selectedClientId;

            MessageBox.Show("Client record saved.");
        }

        private void btnRefreshEvents_Click(object sender, EventArgs e)
        {
            LoadEvents();
        }

        private void btnSaveEvents_Click(object sender, EventArgs e)
        {
            if (eventTable == null)
            {
                return;
            }

            Validate();
            dgvEvents.EndEdit();

            CurrencyManager cm = dgvEvents.DataSource == null
                ? null
                : BindingContext[dgvEvents.DataSource] as CurrencyManager;

            if (cm != null)
            {
                cm.EndCurrentEdit();
            }

            foreach (DataRow row in eventTable.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row["EventName"].ToString()) ||
                    row["EventDate"] == DBNull.Value ||
                    row["GuestCount"] == DBNull.Value ||
                    row["Budget"] == DBNull.Value ||
                    string.IsNullOrWhiteSpace(row["Status"].ToString()))
                {
                    MessageBox.Show("EventName, EventDate, GuestCount, Budget, and Status are required for every event row.");
                    return;
                }
            }

            using (SqlConnection conn = new SqlConnection(activeConnectionString))
            {
                conn.Open();

                SqlDataAdapter saveAdapter = new SqlDataAdapter(eventSql, conn);
                saveAdapter.SelectCommand.Parameters.AddWithValue("@ClientID", Convert.ToInt32(cboClient.SelectedValue));
                saveAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                SqlCommandBuilder builder = new SqlCommandBuilder(saveAdapter);
                saveAdapter.Update(eventTable);
            }

            MessageBox.Show("Event changes saved.");
            LoadEvents();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void dgvEvents_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show("Please correct the current grid value and try again.");
            e.Cancel = true;
        }
    }
}
