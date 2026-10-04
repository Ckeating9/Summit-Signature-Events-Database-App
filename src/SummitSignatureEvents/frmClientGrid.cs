using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SummitSignatureEventsPart2
{
    public class frmClientGrid : Form
    {
        private readonly DataGridView dgvClients = new DataGridView();
        private readonly Label lblConnection = new Label();
        private readonly Button btnLoad = new Button();
        private readonly Button btnAddRow = new Button();
        private readonly Button btnSave = new Button();
        private readonly Button btnClose = new Button();

        private SqlDataAdapter adapter;
        private DataTable clientTable;
        private string activeConnectionString = "";

        private const string clientSql =
            @"SELECT ClientID, FirstName, LastName, Email, Phone, CompanyName
              FROM dbo.Client
              ORDER BY ClientID";

        public frmClientGrid()
        {
            InitializeForm();
            Load += frmClientGrid_Load;
        }

        private void InitializeForm()
        {
            Text = "Client Grid Form";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(980, 560);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            Label title = new Label();
            title.Text = "Client Grid";
            title.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            title.AutoSize = true;
            title.Location = new Point(20, 18);
            Controls.Add(title);

            lblConnection.AutoSize = true;
            lblConnection.Location = new Point(22, 52);
            Controls.Add(lblConnection);

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Location = new Point(20, 82);
            buttons.Size = new Size(940, 42);
            BuildButton(btnLoad, "Load", btnLoad_Click);
            BuildButton(btnAddRow, "Add New Client", btnAddRow_Click);
            BuildButton(btnSave, "Save", btnSave_Click);
            BuildButton(btnClose, "Close", btnClose_Click);
            buttons.Controls.Add(btnLoad);
            buttons.Controls.Add(btnAddRow);
            buttons.Controls.Add(btnSave);
            buttons.Controls.Add(btnClose);
            Controls.Add(buttons);

            dgvClients.Location = new Point(20, 132);
            dgvClients.Size = new Size(940, 390);
            dgvClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvClients.AllowUserToDeleteRows = false;
            dgvClients.DataError += dgvClients_DataError;
            Controls.Add(dgvClients);
        }

        private void BuildButton(Button button, string text, EventHandler handler)
        {
            button.Text = text;
            button.AutoSize = true;
            button.Padding = new Padding(8, 4, 8, 4);
            button.Click += handler;
        }

        private void frmClientGrid_Load(object sender, EventArgs e)
        {
            lblConnection.Text = "Connection: " + Db.DataSourceUsed;
            LoadClients();
        }

        private void LoadClients()
        {
            using (SqlConnection conn = Db.GetConnection())
            {
                activeConnectionString = conn.ConnectionString;

                adapter = new SqlDataAdapter(clientSql, conn);
                adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                clientTable = new DataTable();
                adapter.Fill(clientTable);

                dgvClients.DataSource = clientTable;

                if (dgvClients.Columns["ClientID"] != null)
                {
                    dgvClients.Columns["ClientID"].ReadOnly = true;
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadClients();
        }

        private void btnAddRow_Click(object sender, EventArgs e)
        {
            if (clientTable == null)
            {
                return;
            }

            DataRow row = clientTable.NewRow();
            row["ClientID"] = Convert.ToInt32(Db.ExecuteScalar("SELECT ISNULL(MAX(ClientID), 100) + 1 FROM dbo.Client"));
            row["FirstName"] = "New";
            row["LastName"] = "Client";
            row["Email"] = "new.client" + DateTime.Now.ToString("HHmmss") + "@example.com";
            row["Phone"] = "303-555-0000";
            row["CompanyName"] = DBNull.Value;

            clientTable.Rows.Add(row);

            int newRowIndex = dgvClients.Rows.Count - 2;
            if (newRowIndex >= 0)
            {
                dgvClients.CurrentCell = dgvClients.Rows[newRowIndex].Cells[1];
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (clientTable == null)
            {
                return;
            }

            Validate();
            dgvClients.EndEdit();

            CurrencyManager cm = dgvClients.DataSource == null
                ? null
                : BindingContext[dgvClients.DataSource] as CurrencyManager;

            if (cm != null)
            {
                cm.EndCurrentEdit();
            }

            foreach (DataRow row in clientTable.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row["FirstName"].ToString()) ||
                    string.IsNullOrWhiteSpace(row["LastName"].ToString()) ||
                    string.IsNullOrWhiteSpace(row["Email"].ToString()) ||
                    string.IsNullOrWhiteSpace(row["Phone"].ToString()))
                {
                    MessageBox.Show("FirstName, LastName, Email, and Phone are required for every client row.");
                    return;
                }
            }

            using (SqlConnection conn = new SqlConnection(activeConnectionString))
            {
                conn.Open();

                SqlDataAdapter saveAdapter = new SqlDataAdapter(clientSql, conn);
                saveAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                SqlCommandBuilder builder = new SqlCommandBuilder(saveAdapter);
                saveAdapter.Update(clientTable);
            }

            MessageBox.Show("Client changes saved.");
            LoadClients();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void dgvClients_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show("Please correct the current grid value and try again.");
            e.Cancel = true;
        }
    }
}
