using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace MushkilPaisa
{
    public partial class Form1 : Form
    {
        private decimal balance = 125000m;
        private Label lblBalance;
        private TextBox txtTo, txtAmount, txtNote;
        private DataGridView grid;

        private static readonly Color Navy = Color.FromArgb(18, 32, 64);
        private static readonly Color Accent = Color.FromArgb(0, 150, 136);
        private static readonly Color Bg = Color.FromArgb(244, 246, 250);

        public Form1()
        {
            // If you have Form1.Designer.cs, keep this call. Otherwise delete it.
            InitializeComponent();
            BuildUi();
            AddTx("Opening balance", balance);
        }

        private void BuildUi()
        {
            Text = "MushkilPaisa";
            ClientSize = new Size(980, 620);
            MinimumSize = new Size(900, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Bg;
            Font = new Font("Segoe UI", 10f);

            // Header with balance
            var header = new Panel { Dock = DockStyle.Top, Height = 150, BackColor = Navy };
            var title = new Label
            {
                Text = "MushkilPaisa",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 16f),
                Location = new Point(24, 16),
                AutoSize = true
            };
            var sub = new Label
            {
                Text = "Available balance",
                ForeColor = Color.FromArgb(170, 185, 210),
                Location = new Point(26, 62),
                AutoSize = true
            };
            lblBalance = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 30f),
                Location = new Point(22, 82),
                AutoSize = true
            };
            header.Controls.AddRange(new Control[] { title, sub, lblBalance });

            // Left: actions card
            var card = new Panel { Dock = DockStyle.Left, Width = 340, BackColor = Color.White };
            var cardTitle = new Label
            {
                Text = "Send money",
                Font = new Font("Segoe UI Semibold", 13f),
                Location = new Point(20, 18),
                AutoSize = true
            };
            txtTo = MakeField(card, "Recipient (name / account)", 60);
            txtAmount = MakeField(card, "Amount", 130);
            txtNote = MakeField(card, "Note (optional)", 200);

            var btnSend = MakeButton("Transfer", 275, Accent);
            btnSend.Click += (s, e) => DoTransfer();
            var btnDep = MakeButton("Deposit", 325, Navy);
            btnDep.Click += (s, e) => DoDeposit();
            var btnWd = MakeButton("Withdraw", 375, Color.FromArgb(120, 130, 150));
            btnWd.Click += (s, e) => DoWithdraw();
            card.Controls.AddRange(new Control[] { cardTitle, btnSend, btnDep, btnWd });

            // Right: transactions
            var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var histTitle = new Label
            {
                Text = "Recent transactions",
                Font = new Font("Segoe UI Semibold", 13f),
                Dock = DockStyle.Top,
                Height = 34
            };
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Bg;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Navy;
            grid.ColumnHeadersHeight = 36;
            grid.RowTemplate.Height = 34;
            grid.Columns.Add("date", "Date");
            grid.Columns.Add("desc", "Description");
            grid.Columns.Add("amt", "Amount (PKR)");
            grid.Columns["amt"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            right.Controls.Add(grid);
            right.Controls.Add(histTitle);

            Controls.Add(right);
            Controls.Add(card);
            Controls.Add(header);
        }

        private TextBox MakeField(Panel parent, string label, int top)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                ForeColor = Color.Gray,
                Location = new Point(20, top),
                AutoSize = true
            });
            var tb = new TextBox { Location = new Point(20, top + 24), Width = 300 };
            parent.Controls.Add(tb);
            return tb;
        }

        private Button MakeButton(string text, int top, Color color)
        {
            var b = new Button
            {
                Text = text,
                Location = new Point(20, top),
                Size = new Size(300, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private bool TryGetAmount(out decimal amount)
        {
            if (!decimal.TryParse(txtAmount.Text.Trim(), NumberStyles.Number,
                    CultureInfo.InvariantCulture, out amount) || amount <= 0)
            {
                MessageBox.Show("Enter a valid amount greater than 0.", "Invalid amount",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private bool HasFunds(decimal amount)
        {
            if (amount <= balance) return true;
            MessageBox.Show("Insufficient balance.", "Declined",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        private void DoTransfer()
        {
            if (string.IsNullOrWhiteSpace(txtTo.Text))
            {
                MessageBox.Show("Enter a recipient.", "Missing recipient",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            decimal amt;
            if (!TryGetAmount(out amt) || !HasFunds(amt)) return;

            balance -= amt;
            string note = string.IsNullOrWhiteSpace(txtNote.Text) ? "" : " - " + txtNote.Text.Trim();
            AddTx("Transfer to " + txtTo.Text.Trim() + note, -amt);
            ClearFields();
        }

        private void DoDeposit()
        {
            decimal amt;
            if (!TryGetAmount(out amt)) return;
            balance += amt;
            AddTx("Deposit", amt);
            ClearFields();
        }

        private void DoWithdraw()
        {
            decimal amt;
            if (!TryGetAmount(out amt) || !HasFunds(amt)) return;
            balance -= amt;
            AddTx("Withdrawal", -amt);
            ClearFields();
        }

        // Newest transaction goes on top.
        private void AddTx(string desc, decimal signedAmount)
        {
            string amtText = (signedAmount >= 0 ? "+" : "-") +
                             Math.Abs(signedAmount).ToString("N2", CultureInfo.InvariantCulture);
            grid.Rows.Insert(0, DateTime.Now.ToString("dd MMM yyyy HH:mm"), desc, amtText);
            grid.Rows[0].Cells["amt"].Style.ForeColor =
                signedAmount >= 0 ? Color.FromArgb(0, 140, 80) : Color.FromArgb(200, 50, 50);
            RefreshBalance();
        }

        private void RefreshBalance()
        {
            lblBalance.Text = "PKR " + balance.ToString("N2", CultureInfo.InvariantCulture);
        }

        private void ClearFields()
        {
            txtTo.Clear();
            txtAmount.Clear();
            txtNote.Clear();
        }
    }
}