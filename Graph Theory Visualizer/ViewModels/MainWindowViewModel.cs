using System;
using System.Collections.Generic;
using System.Linq;
//using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Graph_Theory_Visualizer.Dialogs;
using Graph_Theory_Visualizer.Dialogs.Service;
using Graph_Theory_Visualizer.Dialogs.DirectedUndirected;
using SimpleWPF.Input;

namespace Graph_Theory_Visualizer.ViewModels
{
    public class MainWindowViewModel
    {
        private IDialogService _dialogService;

        public ICommand DirectedUndirectedCommand { get; private set; }
        public DialogResults to_return;

        public MainWindowViewModel()
        {
            //Normally we would do this with dependency injection
            _dialogService = new DialogService();

            DirectedUndirectedCommand = new RelayCommand(DirectedUndirected);
        }

        private void DirectedUndirected()
        {
            var dialog = new DirectedUndirectedDialogViewModel("Add Edge", "Directed or Undirected");
            var result = _dialogService.OpenDialog(dialog);

            to_return = result;
            Console.WriteLine(result);
        }
    }
}
