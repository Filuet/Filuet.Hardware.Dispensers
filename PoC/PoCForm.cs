using Filuet.Hardware.Dispensers.Abstractions;
using Filuet.Hardware.Dispensers.Abstractions.Models;
using Filuet.Hardware.Dispensers.Core;
using PoC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoC
{
    public partial class PoCForm : Form
    {
        public PoCForm()
        {
            InitializeComponent();
        }

        public void Initialize(PoG planogram, ICompositeDispenser dispenser)
        {
            _planogram = planogram;
            _dispenser = dispenser;

            skuComboBox.Items.AddRange(_planogram.Products.ToArray());
            skuComboBox.SelectedIndex = 0;

            _factDispenser = (CompositeDispenser)_dispenser;
            foreach (var d in _factDispenser._dispensers)
                dispensersListBox.Items.Add(d);

            if (dispensersListBox.Items.Count == 1)
                dispensersListBox.SelectedIndex = 0;
        }

        private void dispenseButton_Click(object sender, EventArgs e)
        {
            _dispenser.Dispense(new (string productUid, ushort quantity)[] { ("0141", 1) });
        }

        private void addSkuButton_Click(object sender, EventArgs e)
        {
            PoGProduct product = skuComboBox.SelectedItem as PoGProduct;
            uint qty = (uint)qtyNumericUpDown.Value;

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

        private ICompositeDispenser _dispenser;
        private PoG _planogram;
        private CompositeDispenser _factDispenser;

        private void dispensersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            addressesListBox.Items.Clear();
            IDispenser selectedDispenser = dispensersListBox.SelectedItem as IDispenser;

            IEnumerable<PoGRoute> routes =
                _factDispenser._planogram.GetRoutes(selectedDispenser.Id);

            foreach (var r in routes)
                addressesListBox.Items.Add($"{r} of {_planogram.GetProduct(r.Address).ProductUid}");
        }
    }
}