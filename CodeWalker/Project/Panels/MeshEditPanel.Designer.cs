namespace CodeWalker.Project.Panels
{
    partial class MeshEditPanel
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
            this.ModeGroupBox = new System.Windows.Forms.GroupBox();
            this.FaceModeRadio = new System.Windows.Forms.RadioButton();
            this.EdgeModeRadio = new System.Windows.Forms.RadioButton();
            this.VertexModeRadio = new System.Windows.Forms.RadioButton();
            this.InfoLabel = new System.Windows.Forms.Label();
            this.SaveButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.ModeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ModeGroupBox
            // 
            this.ModeGroupBox.Controls.Add(this.FaceModeRadio);
            this.ModeGroupBox.Controls.Add(this.EdgeModeRadio);
            this.ModeGroupBox.Controls.Add(this.VertexModeRadio);
            this.ModeGroupBox.Location = new System.Drawing.Point(3, 3);
            this.ModeGroupBox.Name = "ModeGroupBox";
            this.ModeGroupBox.Size = new System.Drawing.Size(200, 100);
            this.ModeGroupBox.TabIndex = 0;
            this.ModeGroupBox.TabStop = false;
            this.ModeGroupBox.Text = "Edit Mode";
            // 
            // FaceModeRadio
            // 
            this.FaceModeRadio.AutoSize = true;
            this.FaceModeRadio.Location = new System.Drawing.Point(6, 65);
            this.FaceModeRadio.Name = "FaceModeRadio";
            this.FaceModeRadio.Size = new System.Drawing.Size(50, 17);
            this.FaceModeRadio.TabIndex = 2;
            this.FaceModeRadio.Text = "Face";
            this.FaceModeRadio.UseVisualStyleBackColor = true;
            this.FaceModeRadio.CheckedChanged += new System.EventHandler(this.ModeRadio_CheckedChanged);
            // 
            // EdgeModeRadio
            // 
            this.EdgeModeRadio.AutoSize = true;
            this.EdgeModeRadio.Location = new System.Drawing.Point(6, 42);
            this.EdgeModeRadio.Name = "EdgeModeRadio";
            this.EdgeModeRadio.Size = new System.Drawing.Size(51, 17);
            this.EdgeModeRadio.TabIndex = 1;
            this.EdgeModeRadio.Text = "Edge";
            this.EdgeModeRadio.UseVisualStyleBackColor = true;
            this.EdgeModeRadio.CheckedChanged += new System.EventHandler(this.ModeRadio_CheckedChanged);
            // 
            // VertexModeRadio
            // 
            this.VertexModeRadio.AutoSize = true;
            this.VertexModeRadio.Checked = true;
            this.VertexModeRadio.Location = new System.Drawing.Point(6, 19);
            this.VertexModeRadio.Name = "VertexModeRadio";
            this.VertexModeRadio.Size = new System.Drawing.Size(57, 17);
            this.VertexModeRadio.TabIndex = 0;
            this.VertexModeRadio.TabStop = true;
            this.VertexModeRadio.Text = "Vertex";
            this.VertexModeRadio.UseVisualStyleBackColor = true;
            this.VertexModeRadio.CheckedChanged += new System.EventHandler(this.ModeRadio_CheckedChanged);
            // 
            // InfoLabel
            // 
            this.InfoLabel.AutoSize = true;
            this.InfoLabel.Location = new System.Drawing.Point(3, 110);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Size = new System.Drawing.Size(99, 13);
            this.InfoLabel.TabIndex = 1;
            this.InfoLabel.Text = "No selection";
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(3, 135);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(95, 23);
            this.SaveButton.TabIndex = 2;
            this.SaveButton.Text = "Save YDR";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(104, 135);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(95, 23);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // MeshEditPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.InfoLabel);
            this.Controls.Add(this.ModeGroupBox);
            this.Name = "MeshEditPanel";
            this.Size = new System.Drawing.Size(210, 170);
            this.ModeGroupBox.ResumeLayout(false);
            this.ModeGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox ModeGroupBox;
        private System.Windows.Forms.RadioButton FaceModeRadio;
        private System.Windows.Forms.RadioButton EdgeModeRadio;
        private System.Windows.Forms.RadioButton VertexModeRadio;
        private System.Windows.Forms.Label InfoLabel;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button CancelButton;
    }
}
