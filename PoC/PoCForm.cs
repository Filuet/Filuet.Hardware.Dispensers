using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core;
using PoC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Filuet.Infrastructure.Abstractions.Helpers;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading;

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

        public PoCForm()
        {
            InitializeComponent();
        }

        public void Initialize(PoG planogram, ICompositeDispenser dispenser)
        {
            _planogram = planogram;
            _dispenser = dispenser;

            _dispenser.onTest += (sender, e) =>
            {
                MachineIdIsAvailable[e.Dispenser.Id] = e.Severity == Filuet.Hardware.Dispensers.Abstractions.Enums.DispenserStateSeverity.Normal;

                Invoke(new MethodInvoker(delegate ()
                {
                    dispensersListBox.Items.Clear();

                    foreach (var d in _factDispenser._dispensers)
                    {
                        if (MachineIdIsAvailable.ContainsKey(d.Id))
                            dispensersListBox.Items.Add(d);

                        if (d.Id == e.Dispenser.Id)
                            Log(MachineIdIsAvailable[d.Id] ? LogLevel.Information : LogLevel.Warning, $"Dispenser №{e.Dispenser.Id} {e.Message}");
                    }
                }));

                //if (e.Severity == Filuet.Hardware.Dispensers.Abstractions.Enums.DispenserStateSeverity.Inoperable)
                //{
                //    IDispenser inoperable = _factDispenser._dispensers.FirstOrDefault(x => x.Id == e.Dispenser.Id);
                //    if (inoperable != null)
                //        inoperable.Reset();
                //}
            };

            skuComboBox.Items.AddRange(_planogram.Products.ToArray());
            skuComboBox.SelectedIndex = 0;

            _factDispenser = (CompositeDispenser)_dispenser;
            //foreach (var d in _factDispenser._dispensers)
            //{
            //    MachineIdIsAvailable[d.Id] = false;
            //    dispensersListBox.Items.Add(d);
            //}

            //if (dispensersListBox.Items.Count == 1)
            //    dispensersListBox.SelectedIndex = 0;

            planogramRichTextBox.Text = planogram.ToString();

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);
                Invoke(new MethodInvoker(delegate ()
                {
                    retestButton.Enabled = true;
                }));
            });
        }

        private void dispenseButton_Click(object sender, EventArgs e)
        {
            _dispenser.Dispense(ToDispense.Select(x => (x.Product.ProductUid, x.Qty)).ToArray());
        }

        private void addSkuButton_Click(object sender, EventArgs e)
        {
            PoGProduct product = skuComboBox.SelectedItem as PoGProduct;
            ushort qty = (ushort)qtyNumericUpDown.Value;

            bool alreadyExists = false;

            for (int i = 0; i < dispenseListBox.Items.Count; i++)
            {
                ItemToDispense i2d = dispenseListBox.Items[i] as ItemToDispense;
                if (i2d.Product.ProductUid == product.ProductUid)
                {
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

        private void removeItemButton_Click(object sender, EventArgs e)
        {
            if (dispenseListBox.Items.Count > 0 && dispenseListBox.SelectedIndex >= 0)
                dispenseListBox.Items.RemoveAt(dispenseListBox.SelectedIndex);
        }

        private void dispensersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            addressesListBox.Items.Clear();
            IDispenser selectedDispenser = dispensersListBox.SelectedItem as IDispenser;

            IEnumerable<PoGRoute> routes =
                _factDispenser._planogram.GetRoutes(selectedDispenser.Id);

            foreach (var r in routes)
                addressesListBox.Items.Add($"{r} of {_planogram.GetProduct(r.Address).ProductUid}");
        }

        private void savePlanogramButton_Click(object sender, EventArgs e)
        {
            if (planogramRichTextBox.Text.IsValidJson())
            {
                File.WriteAllText("test_planogram.json", planogramRichTextBox.Text);
            }
        }

        public void Log(LogLevel level, string message)
        {
            Invoke(new MethodInvoker(delegate ()
            {
                protoTextBox.Text += $"{DateTime.Now:HH:mm:ss} {level} {message}{Environment.NewLine}";
            }));
        }

        private void retestButton_Click(object sender, EventArgs e)
        {
            retestButton.Enabled = false;
            _dispenser.Test().ContinueWith(x => Invoke(new MethodInvoker(delegate () {
                retestButton.Enabled = true;

            })));
        }

        /// <summary>
        /// Ping addresses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispenseListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (dispenseListBox.SelectedItem != null)
            {
                ItemToDispense toD = (ItemToDispense)dispenseListBox.SelectedItem;
                MessageBox.Show(System.Text.Json.JsonSerializer.Serialize(toD.Product.Routes));
            }
        }

        private Dictionary<uint, bool> MachineIdIsAvailable = new Dictionary<uint, bool>();

        private ICompositeDispenser _dispenser;
        private PoG _planogram;
        private CompositeDispenser _factDispenser;

        private void dispensersListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (MessageBox.Show("Do you really want to reset the machine?", "Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                IDispenser d = dispensersListBox.SelectedItem as IDispenser;
                if (d != null)
                    d.Reset();
            }
        }
    }
}