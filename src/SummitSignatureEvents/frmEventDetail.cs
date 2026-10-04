using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SummitSignatureEventsPart2
{
    public class frmEventDetail : Form
    {
        private readonly TextBox txtEventID = new TextBox();
        private readonly TextBox txtEventName = new TextBox();
        private readonly DateTimePicker dtpEventDate = new DateTimePicker();
        private readonly TextBox txtGuestCount = new TextBox();
        private readonly TextBox txtBudget = new TextBox();
        private readonly ComboBox cboStatus = new ComboBox();
        private readonly ComboBox cboClient = new ComboBox();
        private readonly ComboBox cboVenue = new ComboBox();
        private readonly ComboBox cboEventType = new ComboBox();
        private readonly ComboBox cboLeadPlanner = new ComboBox();
        private readonly Label lblConnection = new Label();
        private readonly Button btnPrevious = new Button();
        private readonly Button btnNext = new Button();
        private readonly Button btnNew = new Button();
        private readonly Button btnSave = new Button();
        private readonly Button btnClose = new Button();

        private readonly List<int> eventIds = new List<int>();
        private int currentIndex = -1;

        public frmEventDetail()
        {
            InitializeForm();
            Load += frmEventDetail_Load;
        }

        private void InitializeForm()
        {
            Text = "Event Detail Form";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(840, 520);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            Label title = new Label();
            title.Text = "Event Detail";
            title.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            title.AutoSize = true;
            title.Location = new Point(20, 18);
            Controls.Add(title);

            lblConnection.AutoSize = true;
            lblConnection.Location = new Point(22, 52);
            Controls.Add(lblConnection);

            TableLayoutPanel table = new TableLayoutPanel();
            table.Location = new Point(20, 86);
            table.Size = new Size(790, 300);
            table.ColumnCount = 4;
            table.RowCount = 5;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));

            AddField(table, 0, 0, "Event ID", txtEventID);
            AddField(table, 0, 1, "Event Name", txtEventName);
            AddField(table, 0, 2, "Event Date", dtpEventDate);
            AddField(table, 0, 3, "Guest Count", txtGuestCount);
            AddField(table, 0, 4, "Budget", txtBudget);
            AddField(table, 2, 0, "Status", cboStatus);
            AddField(table, 2, 1, "Client", cboClient);
            AddField(table, 2, 2, "Venue", cboVenue);
            AddField(table, 2, 3, "Event Type", cboEventType);
            AddField(table, 2, 4, "Lead Planner", cboLeadPlanner);
            Controls.Add(table);

            dtpEventDate.Format = DateTimePickerFormat.Short;
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboClient.DropDownStyle = ComboBoxStyle.DropDownList;
            cboVenue.DropDownStyle = ComboBoxStyle.DropDownList;
            cboEventType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLeadPlanner.DropDownStyle = ComboBoxStyle.DropDownList;

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Location = new Point(20, 410);
            buttons.Size = new Size(790, 45);

            BuildButton(btnPrevious, "Previous", btnPrevious_Click);
            BuildButton(btnNext, "Next", btnNext_Click);
            BuildButton(btnNew, "New", btnNew_Click);
            BuildButton(btnSave, "Save", btnSave_Click);
            BuildButton(btnClose, "Close", btnClose_Click);

            buttons.Controls.Add(btnPrevious);
            buttons.Controls.Add(btnNext);
            buttons.Controls.Add(btnNew);
            buttons.Controls.Add(btnSave);
            buttons.Controls.Add(btnClose);
            Controls.Add(buttons);
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

        private void frmEventDetail_Load(object sender, EventArgs e)
        {
            lblConnection.Text = "Connection: " + Db.DataSourceUsed;

            cboStatus.Items.AddRange(new object[] { "Planned", "Confirmed", "Completed" });
            BindLookup(cboClient, "SELECT ClientID, FirstName + ' ' + LastName AS FullName FROM dbo.Client ORDER BY LastName, FirstName", "FullName", "ClientID");
            BindLookup(cboVenue, "SELECT VenueID, VenueName FROM dbo.Venue ORDER BY VenueName", "VenueName", "VenueID");
            BindLookup(cboEventType, "SELECT EventTypeID, EventTypeName FROM dbo.EventType ORDER BY EventTypeName", "EventTypeName", "EventTypeID");
            BindLookup(cboLeadPlanner, "SELECT EmployeeID, FirstName + ' ' + LastName AS FullName FROM dbo.Employee ORDER BY LastName, FirstName", "FullName", "EmployeeID");

            LoadEventIds();
            if (eventIds.Count > 0)
            {
                currentIndex = 0;
                LoadEvent(eventIds[currentIndex]);
            }
            else
            {
                PrepareNewRecord();
            }
        }

        private void BindLookup(ComboBox comboBox, string query, string displayMember, string valueMember)
        {
            DataTable dt = Db.GetTable(query);
            comboBox.DataSource = dt;
            comboBox.DisplayMember = displayMember;
            comboBox.ValueMember = valueMember;
        }

        private void LoadEventIds()
        {
            eventIds.Clear();
            DataTable dt = Db.GetTable("SELECT EventID FROM dbo.Event ORDER BY EventID");
            foreach (DataRow row in dt.Rows)
            {
                eventIds.Add(Convert.ToInt32(row["EventID"]));
            }
        }

        private void LoadEvent(int eventId)
        {
            DataTable dt = Db.GetTable(
                "SELECT EventID, ClientID, VenueID, EventTypeID, LeadPlannerEmployeeID, EventName, EventDate, GuestCount, Budget, Status FROM dbo.Event WHERE EventID = @EventID",
                new SqlParameter("@EventID", eventId));

            if (dt.Rows.Count == 0)
            {
                return;
            }

            DataRow row = dt.Rows[0];
            txtEventID.Text = row["EventID"].ToString();
            txtEventName.Text = row["EventName"].ToString();
            dtpEventDate.Value = Convert.ToDateTime(row["EventDate"]);
            txtGuestCount.Text = row["GuestCount"].ToString();
            txtBudget.Text = Convert.ToDecimal(row["Budget"]).ToString("0.00");
            cboStatus.SelectedItem = row["Status"].ToString();
            cboClient.SelectedValue = Convert.ToInt32(row["ClientID"]);
            cboVenue.SelectedValue = Convert.ToInt32(row["VenueID"]);
            cboEventType.SelectedValue = Convert.ToInt32(row["EventTypeID"]);
            cboLeadPlanner.SelectedValue = Convert.ToInt32(row["LeadPlannerEmployeeID"]);
        }

        private void PrepareNewRecord()
        {
            object maxValue = Db.ExecuteScalar("SELECT ISNULL(MAX(EventID), 700) + 1 FROM dbo.Event");
            txtEventID.Text = Convert.ToInt32(maxValue).ToString();
            txtEventName.Clear();
            dtpEventDate.Value = DateTime.Today;
            txtGuestCount.Text = "50";
            txtBudget.Text = "5000.00";
            cboStatus.SelectedIndex = 0;
            if (cboClient.Items.Count > 0) cboClient.SelectedIndex = 0;
            if (cboVenue.Items.Count > 0) cboVenue.SelectedIndex = 0;
            if (cboEventType.Items.Count > 0) cboEventType.SelectedIndex = 0;
            if (cboLeadPlanner.Items.Count > 0) cboLeadPlanner.SelectedIndex = 0;
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (eventIds.Count == 0 || currentIndex <= 0)
            {
                return;
            }

            currentIndex--;
            LoadEvent(eventIds[currentIndex]);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (eventIds.Count == 0 || currentIndex >= eventIds.Count - 1)
            {
                return;
            }

            currentIndex++;
            LoadEvent(eventIds[currentIndex]);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            currentIndex = -1;
            PrepareNewRecord();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            int eventId;
            int guestCount;
            decimal budget;

            if (!int.TryParse(txtEventID.Text.Trim(), out eventId))
            {
                MessageBox.Show("Event ID must be a whole number.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEventName.Text))
            {
                MessageBox.Show("Event Name is required.");
                return;
            }

            if (!int.TryParse(txtGuestCount.Text.Trim(), out guestCount))
            {
                MessageBox.Show("Guest Count must be a whole number.");
                return;
            }

            if (!decimal.TryParse(txtBudget.Text.Trim(), out budget))
            {
                MessageBox.Show("Budget must be numeric.");
                return;
            }

            int existingCount = Convert.ToInt32(Db.ExecuteScalar(
                "SELECT COUNT(*) FROM dbo.Event WHERE EventID = @EventID",
                new SqlParameter("@EventID", eventId)));

            string sql;
            if (existingCount > 0)
            {
                sql = @"UPDATE dbo.Event
                        SET ClientID = @ClientID,
                            VenueID = @VenueID,
                            EventTypeID = @EventTypeID,
                            LeadPlannerEmployeeID = @LeadPlannerEmployeeID,
                            EventName = @EventName,
                            EventDate = @EventDate,
                            GuestCount = @GuestCount,
                            Budget = @Budget,
                            Status = @Status
                        WHERE EventID = @EventID";
            }
            else
            {
                sql = @"INSERT INTO dbo.Event
                        (EventID, ClientID, VenueID, EventTypeID, LeadPlannerEmployeeID, EventName, EventDate, GuestCount, Budget, Status)
                        VALUES
                        (@EventID, @ClientID, @VenueID, @EventTypeID, @LeadPlannerEmployeeID, @EventName, @EventDate, @GuestCount, @Budget, @Status)";
            }

            Db.ExecuteNonQuery(
                sql,
                new SqlParameter("@EventID", eventId),
                new SqlParameter("@ClientID", Convert.ToInt32(cboClient.SelectedValue)),
                new SqlParameter("@VenueID", Convert.ToInt32(cboVenue.SelectedValue)),
                new SqlParameter("@EventTypeID", Convert.ToInt32(cboEventType.SelectedValue)),
                new SqlParameter("@LeadPlannerEmployeeID", Convert.ToInt32(cboLeadPlanner.SelectedValue)),
                new SqlParameter("@EventName", txtEventName.Text.Trim()),
                new SqlParameter("@EventDate", dtpEventDate.Value.Date),
                new SqlParameter("@GuestCount", guestCount),
                new SqlParameter("@Budget", budget),
                new SqlParameter("@Status", cboStatus.Text));

            LoadEventIds();
            currentIndex = eventIds.IndexOf(eventId);
            if (currentIndex >= 0)
            {
                LoadEvent(eventId);
            }

            MessageBox.Show("Event saved.");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
