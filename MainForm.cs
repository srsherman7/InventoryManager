using InventoryManager.Data;
using InventoryManager.Forms;
using InventoryManager.Models;
using System.IO.Compression;

namespace InventoryManager
{
    public class MainForm : Form
    {
        private readonly DatabaseService _db;
        private readonly string _dbPath;
        private List<InventoryItem> _items = new();
        private string _searchTerm = string.Empty;

        private Label lblTotalItems = new();
        private Label lblTotalWorth = new();
        private TextBox txtSearch = new();
        private DataGridView grid = new();

        private static readonly Color Accent = Color.FromArgb(52, 152, 219);
        private static readonly Color Green = Color.FromArgb(39, 174, 96);
        private static readonly Color Red = Color.FromArgb(231, 76, 60);
        private static readonly Color Dark = Color.FromArgb(44, 62, 80);
        private static readonly Color Light = Color.FromArgb(245, 247, 250);
        private static readonly Color StatBg = Color.FromArgb(30, 39, 46);
        private static readonly Color Orange = Color.FromArgb(211, 84, 0);

        public MainForm()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dbFolder = Path.Combine(appData, "InventoryManager");
            Directory.CreateDirectory(dbFolder);
            _dbPath = Path.Combine(dbFolder, "inventory.db");
            _db = new DatabaseService(_dbPath);

            SuspendLayout();
            BuildForm();
            BuildGrid();
            BuildToolbar();
            BuildStatsBar();
            BuildMenuStrip();
            ResumeLayout(false);

            LoadData();
        }

        private void BuildForm()
        {
            Text = "Inventory Manager";
            Size = new Size(1060, 700);
            MinimumSize = new Size(860, 540);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Light;
            Font = new Font("Segoe UI", 10f);
        }

        private void BuildMenuStrip()
        {
            var menu = new MenuStrip
            {
                BackColor = Color.FromArgb(22, 30, 36),
                ForeColor = Color.White,
                Renderer = new ToolStripProfessionalRenderer(new DarkMenuColors())
            };

            var fileMenu = new ToolStripMenuItem("File")
            {
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),                
            };

            var backupItem = new ToolStripMenuItem("Backup Database...")
            {
                ShortcutKeys = Keys.Control | Keys.B,
                ShowShortcutKeys = true,
                ForeColor = Color.White
            };
            backupItem.Click += MenuBackup_Click;

            var exportItem = new ToolStripMenuItem("Export to CSV...")
            {
                ShortcutKeys = Keys.Control | Keys.E,
                ShowShortcutKeys = true,
                ForeColor = Color.White
            };
            exportItem.Click += MenuExportCsv_Click;

            fileMenu.DropDownItems.Add(backupItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exportItem);

            menu.Items.Add(fileMenu);
            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        private void BuildStatsBar()
        {
            var bar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = StatBg
            };

            bar.Controls.Add(new Label
            {
                Text = "Inventory Manager",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(20, 0),
                Size = new Size(300, 80)
            });

            var (cardItems, valItems) = MakeStatCard("TOTAL ITEMS", 360, Color.FromArgb(52, 152, 219));
            lblTotalItems = valItems;
            bar.Controls.Add(cardItems);

            var (cardWorth, valWorth) = MakeStatCard("INVENTORY VALUE", 580, Color.FromArgb(39, 174, 96));
            lblTotalWorth = valWorth;
            bar.Controls.Add(cardWorth);

            Controls.Add(bar);
        }

        private (Panel card, Label valueLabel) MakeStatCard(string title, int left, Color accent)
        {
            var card = new Panel
            {
                Location = new Point(left, 14),
                Size = new Size(210, 52),
                BackColor = Color.FromArgb(55, 71, 79)
            };
            card.Controls.Add(new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(5, 52),
                BackColor = accent
            });
            card.Controls.Add(new Label
            {
                Text = title,
                Location = new Point(12, 5),
                Size = new Size(195, 18),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(189, 195, 199)
            });
            var val = new Label
            {
                Text = "0",
                Location = new Point(12, 24),
                Size = new Size(195, 24),
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White
            };
            card.Controls.Add(val);
            return (card, val);
        }

        private void BuildToolbar()
        {
            var bar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.White
            };
            bar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 220, 230)),
                    0, bar.Height - 1, bar.Width, bar.Height - 1);

            bar.Controls.Add(new Label
            {
                Text = "🔍",
                Location = new Point(16, 15),
                Size = new Size(28, 28),
                Font = new Font("Segoe UI", 12f),
                TextAlign = ContentAlignment.MiddleCenter
            });

            txtSearch = new TextBox
            {
                Location = new Point(48, 15),
                Size = new Size(300, 28),
                Font = new Font("Segoe UI", 10.5f),
                BorderStyle = BorderStyle.None,
                PlaceholderText = "Search by name or type..."
            };
            txtSearch.TextChanged += (s, e) => { _searchTerm = txtSearch.Text; LoadData(); };
            bar.Controls.Add(txtSearch);

            int x = 380;
            bar.Controls.Add(Btn("X  Clear", Color.FromArgb(149, 165, 166), ref x, (s, e) => txtSearch.Clear()));
            bar.Controls.Add(Btn("+  Add Item", Green, ref x, BtnAdd_Click));
            bar.Controls.Add(Btn("Edit", Accent, ref x, BtnEdit_Click));
            bar.Controls.Add(Btn("Delete", Red, ref x, BtnDelete_Click));

            Controls.Add(bar);
        }

        private Button Btn(string text, Color color, ref int x, EventHandler handler)
        {
            int w = text.Contains("Add") ? 118 : 96;
            var b = new Button
            {
                Text = text,
                Location = new Point(x, 11),
                Size = new Size(w, 34),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += handler;
            x += w + 8;
            return b;
        }
        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (!grid.CurrentCell.RowIndex.Equals(e.RowIndex) ||
                !grid.CurrentCell.ColumnIndex.Equals(e.ColumnIndex)) return;

            e.Paint(e.ClipBounds, DataGridViewPaintParts.All);

            using var pen = new Pen(Color.FromArgb(52, 152, 219), 2);
            var rect = e.CellBounds;
            rect.Inflate(-1, -1);
            e.Graphics.DrawRectangle(pen, rect);

            e.Handled = true;
        }
        private void BuildGrid()
        {
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Light,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(225, 228, 235),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 42,
                RowTemplate = { Height = 44 },
                EnableHeadersVisualStyles = false,
                AutoGenerateColumns = false,
                Font = new Font("Segoe UI", 10f)
            };

            grid.CellPainting += Grid_CellPainting;

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Dark,
                ForeColor = Color.White,
                SelectionBackColor = Dark,        // ← keeps header same color when row selected
                SelectionForeColor = Color.White, // ← keeps text white
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0)
            };
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Dark,
                SelectionBackColor = Color.FromArgb(214, 234, 248),
                SelectionForeColor = Dark,
                Padding = new Padding(6, 0, 0, 0)
            };
            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(250, 251, 254),
                SelectionBackColor = Color.FromArgb(214, 234, 248),
                SelectionForeColor = Dark
            };

            AddTextCol("ID", 55, false, DataGridViewContentAlignment.MiddleCenter);
            AddTextCol("Product Name", 0, true, DataGridViewContentAlignment.MiddleLeft);
            AddTextCol("Type", 140, false, DataGridViewContentAlignment.MiddleLeft);
            AddTextCol("Qty on Hand", 110, false, DataGridViewContentAlignment.MiddleCenter);
            AddTextCol("Price / Item", 120, false, DataGridViewContentAlignment.MiddleRight);
            AddTextCol("Total Value", 130, false, DataGridViewContentAlignment.MiddleRight);

            var colMinus = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Name = "colMinus",
                Width = 44,
                FlatStyle = FlatStyle.Flat,
                Text = "-",
                UseColumnTextForButtonValue = true
            };
            colMinus.DefaultCellStyle.BackColor = Red;
            colMinus.DefaultCellStyle.ForeColor = Color.White;
            colMinus.DefaultCellStyle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            colMinus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns.Add(colMinus);

            var colPlus = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Name = "colPlus",
                Width = 44,
                FlatStyle = FlatStyle.Flat,
                Text = "+",
                UseColumnTextForButtonValue = true
            };
            colPlus.DefaultCellStyle.BackColor = Green;
            colPlus.DefaultCellStyle.ForeColor = Color.White;
            colPlus.DefaultCellStyle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            colPlus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns.Add(colPlus);

            grid.CellClick += Grid_CellClick;
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditSelectedItem(); };

            Controls.Add(grid);
        }

        private void AddTextCol(string header, int width, bool fill, DataGridViewContentAlignment align)
        {
            var col = new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                Width = width,
                AutoSizeMode = fill ? DataGridViewAutoSizeColumnMode.Fill : DataGridViewAutoSizeColumnMode.None,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                ReadOnly = true
            };
            col.DefaultCellStyle.Alignment = align;
            grid.Columns.Add(col);
        }

        private void LoadData()
        {
            _items = _db.GetAll(_searchTerm);

            grid.SuspendLayout();
            grid.Rows.Clear();

            foreach (var item in _items)
            {
                int rowIdx = grid.Rows.Add(
                    item.Id,
                    item.ProductName,
                    item.ProductType,
                    item.QuantityOnHand,
                    item.PricePerItem.ToString("C2"),
                    item.TotalValue.ToString("C2")
                );

                var qtyCell = grid.Rows[rowIdx].Cells[3];
                if (item.QuantityOnHand == 0) qtyCell.Style.ForeColor = Red;
                else if (item.QuantityOnHand <= 5) qtyCell.Style.ForeColor = Orange;
                else qtyCell.Style.ForeColor = Green;
            }

            grid.ResumeLayout();

            lblTotalItems.Text = _items.Count.ToString("N0");
            lblTotalWorth.Text = _items.Sum(i => i.TotalValue).ToString("C2");
        }

        private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _items.Count) return;

            string colName = grid.Columns[e.ColumnIndex].Name;
            if (colName != "colMinus" && colName != "colPlus") return;

            var item = _items[e.RowIndex];

            if (colName == "colMinus")
            {
                if (item.QuantityOnHand <= 0)
                {
                    MessageBox.Show("Quantity is already 0.", "Cannot Subtract",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                _db.UpdateQuantity(item.Id, item.QuantityOnHand - 1);
            }
            else
            {
                _db.UpdateQuantity(item.Id, item.QuantityOnHand + 1);
            }

            LoadData();
            if (e.RowIndex < grid.Rows.Count)
                grid.Rows[e.RowIndex].Selected = true;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var dlg = new ItemDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
            {
                _db.Add(dlg.Result);
                LoadData();
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e) => EditSelectedItem();

        private void EditSelectedItem()
        {
            InventoryItem? item = GetSelectedItem();
            if (item == null)
            {
                MessageBox.Show("Please select an item to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var dlg = new ItemDialog(item);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
            {
                _db.Update(dlg.Result);
                LoadData();
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            InventoryItem? item = GetSelectedItem();
            if (item == null)
            {
                MessageBox.Show("Please select an item to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show(
                    $"Delete \"{item.ProductName}\"? This cannot be undone.",
                    "Confirm Delete", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                _db.Delete(item.Id);
                LoadData();
            }
        }

        private InventoryItem? GetSelectedItem()
        {
            if (grid.SelectedRows.Count == 0) return null;
            int idx = grid.SelectedRows[0].Index;
            return (idx >= 0 && idx < _items.Count) ? _items[idx] : null;
        }

        private void MenuBackup_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Choose where to save the backup zip",
                UseDescriptionForTitle = true,
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string zipPath = Path.Combine(dlg.SelectedPath, $"inventory_backup_{timestamp}.zip");

                string tempDb = Path.GetTempFileName();
                File.Copy(_dbPath, tempDb, overwrite: true);

                using (var zip = System.IO.Compression.ZipFile.Open(zipPath, System.IO.Compression.ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(tempDb, "inventory.db");
                }

                File.Delete(tempDb);

                MessageBox.Show($"Backup saved to:\n{zipPath}", "Backup Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MenuExportCsv_Click(object? sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title = "Export Inventory as CSV",
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"inventory_export_{DateTime.Now:yyyy-MM-dd}",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var lines = new List<string>
                {
                    "ID,Product Name,Product Type,Quantity on Hand,Price Per Item,Total Value"
                };

                foreach (var item in _db.GetAll())
                {
                    lines.Add(string.Join(",",
                        item.Id,
                        CsvEscape(item.ProductName),
                        CsvEscape(item.ProductType),
                        item.QuantityOnHand,
                        item.PricePerItem.ToString("F2"),
                        item.TotalValue.ToString("F2")
                    ));
                }

                File.WriteAllLines(dlg.FileName, lines, System.Text.Encoding.UTF8);

                MessageBox.Show($"Exported {lines.Count - 1} items to:\n{dlg.FileName}",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string CsvEscape(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }

    internal class DarkMenuColors : ProfessionalColorTable
    {
        private static readonly Color BgDark = Color.FromArgb(22, 30, 36);
        private static readonly Color BgHover = Color.FromArgb(52, 152, 219);
        private static readonly Color BgDropdown = Color.FromArgb(36, 47, 56);
        private static readonly Color Border = Color.FromArgb(55, 71, 79);

        public override Color MenuItemSelected => BgHover;
        public override Color MenuItemBorder => BgHover;
        public override Color MenuBorder => Border;
        public override Color MenuItemSelectedGradientBegin => BgHover;
        public override Color MenuItemSelectedGradientEnd => BgHover;
        public override Color MenuItemPressedGradientBegin => BgDropdown;
        public override Color MenuItemPressedGradientEnd => BgDropdown;
        public override Color ToolStripDropDownBackground => BgDropdown;
        public override Color ImageMarginGradientBegin => BgDropdown;
        public override Color ImageMarginGradientMiddle => BgDropdown;
        public override Color ImageMarginGradientEnd => BgDropdown;
        public override Color MenuStripGradientBegin => BgDark;
        public override Color MenuStripGradientEnd => BgDark;
        public override Color SeparatorDark => Border;
        public override Color SeparatorLight => Border;
    }
}