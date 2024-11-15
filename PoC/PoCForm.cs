using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core;
using PoC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading;
using System.Drawing;
using Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus;

namespace PoC
{
    public partial class PoCForm : Form
    {
        private IEnumerable<ItemToDispense> ToDispense
        {
            get
            {
                foreach (ItemToDispense i in dispenseListBox.Items)
                    yield return i;
            }
        }

        public PoCForm() {
            InitializeComponent();

            planogramTreeView.NodeMouseClick += (sender, args) => planogramTreeView.SelectedNode = args.Node;
        }

        public void Initialize(Pog planogram, IVendingMachine dispenser) {
            _planogram = planogram;
            _dispenser = dispenser;

            _dispenser.onTest += (sender, e) => {
                MachineIdIsAvailable[e.Dispenser.Id] = e.Severity == Filuet.Hardware.Dispensers.Abstractions.Enums.DispenserStateSeverity.Normal;

                Invoke(new MethodInvoker(delegate ()
                {
                    dispensersListBox.Items.Clear();

                    foreach (var d in _factDispenser._dispensers) {
                        if (MachineIdIsAvailable.ContainsKey(d.Id))
                            dispensersListBox.Items.Add(d);

                        if (d.Id == e.Dispenser.Id)
                            Log(MachineIdIsAvailable[d.Id] ? LogLevel.Information : LogLevel.Warning, $"Machine {e.Dispenser.Id} status: {e.Message}");
                    }
                }));
            };

            _dispenser.onDataMoving += (sender, e) => {
                Invoke(new MethodInvoker(delegate ()
                {
                    telemetryTextBox.Text = $"{DateTime.Now:HH:mm:ss.fff} {(e.direction ? "→" : "←")} {e.data} [{e.message}] {Environment.NewLine}{telemetryTextBox.Text}";
                }));
            };

            skuComboBox.Items.AddRange(_planogram.Products.ToArray());
            if (skuComboBox.Items.Count > 0)
                skuComboBox.SelectedIndex = 0;

            _factDispenser = (VendingMachine)_dispenser;

            PopulatePlanogramTreeView();

            Task.Factory.StartNew(() => {
                Thread.Sleep(5000);
                Invoke(new MethodInvoker(delegate ()
                {
                    retestButton.Enabled = true;
                }));
            });
        }

        private void PopulatePlanogramTreeView() {
            planogramTreeView.Nodes.Clear();

            foreach (var p in _planogram.Products) {
                TreeNode product = new TreeNode(p.Product) { Tag = p };
                product.NodeFont = new Font("Segoe UI", 8, FontStyle.Bold);
                planogramTreeView.Nodes.Add(product);
                foreach (var a in p.Routes) {
                    TreeNode node = new TreeNode($"{a.Address} ({a.Quantity}/{a.MaxQuantity}{(a.MockedQuantity.HasValue && a.MockedQuantity.Value != a.Quantity ? ", real qty: " + a.MockedQuantity.Value : "")})".Trim()) { Tag = a };

                    if (!a.Active.HasValue) {
                        if (a.MockedActive.HasValue) {
                            node.ImageIndex = a.MockedActive.Value ? 8 : 9;
                        }
                        else node.ImageIndex = 3;
                    }
                    else {
                        if (a.Active.Value) {
                            if (a.MockedActive.HasValue) {
                                node.ImageIndex = a.MockedActive.Value ? 1 : 4;
                            }
                            else node.ImageIndex = 6;
                        }
                        else {
                            if (a.MockedActive.HasValue) {
                                node.ImageIndex = a.MockedActive.Value ? 5 : 2;
                            }
                            else node.ImageIndex = 7;
                        }
                    }

                    product.Nodes.Add(node);
                }
            }

            planogramTreeView.ExpandAll();
        }

        private void _dispenser_onPlanogramClarification(object sender, PlanogramEventArgs e) {
            throw new NotImplementedException();
        }

        private void dispenseButton_Click(object sender, EventArgs e) {
            if (ToDispense == null || !ToDispense.Any()) {
                MessageBox.Show("Add some products first", "No products provided", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _dispenser.DispenseAsync(new Cart(ToDispense.Select(x => new CartItem { ProductUid = x.Product.Product, Quantity = x.Qty })));
        }

        private void addSkuButton_Click(object sender, EventArgs e) {
            PogProduct product = skuComboBox.SelectedItem as PogProduct;
            ushort qty = (ushort)qtyNumericUpDown.Value;

            bool alreadyExists = false;

            for (int i = 0; i < dispenseListBox.Items.Count; i++) {
                ItemToDispense i2d = dispenseListBox.Items[i] as ItemToDispense;
                if (i2d.Product.Product == product.Product) {
                    i2d.Qty += qty;
                    alreadyExists = true;
                    dispenseListBox.Items.RemoveAt(i);
                    dispenseListBox.Items.Add(new ItemToDispense { Product = i2d.Product, Qty = i2d.Qty });
                }

                //if (i.Product.ProductUid == product.ProductUid)
                //{
                //    uint newQty = i.Qty + qty;
                //    alreadyExists = true;
                //    dispenseListBox.Items.Remove(i);
                //    dispenseListBox.Items.Add(new ItemToDispense { Product = i.Product, Qty = newQty });
                //}
            }

            if (!alreadyExists)
                dispenseListBox.Items.Add(new ItemToDispense { Product = product, Qty = qty });
        }

        private void removeItemButton_Click(object sender, EventArgs e) {
            if (dispenseListBox.Items.Count == 1)
                dispenseListBox.Items.Clear();

            if (dispenseListBox.Items.Count > 0 && dispenseListBox.SelectedIndex >= 0)
                dispenseListBox.Items.RemoveAt(dispenseListBox.SelectedIndex);
        }

        private void dispensersListBox_SelectedIndexChanged(object sender, EventArgs e) {
            addressesListBox.Items.Clear();
            IDispenser selectedDispenser = dispensersListBox.SelectedItem as IDispenser;

            Func<string, bool> isValidAddress = x => {
                if (selectedDispenser is VisionEsPlusWrapper)
                    return x.StartsWith(selectedDispenser.Id + "/");

                return false;
            };

            if (selectedDispenser != null) {
                foreach (var p in Planogram.Products)
                    foreach (var r in p.Routes)
                        if (isValidAddress(r.Address))
                            addressesListBox.Items.Add($"{r} of {_planogram.GetProduct(r.Address).Product}");
            }
        }

        private void savePlanogramButton_Click(object sender, EventArgs e) {
            //if (planogramRichTextBox.Text.IsValidJson())
            //{
            //    File.WriteAllText("test_planogram.json", planogramRichTextBox.Text);
            //}
        }

        public void Log(LogLevel level, string message) {
            Invoke(new MethodInvoker(delegate ()
            {
                Color color = Color.Black;
                if (level == LogLevel.Warning)
                    color = Color.Orange;
                else if (level == LogLevel.Error)
                    color = Color.Red;
                else if (level == LogLevel.Verbose)
                    color = Color.Purple;

                protoTextBox.AppendText($"{DateTime.Now:HH:mm:ss.fff}\t");
                protoTextBox.AppendText(message + Environment.NewLine, color);
            }));
        }

        private void retestButton_Click(object sender, EventArgs e) {
            retestButton.Enabled = false;
            _dispenser.TestAsync().ContinueWith(x => Invoke(new MethodInvoker(delegate ()
            {
                retestButton.Enabled = true;
            })));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispenseListBox_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (dispenseListBox.SelectedItem != null) {
                ItemToDispense toD = (ItemToDispense)dispenseListBox.SelectedItem;
                MessageBox.Show(System.Text.Json.JsonSerializer.Serialize(toD.Product.Routes), $"Routes with {toD.Product.Product}");
            }
        }

        internal Pog Planogram
        {
            get => _planogram;
            set
            {
                _planogram = value;

                Invoke(new MethodInvoker(delegate ()
                {
                    PopulatePlanogramTreeView();
                }));
            }
        }

        private void dispensersListBox_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (MessageBox.Show("Do you really want to reset the machine?", "Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes) {
                IDispenser d = dispensersListBox.SelectedItem as IDispenser;
                if (d != null)
                    d.Reset();
            }
        }

        private void unlockButton_Click(object sender, EventArgs e) {
            foreach (var x in dispensersListBox.SelectedItems) {
                IDispenser d = (IDispenser)x;
                d.Unlock();
                protoTextBox.AppendText($"{DateTime.Now:HH:mm:ss.fff}\t");
                protoTextBox.AppendText($"Unlock Machine {d.Id}{Environment.NewLine}", Color.Green);
            }
        }

        private void resetButton_Click(object sender, EventArgs e) {
            foreach (var x in dispensersListBox.SelectedItems) {
                IDispenser d = (IDispenser)x;
                d.Reset();
                protoTextBox.AppendText($"{DateTime.Now:HH:mm:ss.fff}\t");
                protoTextBox.AppendText($"Reset Machine {d.Id}{Environment.NewLine}", Color.Green);
            }
        }

        private void lightOnButton_Click(object sender, EventArgs e) {
            foreach (var x in dispensersListBox.SelectedItems) {
                IDispenser d = (IDispenser)x;
                ILightEmitter lem = _factDispenser._lightEmitters.FirstOrDefault(x => x.Id == d.Id);
                if (lem != null)
                    lem.LightOn();
            }
        }

        private void lightOffButton_Click(object sender, EventArgs e) {
            foreach (var x in dispensersListBox.SelectedItems) {
                IDispenser d = (IDispenser)x;
                ILightEmitter lem = _factDispenser._lightEmitters.FirstOrDefault(x => x.Id == d.Id);
                if (lem != null)
                    lem.LightOff();
            }
        }

        private void planogramTreeView_MouseClick(object sender, MouseEventArgs e) {
            PogRoute route = SelectedRoute;
            if (route != null && e.Button == MouseButtons.Right) {
                if (!route.Active.HasValue) {
                    activateToolStripMenuItem.Enabled = true;
                    deactivateToolStripMenuItem.Enabled = true;
                }
                else {
                    activateToolStripMenuItem.Enabled = !route.Active.Value;
                    deactivateToolStripMenuItem.Enabled = route.Active.Value;
                }
                contextMenuStrip1.Show(MousePosition);
            }
        }

        private PogRoute SelectedRoute
        {
            get
            {
                if (planogramTreeView.SelectedNode != null) {
                    object tag = planogramTreeView.SelectedNode.Tag;

                    if (tag != null && tag is PogRoute)
                        return (PogRoute)tag;
                }

                return null;
            }
        }

        private void pingToolStripMenuItem_Click(object sender, EventArgs e) {
            foreach (var d in _factDispenser._dispensers) {
                IEnumerable<(string address, bool? isActive)> result = d.Ping(SelectedRoute.Address);
                foreach (var r in result)
                    if (r.isActive.HasValue) {
                        _planogram.GetRoute(r.address).Active = r.isActive.Value;
                        PopulatePlanogramTreeView();
                        //PogRoute a = SelectedRoute;
                        //planogramTreeView.SelectedNode.Text = $"{a.Address} ({a.Quantity}/{a.MaxQuantity}{(a.MockedQuantity.HasValue && a.MockedQuantity.Value != a.Quantity ? ", real qty: " + a.MockedQuantity.Value : "")})".Trim();
                    }
            }
        }

        private void activateToolStripMenuItem_Click(object sender, EventArgs e) {
            PogRoute route = SelectedRoute;
            _planogram.GetRoute(route.Address).Active = true;
            _planogram.Write(PLANOGRAM_FILE);
            PopulatePlanogramTreeView();
        }

        private void deactivateToolStripMenuItem_Click(object sender, EventArgs e) {
            PogRoute route = SelectedRoute;
            _planogram.GetRoute(route.Address).Active = false;
            _planogram.Write(PLANOGRAM_FILE);
            PopulatePlanogramTreeView();
        }

        const string PLANOGRAM_FILE = "C:/Filuet/Dispensing/test_planogram.json";
        private Dictionary<int, bool> MachineIdIsAvailable = new Dictionary<int, bool>();
        private IVendingMachine _dispenser;
        private Pog _planogram;
        private VendingMachine _factDispenser;
    }
}