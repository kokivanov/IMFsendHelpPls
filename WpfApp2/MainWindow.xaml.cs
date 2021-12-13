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
using System.IO.MemoryMappedFiles;
using System.IO;
using Microsoft.Win32;

namespace WpfApp2
{

    public class VMDeamonProcess : INotifyPropertyChanged
    {

        private MemoryMappedFile _gate;
        private MemoryMappedViewAccessor _accessor;
        private Mutex _mutex;

        public VMDeamonProcess(string pn, uint progress = 0) 
        {
            this._processName = pn;

            _gate = MemoryMappedFile.CreateNew(pn + "mmf", sizeof(Int64));
            _accessor = _gate.CreateViewAccessor();
            _mutex = new Mutex(true, pn + "mutex");
        }

        ~VMDeamonProcess() {
            _accessor.Dispose();
            _gate.Dispose();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private Int64 _progress;
        public Int64 Progress
        {
            get
            {
                return _progress;
            }

            set 
            {
                this._progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public void Update() 
        {
            byte[] bytes = new byte[sizeof(Int64)];
            _mutex.WaitOne();
            _accessor.ReadArray(0, bytes, 0, bytes.Length);
            _mutex.ReleaseMutex();
            Int64 text = BitConverter.ToInt64(bytes, 0);
            this.Progress = text;
        }

        private string _processName = string.Empty;
        public string ProcessName 
        {
            get { return _processName; }
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

  

    public class VMMain : INotifyPropertyChanged {
        public ObservableCollection<VMDeamonProcess> VMDeamons { get; set; }

        private int _overallStatus = 0;
        public int OverallStatus { 
            get { return _overallStatus; }
            set { _overallStatus = value; OnPropertyChanged("OverallStatus"); }
        }

        public VMMain() 
        {
            FilePathTextBox = "File path here...";
            FilePathTextBoxIsUnlocked = false;
        }

        private string _filePathTextBox;
        public string FilePathTextBox
        {
            get => _filePathTextBox;
            set { _filePathTextBox = value; OnPropertyChanged("FilePathTextBox"); }
        }

        private bool _filePathTextBoxIsUnlocked;
        public bool FilePathTextBoxIsUnlocked
        {
            get => _filePathTextBoxIsUnlocked;
            set { _filePathTextBoxIsUnlocked = value; OnPropertyChanged("FilePathTextBoxIsUnlocked"); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    public partial class MainWindow : Window
    {
        private MemoryMappedFile _filePathMMF;
        private MemoryMappedViewAccessor _filePathMMFaccessor;

        public VMMain MainVM { get; set; }

        public MainWindow()
        {
            MainVM = new VMMain();

            MainVM.VMDeamons = new ObservableCollection<VMDeamonProcess>();

            DataContext = MainVM;

            InitializeComponent();

        }

        ~MainWindow() {
            _filePathMMF.Dispose();
            _filePathMMFaccessor.Dispose(); 
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ChooseFileButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            MainVM.FilePathTextBoxIsUnlocked = true;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != null)
            {
                MainVM.FilePathTextBox = openFileDialog.FileName;
            }

        }

        private void StartSearch() {
            
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Datagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
