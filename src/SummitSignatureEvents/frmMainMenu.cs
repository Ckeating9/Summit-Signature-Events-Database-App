using System;
using System.Drawing;
using System.Windows.Forms;

namespace SummitSignatureEventsPart2
{
    public class frmMainMenu : Form
    {
        private readonly Label lblConnection = new Label();

        public frmMainMenu()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            Text = "Summit Signature Events";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(720, 430);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Label title = new Label
            {
                Text = "Summit Signature Events",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(34, 28)
            };
            Controls.Add(title);

            Label subtitle = new Label
            {
                Text = "Client and event management desktop application",
                AutoSize = true,
                Location = new Point(38, 75)
            };
            Controls.Add(subtitle);

            lblConnection.AutoSize = true;
            lblConnection.Location = new Point(38, 105);
            lblConnection.Text = "SQL Server: " + Db.DataSourceUsed;
            Controls.Add(lblConnection);

            AddMenuButton("Event Detail", "View, create, and update individual event records.", 140, OpenEventDetail);
            AddMenuButton("Client Grid", "Review and edit multiple client records in one view.", 210, OpenClientGrid);
            AddMenuButton("Client & Events", "Manage a client and the events associated with that client.", 280, OpenClientEvents);

            Button exit = new Button
            {
                Text = "Exit",
                Size = new Size(120, 38),
                Location = new Point(554, 365)
            };
            exit.Click += (sender, e) => Close();
            Controls.Add(exit);
        }

        private void AddMenuButton(string title, string description, int top, EventHandler handler)
        {
            Button button = new Button
            {
                Text = title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Size = new Size(190, 48),
                Location = new Point(40, top)
            };
            button.Click += handler;
            Controls.Add(button);

            Label label = new Label
            {
                Text = description,
                AutoSize = false,
                Size = new Size(420, 48),
                Location = new Point(250, top + 3)
            };
            Controls.Add(label);
        }

        private void OpenEventDetail(object sender, EventArgs e)
        {
            using (frmEventDetail form = new frmEventDetail())
            {
                form.ShowDialog(this);
            }
        }

        private void OpenClientGrid(object sender, EventArgs e)
        {
            using (frmClientGrid form = new frmClientGrid())
            {
                form.ShowDialog(this);
            }
        }

        private void OpenClientEvents(object sender, EventArgs e)
        {
            using (frmClientEventsMain form = new frmClientEventsMain())
            {
                form.ShowDialog(this);
            }
        }
    }
}
