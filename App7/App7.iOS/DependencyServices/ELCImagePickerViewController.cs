﻿using AssetsLibrary;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using UIKit;

namespace App7.iOS.DependencyServices
{
    public class AssetResult
    {
        public UIImage Image { get; set; }
        public string Name { get; set; }
    }

    public class ELCImagePickerViewController : UINavigationController
    {
        public int MaximumImagesCount { get; set; }

        readonly TaskCompletionSource<List<AssetResult>> _TaskCompletionSource = new TaskCompletionSource<List<AssetResult>>();
        public Task<List<AssetResult>> Completion
        {
            get
            {
                return _TaskCompletionSource.Task;
            }
        }

        public static ELCImagePickerViewController Instance
        {
            get
            {
                var albumPicker = new ELCAlbumPickerController();
                var picker = new ELCImagePickerViewController(albumPicker);
                albumPicker.Parent = picker;
                picker.MaximumImagesCount = 20;
                picker.NavigationBar.BarStyle = UIBarStyle.Black;
                return picker;
            }
        }

        ELCImagePickerViewController(UIViewController rootController) : base(rootController)
        {

        }

        void SelectedAssets(List<ALAsset> assets)
        {
            var results = new List<AssetResult>(assets.Count);
            foreach (var asset in assets)
            {
                var obj = asset.AssetType;
                if (obj == default(ALAssetType))
                    continue;

                var rep = asset.DefaultRepresentation;
                if (rep != null)
                {
                    var result = new AssetResult();
                    UIImageOrientation orientation = UIImageOrientation.Up;
                    var cgImage = rep.GetFullScreenImage();
                    var equalindex = asset.AssetUrl.Query.IndexOf('=');
                    var nameChanged = asset.AssetUrl.Query.Replace("&ext=", ".");
                    var name = nameChanged.Substring(equalindex + 1);
                    result.Image = new UIImage(cgImage, 1.0f, orientation);
                    result.Name = name;
                    results.Add(result);
                }
            }

            _TaskCompletionSource.TrySetResult(results);
        }

        void CancelledPicker()
        {
            _TaskCompletionSource.TrySetCanceled();
        }

        bool ShouldSelectAsset(int previousCount)
        {
            var shouldSelect = MaximumImagesCount <= 0 || previousCount < MaximumImagesCount;
            if (!shouldSelect)
            {
                string title = string.Format("Only {0} photos please!", MaximumImagesCount.ToString());
                string message = string.Format("You can only send {0} photos at a time.", MaximumImagesCount.ToString());
                var alert = new UIAlertView(title, message, null, null, "Okay");
                alert.Show();
            }
            return shouldSelect;
        }

        public class ELCAlbumPickerController : UITableViewController
        {
            static readonly NSObject _Dispatcher = new NSObject();
            readonly List<ALAssetsGroup> AssetGroups = new List<ALAssetsGroup>();

            ALAssetsLibrary Library;

            WeakReference _Parent;
            public ELCImagePickerViewController Parent
            {
                get
                {
                    return _Parent == null ? null : _Parent.Target as ELCImagePickerViewController;
                }
                set
                {
                    _Parent = new WeakReference(value);
                }
            }

            public ELCAlbumPickerController()
            {
            }

            public override void ViewDidLoad()
            {
                base.ViewDidLoad();

                NavigationItem.Title = "Loading...";
                var cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
                cancelButton.Clicked += CancelClicked;
                NavigationItem.RightBarButtonItem = cancelButton;

                AssetGroups.Clear();

                Library = new ALAssetsLibrary();
                Library.Enumerate(ALAssetsGroupType.All, GroupsEnumerator, GroupsEnumeratorFailed);
            }

            public override void ViewDidDisappear(bool animated)
            {
                base.ViewDidDisappear(animated);
                if (IsMovingFromParentViewController || IsBeingDismissed)
                {
                    NavigationItem.RightBarButtonItem.Clicked -= CancelClicked;
                }
            }

            void CancelClicked(object sender = null, EventArgs e = null)
            {
                var parent = Parent;
                if (parent != null)
                {
                    parent.CancelledPicker();
                }
            }

            void GroupsEnumeratorFailed(NSError error)
            {
                Console.WriteLine("Enumerator failed!");
            }

            void GroupsEnumerator(ALAssetsGroup agroup, ref bool stop)
            {
                if (agroup == null)
                {
                    return;
                }

                // added fix for camera albums order
                if (agroup.Name.ToString().ToLower() == "camera roll" && agroup.Type == ALAssetsGroupType.SavedPhotos)
                {
                    AssetGroups.Insert(0, agroup);
                }
                else
                {
                    AssetGroups.Add(agroup);
                }

                _Dispatcher.BeginInvokeOnMainThread(ReloadTableView);
            }

            void ReloadTableView()
            {
                TableView.ReloadData();
                NavigationItem.Title = "Photos";
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return 1;
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                return AssetGroups.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                const string cellIdentifier = "Cell";

                var cell = tableView.DequeueReusableCell(cellIdentifier);
                if (cell == null)
                {
                    cell = new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier);
                }

                // Get count
                var g = AssetGroups[indexPath.Row];
                g.SetAssetsFilter(ALAssetsFilter.AllPhotos);
                var gCount = g.Count;
                cell.TextLabel.Text = string.Format("{0} ({1})", g.Name, gCount);
                try
                {
                    cell.ImageView.Image = new UIImage(g.PosterImage);
                }
                catch (Exception ex)
                {
                      Console.WriteLine("Failed to set thumbnail {0}", ex);
                }
                cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var assetGroup = AssetGroups[indexPath.Row];
                assetGroup.SetAssetsFilter(ALAssetsFilter.AllPhotos);
                var picker = new ELCAssetTablePicker(assetGroup);
                picker.Parent = Parent;
                NavigationController.PushViewController(picker, true);
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 57;
            }
        }


        class ELCAssetTablePicker : UITableViewController
        {
            static readonly NSObject _Dispatcher = new NSObject();

            int Columns = 4;
            public bool SingleSelection { get; }
            public bool ImmediateReturn { get; }
            readonly ALAssetsGroup AssetGroup;

            readonly List<ELCAsset> ElcAssets = new List<ELCAsset>();

            WeakReference _Parent;
            public ELCImagePickerViewController Parent
            {
                get
                {
                    return _Parent == null ? null : _Parent.Target as ELCImagePickerViewController;
                }
                set
                {
                    _Parent = new WeakReference(value);
                }
            }

            public ELCAssetTablePicker(ALAssetsGroup assetGroup)
            {
                AssetGroup = assetGroup;
            }

            public override void ViewDidLoad()
            {
                TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
                TableView.AllowsSelection = false;

                if (!ImmediateReturn)
                {
                    var doneButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
                    doneButtonItem.Clicked += DoneClicked;
                    NavigationItem.RightBarButtonItem = doneButtonItem;
                    NavigationItem.Title = "Loading...";
                }

                Task.Run((Action)PreparePhotos);
            }

            public override void ViewWillAppear(bool animated)
            {
                base.ViewWillAppear(animated);
                Columns = (int)(View.Bounds.Size.Width / 80f);
            }

            public override void ViewDidDisappear(bool animated)
            {
                base.ViewDidDisappear(animated);
                if (IsMovingFromParentViewController || IsBeingDismissed)
                {
                    NavigationItem.RightBarButtonItem.Clicked -= DoneClicked;
                }
            }

            public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
            {
                base.DidRotate(fromInterfaceOrientation);
                Columns = (int)(View.Bounds.Size.Width / 80f);
                TableView.ReloadData();
            }

            void PreparePhotos()
            {
                AssetGroup.Enumerate(PhotoEnumerator);

                _Dispatcher.BeginInvokeOnMainThread(() =>
                {
                    TableView.ReloadData();
                    // scroll to bottom
                    int section = (int)NumberOfSections(TableView) - 1;
                    int row = (int)TableView.NumberOfRowsInSection(section) - 1;
                    if (section >= 0 && row >= 0)
                    {
                        var ip = NSIndexPath.FromRowSection(row, section);
                        TableView.ScrollToRow(ip, UITableViewScrollPosition.Bottom, false);
                    }
                    NavigationItem.Title = SingleSelection ? "Select Photo" : "Select Photos";
                });
            }

            void PhotoEnumerator(ALAsset result, nint index, ref bool stop)
            {
                if (result == null)
                {
                    return;
                }

                ELCAsset elcAsset = new ELCAsset(this, result);

                bool isAssetFiltered = false;

                if (result.DefaultRepresentation == null)
                    isAssetFiltered = true;

                if (!isAssetFiltered)
                {
                    ElcAssets.Add(elcAsset);
                }
            }

            void DoneClicked(object sender = null, EventArgs e = null)
            {
                var selected = new List<ALAsset>();

                foreach (var asset in ElcAssets)
                {
                    if (asset.Selected)
                    {
                        selected.Add(asset.Asset);
                    }
                }

                var parent = Parent;
                if (parent != null)
                {
                    parent.SelectedAssets(selected);
                }
            }

            bool ShouldSelectAsset()
            {
                int selectionCount = TotalSelectedAssets;
                bool shouldSelect = true;

                var parent = Parent;
                if (parent != null)
                {
                    shouldSelect = parent.ShouldSelectAsset(selectionCount);
                }

                return shouldSelect;
            }

            void AssetSelected(ELCAsset asset, bool selected)
            {
                TotalSelectedAssets += (selected) ? 1 : -1;

                if (SingleSelection)
                {
                    foreach (var elcAsset in ElcAssets)
                    {
                        if (asset != elcAsset)
                        {
                            elcAsset.Selected = false;
                        }
                    }
                }
                if (ImmediateReturn)
                {
                    var parent = Parent;
                    var obj = new List<ALAsset>(1);
                    obj.Add(asset.Asset);
                    parent.SelectedAssets(obj);
                }
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return 1;
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                if (Columns <= 0)
                    return 4;
                int numRows = (int)Math.Ceiling((float)ElcAssets.Count / Columns);
                return numRows;
            }

            List<ELCAsset> AssetsForIndexPath(NSIndexPath path)
            {
                int index = path.Row * Columns;
                int length = Math.Min(Columns, ElcAssets.Count - index);
                return ElcAssets.GetRange(index, length);
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                const string cellIdentifier = "Cell";

                var cell = TableView.DequeueReusableCell(cellIdentifier) as ELCAssetCell;
                if (cell == null)
                {
                    cell = new ELCAssetCell(UITableViewCellStyle.Default, cellIdentifier);
                }
                cell.SetAssets(AssetsForIndexPath(indexPath), Columns);
                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 79;
            }

            public int TotalSelectedAssets;

            public class ELCAsset
            {
                public readonly ALAsset Asset;
                readonly WeakReference _Parent;

                bool _Selected;

                public ELCAsset(ELCAssetTablePicker parent, ALAsset asset)
                {
                    _Parent = new WeakReference(parent);
                    Asset = asset;
                }

                public bool Selected
                {
                    get
                    {
                        return _Selected;
                    }

                    set
                    {
                        var parent = _Parent.Target as ELCAssetTablePicker;
                        if (value && parent != null && !parent.ShouldSelectAsset())
                        {
                            return;
                        }

                        _Selected = value;

                        if (parent != null)
                        {
                            parent.AssetSelected(this, value);
                        }
                    }
                }
            }

            class ELCAssetCell : UITableViewCell
            {
                List<ELCAsset> RowAssets;
                int Columns;
                readonly List<UIImageView> ImageViewArray = new List<UIImageView>();
                readonly List<UIImageView> OverlayViewArray = new List<UIImageView>();

                public ELCAssetCell(UITableViewCellStyle style, string reuseIdentifier) : base(style, reuseIdentifier)
                {
                    UITapGestureRecognizer tapRecognizer = new UITapGestureRecognizer(CellTapped);
                    AddGestureRecognizer(tapRecognizer);

                }

                public void SetAssets(List<ELCAsset> assets, int columns)
                {
                    RowAssets = assets;
                    Columns = columns;

                    foreach (var view in ImageViewArray)
                    {
                        view.RemoveFromSuperview();
                    }
                    foreach (var view in OverlayViewArray)
                    {
                        view.RemoveFromSuperview();
                    }

                    UIImage overlayImage = null;
                    for (int i = 0; i < RowAssets.Count; i++)
                    {
                        var asset = RowAssets[i];

                        if (i < ImageViewArray.Count)
                        {
                            var imageView = ImageViewArray[i];
                            imageView.Image = new UIImage(asset.Asset.Thumbnail);
                        }
                        else
                        {
                            var imageView = new UIImageView(new UIImage(asset.Asset.Thumbnail));
                            ImageViewArray.Add(imageView);
                        }

                        if (i < OverlayViewArray.Count)
                        {
                            var overlayView = OverlayViewArray[i];
                            overlayView.Hidden = !asset.Selected;
                        }
                        else
                        {
                            try
                            {
                                if (overlayImage == null)
                                {
                                    overlayImage = UIImage.FromBundle("imageSelected");
                                }
                            }
                            catch (Exception ex)
                            {
                               }

                            var overlayView = new UIImageView(overlayImage);
                            OverlayViewArray.Add(overlayView);
                            overlayView.Hidden = !asset.Selected;
                        }
                    }
                }

                void CellTapped(UITapGestureRecognizer tapRecognizer)
                {
                    PointF point = (PointF)tapRecognizer.LocationInView(this);
                    var totalWidth = Columns * 75 + (Columns - 1) * 4;
                    var startX = (Bounds.Size.Width - totalWidth) / 2;

                    var frame = new RectangleF((float)startX, 2, 75, 75);
                    for (int i = 0; i < RowAssets.Count; ++i)
                    {
                        if (frame.Contains(point))
                        {
                            ELCAsset asset = RowAssets[i];
                            asset.Selected = !asset.Selected;
                            var overlayView = OverlayViewArray[i];
                            overlayView.Hidden = !asset.Selected;
                            break;
                        }
                        var x = frame.X + frame.Width + 4;
                        frame = new RectangleF(x, frame.Y, frame.Width, frame.Height);
                    }
                }

                public override void LayoutSubviews()
                {
                    var totalWidth = Columns * 75 + (Columns - 1) * 4;
                    var startX = (Bounds.Size.Width - totalWidth) / 2;

                    var frame = new RectangleF((float)startX, 2, 75, 75);

                    int i = 0;
                    foreach (var imageView in ImageViewArray)
                    {
                        imageView.Frame = frame;
                        AddSubview(imageView);

                        var overlayView = OverlayViewArray[i++];
                        overlayView.Frame = frame;
                        AddSubview(overlayView);

                        var x = frame.X + frame.Width + 4;
                        frame = new RectangleF(x, frame.Y, frame.Width, frame.Height);
                    }
                }
            }
        }
    }
}