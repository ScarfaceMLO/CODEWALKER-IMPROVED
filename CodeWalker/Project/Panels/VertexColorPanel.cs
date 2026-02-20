using System;
using System.Drawing;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using SharpDX;
using Color = System.Drawing.Color;

namespace CodeWalker.Project.Panels
{
    public partial class VertexColorPanel : ProjectPanel
    {
        private ProjectForm ProjectForm;
        private YdrFile CurrentYdr;
        private System.Drawing.Color SelectedColor = System.Drawing.Color.White;

        public VertexColorPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Icon = CodeWalker.IconHelper.AppIcon;
        }

        public void LoadYdr(YdrFile ydr)
        {
            CurrentYdr = ydr;
            if (ydr?.Drawable != null)
            {
                Text = $"Vertex Colors - {ydr.Name}";
                StatusLabel.Text = $"Loaded: {ydr.Name} - Ready to apply color";
                
                // Always enable the checkbox - we'll check project status when saving
                AutoReloadCheckBox.Enabled = true;
                AutoReloadCheckBox.Checked = true;
                AutoReloadCheckBox.Text = "Auto-reload project after save";
            }
            else
            {
                StatusLabel.Text = "No YDR loaded";
                AutoReloadCheckBox.Enabled = false;
            }
        }
        private void PickColorButton_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.Color = SelectedColor;
                colorDialog.FullOpen = true;
                
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    SelectedColor = colorDialog.Color;
                    ColorPreviewPanel.BackColor = SelectedColor;
                    StatusLabel.Text = $"Color selected: R={SelectedColor.R}, G={SelectedColor.G}, B={SelectedColor.B}, A={SelectedColor.A}";
                }
            }
        }

        private void ApplyColorToAllButton_Click(object sender, EventArgs e)
        {
            if (CurrentYdr?.Drawable?.AllModels == null)
            {
                MessageBox.Show("No YDR loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(
                $"Apply color (R={SelectedColor.R}, G={SelectedColor.G}, B={SelectedColor.B}, A={SelectedColor.A}) to ALL vertices of ALL geometries?\n\nThis will modify the entire object.",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                int totalVerticesModified = 0;
                int totalGeometries = 0;

                // Apply to all models
                foreach (var model in CurrentYdr.Drawable.AllModels)
                {
                    if (model?.Geometries == null) continue;

                    // Apply to all geometries in this model
                    foreach (var geom in model.Geometries)
                    {
                        if (geom?.VertexData?.Info == null) continue;

                        // Find color component
                        int colorComponentIndex = -1;
                        var flags = geom.VertexData.Info.Flags;
                        
                        for (int i = 0; i < 16; i++)
                        {
                            if (((flags >> i) & 0x1) == 1)
                            {
                                var ct = geom.VertexData.Info.GetComponentType(i);
                                if (ct == VertexComponentType.Colour || ct == VertexComponentType.UByte4)
                                {
                                    colorComponentIndex = i;
                                    break;
                                }
                            }
                        }

                        if (colorComponentIndex == -1)
                            continue; // No color component in this geometry

                        // Apply color to all vertices
                        int vertexCount = geom.VerticesCount;
                        var sharpColor = new SharpDX.Color(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A);
                        
                        for (int v = 0; v < vertexCount; v++)
                        {
                            geom.VertexData.SetColour(v, colorComponentIndex, sharpColor);
                            totalVerticesModified++;
                        }

                        totalGeometries++;
                    }
                }

                StatusLabel.Text = $"Applied color to {totalVerticesModified} vertices across {totalGeometries} geometries";
                MessageBox.Show(
                    $"Successfully applied color to:\n- {totalGeometries} geometries\n- {totalVerticesModified} total vertices",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error: {ex.Message}";
                MessageBox.Show($"Error applying color: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveYdrButton_Click(object sender, EventArgs e)
        {
            if (CurrentYdr?.Drawable == null)
            {
                MessageBox.Show("No YDR loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "YDR Files (*.ydr)|*.ydr|All Files (*.*)|*.*";
                saveDialog.FileName = CurrentYdr.Name + ".ydr";
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        byte[] data = CurrentYdr.Save();
                        System.IO.File.WriteAllBytes(saveDialog.FileName, data);
                        
                        StatusLabel.Text = $"Saved to {saveDialog.FileName}";
                        MessageBox.Show($"YDR file saved successfully to:\n{saveDialog.FileName}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Check if auto-reload is enabled
                        if (AutoReloadCheckBox.Checked)
                        {
                            ReloadProjectAfterSave();
                        }
                        else
                        {
                            // Show manual reload message
                            ReloadDrawableInWorld(saveDialog.FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        StatusLabel.Text = $"Error saving: {ex.Message}";
                        MessageBox.Show($"Error saving YDR file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ReloadProjectAfterSave()
        {
            try
            {
                // Check if ProjectForm exists
                if (ProjectForm == null)
                {
                    StatusLabel.Text = "YDR saved - ProjectForm is NULL";
                    MessageBox.Show(
                        "YDR file saved successfully!\n\n" +
                        "Error: ProjectForm is NULL\n" +
                        "This should not happen - please report this bug.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Check if a project is loaded
                if (ProjectForm.CurrentProjectFile == null)
                {
                    StatusLabel.Text = "YDR saved - No project loaded";
                    MessageBox.Show(
                        "YDR file saved successfully!\n\n" +
                        "Note: No project is currently loaded.\n\n" +
                        "To see vertex color changes in the 3D view:\n" +
                        "1. Open a project (File > Open Project)\n" +
                        "2. The changes will be visible when the project loads",
                        "YDR Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Check if the project has been saved (has a filepath)
                string projectFilePath = ProjectForm.CurrentProjectFile.Filepath;
                
                if (string.IsNullOrEmpty(projectFilePath))
                {
                    StatusLabel.Text = "YDR saved - Please save project first";
                    
                    // Project exists but hasn't been saved yet - ask user to save it
                    var result = MessageBox.Show(
                        "YDR file saved successfully!\n\n" +
                        "Your project hasn't been saved yet.\n" +
                        "Auto-reload requires a saved project file.\n\n" +
                        "Would you like to save your project now?",
                        "Save Project First",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        // Trigger save project
                        ProjectForm.SaveProject();
                        
                        // Check if project was saved successfully
                        if (!string.IsNullOrEmpty(ProjectForm.CurrentProjectFile.Filepath))
                        {
                            // Now reload with the saved project
                            StatusLabel.Text = "Reloading project...";
                            ProjectForm.ReloadProject();
                            StatusLabel.Text = "Project reloaded successfully - Vertex colors updated";
                        }
                        else
                        {
                            StatusLabel.Text = "YDR saved - Project save cancelled";
                        }
                    }
                    return;
                }

                // All checks passed - proceed with reload
                StatusLabel.Text = "Reloading project...";
                ProjectForm.ReloadProject();
                StatusLabel.Text = "Project reloaded successfully - Vertex colors updated";
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error reloading project: {ex.Message}";
                MessageBox.Show(
                    $"YDR saved successfully, but failed to reload project:\n\n{ex.Message}\n\nPlease reload manually.",
                    "Reload Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void ReloadDrawableInWorld(string ydrFilePath)
        {
            try
            {
                // Get the current project file path
                string projectFilePath = ProjectForm?.CurrentProjectFile?.Filepath;
                
                if (!string.IsNullOrEmpty(projectFilePath))
                {
                    // Show message to user with project path for easy reopening
                    MessageBox.Show(
                        $"YDR file saved successfully!\\n\\nTo see the vertex color changes in the 3D view, please:\\n\\n1. Close the current project\\n2. Reopen it from File > Open Project\\n\\nProject path:\\n{projectFilePath}",
                        "Reload Project to See Changes",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    
                    StatusLabel.Text = "YDR saved - Please reload project to see changes";
                }
                else
                {
                    StatusLabel.Text = "YDR saved successfully";
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"YDR saved - {ex.Message}";
            }
        }
    }
}
