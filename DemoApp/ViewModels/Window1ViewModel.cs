﻿using System;
using System.Collections.Generic;
using System.Linq;
using DiagramDesigner;
using DemoApp.Persistence.Common;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DemoApp
{
    public class Window1ViewModel : INPCBase
    {

        private List<string> savedDiagrams = new List<string>();
        private string? savedDiagramId;
        private List<SelectableDesignerItemViewModelBase> itemsToRemove;
        private List<SelectableDesignerItemViewModelBase> itemsToCopy;
        private List<SelectableDesignerItemViewModelBase> itemsToAlign;
        private IMessageBoxService messageBoxService;
        private IDatabaseAccessService databaseAccessService;
        private DiagramViewModel diagramViewModel = new DiagramViewModel();
        private bool isBusy = false;


        public Window1ViewModel()
        {
            messageBoxService = ApplicationServicesProvider.Instance.Provider.MessageBoxService;
            databaseAccessService = ApplicationServicesProvider.Instance.Provider.DatabaseAccessService;

            foreach (var savedDiagram in databaseAccessService.FetchAllDiagram())
            {
                savedDiagrams.Add(savedDiagram.Id);
            }

            ToolBoxViewModel = new ToolBoxViewModel();
            DiagramViewModel = new DiagramViewModel();

            DeleteSelectedItemsCommand = new SimpleCommand(ExecuteDeleteSelectedItemsCommand);
            CreateNewDiagramCommand = new SimpleCommand(ExecuteCreateNewDiagramCommand);
            SaveDiagramCommand = new SimpleCommand(ExecuteSaveDiagramCommand);
            LoadDiagramCommand = new SimpleCommand(ExecuteLoadDiagramCommand);
            GroupCommand = new SimpleCommand(ExecuteGroupCommand);
            CopySelectedItemsCommand = new SimpleCommand(ExecuteCopySelectedItemsCommand);
            AlignLeftCommand = new SimpleCommand(ExecuteAlignLeftCommand);
            PrintCommand = new SimpleCommand(ExecutePrintCommand);
            //OrthogonalPathFinder is a pretty bad attempt at finding path points, it just shows you, you can swap this out with relative
            //ease if you wish just create a new IPathFinder class and pass it in right here
            ConnectorViewModel.PathFinder = new OrthogonalPathFinder();

            


        }

        public SimpleCommand PrintCommand { get; private set; }
        public SimpleCommand DeleteSelectedItemsCommand { get; private set; }
        public SimpleCommand CopySelectedItemsCommand { get; private set; }
        public SimpleCommand AlignLeftCommand { get; private set; }
        public SimpleCommand CreateNewDiagramCommand { get; private set; }
        public SimpleCommand SaveDiagramCommand { get; private set; }
        public SimpleCommand GroupCommand { get; private set; }
        public SimpleCommand LoadDiagramCommand { get; private set; }
        public ToolBoxViewModel ToolBoxViewModel { get; private set; }


        public DiagramViewModel DiagramViewModel
        {
            get
            {
                return diagramViewModel;
            }
            set
            {
                if (diagramViewModel != value)
                {
                    diagramViewModel = value;
                    NotifyChanged("DiagramViewModel");
                }
            }
        }

        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    NotifyChanged("IsBusy");
                }
            }
        }


        public List<string> SavedDiagrams
        {
            get
            {
                return savedDiagrams;
            }
            set
            {
                if (savedDiagrams != value)
                {
                    savedDiagrams = value;
                    NotifyChanged("SavedDiagrams");
                }
            }
        }

        public string? SavedDiagramId
        {
            get
            {
                return savedDiagramId;
            }
            set
            {
                if (savedDiagramId != value)
                {
                    savedDiagramId = value;
                    NotifyChanged("SavedDiagramId");
                }
            }
        }

        public void ExecutePrintCommand(object sender)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                UIElement parent = App.Current.MainWindow; // get root view
                printDialog.PrintVisual(parent, "Print");
            }
        }

        private void ExecuteAlignLeftCommand(object parameter)
        {
            itemsToAlign = DiagramViewModel.SelectedItems;
            // Check which type of item that is copied
            int cnt = 0;
            double left = 0;
            foreach (var selectedItem in itemsToAlign.OfType<PersistDesignerItemViewModel>().ToList())
            {
                if (cnt == 0)
                {
                    left = selectedItem.Left;
                }
                cnt++;
                selectedItem.Left = left;
            }
            foreach (var selectedItem in itemsToAlign.OfType<SettingsDesignerItemViewModel>().ToList())
            {
                if (cnt == 0)
                {
                    left = selectedItem.Left;
                }
                cnt++;
                selectedItem.Left = left;
            }
        }

        private void ExecuteCopySelectedItemsCommand(object parameter)
        {
            itemsToCopy = DiagramViewModel.SelectedItems;
            // Check which type of item that is copied
            foreach (var selectedItem in itemsToCopy.OfType<PersistDesignerItemViewModel>().ToList())
            {
                PersistDesignerItemViewModel newPersist = new PersistDesignerItemViewModel();
                newPersist.Left = 10; // location where the copied
                newPersist.Top = 10; // location where the copied
                newPersist.HostUrl = selectedItem.HostUrl; // copy the same host url into the new one
                DiagramViewModel.AddItemCommand.Execute(newPersist);
            }
            foreach (var selectedItem in itemsToCopy.OfType<SettingsDesignerItemViewModel>().ToList())
            {
                SettingsDesignerItemViewModel newSettings = new SettingsDesignerItemViewModel();
                newSettings.Left = 10; // location where the copied
                newSettings.Top = 10; // location where the copied
                DiagramViewModel.AddItemCommand.Execute(newSettings);
            }
        }

        private void ExecuteDeleteSelectedItemsCommand(object parameter)
        {
            itemsToRemove = DiagramViewModel.SelectedItems;
            List<SelectableDesignerItemViewModelBase> connectionsToAlsoRemove = new List<SelectableDesignerItemViewModelBase>();

            foreach (var connector in DiagramViewModel.Items.OfType<ConnectorViewModel>())
            {
                if (ItemsToDeleteHasConnector(itemsToRemove, connector.SourceConnectorInfo))
                {
                    connectionsToAlsoRemove.Add(connector);
                }

                if (ItemsToDeleteHasConnector(itemsToRemove, (FullyCreatedConnectorInfo)connector.SinkConnectorInfo))
                {
                    connectionsToAlsoRemove.Add(connector);
                }

            }
            itemsToRemove.AddRange(connectionsToAlsoRemove);
            foreach (var selectedItem in itemsToRemove)
            {
                DiagramViewModel.RemoveItemCommand.Execute(selectedItem);
            }
        }

        private void ExecuteCreateNewDiagramCommand(object parameter)
        {
            //ensure that itemsToRemove is cleared ready for any new changes within a session
            itemsToRemove = new List<SelectableDesignerItemViewModelBase>();
            SavedDiagramId = null;
            DiagramViewModel.CreateNewDiagramCommand.Execute(null);
        }

        private void ExecuteSaveDiagramCommand(object parameter)
        {
            if (!DiagramViewModel.Items.Any())
            {
                messageBoxService.ShowError("There must be at least one item in order save a diagram");
                return;
            }

            IsBusy = true;
            DiagramItem wholeDiagramToSave = null;

            Task<string> task = Task.Factory.StartNew<string>(() =>
            {

                if (SavedDiagramId != null)
                {
                    string currentSavedDiagramId = SavedDiagramId;
                    wholeDiagramToSave = databaseAccessService.FetchDiagram(currentSavedDiagramId);

                    //If we have a saved diagram, we need to make sure we clear out all the removed items that
                    //the user deleted as part of this work sesssion
                    foreach (var itemToRemove in itemsToRemove)
                    {
                        DeleteFromDatabase(wholeDiagramToSave, itemToRemove);
                    }
                    //start with empty collections of connections and items, which will be populated based on current diagram
                    wholeDiagramToSave.ConnectionIds = new List<string>();
                    wholeDiagramToSave.DesignerItems = new List<DiagramItemData>();
                }
                else
                {
                    wholeDiagramToSave = new DiagramItem();
                }

                //ensure that itemsToRemove is cleared ready for any new changes within a session
                itemsToRemove = new List<SelectableDesignerItemViewModelBase>();

                SavePersistDesignerItem(wholeDiagramToSave, DiagramViewModel);

                wholeDiagramToSave.Id = databaseAccessService.SaveDiagram(wholeDiagramToSave);
                return wholeDiagramToSave.Id;
            });
            task.ContinueWith((ant) =>
            {
                string wholeDiagramToSaveId = ant.Result;
                if (!savedDiagrams.Contains(wholeDiagramToSaveId))
                {
                    List<string> newDiagrams = new List<string>(savedDiagrams);
                    newDiagrams.Add(wholeDiagramToSaveId);
                    SavedDiagrams = newDiagrams;

                }
                IsBusy = false;
                messageBoxService.ShowInformation(string.Format("Finished saving Diagram Id : {0}", wholeDiagramToSaveId));

            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void SavePersistDesignerItem(IDiagramItem wholeDiagramToSave, IDiagramViewModel diagramViewModel)
        {
            //Save all PersistDesignerItemViewModel
            foreach (var persistItemVM in diagramViewModel.Items.OfType<PersistDesignerItemViewModel>())
            {
                PersistDesignerItem persistDesignerItem = new PersistDesignerItem(persistItemVM.Id, persistItemVM.Left, persistItemVM.Top, persistItemVM.ItemWidth, persistItemVM.ItemHeight, persistItemVM.HostUrl);
                persistItemVM.Id = databaseAccessService.SavePersistDesignerItem(persistDesignerItem);
                wholeDiagramToSave.DesignerItems.Add(new DiagramItemData(persistDesignerItem.Id, typeof(PersistDesignerItem)));
            }
            //Save all SettingsDesignerItemViewModel
            foreach (var settingsItemVM in diagramViewModel.Items.OfType<SettingsDesignerItemViewModel>())
            {
                SettingsDesignerItem settingsDesignerItem = new SettingsDesignerItem(settingsItemVM.Id, settingsItemVM.Left, settingsItemVM.Top, settingsItemVM.ItemWidth, settingsItemVM.ItemHeight, settingsItemVM.Setting1);
                settingsItemVM.Id = databaseAccessService.SaveSettingDesignerItem(settingsDesignerItem);
                wholeDiagramToSave.DesignerItems.Add(new DiagramItemData(settingsDesignerItem.Id, typeof(SettingsDesignerItem)));
            }
            //Save all GroupingDesignerItemViewModel
            foreach (var groupingItemVM in diagramViewModel.Items.OfType<GroupingDesignerItemViewModel>())
            {
                GroupDesignerItem groupDesignerItem = new GroupDesignerItem(groupingItemVM.Id, groupingItemVM.Left, groupingItemVM.Top, groupingItemVM.ItemWidth, groupingItemVM.ItemHeight);
                if(groupingItemVM.Items != null && groupingItemVM.Items.Count > 0)
                {
                    SavePersistDesignerItem(groupDesignerItem, groupingItemVM);
                }
                groupingItemVM.Id = databaseAccessService.SaveGroupingDesignerItem(groupDesignerItem);
                wholeDiagramToSave.DesignerItems.Add(new DiagramItemData(groupDesignerItem.Id, typeof(GroupDesignerItem)));
            }
            //Save all connections which should now have their Connection.DataItems filled in with correct Ids
            foreach (var connectionVM in diagramViewModel.Items.OfType<ConnectorViewModel>())
            {
                FullyCreatedConnectorInfo sinkConnector = connectionVM.SinkConnectorInfo as FullyCreatedConnectorInfo;

                Connection connection = new Connection(
                    connectionVM.Id,
                    connectionVM.SourceConnectorInfo.DataItem.Id,
                    GetOrientationFromConnector(connectionVM.SourceConnectorInfo.Orientation),
                    GetTypeOfDiagramItem(connectionVM.SourceConnectorInfo.DataItem),
                    sinkConnector.DataItem.Id,
                    GetOrientationFromConnector(sinkConnector.Orientation),
                    GetTypeOfDiagramItem(sinkConnector.DataItem));

                connectionVM.Id = databaseAccessService.SaveConnection(connection);
                wholeDiagramToSave.ConnectionIds.Add(connectionVM.Id);
            }
        }

        private void ExecuteLoadDiagramCommand(object parameter)
        {
            IsBusy = true;
            DiagramItem wholeDiagramToLoad = null;
            if (SavedDiagramId == null)
            {
                messageBoxService.ShowError("You need to select a diagram to load");
                return;
            }

            Task<DiagramViewModel> task = Task.Factory.StartNew<DiagramViewModel>(() =>
            {
                //ensure that itemsToRemove is cleared ready for any new changes within a session
                itemsToRemove = new List<SelectableDesignerItemViewModelBase>();
                DiagramViewModel diagramViewModel = new DiagramViewModel();

                wholeDiagramToLoad = databaseAccessService.FetchDiagram(SavedDiagramId);

                LoadPerstistDesignerItems(wholeDiagramToLoad, diagramViewModel);

                return diagramViewModel;
            });
            task.ContinueWith((ant) =>
            {
                this.DiagramViewModel = ant.Result;
                IsBusy = false;
                messageBoxService.ShowInformation(string.Format("Finished loading Diagram Id : {0}", wholeDiagramToLoad.Id));

            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void LoadPerstistDesignerItems(IDiagramItem wholeDiagramToLoad, IDiagramViewModel diagramViewModel)
        {
            //load diagram items
            foreach (DiagramItemData diagramItemData in wholeDiagramToLoad.DesignerItems)
            {
                if (diagramItemData.ItemType == typeof(PersistDesignerItem))
                {
                    PersistDesignerItem persistedDesignerItem = databaseAccessService.FetchPersistDesignerItem(diagramItemData.ItemId);
                    PersistDesignerItemViewModel persistDesignerItemViewModel =
                        new PersistDesignerItemViewModel(persistedDesignerItem.Id, diagramViewModel, persistedDesignerItem.Left, persistedDesignerItem.Top, persistedDesignerItem.ItemWidth, persistedDesignerItem.ItemHeight, persistedDesignerItem.HostUrl);
                    diagramViewModel.Items.Add(persistDesignerItemViewModel);
                }
                if (diagramItemData.ItemType == typeof(SettingsDesignerItem))
                {
                    SettingsDesignerItem settingsDesignerItem = databaseAccessService.FetchSettingsDesignerItem(diagramItemData.ItemId);
                    SettingsDesignerItemViewModel settingsDesignerItemViewModel =
                        new SettingsDesignerItemViewModel(settingsDesignerItem.Id, diagramViewModel, settingsDesignerItem.Left, settingsDesignerItem.Top, settingsDesignerItem.ItemWidth, settingsDesignerItem.ItemHeight, settingsDesignerItem.Setting1);
                    diagramViewModel.Items.Add(settingsDesignerItemViewModel);
                }
                if (diagramItemData.ItemType == typeof(GroupDesignerItem))
                {
                    GroupDesignerItem groupDesignerItem = databaseAccessService.FetchGroupingDesignerItem(diagramItemData.ItemId);
                    GroupingDesignerItemViewModel groupingDesignerItemViewModel =
                        new GroupingDesignerItemViewModel(groupDesignerItem.Id, diagramViewModel, groupDesignerItem.Left, groupDesignerItem.Top, groupDesignerItem.ItemWidth, groupDesignerItem.ItemHeight);
                    if(groupDesignerItem.DesignerItems != null && groupDesignerItem.DesignerItems.Count > 0)
                    {
                        LoadPerstistDesignerItems(groupDesignerItem, groupingDesignerItemViewModel);
                    }
                    diagramViewModel.Items.Add(groupingDesignerItemViewModel);
                }
            }
            //load connection items
            foreach (string connectionId in wholeDiagramToLoad.ConnectionIds)
            {
                Connection connection = databaseAccessService.FetchConnection(connectionId);

                DesignerItemViewModelBase sourceItem = GetConnectorDataItem(diagramViewModel, connection.SourceId, connection.SourceType);
                ConnectorOrientation sourceConnectorOrientation = GetOrientationForConnector(connection.SourceOrientation);
                FullyCreatedConnectorInfo sourceConnectorInfo = GetFullConnectorInfo(connection.Id, sourceItem, sourceConnectorOrientation);

                DesignerItemViewModelBase sinkItem = GetConnectorDataItem(diagramViewModel, connection.SinkId, connection.SinkType);
                ConnectorOrientation sinkConnectorOrientation = GetOrientationForConnector(connection.SinkOrientation);
                FullyCreatedConnectorInfo sinkConnectorInfo = GetFullConnectorInfo(connection.Id, sinkItem, sinkConnectorOrientation);

                ConnectorViewModel connectionVM = new ConnectorViewModel(connection.Id, diagramViewModel, sourceConnectorInfo, sinkConnectorInfo);
                diagramViewModel.Items.Add(connectionVM);
            }
        }

        private void ExecuteGroupCommand(object parameter)
        {
            if (diagramViewModel.SelectedItems.Count > 0)
            {
                // if only one selected item is a Grouping item -> ungroup
                if (diagramViewModel.SelectedItems[0] is GroupingDesignerItemViewModel && diagramViewModel.SelectedItems.Count == 1)
                {
                    GroupingDesignerItemViewModel groupObject = diagramViewModel.SelectedItems[0] as GroupingDesignerItemViewModel;
                    foreach (var item in groupObject.Items)
                    {

                        if (item is DesignerItemViewModelBase)
                        {
                            DesignerItemViewModelBase tmp = (DesignerItemViewModelBase)item;
                            tmp.Top += groupObject.Top;
                            tmp.Left += groupObject.Left;
                        }
                        diagramViewModel.AddItemCommand.Execute(item);
                        item.Parent = DiagramViewModel;
                    }

                    // "cut" connections between DiagramItems and the Group
                    List<SelectableDesignerItemViewModelBase> GroupedItemsToRemove = new List<SelectableDesignerItemViewModelBase>();
                    foreach (var connector in DiagramViewModel.Items.OfType<ConnectorViewModel>())
                    {
                        if (groupObject == connector.SourceConnectorInfo.DataItem)
                        {
                            GroupedItemsToRemove.Add(connector);
                        }

                        if (groupObject == ((FullyCreatedConnectorInfo)connector.SinkConnectorInfo).DataItem)
                        {
                            GroupedItemsToRemove.Add(connector);
                        }
                    }
                    GroupedItemsToRemove.Add(groupObject);
                    foreach (var selectedItem in GroupedItemsToRemove)
                    {
                        DiagramViewModel.RemoveItemCommand.Execute(selectedItem);
                    }

                }
                else if (diagramViewModel.SelectedItems.Count > 1)
                {
                    double margin = 15;
                    Rect rekt = GetBoundingRectangle(diagramViewModel.SelectedItems, margin);

                    GroupingDesignerItemViewModel groupItem = new GroupingDesignerItemViewModel("0", this.diagramViewModel, rekt.Left, rekt.Top);
                    groupItem.ItemWidth = rekt.Width;
                    groupItem.ItemHeight = rekt.Height;
                    foreach (var item in diagramViewModel.SelectedItems)
                    {
                        if (item is DesignerItemViewModelBase)
                        {
                            DesignerItemViewModelBase tmp = (DesignerItemViewModelBase)item;
                            tmp.Top -= rekt.Top;
                            tmp.Left -= rekt.Left;
                        }
                        groupItem.Items.Add(item);
                        item.Parent = groupItem;

                    }

                    // "cut" connections between DiagramItems which are going to be grouped and
                    // Diagramitems which are not going to be grouped
                    List<SelectableDesignerItemViewModelBase> GroupedItemsToRemove = DiagramViewModel.SelectedItems;
                    List<SelectableDesignerItemViewModelBase> connectionsToAlsoRemove = new List<SelectableDesignerItemViewModelBase>();

                    foreach (var connector in DiagramViewModel.Items.OfType<ConnectorViewModel>())
                    {
                        if (ItemsToDeleteHasConnector(GroupedItemsToRemove, connector.SourceConnectorInfo))
                        {
                            connectionsToAlsoRemove.Add(connector);
                        }

                        if (ItemsToDeleteHasConnector(GroupedItemsToRemove, (FullyCreatedConnectorInfo)connector.SinkConnectorInfo))
                        {
                            connectionsToAlsoRemove.Add(connector);
                        }

                    }
                    GroupedItemsToRemove.AddRange(connectionsToAlsoRemove);
                    foreach (var selectedItem in GroupedItemsToRemove)
                    {
                        DiagramViewModel.RemoveItemCommand.Execute(selectedItem);
                    }

                    diagramViewModel.SelectedItems.Clear();
                    this.diagramViewModel.Items.Add(groupItem);
                }
            }

        }

        private static Rect GetBoundingRectangle(IEnumerable<SelectableDesignerItemViewModelBase> items, double margin)
        {
            double x1 = Double.MaxValue;
            double y1 = Double.MaxValue;
            double x2 = Double.MinValue;
            double y2 = Double.MinValue;

            foreach (DesignerItemViewModelBase item in items.OfType<DesignerItemViewModelBase>())
            {
                x1 = Math.Min(item.Left - margin, x1);
                y1 = Math.Min(item.Top - margin, y1);

                x2 = Math.Max(item.Left + item.ItemWidth + margin, x2);
                y2 = Math.Max(item.Top + item.ItemHeight + margin, y2);
            }

            return new Rect(new Point(x1, y1), new Point(x2, y2));
        }

        private FullyCreatedConnectorInfo GetFullConnectorInfo(string connectorId, DesignerItemViewModelBase dataItem, ConnectorOrientation connectorOrientation)
        {
            switch (connectorOrientation)
            {
                case ConnectorOrientation.Top:
                    return dataItem.TopConnector;
                case ConnectorOrientation.Left:
                    return dataItem.LeftConnector;
                case ConnectorOrientation.Right:
                    return dataItem.RightConnector;
                case ConnectorOrientation.Bottom:
                    return dataItem.BottomConnector;

                default:
                    throw new InvalidOperationException(
                        string.Format("Found invalid persisted Connector Orientation for Connector Id: {0}", connectorId));
            }
        }

        private Type GetTypeOfDiagramItem(DesignerItemViewModelBase vmType)
        {
            if (vmType is PersistDesignerItemViewModel)
                return typeof(PersistDesignerItem);
            if (vmType is SettingsDesignerItemViewModel)
                return typeof(SettingsDesignerItem);
            if (vmType is GroupingDesignerItemViewModel)
                return typeof(GroupDesignerItem);
            

            throw new InvalidOperationException(string.Format("Unknown diagram type. Currently only {0} and {1} are supported",
                typeof(PersistDesignerItem).AssemblyQualifiedName,
                typeof(SettingsDesignerItemViewModel).AssemblyQualifiedName
                ));

        }

        private DesignerItemViewModelBase GetConnectorDataItem(IDiagramViewModel diagramViewModel, string conectorDataItemId, Type connectorDataItemType)
        {
            DesignerItemViewModelBase dataItem = null;

            if (connectorDataItemType == typeof(PersistDesignerItem))
            {
                dataItem = diagramViewModel.Items.OfType<PersistDesignerItemViewModel>().Single(x => x.Id == conectorDataItemId);
            }

            if (connectorDataItemType == typeof(SettingsDesignerItem))
            {
                dataItem = diagramViewModel.Items.OfType<SettingsDesignerItemViewModel>().Single(x => x.Id == conectorDataItemId);
            }
            if (connectorDataItemType == typeof(GroupDesignerItem))
            {
                dataItem = diagramViewModel.Items.OfType<GroupingDesignerItemViewModel>().Single(x => x.Id == conectorDataItemId);
            }
            return dataItem;
        }


        private MyOrientation GetOrientationFromConnector(ConnectorOrientation connectorOrientation)
        {
            MyOrientation result = MyOrientation.None;
            switch (connectorOrientation)
            {
                case ConnectorOrientation.Bottom:
                    result = MyOrientation.Bottom;
                    break;
                case ConnectorOrientation.Left:
                    result = MyOrientation.Left;
                    break;
                case ConnectorOrientation.Top:
                    result = MyOrientation.Top;
                    break;
                case ConnectorOrientation.Right:
                    result = MyOrientation.Right;
                    break;
            }
            return result;
        }


        private ConnectorOrientation GetOrientationForConnector(MyOrientation persistedOrientation)
        {
            ConnectorOrientation result = ConnectorOrientation.None;
            switch (persistedOrientation)
            {
                case MyOrientation.Bottom:
                    result = ConnectorOrientation.Bottom;
                    break;
                case MyOrientation.Left:
                    result = ConnectorOrientation.Left;
                    break;
                case MyOrientation.Top:
                    result = ConnectorOrientation.Top;
                    break;
                case MyOrientation.Right:
                    result = ConnectorOrientation.Right;
                    break;
            }
            return result;
        }

        private bool ItemsToDeleteHasConnector(List<SelectableDesignerItemViewModelBase> itemsToRemove, FullyCreatedConnectorInfo connector)
        {
            return itemsToRemove.Contains(connector.DataItem);
        }



        private void DeleteFromDatabase(DiagramItem wholeDiagramToAdjust, SelectableDesignerItemViewModelBase itemToDelete)
        {

            //make sure the item is removes from Diagram as well as removing them as individual items from database
            if (itemToDelete is PersistDesignerItemViewModel)
            {
                DiagramItemData diagramItemToRemoveFromParent = wholeDiagramToAdjust.DesignerItems.Where(x => x.ItemId == itemToDelete.Id && x.ItemType == typeof(PersistDesignerItem)).Single();
                wholeDiagramToAdjust.DesignerItems.Remove(diagramItemToRemoveFromParent);
                databaseAccessService.DeletePersistDesignerItem(itemToDelete.Id);
            }
            if (itemToDelete is SettingsDesignerItemViewModel)
            {
                DiagramItemData diagramItemToRemoveFromParent = wholeDiagramToAdjust.DesignerItems.Where(x => x.ItemId == itemToDelete.Id && x.ItemType == typeof(SettingsDesignerItem)).Single();
                wholeDiagramToAdjust.DesignerItems.Remove(diagramItemToRemoveFromParent);
                databaseAccessService.DeleteSettingDesignerItem(itemToDelete.Id);
            }
            if (itemToDelete is ConnectorViewModel)
            {
                wholeDiagramToAdjust.ConnectionIds.Remove(itemToDelete.Id);
                databaseAccessService.DeleteConnection(itemToDelete.Id);
            }

            databaseAccessService.SaveDiagram(wholeDiagramToAdjust);


        }

    }
}
