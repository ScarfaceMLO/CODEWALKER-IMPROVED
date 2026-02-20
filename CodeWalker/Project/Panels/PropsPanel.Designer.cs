namespace CodeWalker.Project.Panels
{
    partial class PropsPanel
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
            this.components = new System.ComponentModel.Container();
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.SearchTextBox = new System.Windows.Forms.TextBox();
            this.PropsListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ShowPreviewButton = new System.Windows.Forms.Button();
            this.PreviewPanelContainer = new System.Windows.Forms.Panel();
            
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // SplitContainer
            // 
            this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer.Name = "SplitContainer";
            this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.Controls.Add(this.PropsListView);
            this.SplitContainer.Panel1.Controls.Add(this.ShowPreviewButton);
            this.SplitContainer.Panel1.Controls.Add(this.SearchTextBox);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.PreviewPanelContainer);
            this.SplitContainer.Size = new System.Drawing.Size(300, 600);
            this.SplitContainer.SplitterDistance = 300;
            this.SplitContainer.TabIndex = 0;
            
            // 
            // SearchTextBox
            // 
            this.SearchTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.SearchTextBox.Location = new System.Drawing.Point(0, 0);
            this.SearchTextBox.Name = "SearchTextBox";
            this.SearchTextBox.Size = new System.Drawing.Size(300, 20);
            this.SearchTextBox.TabIndex = 0;
            this.SearchTextBox.TextChanged += new System.EventHandler(this.SearchTextBox_TextChanged);
            // 
            // ShowPreviewButton
            // 
            this.ShowPreviewButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.ShowPreviewButton.Location = new System.Drawing.Point(0, 20);
            this.ShowPreviewButton.Name = "ShowPreviewButton";
            this.ShowPreviewButton.Size = new System.Drawing.Size(300, 25);
            this.ShowPreviewButton.TabIndex = 2;
            this.ShowPreviewButton.Text = "▶ Show Preview (Docked)";
            this.ShowPreviewButton.UseVisualStyleBackColor = true;
            this.ShowPreviewButton.Click += new System.EventHandler(this.ShowPreviewButton_Click);
            
            // 
            // PropsListView
            // 
            this.PropsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.PropsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropsListView.FullRowSelect = true;
            this.PropsListView.GridLines = true;
            this.PropsListView.HideSelection = false;
            this.PropsListView.Location = new System.Drawing.Point(0, 45);
            this.PropsListView.Name = "PropsListView";
            this.PropsListView.Size = new System.Drawing.Size(300, 255);
            this.PropsListView.TabIndex = 1;
            this.PropsListView.UseCompatibleStateImageBehavior = false;
            this.PropsListView.View = System.Windows.Forms.View.Details;
            this.PropsListView.VirtualMode = true;
            this.PropsListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.PropsListView_RetrieveVirtualItem);
            this.PropsListView.SelectedIndexChanged += new System.EventHandler(this.PropsListView_SelectedIndexChanged);
            this.PropsListView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.PropsListView_ItemDrag);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 140;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Asset";
            this.columnHeader2.Width = 120;
            
            // 
            // PreviewPanelContainer
            // 
            this.PreviewPanelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PreviewPanelContainer.Location = new System.Drawing.Point(0, 0);
            this.PreviewPanelContainer.Name = "PreviewPanelContainer";
            this.PreviewPanelContainer.Size = new System.Drawing.Size(300, 296);
            this.PreviewPanelContainer.TabIndex = 0;

            // 
            // PropsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SplitContainer);
            this.Name = "PropsPanel";
            this.Text = "Props";
            this.Size = new System.Drawing.Size(300, 600);
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel1.PerformLayout();
            this.SplitContainer.Panel2.ResumeLayout(false);
            this.SplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer SplitContainer;
        private System.Windows.Forms.TextBox SearchTextBox;
        private System.Windows.Forms.Button ShowPreviewButton;
        private System.Windows.Forms.ListView PropsListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Panel PreviewPanelContainer;
    }
}
