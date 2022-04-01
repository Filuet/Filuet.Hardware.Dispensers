
namespace PoC
{
    partial class PoCForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dispensersListBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.addressesListBox = new System.Windows.Forms.ListBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lightOffButton = new System.Windows.Forms.Button();
            this.lightOnButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.unlockButton = new System.Windows.Forms.Button();
            this.retestButton = new System.Windows.Forms.Button();
            this.dispenseButton = new System.Windows.Forms.Button();
            this.protoTextBox = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.qtyNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.removeItemButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.dispenseListBox = new System.Windows.Forms.ListBox();
            this.addSkuButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.skuComboBox = new System.Windows.Forms.ComboBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.savePlanogramButton = new System.Windows.Forms.Button();
            this.planogramRichTextBox = new System.Windows.Forms.RichTextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.qtyNumericUpDown)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dispensersListBox
            // 
            this.dispensersListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dispensersListBox.FormattingEnabled = true;
            this.dispensersListBox.ItemHeight = 15;
            this.dispensersListBox.Location = new System.Drawing.Point(538, 79);
            this.dispensersListBox.Name = "dispensersListBox";
            this.dispensersListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.dispensersListBox.Size = new System.Drawing.Size(178, 64);
            this.dispensersListBox.TabIndex = 0;
            this.dispensersListBox.SelectedIndexChanged += new System.EventHandler(this.dispensersListBox_SelectedIndexChanged);
            this.dispensersListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.dispensersListBox_MouseDoubleClick);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(533, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Dispensers";
            // 
            // addressesListBox
            // 
            this.addressesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addressesListBox.FormattingEnabled = true;
            this.addressesListBox.ItemHeight = 15;
            this.addressesListBox.Location = new System.Drawing.Point(538, 149);
            this.addressesListBox.Name = "addressesListBox";
            this.addressesListBox.Size = new System.Drawing.Size(178, 274);
            this.addressesListBox.TabIndex = 3;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(730, 459);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lightOffButton);
            this.tabPage1.Controls.Add(this.lightOnButton);
            this.tabPage1.Controls.Add(this.resetButton);
            this.tabPage1.Controls.Add(this.unlockButton);
            this.tabPage1.Controls.Add(this.retestButton);
            this.tabPage1.Controls.Add(this.dispenseButton);
            this.tabPage1.Controls.Add(this.protoTextBox);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.addressesListBox);
            this.tabPage1.Controls.Add(this.dispensersListBox);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(722, 431);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Dispensing";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lightOffButton
            // 
            this.lightOffButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lightOffButton.Location = new System.Drawing.Point(630, 47);
            this.lightOffButton.Name = "lightOffButton";
            this.lightOffButton.Size = new System.Drawing.Size(70, 23);
            this.lightOffButton.TabIndex = 18;
            this.lightOffButton.Text = "Light Off";
            this.lightOffButton.UseVisualStyleBackColor = true;
            this.lightOffButton.Click += new System.EventHandler(this.lightOffButton_Click);
            // 
            // lightOnButton
            // 
            this.lightOnButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lightOnButton.Location = new System.Drawing.Point(556, 47);
            this.lightOnButton.Name = "lightOnButton";
            this.lightOnButton.Size = new System.Drawing.Size(70, 23);
            this.lightOnButton.TabIndex = 17;
            this.lightOnButton.Text = "Light On";
            this.lightOnButton.UseVisualStyleBackColor = true;
            this.lightOnButton.Click += new System.EventHandler(this.lightOnButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.resetButton.Location = new System.Drawing.Point(662, 21);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(58, 23);
            this.resetButton.TabIndex = 16;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // unlockButton
            // 
            this.unlockButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.unlockButton.Location = new System.Drawing.Point(600, 21);
            this.unlockButton.Name = "unlockButton";
            this.unlockButton.Size = new System.Drawing.Size(58, 23);
            this.unlockButton.TabIndex = 15;
            this.unlockButton.Text = "Unlock";
            this.unlockButton.UseVisualStyleBackColor = true;
            this.unlockButton.Click += new System.EventHandler(this.unlockButton_Click);
            // 
            // retestButton
            // 
            this.retestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.retestButton.Enabled = false;
            this.retestButton.Location = new System.Drawing.Point(538, 21);
            this.retestButton.Name = "retestButton";
            this.retestButton.Size = new System.Drawing.Size(58, 23);
            this.retestButton.TabIndex = 14;
            this.retestButton.Text = "Retest";
            this.retestButton.UseVisualStyleBackColor = true;
            this.retestButton.Click += new System.EventHandler(this.retestButton_Click);
            // 
            // dispenseButton
            // 
            this.dispenseButton.Location = new System.Drawing.Point(3, 111);
            this.dispenseButton.Name = "dispenseButton";
            this.dispenseButton.Size = new System.Drawing.Size(75, 23);
            this.dispenseButton.TabIndex = 12;
            this.dispenseButton.Text = "Dispense";
            this.dispenseButton.UseVisualStyleBackColor = true;
            this.dispenseButton.Click += new System.EventHandler(this.dispenseButton_Click);
            // 
            // protoTextBox
            // 
            this.protoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.protoTextBox.Location = new System.Drawing.Point(3, 140);
            this.protoTextBox.Name = "protoTextBox";
            this.protoTextBox.Size = new System.Drawing.Size(529, 284);
            this.protoTextBox.TabIndex = 13;
            this.protoTextBox.Text = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.qtyNumericUpDown);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.removeItemButton);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.dispenseListBox);
            this.groupBox1.Controls.Add(this.addSkuButton);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.skuComboBox);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(529, 102);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Goods list";
            // 
            // qtyNumericUpDown
            // 
            this.qtyNumericUpDown.Location = new System.Drawing.Point(80, 46);
            this.qtyNumericUpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.qtyNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.qtyNumericUpDown.Name = "qtyNumericUpDown";
            this.qtyNumericUpDown.Size = new System.Drawing.Size(102, 23);
            this.qtyNumericUpDown.TabIndex = 11;
            this.qtyNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 15);
            this.label4.TabIndex = 10;
            this.label4.Text = "Choose qty";
            // 
            // removeItemButton
            // 
            this.removeItemButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.removeItemButton.Location = new System.Drawing.Point(435, 14);
            this.removeItemButton.Name = "removeItemButton";
            this.removeItemButton.Size = new System.Drawing.Size(60, 23);
            this.removeItemButton.TabIndex = 9;
            this.removeItemButton.Text = "Remove";
            this.removeItemButton.UseVisualStyleBackColor = true;
            this.removeItemButton.Click += new System.EventHandler(this.removeItemButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(204, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "Dispense list";
            // 
            // dispenseListBox
            // 
            this.dispenseListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dispenseListBox.FormattingEnabled = true;
            this.dispenseListBox.ItemHeight = 15;
            this.dispenseListBox.Location = new System.Drawing.Point(282, 15);
            this.dispenseListBox.Name = "dispenseListBox";
            this.dispenseListBox.Size = new System.Drawing.Size(149, 79);
            this.dispenseListBox.TabIndex = 6;
            this.dispenseListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.dispenseListBox_MouseDoubleClick);
            // 
            // addSkuButton
            // 
            this.addSkuButton.Location = new System.Drawing.Point(135, 75);
            this.addSkuButton.Name = "addSkuButton";
            this.addSkuButton.Size = new System.Drawing.Size(47, 23);
            this.addSkuButton.TabIndex = 7;
            this.addSkuButton.Text = "Add";
            this.addSkuButton.UseVisualStyleBackColor = true;
            this.addSkuButton.Click += new System.EventHandler(this.addSkuButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Choose sku";
            // 
            // skuComboBox
            // 
            this.skuComboBox.FormattingEnabled = true;
            this.skuComboBox.Location = new System.Drawing.Point(80, 15);
            this.skuComboBox.Name = "skuComboBox";
            this.skuComboBox.Size = new System.Drawing.Size(102, 23);
            this.skuComboBox.TabIndex = 4;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.savePlanogramButton);
            this.tabPage2.Controls.Add(this.planogramRichTextBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(722, 431);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Planogram";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // savePlanogramButton
            // 
            this.savePlanogramButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.savePlanogramButton.Location = new System.Drawing.Point(644, 405);
            this.savePlanogramButton.Name = "savePlanogramButton";
            this.savePlanogramButton.Size = new System.Drawing.Size(75, 23);
            this.savePlanogramButton.TabIndex = 1;
            this.savePlanogramButton.Text = "Save";
            this.savePlanogramButton.UseVisualStyleBackColor = true;
            this.savePlanogramButton.Click += new System.EventHandler(this.savePlanogramButton_Click);
            // 
            // planogramRichTextBox
            // 
            this.planogramRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.planogramRichTextBox.Location = new System.Drawing.Point(3, 3);
            this.planogramRichTextBox.Name = "planogramRichTextBox";
            this.planogramRichTextBox.Size = new System.Drawing.Size(716, 396);
            this.planogramRichTextBox.TabIndex = 0;
            this.planogramRichTextBox.Text = "";
            // 
            // PoCForm
            // 
            this.AcceptButton = this.dispenseButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(730, 459);
            this.Controls.Add(this.tabControl1);
            this.Name = "PoCForm";
            this.Text = "Dispensing PoC";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.qtyNumericUpDown)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox dispensersListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox addressesListBox;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button dispenseButton;
        private System.Windows.Forms.RichTextBox protoTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown qtyNumericUpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button removeItemButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox dispenseListBox;
        private System.Windows.Forms.Button addSkuButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox skuComboBox;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button savePlanogramButton;
        private System.Windows.Forms.RichTextBox planogramRichTextBox;
        private System.Windows.Forms.Button retestButton;
        private System.Windows.Forms.Button unlockButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button lightOffButton;
        private System.Windows.Forms.Button lightOnButton;
    }
}