using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Threading;

namespace WpfApp2
{

    public class VMDeamonProcess : INotifyPropertyChanged
    {
        public VMDeamonProcess(string pn = "", uint progress = 0) 
        {
            this.ProcessName = pn;
            this.Progress = progress;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private uint _progress = 0;
        public uint Progress 
        {
            get { return _progress; }
            set { 
                _progress = value; 
                OnPropertyChanged("Progress");
            }
        }

        private string _processName = string.Empty;
        public string ProcessName 
        {
            get { return _processName; }
            set { 
                _processName = value;
                OnPropertyChanged("processName");
            }
        }


        private bool _isRunning = false;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                this._isRunning = value;
                OnPropertyChanged("isRunning");
            }
        }

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }


    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    // 1. Serching algorythm (begins from, ends at)
    // 2. Writing status to MMF
    // 3. Reading statuses from MMF to ObservableCollection
    // 4. Show it as datagrid
    public partial class MainWindow : Window
    {

        public ObservableCollection<VMDeamonProcess> VMDeamons { get; set; }

        private string filePath = "";

        public MainWindow()
        {
            VMDeamons = new ObservableCollection<VMDeamonProcess>();
            
            for (int i = 0; i < 10; i++ )
                VMDeamons.Add(new VMDeamonProcess($"Process {i}", (uint)i*10));

            DataContext = this;

            InitializeComponent();

        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(2000);
            VMDeamons[3].Progress = 45;
        }
    }
}
