namespace CodeWalker.Project.Panels
{
    partial class VertexColorPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MainPanel = new System.Windows.Forms.Panel();
            this.ColorGroupBox = new System.Windows.Forms.GroupBox();
            this.ColorPreviewPanel = new System.Windows.Forms.Panel();
            this.PickColorButton = new System.Windows.Forms.Button();
            this.ActionsGroupBox = new System.Windows.Forms.GroupBox();
            this.ApplyColorToAllButton = new System.Windows.Forms.Button();
            this.SaveYdrButton = new System.Windows.Forms.Button();
            this.AutoReloadCheckBox = new System.Windows.Forms.CheckBox();
            this.StatusLabel = new System.Windows.Forms.Label();
            
            this.MainPanel.SuspendLayout();
            this.ColorGroupBox.SuspendLayout();
            this.ActionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // MainPanel
            // 
            this.MainPanel.AutoScroll = true;
            this.MainPanel.Controls.Add(this.ColorGroupBox);
            this.MainPanel.Controls.Add(this.ActionsGroupBox);
            this.MainPanel.Controls.Add(this.StatusLabel);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Padding = new System.Windows.Forms.Padding(10);
            this.MainPanel.Size = new System.Drawing.Size(350, 400);
            this.MainPanel.TabIndex = 0;
            
            // 
            // ColorGroupBox
            // 
            this.ColorGroupBox.Controls.Add(this.PickColorButton);
            this.ColorGroupBox.Controls.Add(this.ColorPreviewPanel);
            this.ColorGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.ColorGroupBox.Location = new System.Drawing.Point(10, 10);
            this.ColorGroupBox.Name = "ColorGroupBox";
            this.ColorGroupBox.Padding = new System.Windows.Forms.Padding(10);
            this.ColorGroupBox.Size = new System.Drawing.Size(330, 150);
            this.ColorGroupBox.TabIndex = 0;
            this.ColorGroupBox.TabStop = false;
            this.ColorGroupBox.Text = "Select Color";
            
            // 
            // ColorPreviewPanel
            // 
            this.ColorPreviewPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ColorPreviewPanel.Location = new System.Drawing.Point(10, 25);
            this.ColorPreviewPanel.Name = "ColorPreviewPanel";
            this.ColorPreviewPanel.Size = new System.Drawing.Size(310, 50);
            this.ColorPreviewPanel.TabIndex = 0;
            this.ColorPreviewPanel.BackColor = System.Drawing.Color.White;
            
            // 
            // PickColorButton
            // 
            this.PickColorButton.Location = new System.Drawing.Point(10, 90);
            this.PickColorButton.Name = "PickColorButton";
            this.PickColorButton.Size = new System.Drawing.Size(310, 40);
            this.PickColorButton.TabIndex = 1;
            this.PickColorButton.Text = "Pick Color";
            this.PickColorButton.UseVisualStyleBackColor = true;
            this.PickColorButton.Click += new System.EventHandler(this.PickColorButton_Click);
            
            // 
            // ActionsGroupBox
            // 
            this.ActionsGroupBox.Controls.Add(this.ApplyColorToAllButton);
            this.ActionsGroupBox.Controls.Add(this.SaveYdrButton);
            this.ActionsGroupBox.Controls.Add(this.AutoReloadCheckBox);
            this.ActionsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.ActionsGroupBox.Location = new System.Drawing.Point(10, 160);
            this.ActionsGroupBox.Name = "ActionsGroupBox";
            this.ActionsGroupBox.Padding = new System.Windows.Forms.Padding(10);
            this.ActionsGroupBox.Size = new System.Drawing.Size(330, 170);
            this.ActionsGroupBox.TabIndex = 1;
            this.ActionsGroupBox.TabStop = false;
            this.ActionsGroupBox.Text = "Actions";
            
            // 
            // ApplyColorToAllButton
            // 
            this.ApplyColorToAllButton.Location = new System.Drawing.Point(10, 25);
            this.ApplyColorToAllButton.Name = "ApplyColorToAllButton";
            this.ApplyColorToAllButton.Size = new System.Drawing.Size(310, 40);
            this.ApplyColorToAllButton.TabIndex = 0;
            this.ApplyColorToAllButton.Text = "Apply Color to Entire Object";
            this.ApplyColorToAllButton.UseVisualStyleBackColor = true;
            this.ApplyColorToAllButton.Click += new System.EventHandler(this.ApplyColorToAllButton_Click);
            
            // 
            // SaveYdrButton
            // 
            this.SaveYdrButton.Location = new System.Drawing.Point(10, 75);
            this.SaveYdrButton.Name = "SaveYdrButton";
            this.SaveYdrButton.Size = new System.Drawing.Size(310, 40);
            this.SaveYdrButton.TabIndex = 1;
            this.SaveYdrButton.Text = "Save YDR File";
            this.SaveYdrButton.UseVisualStyleBackColor = true;
            this.SaveYdrButton.Click += new System.EventHandler(this.SaveYdrButton_Click);
            
            // 
            // AutoReloadCheckBox
            // 
            this.AutoReloadCheckBox.AutoSize = true;
            this.AutoReloadCheckBox.Checked = true;
            this.AutoReloadCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoReloadCheckBox.Location = new System.Drawing.Point(10, 130);
            this.AutoReloadCheckBox.Name = "AutoReloadCheckBox";
            this.AutoReloadCheckBox.Size = new System.Drawing.Size(200, 17);
            this.AutoReloadCheckBox.TabIndex = 2;
            this.AutoReloadCheckBox.Text = "Auto-reload project after save";
            this.AutoReloadCheckBox.UseVisualStyleBackColor = true;
            
            // 
            // StatusLabel
            // 
            this.StatusLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.StatusLabel.Location = new System.Drawing.Point(10, 370);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(330, 20);
            this.StatusLabel.TabIndex = 2;
            this.StatusLabel.Text = "Ready";
            
            // 
            // VertexColorPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MainPanel);
            this.Name = "VertexColorPanel";
            this.Size = new System.Drawing.Size(350, 400);
            this.Text = "Vertex Color Editor";
            this.MainPanel.ResumeLayout(false);
            this.ColorGroupBox.ResumeLayout(false);
            this.ActionsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.GroupBox ColorGroupBox;
        private System.Windows.Forms.Panel ColorPreviewPanel;
        private System.Windows.Forms.Button PickColorButton;
        private System.Windows.Forms.GroupBox ActionsGroupBox;
        private System.Windows.Forms.Button ApplyColorToAllButton;
        private System.Windows.Forms.Button SaveYdrButton;
        private System.Windows.Forms.CheckBox AutoReloadCheckBox;
        private System.Windows.Forms.Label StatusLabel;
    }
}
