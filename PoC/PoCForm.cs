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

        public void Initialize(PoG planogram, IVendingMachine dispenser) {
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
                            Log(MachineIdIsAvailable[d.Id] ? LogLevel.Information : LogLevel.Warning, $"{e.Dispenser.Alias} status: {e.Message}");
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
                TreeNode product = new TreeNode(p.ProductUid) { Tag = p };
                planogramTreeView.Nodes.Add(product);
                foreach (var a in p.Routes) {
                    product.Nodes.Add(new TreeNode($"{a.Address} qty={a.Quantity} {(!a.Active.HasValue ? "unknown" : (a.Active.Value ? "active" : "inactive"))}") { Tag = a });
                }
            }
        }

        private void _dispenser_onPlanogramClarification(object sender, PlanogramEventArgs e) {
            throw new NotImplementedException();
        }

        private void dispenseButton_Click(object sender, EventArgs e) {
            if (ToDispense == null || !ToDispense.Any()) {
                MessageBox.Show("Add some products first", "No products provided", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _dispenser.Dispense(true, ToDispense.Select(x => (x.Product.ProductUid, x.Qty)).ToArray());
        }

        private void addSkuButton_Click(object sender, EventArgs e) {
            PoGProduct product = skuComboBox.SelectedItem as PoGProduct;
            ushort qty = (ushort)qtyNumericUpDown.Value;

            bool alreadyExists = false;

            for (int i = 0; i < dispenseListBox.Items.Count; i++) {
                ItemToDispense i2d = dispenseListBox.Items[i] as ItemToDispense;
                if (i2d.Product.ProductUid == product.ProductUid) {
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

            if (selectedDispenser != null) {
                IEnumerable<PoGRoute> routes =
                    _factDispenser._planogram.GetRoutes(selectedDispenser.Id);

                foreach (var r in routes)
                    addressesListBox.Items.Add($"{r} of {_planogram.GetProduct(r.Address).ProductUid}");
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
            _dispenser.Test().ContinueWith(x => Invoke(new MethodInvoker(delegate ()
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
                MessageBox.Show(System.Text.Json.JsonSerializer.Serialize(toD.Product.Routes), $"Routes with {toD.Product.ProductUid}");
            }
        }

        internal PoG Planogram
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
            }
        }

        private void resetButton_Click(object sender, EventArgs e) {
            foreach (var x in dispensersListBox.SelectedItems) {
                IDispenser d = (IDispenser)x;
                d.Reset();
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
            if (SelectedRoute != null && e.Button == MouseButtons.Right)
                contextMenuStrip1.Show(MousePosition);
        }

        private PoGRoute SelectedRoute
        {
            get
            {
                if (planogramTreeView.SelectedNode != null) {
                    object tag = planogramTreeView.SelectedNode.Tag;

                    if (tag != null && tag is PoGRoute)
                        return (PoGRoute)tag;
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
                        PoGRoute a = SelectedRoute;
                        planogramTreeView.SelectedNode.Text = $"{a.Address} qty={a.Quantity} {(!a.Active.HasValue ? "unknown" : (a.Active.Value ? "active" : "inactive"))}";
                    }
            }
        }

        private Dictionary<uint, bool> MachineIdIsAvailable = new Dictionary<uint, bool>();
        private IVendingMachine _dispenser;
        private PoG _planogram;
        private VendingMachine _factDispenser;

        private void activateToolStripMenuItem_Click(object sender, EventArgs e) {
            foreach (var d in _factDispenser._dispensers) {
                d.ActivateAsync(SelectedRoute.Address);
            }
        }
    }
}