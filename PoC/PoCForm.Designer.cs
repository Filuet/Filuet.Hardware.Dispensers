
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
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            dispensersListBox = new System.Windows.Forms.ListBox();
            label1 = new System.Windows.Forms.Label();
            addressesListBox = new System.Windows.Forms.ListBox();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            tabControl2 = new System.Windows.Forms.TabControl();
            tabPage3 = new System.Windows.Forms.TabPage();
            protoTextBox = new System.Windows.Forms.RichTextBox();
            tabPage4 = new System.Windows.Forms.TabPage();
            telemetryTextBox = new System.Windows.Forms.RichTextBox();
            lightOffButton = new System.Windows.Forms.Button();
            lightOnButton = new System.Windows.Forms.Button();
            resetButton = new System.Windows.Forms.Button();
            unlockButton = new System.Windows.Forms.Button();
            retestButton = new System.Windows.Forms.Button();
            dispenseButton = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            qtyNumericUpDown = new System.Windows.Forms.NumericUpDown();
            label4 = new System.Windows.Forms.Label();
            removeItemButton = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            dispenseListBox = new System.Windows.Forms.ListBox();
            addSkuButton = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            skuComboBox = new System.Windows.Forms.ComboBox();
            tabPage2 = new System.Windows.Forms.TabPage();
            planogramTreeView = new System.Windows.Forms.TreeView();
            savePlanogramButton = new System.Windows.Forms.Button();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            pingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            activateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabControl2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)qtyNumericUpDown).BeginInit();
            tabPage2.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // dispensersListBox
            // 
            dispensersListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            dispensersListBox.FormattingEnabled = true;
            dispensersListBox.ItemHeight = 15;
            dispensersListBox.Location = new System.Drawing.Point(534, 124);
            dispensersListBox.Name = "dispensersListBox";
            dispensersListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            dispensersListBox.Size = new System.Drawing.Size(178, 64);
            dispensersListBox.TabIndex = 0;
            dispensersListBox.SelectedIndexChanged += dispensersListBox_SelectedIndexChanged;
            dispensersListBox.MouseDoubleClick += dispensersListBox_MouseDoubleClick;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(529, 3);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(63, 15);
            label1.TabIndex = 1;
            label1.Text = "Dispensers";
            // 
            // addressesListBox
            // 
            addressesListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            addressesListBox.FormattingEnabled = true;
            addressesListBox.ItemHeight = 15;
            addressesListBox.Location = new System.Drawing.Point(534, 194);
            addressesListBox.Name = "addressesListBox";
            addressesListBox.Size = new System.Drawing.Size(178, 229);
            addressesListBox.TabIndex = 3;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(800, 462);
            tabControl1.TabIndex = 5;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(tabControl2);
            tabPage1.Controls.Add(lightOffButton);
            tabPage1.Controls.Add(lightOnButton);
            tabPage1.Controls.Add(resetButton);
            tabPage1.Controls.Add(unlockButton);
            tabPage1.Controls.Add(retestButton);
            tabPage1.Controls.Add(dispenseButton);
            tabPage1.Controls.Add(groupBox1);
            tabPage1.Controls.Add(addressesListBox);
            tabPage1.Controls.Add(dispensersListBox);
            tabPage1.Controls.Add(label1);
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(3);
            tabPage1.Size = new System.Drawing.Size(792, 434);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Dispensing";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabControl2
            // 
            tabControl2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl2.Controls.Add(tabPage3);
            tabControl2.Controls.Add(tabPage4);
            tabControl2.Location = new System.Drawing.Point(3, 140);
            tabControl2.Name = "tabControl2";
            tabControl2.SelectedIndex = 0;
            tabControl2.Size = new System.Drawing.Size(525, 285);
            tabControl2.TabIndex = 19;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(protoTextBox);
            tabPage3.Location = new System.Drawing.Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new System.Windows.Forms.Padding(3);
            tabPage3.Size = new System.Drawing.Size(517, 257);
            tabPage3.TabIndex = 0;
            tabPage3.Text = "Events";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // protoTextBox
            // 
            protoTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            protoTextBox.Location = new System.Drawing.Point(3, 3);
            protoTextBox.Name = "protoTextBox";
            protoTextBox.Size = new System.Drawing.Size(511, 251);
            protoTextBox.TabIndex = 13;
            protoTextBox.Text = "";
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(telemetryTextBox);
            tabPage4.Location = new System.Drawing.Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new System.Windows.Forms.Padding(3);
            tabPage4.Size = new System.Drawing.Size(517, 257);
            tabPage4.TabIndex = 1;
            tabPage4.Text = "Telemetry";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // telemetryTextBox
            // 
            telemetryTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            telemetryTextBox.Location = new System.Drawing.Point(3, 3);
            telemetryTextBox.Name = "telemetryTextBox";
            telemetryTextBox.Size = new System.Drawing.Size(511, 251);
            telemetryTextBox.TabIndex = 0;
            telemetryTextBox.Text = "...";
            // 
            // lightOffButton
            // 
            lightOffButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            lightOffButton.Location = new System.Drawing.Point(624, 95);
            lightOffButton.Name = "lightOffButton";
            lightOffButton.Size = new System.Drawing.Size(70, 23);
            lightOffButton.TabIndex = 18;
            lightOffButton.Text = "Light Off";
            lightOffButton.UseVisualStyleBackColor = true;
            lightOffButton.Click += lightOffButton_Click;
            // 
            // lightOnButton
            // 
            lightOnButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            lightOnButton.Location = new System.Drawing.Point(553, 95);
            lightOnButton.Name = "lightOnButton";
            lightOnButton.Size = new System.Drawing.Size(70, 23);
            lightOnButton.TabIndex = 17;
            lightOnButton.Text = "Light On";
            lightOnButton.UseVisualStyleBackColor = true;
            lightOnButton.Click += lightOnButton_Click;
            // 
            // resetButton
            // 
            resetButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            resetButton.Location = new System.Drawing.Point(624, 66);
            resetButton.Name = "resetButton";
            resetButton.Size = new System.Drawing.Size(70, 23);
            resetButton.TabIndex = 16;
            resetButton.Text = "Reset";
            resetButton.UseVisualStyleBackColor = true;
            resetButton.Click += resetButton_Click;
            // 
            // unlockButton
            // 
            unlockButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            unlockButton.Location = new System.Drawing.Point(553, 66);
            unlockButton.Name = "unlockButton";
            unlockButton.Size = new System.Drawing.Size(70, 23);
            unlockButton.TabIndex = 15;
            unlockButton.Text = "Unlock";
            unlockButton.UseVisualStyleBackColor = true;
            unlockButton.Click += unlockButton_Click;
            // 
            // retestButton
            // 
            retestButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            retestButton.Enabled = false;
            retestButton.Location = new System.Drawing.Point(553, 22);
            retestButton.Name = "retestButton";
            retestButton.Size = new System.Drawing.Size(141, 23);
            retestButton.TabIndex = 14;
            retestButton.Text = "Retest All";
            retestButton.UseVisualStyleBackColor = true;
            retestButton.Click += retestButton_Click;
            // 
            // dispenseButton
            // 
            dispenseButton.Location = new System.Drawing.Point(3, 111);
            dispenseButton.Name = "dispenseButton";
            dispenseButton.Size = new System.Drawing.Size(75, 23);
            dispenseButton.TabIndex = 12;
            dispenseButton.Text = "Dispense";
            dispenseButton.UseVisualStyleBackColor = true;
            dispenseButton.Click += dispenseButton_Click;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBox1.Controls.Add(qtyNumericUpDown);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(removeItemButton);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(dispenseListBox);
            groupBox1.Controls.Add(addSkuButton);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(skuComboBox);
            groupBox1.Location = new System.Drawing.Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(525, 102);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Goods list";
            // 
            // qtyNumericUpDown
            // 
            qtyNumericUpDown.Location = new System.Drawing.Point(80, 46);
            qtyNumericUpDown.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
            qtyNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            qtyNumericUpDown.Name = "qtyNumericUpDown";
            qtyNumericUpDown.Size = new System.Drawing.Size(102, 23);
            qtyNumericUpDown.TabIndex = 11;
            qtyNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(10, 48);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(67, 15);
            label4.TabIndex = 10;
            label4.Text = "Choose qty";
            // 
            // removeItemButton
            // 
            removeItemButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            removeItemButton.Location = new System.Drawing.Point(431, 14);
            removeItemButton.Name = "removeItemButton";
            removeItemButton.Size = new System.Drawing.Size(60, 23);
            removeItemButton.TabIndex = 9;
            removeItemButton.Text = "Remove";
            removeItemButton.UseVisualStyleBackColor = true;
            removeItemButton.Click += removeItemButton_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(204, 18);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(72, 15);
            label3.TabIndex = 8;
            label3.Text = "Dispense list";
            // 
            // dispenseListBox
            // 
            dispenseListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dispenseListBox.FormattingEnabled = true;
            dispenseListBox.ItemHeight = 15;
            dispenseListBox.Location = new System.Drawing.Point(282, 15);
            dispenseListBox.Name = "dispenseListBox";
            dispenseListBox.Size = new System.Drawing.Size(145, 79);
            dispenseListBox.TabIndex = 6;
            dispenseListBox.MouseDoubleClick += dispenseListBox_MouseDoubleClick;
            // 
            // addSkuButton
            // 
            addSkuButton.Location = new System.Drawing.Point(135, 75);
            addSkuButton.Name = "addSkuButton";
            addSkuButton.Size = new System.Drawing.Size(47, 23);
            addSkuButton.TabIndex = 7;
            addSkuButton.Text = "Add";
            addSkuButton.UseVisualStyleBackColor = true;
            addSkuButton.Click += addSkuButton_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(10, 18);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(68, 15);
            label2.TabIndex = 6;
            label2.Text = "Choose sku";
            // 
            // skuComboBox
            // 
            skuComboBox.FormattingEnabled = true;
            skuComboBox.Location = new System.Drawing.Point(80, 15);
            skuComboBox.Name = "skuComboBox";
            skuComboBox.Size = new System.Drawing.Size(102, 23);
            skuComboBox.TabIndex = 4;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(planogramTreeView);
            tabPage2.Controls.Add(savePlanogramButton);
            tabPage2.Location = new System.Drawing.Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new System.Windows.Forms.Padding(3);
            tabPage2.Size = new System.Drawing.Size(792, 434);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Planogram";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // planogramTreeView
            // 
            planogramTreeView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            planogramTreeView.Location = new System.Drawing.Point(3, 3);
            planogramTreeView.Name = "planogramTreeView";
            planogramTreeView.Size = new System.Drawing.Size(786, 397);
            planogramTreeView.TabIndex = 2;
            planogramTreeView.MouseClick += planogramTreeView_MouseClick;
            // 
            // savePlanogramButton
            // 
            savePlanogramButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            savePlanogramButton.Location = new System.Drawing.Point(714, 405);
            savePlanogramButton.Name = "savePlanogramButton";
            savePlanogramButton.Size = new System.Drawing.Size(75, 23);
            savePlanogramButton.TabIndex = 1;
            savePlanogramButton.Text = "Save";
            savePlanogramButton.UseVisualStyleBackColor = true;
            savePlanogramButton.Click += savePlanogramButton_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { pingToolStripMenuItem, activateToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(118, 48);
            // 
            // pingToolStripMenuItem
            // 
            pingToolStripMenuItem.Name = "pingToolStripMenuItem";
            pingToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            pingToolStripMenuItem.Text = "Ping";
            pingToolStripMenuItem.Click += pingToolStripMenuItem_Click;
            // 
            // activateToolStripMenuItem
            // 
            activateToolStripMenuItem.Name = "activateToolStripMenuItem";
            activateToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            activateToolStripMenuItem.Text = "Activate";
            activateToolStripMenuItem.Click += activateToolStripMenuItem_Click;
            // 
            // PoCForm
            // 
            AcceptButton = dispenseButton;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 462);
            Controls.Add(tabControl1);
            Name = "PoCForm";
            Text = "Dispensing PoC";
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabControl2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)qtyNumericUpDown).EndInit();
            tabPage2.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
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
        private System.Windows.Forms.Button retestButton;
        private System.Windows.Forms.Button unlockButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button lightOffButton;
        private System.Windows.Forms.Button lightOnButton;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.RichTextBox telemetryTextBox;
        private System.Windows.Forms.TreeView planogramTreeView;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem pingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem activateToolStripMenuItem;
    }
}