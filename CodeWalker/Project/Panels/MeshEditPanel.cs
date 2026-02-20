using System;
using System.IO;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using CodeWalker.World;

using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.Project.Panels
{
    public partial class MeshEditPanel : DockContent
    {
        private ProjectForm ProjectForm;
        private MeshEditor meshEditor;

        public MeshEditPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (ProjectForm != null && ProjectForm.Theme != null)
            {
                FormTheme.SetTheme(this, ProjectForm.Theme);
            }
            this.Icon = CodeWalker.IconHelper.AppIcon;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (meshEditor != null && meshEditor.IsActive)
            {
                ProjectForm.WorldForm?.ExitMeshEditMode();
            }
        }

        public void SetMeshEditor(MeshEditor editor)
        {
            meshEditor = editor;
            if (meshEditor?.CurrentYdr != null)
            {
                Text = $"Mesh Editor - {meshEditor.CurrentYdr.Name}";
            }
            else
            {
                Text = "Mesh Editor";
            }
            UpdateInfoLabel();
        }

        private void ModeRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (meshEditor == null) return;

            if (VertexModeRadio.Checked)
            {
                meshEditor.CurrentMode = MeshEditMode.Vertex;
            }
            else if (EdgeModeRadio.Checked)
            {
                meshEditor.CurrentMode = MeshEditMode.Edge;
            }
            else if (FaceModeRadio.Checked)
            {
                meshEditor.CurrentMode = MeshEditMode.Face;
            }

            UpdateInfoLabel();
        }

        public void UpdateInfoLabel()
        {
            if (meshEditor == null || !meshEditor.IsActive)
            {
                InfoLabel.Text = "No active mesh";
                return;
            }

            int selectedCount = meshEditor.SelectedElements.Count;
            string modeText = meshEditor.CurrentMode.ToString();
            
            if (selectedCount == 0)
            {
                InfoLabel.Text = $"Mode: {modeText}\nNo selection";
            }
            else
            {
                InfoLabel.Text = $"Mode: {modeText}\n{selectedCount} element(s) selected";
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (meshEditor == null || !meshEditor.IsActive)
            {
                MessageBox.Show("No active mesh to save", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to save the modifications to the YDR file?\n\n" +
                "This will permanently modify the mesh geometry.\n" +
                "It is recommended to create a backup first.",
                "Confirm Save",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                var ydrData = meshEditor.SaveModifications();
                
                if (ydrData == null)
                {
                    MessageBox.Show("Failed to save YDR data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var ydr = meshEditor.CurrentYdr;
                string defaultName = ydr?.Name ?? "geometry";
                if (!defaultName.EndsWith(".ydr", StringComparison.OrdinalIgnoreCase))
                {
                    defaultName += ".ydr";
                }

                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "YDR Files|*.ydr|All Files|*.*";
                    sfd.FileName = defaultName;
                    sfd.Title = "Save Modified YDR";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllBytes(sfd.FileName, ydrData);
                        
                        // Show success message with reload instructions
                        // Show success message
                        string reloadMsg = "YDR file saved successfully!";
                        MessageBox.Show(reloadMsg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // Quitter le mode d'édition
                ExitMeshEditMode();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving YDR: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel?\n\nAll modifications will be lost.",
                "Confirm Cancel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                meshEditor?.Cancel();
                ExitMeshEditMode();
            }
        }

        private void ExitMeshEditMode()
        {
            ProjectForm.WorldForm?.ExitMeshEditMode();
            ProjectForm.HideMeshEditPanel();
        }
    }
}
