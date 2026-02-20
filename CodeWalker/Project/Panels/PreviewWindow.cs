using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using CodeWalker.Rendering;
using SharpDX;
using SharpDX.Direct3D11;

namespace CodeWalker.Project.Panels
{
    public partial class PreviewWindow : Form, DXForm
    {
        public Renderer Renderer = null;
        private Archetype CurrentArchetype = null;
        private DrawableBase CurrentDrawable = null;
        private FragType CurrentFragment = null;
        private uint CurrentModelHash = 0;
        private GameFileCache GameFileCache = null;
        private bool formopen = false;
        private Stopwatch frametimer = new Stopwatch();
        private CodeWalker.World.Entity camEntity = new CodeWalker.World.Entity();

        public PreviewWindow(GameFileCache gameFileCache)
        {
            InitializeComponent();
            
            GameFileCache = gameFileCache;
            Renderer = new Renderer(this, gameFileCache);
            
            // Setup renderer like ModelForm
            Renderer.controllightdir = !CodeWalker.Properties.Settings.Default.Skydome;
            Renderer.rendercollisionmeshes = false;
            Renderer.renderclouds = false;
            Renderer.rendermoon = false;
            Renderer.renderskeletons = false;
            Renderer.renderfragwindows = false;
            Renderer.SelectionFlagsTestAll = true; // CRUCIAL for rendering isolated drawables!
            
            this.Text = "Props Preview";
            this.Size = new System.Drawing.Size(600, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.ShowInTaskbar = false;
            
            CrashLogger.Log("[PREVIEW-WINDOW] Created");
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 600);
            this.Name = "PreviewWindow";
            this.ResumeLayout(false);
        }

        public void Init()
        {
            CrashLogger.Log("[PREVIEW-WINDOW] Init START");
            
            bool initedOk = Renderer.Init();
            if (!initedOk)
            {
                CrashLogger.Log("[PREVIEW-WINDOW] Renderer.Init() failed");
                MessageBox.Show("Failed to initialize preview renderer!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            
            Renderer.Start();
            CrashLogger.Log("[PREVIEW-WINDOW] Init COMPLETE - DXManager started");
        }

        public void SetArchetype(Archetype archetype)
        {
            if (this.IsDisposed || !formopen) return;
            
            try
            {
                CrashLogger.Log($"[PREVIEW-WINDOW] SetArchetype: {archetype?.Name ?? "null"}");
                CurrentArchetype = archetype;
                CurrentDrawable = null;
                CurrentFragment = null;
                CurrentModelHash = 0;

                if (Renderer == null || GameFileCache == null) return;

                if (archetype != null)
                {
                    var hash = archetype.Hash;

                    // Try to upgrade to real archetype from YTYPs
                    var realArch = GameFileCache.GetArchetype(hash);
                    if (realArch != null)
                    {
                        archetype = realArch;
                        CurrentArchetype = realArch;
                    }

                    var type = archetype._BaseArchetypeDef.assetType;

                    // Load asset if needed
                    if (type == rage__fwArchetypeDef__eAssetType.ASSET_TYPE_DRAWABLE)
                    {
                        var ydr = GameFileCache.GetYdr(hash);
                        if (ydr != null)
                        {
                            if (!ydr.Loaded)
                            {
                                byte[] data = ydr.RpfFileEntry.File.ExtractFile(ydr.RpfFileEntry);
                                ydr.Load(data, ydr.RpfFileEntry);
                            }
                            CurrentDrawable = ydr.Drawable;
                            CurrentModelHash = ydr.RpfFileEntry?.ShortNameHash ?? hash;
                        }
                    }
                    else if (type == rage__fwArchetypeDef__eAssetType.ASSET_TYPE_FRAGMENT)
                    {
                        var yft = GameFileCache.GetYft(hash);
                        if (yft != null)
                        {
                            if (!yft.Loaded)
                            {
                                byte[] data = yft.RpfFileEntry.File.ExtractFile(yft.RpfFileEntry);
                                yft.Load(data, yft.RpfFileEntry);
                            }
                            CurrentFragment = yft.Fragment;
                            CurrentDrawable = yft.Fragment?.Drawable;
                            
                            CurrentModelHash = yft.RpfFileEntry?.ShortNameHash ?? hash;
                            var namelower = yft.RpfFileEntry?.GetShortNameLower();
                            if (namelower?.EndsWith("_hi") ?? false)
                            {
                                CurrentModelHash = JenkHash.GenHash(namelower.Substring(0, namelower.Length - 3));
                            }
                            if (CurrentModelHash == 0) CurrentModelHash = hash;
                        }
                    }
                    else if (type == rage__fwArchetypeDef__eAssetType.ASSET_TYPE_DRAWABLEDICTIONARY)
                    {
                        var ydd = GameFileCache.GetYdd(hash);
                        if (ydd != null)
                        {
                            if (!ydd.Loaded)
                            {
                                byte[] data = ydd.RpfFileEntry.File.ExtractFile(ydd.RpfFileEntry);
                                ydd.Load(data, ydd.RpfFileEntry);
                            }
                        }
                    }

                    // Fallback
                    if (CurrentDrawable == null && CurrentFragment == null)
                    {
                        CurrentDrawable = GameFileCache.TryGetDrawable(archetype);
                        if (!string.IsNullOrEmpty(archetype.AssetName))
                            CurrentModelHash = JenkHash.GenHash(archetype.AssetName);
                        else
                            CurrentModelHash = archetype.Hash;
                    }

                    // Update bounds if dummy
                    if (CurrentDrawable != null && archetype.BSRadius <= 0.1f)
                    {
                        archetype.BSRadius = CurrentDrawable.BoundingSphereRadius;
                        archetype.BSCenter = CurrentDrawable.BoundingCenter;
                        archetype.BBMin = CurrentDrawable.BoundingBoxMin;
                        archetype.BBMax = CurrentDrawable.BoundingBoxMax;
                    }

                    UpdateLODs();

                    // Camera positioning
                    Vector3 center = archetype.BSCenter;
                    float radius = archetype.BSRadius;

                    if (CurrentDrawable != null)
                    {
                        center = CurrentDrawable.BoundingCenter;
                        radius = CurrentDrawable.BoundingSphereRadius;
                    }

                    if (radius < 0.1f) radius = 1.0f;

                    camEntity.Position = center;
                    Renderer.camera.TargetDistance = radius * 2.5f;
                    Renderer.camera.CurrentDistance = Renderer.camera.TargetDistance;
                    Renderer.camera.UpdateProj = true;
                }
            }
            catch (Exception ex)
            {
                CrashLogger.Log($"[PREVIEW-WINDOW] EXCEPTION in SetArchetype: {ex.Message}");
            }
        }

        private void UpdateLODs()
        {
            if (Renderer == null) return;
            Renderer.SelectionModelDrawFlags.Clear();

            if (CurrentDrawable != null)
            {
                ExcludeLowerLODs(CurrentDrawable);
            }

            if (CurrentFragment != null)
            {
                if (CurrentFragment.Drawable != null && CurrentFragment.Drawable != CurrentDrawable)
                {
                    ExcludeLowerLODs(CurrentFragment.Drawable);
                }
                if (CurrentFragment.DrawableCloth != null)
                {
                    ExcludeLowerLODs(CurrentFragment.DrawableCloth);
                }

                var pl1 = CurrentFragment.PhysicsLODGroup?.PhysicsLOD1;
                if (pl1?.Children?.data_items != null)
                {
                    foreach (var child in pl1.Children.data_items)
                    {
                        if (child.Drawable1 != null) ExcludeLowerLODs(child.Drawable1);
                        if (child.Drawable2 != null) ExcludeLowerLODs(child.Drawable2);
                    }
                }

                var darr = CurrentFragment.DrawableArray?.data_items;
                if (darr != null)
                {
                    foreach (var d in darr)
                    {
                        ExcludeAllLODs(d);
                    }
                }
            }
        }

        private void ExcludeLowerLODs(DrawableBase drawable)
        {
            var models = drawable?.DrawableModels;
            if (models == null) return;
            if (models.Med != null) foreach (var m in models.Med) Renderer.SelectionModelDrawFlags[m] = false;
            if (models.Low != null) foreach (var m in models.Low) Renderer.SelectionModelDrawFlags[m] = false;
            if (models.VLow != null) foreach (var m in models.VLow) Renderer.SelectionModelDrawFlags[m] = false;
        }

        private void ExcludeAllLODs(DrawableBase drawable)
        {
            var models = drawable?.DrawableModels;
            if (models == null) return;
            if (models.High != null) foreach (var m in models.High) Renderer.SelectionModelDrawFlags[m] = false;
            if (models.Med != null) foreach (var m in models.Med) Renderer.SelectionModelDrawFlags[m] = false;
            if (models.Low != null) foreach (var m in models.Low) Renderer.SelectionModelDrawFlags[m] = false;
            if (models.VLow != null) foreach (var m in models.VLow) Renderer.SelectionModelDrawFlags[m] = false;
        }

        // DXForm Interface
        public Form Form => this;

        public void InitScene(Device device)
        {
            CrashLogger.Log("[PREVIEW-WINDOW] InitScene START");
            
            try
            {
                Renderer.DeviceCreated(device, this.ClientSize.Width, this.ClientSize.Height);
                
                // Setup camera
                Renderer.camera.FollowEntity = camEntity;
                Renderer.camera.FollowEntity.Position = Vector3.Zero;
                Renderer.camera.FollowEntity.Orientation = Quaternion.LookAtLH(Vector3.Zero, Vector3.Up, Vector3.ForwardLH);
                
                Renderer.camera.TargetRotation = new Vector3(0.5f * (float)Math.PI, 0.2f, 0.0f);
                Renderer.camera.CurrentRotation = Renderer.camera.TargetRotation;
                Renderer.camera.TargetDistance = 5.0f;
                Renderer.camera.CurrentDistance = 5.0f;
                
                formopen = true;
                
                // Start content thread
                new Thread(new ThreadStart(ContentThread)).Start();
                
                frametimer.Start();
                CrashLogger.Log("[PREVIEW-WINDOW] InitScene COMPLETE");
            }
            catch (Exception ex)
            {
                CrashLogger.Log($"[PREVIEW-WINDOW] EXCEPTION in InitScene: {ex.Message}");
            }
        }

        public void CleanupScene()
        {
            CrashLogger.Log("[PREVIEW-WINDOW] CleanupScene START");
            formopen = false;
            Renderer.DeviceDestroyed();
            frametimer.Stop();
            CrashLogger.Log("[PREVIEW-WINDOW] CleanupScene COMPLETE");
        }

        public void RenderScene(DeviceContext context)
        {
            if (Renderer == null) return;

            float elapsed = (float)frametimer.Elapsed.TotalSeconds;
            frametimer.Restart();

            if (!Monitor.TryEnter(Renderer.RenderSyncRoot, 10))
            {
                return;
            }

            try
            {
                Renderer.Update(elapsed, 0, 0);
                Renderer.BeginRender(context);
                Renderer.RenderSkyAndClouds(); // CRUCIAL for lighting!
                
                // Render the preview item
                if (CurrentFragment != null)
                {
                    Renderer.RenderFragment(CurrentArchetype, null, CurrentFragment, CurrentModelHash, null);
                }
                else if (CurrentDrawable != null)
                {
                    Renderer.RenderDrawable(CurrentDrawable, CurrentArchetype, null, CurrentModelHash, null, null, null);
                }

                Renderer.RenderQueued();
                Renderer.RenderFinalPass();
                Renderer.EndRender();
            }
            catch (Exception ex)
            {
                CrashLogger.Log($"[PREVIEW-WINDOW] EXCEPTION in RenderScene: {ex.Message}");
            }
            finally
            {
                Monitor.Exit(Renderer.RenderSyncRoot);
            }
        }

        public void BuffersResized(int w, int h)
        {
            if (Renderer != null)
            {
                Renderer.BuffersResized(w, h);
            }
        }

        public bool ConfirmQuit()
        {
            return true;
        }

        private void ContentThread()
        {
            CrashLogger.Log("[PREVIEW-WINDOW] ContentThread START");
            
            while (formopen && !IsDisposed)
            {
                bool itemsPending = Renderer.ContentThreadProc();
                
                if (!itemsPending)
                {
                    Thread.Sleep(20);
                }
            }
            
            CrashLogger.Log("[PREVIEW-WINDOW] ContentThread END");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            CrashLogger.Log("[PREVIEW-WINDOW] OnFormClosing");
            formopen = false;
            base.OnFormClosing(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MouseLastPoint = e.Location;
            }
            base.OnMouseDown(e);
        }

        private System.Drawing.Point MouseLastPoint;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.X - MouseLastPoint.X;
                int dy = e.Y - MouseLastPoint.Y;
                Renderer.camera.MouseRotate(dx, dy);
                MouseLastPoint = e.Location;
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Renderer.camera.MouseZoom(e.Delta);
            base.OnMouseWheel(e);
        }
    }
}
