using Graph_Theory_Visualizer.Dialogs.Service;
using SimpleWPF.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace Graph_Theory_Visualizer.Dialogs.DirectedUndirected
{
    public class DirectedUndirectedDialogViewModel : DialogViewModelBase<DialogResults>
    {
        public ICommand DirectedCommand { get; private set; }
        public ICommand UndirectedCommand { get; private set; }

        public DirectedUndirectedDialogViewModel(string title, string message) : base(title, message)
        {
            DirectedCommand = new RelayCommand<IDialogWindow>(Directed);
            UndirectedCommand = new RelayCommand<IDialogWindow>(Undirected);
        }

        private void Directed(IDialogWindow window)
        {
            CloseDialogWithResult(window, DialogResults.directed);
        }

        private void Undirected(IDialogWindow window)
        {
            CloseDialogWithResult(window, DialogResults.undirected);
        }
    }
}
