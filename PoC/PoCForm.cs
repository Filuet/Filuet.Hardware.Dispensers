using Filuet.Hardware.Dispensers.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

        public void Initialize(ICompositeDispenser dispenser)
        {
            _dispenser = dispenser;
        }

        private ICompositeDispenser _dispenser;
    }
}
