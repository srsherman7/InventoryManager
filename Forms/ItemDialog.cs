using InventoryManager.Models;

namespace InventoryManager.Forms
{
    public class ItemDialog : Form
    {
        private TextBox txtName = null!;
        private TextBox txtType = null!;
        private NumericUpDown numQty = null!;
        private NumericUpDown numPrice = null!;
        private Button btnSave = null!;
        private Button btnCancel = null!;

        public InventoryItem? Result { get; private set; }
        private readonly InventoryItem? _existing;

        public ItemDialog(InventoryItem? existing = null)
        {
            _existing = existing;
            BuildUI();
            if (existing != null)
                PopulateFields(existing);
        }

        private void BuildUI()
        {
            Text = _existing == null ? "Add New Item" : "Edit Item";
            Size = new Size(420, 340);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 10f);

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 20, 28, 16)
            };
            Controls.Add(panel);

            int y = 0;

            panel.Controls.Add(MakeLabel("Product Name", y));
            txtName = new TextBox { Top = y + 22, Left = 0, Width = 360, Height = 30, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtName);
            y += 64;

            panel.Controls.Add(MakeLabel("Product Type", y));
            txtType = new TextBox { Top = y + 22, Left = 0, Width = 360, Height = 30, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtType);
            y += 64;

            panel.Controls.Add(MakeLabel("Quantity on Hand", y));
            numQty = new NumericUpDown
            {
                Top = y + 22, Left = 0, Width = 170, Font = new Font("Segoe UI", 10f),
                Minimum = 0, Maximum = 999999, DecimalPlaces = 0
            };
            panel.Controls.Add(numQty);
            y += 64;

            panel.Controls.Add(MakeLabel("Price Per Item ($)", y));
            numPrice = new NumericUpDown
            {
                Top = y + 22, Left = 0, Width = 170, Font = new Font("Segoe UI", 10f),
                Minimum = 0, Maximum = 99999, DecimalPlaces = 2, Increment = 0.25m
            };
            panel.Controls.Add(numPrice);
            y += 72;

            btnSave = new Button
            {
                Text = _existing == null ? "✓  Add Item" : "✓  Save Changes",
                Top = y, Left = 0, Width = 170, Height = 38,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            panel.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "✕  Cancel",
                Top = y, Left = 188, Width = 170, Height = 38,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
            panel.Controls.Add(btnCancel);
        }

        private Label MakeLabel(string text, int top) =>
            new Label { Text = text, Top = top, Left = 0, AutoSize = true, ForeColor = Color.FromArgb(80, 80, 100), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };

        private void PopulateFields(InventoryItem item)
        {
            txtName.Text = item.ProductName;
            txtType.Text = item.ProductType;
            numQty.Value = item.QuantityOnHand;
            numPrice.Value = item.PricePerItem;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Product name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtType.Text))
            {
                MessageBox.Show("Product type is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtType.Focus();
                return;
            }

            Result = new InventoryItem
            {
                Id = _existing?.Id ?? 0,
                ProductName = txtName.Text.Trim(),
                ProductType = txtType.Text.Trim(),
                QuantityOnHand = (int)numQty.Value,
                PricePerItem = numPrice.Value
            };
            DialogResult = DialogResult.OK;
        }
    }
}
