using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using CodeWalker.Rendering;
using SharpDX.Direct3D11;
using CodeWalker.World;

namespace CodeWalker.Project.Panels
{
    public partial class PropsPanel : ProjectPanel
    {
        public ProjectForm ProjectForm { get; set; }
        private List<Archetype> AllArchetypes = new List<Archetype>();
        private List<Archetype> FilteredArchetypes = new List<Archetype>();
        private Timer SearchTimer = new Timer();
        private Archetype SelectedArchetype = null;

        public PropsPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
            
            SearchTimer.Interval = 500;
            SearchTimer.Tick += SearchTimer_Tick;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Icon = CodeWalker.IconHelper.AppIcon;
        }

        private PreviewWindow previewWindow = null;
        private PreviewForm dockedPreview = null;
        private bool isDockedMode = true; // Start with docked mode

        public void Init()
        {
            if (AllArchetypes.Count > 0) return;

            Task.Run(async () =>
            {
                var gfc = ProjectForm.WorldForm.GameFileCache;
                while (!gfc.IsInited)
                {
                    await Task.Delay(100);
                }

                // Wait a bit for dictionaries to populate
                int retries = 0;
                while ((gfc.YtypDict == null || gfc.YtypDict.Count == 0) && retries < 20) // Wait up to 2 seconds
                {
                    await Task.Delay(100);
                    retries++;
                }

                var archs = new List<Archetype>();
                var addedHashes = new HashSet<uint>();

                // 1. Add from Ytyps
                if (gfc.YtypDict != null)
                {
                    foreach (var ytyp in gfc.YtypDict.Values)
                    {
                        if (ytyp.Loaded && ytyp.AllArchetypes != null)
                        {
                            foreach (var a in ytyp.AllArchetypes)
                            {
                                if (addedHashes.Add(a.Hash))
                                {
                                    archs.Add(a);
                                }
                            }
                        }
                    }
                }

                // 2. Add from YdrDict (Drawables) - Fallback for props not in archetypes
                if (gfc.YdrDict != null)
                {
                    foreach (var kvp in gfc.YdrDict)
                    {
                        if (addedHashes.Add(kvp.Key))
                        {
                            var a = new Archetype();
                            a.Hash = kvp.Key;
                            a._BaseArchetypeDef.assetType = rage__fwArchetypeDef__eAssetType.ASSET_TYPE_DRAWABLE;
                            a._BaseArchetypeDef.name = kvp.Key;
                            a._BaseArchetypeDef.assetName = kvp.Key;
                            a.BSRadius = 5.0f; // Default radius
                            archs.Add(a);
                        }
                    }
                }

                // 3. Add from YftDict (Fragments)
                if (gfc.YftDict != null)
                {
                    foreach (var kvp in gfc.YftDict)
                    {
                        if (addedHashes.Add(kvp.Key))
                        {
                            var a = new Archetype();
                            a.Hash = kvp.Key;
                            a._BaseArchetypeDef.assetType = rage__fwArchetypeDef__eAssetType.ASSET_TYPE_FRAGMENT;
                            a._BaseArchetypeDef.name = kvp.Key;
                            a._BaseArchetypeDef.assetName = kvp.Key;
                            a.BSRadius = 5.0f; // Default radius
                            archs.Add(a);
                        }
                    }
                }

                AllArchetypes = archs.OrderBy(a => a.Name).ToList();
                FilteredArchetypes = AllArchetypes;
                
                Invoke(new Action(() =>
                {
                    UpdateListViewRowCount();
                }));
            });
        }

        private void UpdateListViewRowCount()
        {
            PropsListView.VirtualListSize = FilteredArchetypes.Count;
            PropsListView.Invalidate();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            SearchTimer.Stop();
            FilterList();
        }

        private void FilterList()
        {
            string text = SearchTextBox.Text.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(text))
            {
                FilteredArchetypes = AllArchetypes;
            }
            else
            {
                FilteredArchetypes = AllArchetypes.Where(a => a.Name.ToLowerInvariant().Contains(text) || a.AssetName.ToLowerInvariant().Contains(text)).ToList();
            }
            UpdateListViewRowCount();
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            SearchTimer.Stop();
            SearchTimer.Start();
        }

        private void PropsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex >= 0 && e.ItemIndex < FilteredArchetypes.Count)
            {
                var arch = FilteredArchetypes[e.ItemIndex];
                e.Item = new ListViewItem(new string[] { arch.Name, arch.AssetName });
                e.Item.Tag = arch;
            }
        }

        private void PropsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PropsListView.SelectedIndices.Count > 0)
            {
                int index = PropsListView.SelectedIndices[0];
                if (index >= 0 && index < FilteredArchetypes.Count)
                {
                     SelectedArchetype = FilteredArchetypes[index];
                     
                     // Update docked preview if active
                     if (dockedPreview != null && !dockedPreview.IsDisposed)
                     {
                         dockedPreview.SetArchetype(SelectedArchetype);
                     }
                     
                     // Update window preview if open
                     if (previewWindow != null && !previewWindow.IsDisposed)
                     {
                         previewWindow.SetArchetype(SelectedArchetype);
                     }
                }
            }
        }

        private void EditVertexColorsButton_Click(object sender, EventArgs e)
        {
            if (SelectedArchetype == null)
            {
                MessageBox.Show("Please select a prop first", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Load the YDR file for this archetype
            var gfc = ProjectForm.WorldForm.GameFileCache;
            if (gfc == null) return;

            try
            {
                // Get the drawable for this archetype
                var drawable = gfc.TryGetDrawable(SelectedArchetype);
                if (drawable == null)
                {
                    MessageBox.Show("Could not load drawable for this archetype", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create a YdrFile from the drawable
                var ydr = new YdrFile();
                ydr.Drawable = drawable as Drawable;
                if (ydr.Drawable == null)
                {
                    MessageBox.Show("This archetype does not have a valid Drawable", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                ydr.Name = SelectedArchetype.Name;

                // Show the vertex color panel and load the YDR
                ProjectForm.ShowVertexColorPanel();
                if (ProjectForm.VertexColorPanel != null)
                {
                    ProjectForm.VertexColorPanel.LoadYdr(ydr);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading YDR: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PropsListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedArchetype != null)
            {
                DoDragDrop(SelectedArchetype, DragDropEffects.Copy);
            }
        }

        private void ShowPreviewButton_Click(object sender, EventArgs e)
        {
            // Right-click toggles mode
            if (e is MouseEventArgs me && me.Button == MouseButtons.Right)
            {
                isDockedMode = !isDockedMode;
                
                // Close any open preview
                if (dockedPreview != null)
                {
                    PreviewPanelContainer.Controls.Remove(dockedPreview);
                    dockedPreview.Dispose();
                    dockedPreview = null;
                }
                if (previewWindow != null && !previewWindow.IsDisposed)
                {
                    previewWindow.Close();
                }
                
                // Update button text
                ShowPreviewButton.Text = isDockedMode ? "▶ Show Preview (Docked)" : "▶ Show Preview (Window)";
                return;
            }
            
            // Left-click shows/hides preview
            if (isDockedMode)
            {
                // Docked mode
                if (dockedPreview == null || dockedPreview.IsDisposed)
                {
                    CrashLogger.Log("[PROPS-PANEL] Creating docked preview");
                    dockedPreview = new PreviewForm();
                    dockedPreview.Init(ProjectForm.WorldForm.GameFileCache);
                    dockedPreview.TopLevel = false;
                    dockedPreview.FormBorderStyle = FormBorderStyle.None;
                    dockedPreview.Dock = DockStyle.Fill;
                    PreviewPanelContainer.Controls.Add(dockedPreview);
                    dockedPreview.Show();
                    
                    // Send current selection
                    if (SelectedArchetype != null)
                    {
                        dockedPreview.SetArchetype(SelectedArchetype);
                    }
                    
                    ShowPreviewButton.Text = "▼ Hide Preview (Docked)";
                }
                else
                {
                    CrashLogger.Log("[PROPS-PANEL] Hiding docked preview");
                    PreviewPanelContainer.Controls.Remove(dockedPreview);
                    dockedPreview.Dispose();
                    dockedPreview = null;
                    ShowPreviewButton.Text = "▶ Show Preview (Docked)";
                }
            }
            else
            {
                // Window mode
                if (previewWindow == null || previewWindow.IsDisposed)
                {
                    CrashLogger.Log("[PROPS-PANEL] Creating preview window");
                    previewWindow = new PreviewWindow(ProjectForm.WorldForm.GameFileCache);
                    previewWindow.Init();
                    previewWindow.Owner = ProjectForm.WorldForm;
                    previewWindow.Show();
                    
                    previewWindow.FormClosed += (s, args) =>
                    {
                        CrashLogger.Log("[PROPS-PANEL] Preview window closed");
                        previewWindow = null;
                        ShowPreviewButton.Text = "▶ Show Preview (Window)";
                    };
                    
                    // Send current selection
                    if (SelectedArchetype != null)
                    {
                        previewWindow.SetArchetype(SelectedArchetype);
                    }
                    
                    ShowPreviewButton.Text = "▼ Hide Preview (Window)";
                }
                else
                {
                    CrashLogger.Log("[PROPS-PANEL] Closing preview window");
                    previewWindow.Close();
                }
            }
        }
    }
}
