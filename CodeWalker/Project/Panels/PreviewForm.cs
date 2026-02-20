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
    public class PreviewForm : Form, DXForm
    {
        public Renderer Renderer = null;
        private Archetype CurrentArchetype = null;
        private DrawableBase CurrentDrawable = null;
        private FragType CurrentFragment = null;
        private uint CurrentModelHash = 0;
        private GameFileCache GameFileCache = null;
        private volatile bool IsRendering = false;
        private Stopwatch frametimer = new Stopwatch();
        private CodeWalker.World.Entity camEntity = new CodeWalker.World.Entity();

        public PreviewForm()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopLevel = false;
            this.Visible = true;
            this.BackColor = System.Drawing.Color.CornflowerBlue;
        }

        public void Init(GameFileCache gameFileCache)
        {
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
            
            Renderer.Init();
            Renderer.Start();
        }

        public void SetArchetype(Archetype archetype)
        {
            if (!IsRendering) return;
            
            try
            {
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
                        if (ydd != null && !ydd.Loaded)
                        {
                            byte[] data = ydd.RpfFileEntry.File.ExtractFile(ydd.RpfFileEntry);
                            ydd.Load(data, ydd.RpfFileEntry);
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
            catch { }
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
                    ExcludeLowerLODs(CurrentFragment.Drawable);
                if (CurrentFragment.DrawableCloth != null)
                    ExcludeLowerLODs(CurrentFragment.DrawableCloth);

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
                        ExcludeAllLODs(d);
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
            Renderer.DeviceCreated(device, this.ClientSize.Width, this.ClientSize.Height);
            
            // Setup camera
            Renderer.camera.FollowEntity = camEntity;
            Renderer.camera.FollowEntity.Position = Vector3.Zero;
            Renderer.camera.FollowEntity.Orientation = Quaternion.LookAtLH(Vector3.Zero, Vector3.Up, Vector3.ForwardLH);
            
            Renderer.camera.TargetRotation = new Vector3(0.5f * (float)Math.PI, 0.2f, 0.0f);
            Renderer.camera.CurrentRotation = Renderer.camera.TargetRotation;
            Renderer.camera.TargetDistance = 5.0f;
            Renderer.camera.CurrentDistance = 5.0f;
            
            IsRendering = true;
            new Thread(new ThreadStart(ContentThread)).Start();
            frametimer.Start();
        }

        public void CleanupScene()
        {
            IsRendering = false;
            Renderer.DeviceDestroyed();
            frametimer.Stop();
        }

        public void RenderScene(DeviceContext context)
        {
            if (Renderer == null) return;

            float elapsed = (float)frametimer.Elapsed.TotalSeconds;
            frametimer.Restart();

            if (!Monitor.TryEnter(Renderer.RenderSyncRoot, 10))
                return;

            try
            {
                Renderer.Update(elapsed, 0, 0);
                Renderer.BeginRender(context);
                Renderer.RenderSkyAndClouds();
                
                if (CurrentFragment != null)
                    Renderer.RenderFragment(CurrentArchetype, null, CurrentFragment, CurrentModelHash, null);
                else if (CurrentDrawable != null)
                    Renderer.RenderDrawable(CurrentDrawable, CurrentArchetype, null, CurrentModelHash, null, null, null);

                Renderer.RenderQueued();
                Renderer.RenderFinalPass();
                Renderer.EndRender();
            }
            catch { }
            finally
            {
                Monitor.Exit(Renderer.RenderSyncRoot);
            }
        }

        public void BuffersResized(int w, int h)
        {
            if (Renderer != null)
                Renderer.BuffersResized(w, h);
        }

        public bool ConfirmQuit()
        {
            return true;
        }

        private void ContentThread()
        {
            while (IsRendering && !IsDisposed)
            {
                bool itemsPending = Renderer.ContentThreadProc();
                if (!itemsPending)
                    Thread.Sleep(20);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                MouseLastPoint = e.Location;
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
