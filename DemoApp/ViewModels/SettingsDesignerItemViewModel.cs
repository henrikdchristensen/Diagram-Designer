using System;
using DiagramDesigner;
using System.Windows.Input;

namespace DemoApp
{
    public class SettingsDesignerItemViewModel : DesignerItemViewModelBase, ISupportDataChanges
    {
        private IUIVisualizerService visualiserService;

        public SettingsDesignerItemViewModel(string id, IDiagramViewModel parent, double left, double top, string setting1)
            : base(id, parent, left, top)
        {

            this.Setting1 = setting1;
            Init();
        }

        public SettingsDesignerItemViewModel(string id, IDiagramViewModel parent, double left, double top, double itemWidth, double itemHeight, string setting1)
             : base(id, parent, left, top, itemWidth, itemHeight)
        {

            this.Setting1 = setting1;
            Init();
        }

        public SettingsDesignerItemViewModel()
        {
            Init();
        }

        public String Setting1 { get; set; }
        public ICommand ShowDataChangeWindowCommand { get; private set; }

        public void ExecuteShowDataChangeWindowCommand(object parameter)
        {
            SettingsDesignerItemData data = new SettingsDesignerItemData(Setting1);
            if (visualiserService.ShowDialog(data) == true)
            {
                this.Setting1 = data.Setting1;
            }
        }

        private void Init()
        {
            visualiserService = ApplicationServicesProvider.Instance.Provider.VisualizerService;
            ShowDataChangeWindowCommand = new SimpleCommand(ExecuteShowDataChangeWindowCommand);
            this.ShowConnectors = false;
        }
    }
}
