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

        private MemoryMappedFile _foundGate; // MMF for amount of words found
        private MemoryMappedViewAccessor _foundAccessor; // 

        private MemoryMappedFile _progressGate; // Percent of data that have been computed
        private MemoryMappedViewAccessor _progressAccessor;

        private Mutex _mutex;

        public VMDeamonProcess(int pn) 
        {
            this._processName = pn;

            _foundGate = MemoryMappedFile.CreateNew(pn + "FoundMMF", sizeof(Int64));
            _foundAccessor = _foundGate.CreateViewAccessor();
            _progressGate = MemoryMappedFile.CreateNew(pn + "ProgressMMF", sizeof(int));
            _progressAccessor = _progressGate.CreateViewAccessor();
            _mutex = new Mutex(true, pn + "Mutex");
            _mutex.ReleaseMutex();


        }

        ~VMDeamonProcess() {
            _foundAccessor.Dispose();
            _foundGate.Dispose();
            _mutex.ReleaseMutex();
            _mutex?.Dispose();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private int _progress;
        public int Progress
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

        public VMDeamonProcess Update() 
        {
            byte[] bytes1 = new byte[sizeof(int)];
            byte[] bytes2 = new byte[sizeof(Int64)];
            _mutex.WaitOne();
            _progressAccessor.ReadArray(0, bytes1, 0, bytes1.Length);
            _foundAccessor.ReadArray(0, bytes2, 0, bytes2.Length);
            _mutex.ReleaseMutex();
            int text1 = BitConverter.ToInt32(bytes1, 0);
            Int64 text2 = BitConverter.ToInt64(bytes2, 0);
            this.Progress = text1;
            this.Found = text2;
            if (Progress == 100 || Progress == -1)
                IsRunning = false;
            return this;
        }

        private int _processName;
        public int ProcessName 
        {
            get { return _processName; }
        }

        private Int64 _found;
        public Int64 Found
        {
            get { return _found; }
            set 
            { 
                _found = value;
                OnPropertyChanged("Found");
            }
        }


        private bool _isRunning = true;
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

        private bool _isPathValid = false; //!TODO Link to Start button enabled
        public bool IsPathValid { 
            get { return _isPathValid; } 
            set { _isPathValid = value; OnPropertyChanged("IsPathValid"); }
        }

        public void UpdateOverall() {
            _overallProgress = 0;
            foreach (var i in VMDeamons)
            {
                i.Update();
                _overallProgress += i.Progress / VMDeamons.Count;
            }
            OverallProgress = _overallProgress;
        }

        private int _overallProgress = 0;
        public int OverallProgress
        { 
            get { return _overallProgress; }
            set { _overallProgress = value; OnPropertyChanged("OverallProgress"); }
        }

        public VMMain() 
        {
            FilePathTextBox = "";
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

        private MemoryMappedFile _serchedwordMMF;
        private MemoryMappedViewAccessor _serchedwordMMFaccessor;

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
            _serchedwordMMF.Dispose();
            _serchedwordMMFaccessor.Dispose();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartSearch();
        }

        private async Task<bool> isDone() {
            return MainVM.VMDeamons.All(x => x.Update().Progress == 100);
        }

        private async void whenAllDone()  // Asynchronous wait till all deamons finish work
        {
            while (!(isDone()).Result) { 
                Thread.Sleep(250);
            }
            allDone();
        }

        private async void allDone() // !TODO: Notify user how many repetitions were found
        { 
            
        }

        private void ChooseFileButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            MainVM.FilePathTextBoxIsUnlocked = true;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "txt files (*.txt)|*.txt";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != null)
            {
                if (MainVM.FilePathTextBox != openFileDialog.FileName && (MainVM.FilePathTextBox == "" || MainVM.FilePathTextBox == "c:\\"))
                    MainVM.FilePathTextBox = openFileDialog.FileName;
            }

            validateUserEntry();
        }

        private void validateUserEntry() // Validates file path and unlock Start button
        {
            if (!File.Exists(MainVM.FilePathTextBox))
            {
                MainVM.IsPathValid = false;
                string messageBoxText = "Can't find file in provided path.";
                string caption = "Invalid file path";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result;

                result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            }
            else 
            {
                MainVM.IsPathValid = true;
            }
            
        }

        private void StartSearch() { // Called when Start button pressed
            if (!MainVM.IsPathValid) {
                return;
            }

            var buff1 = Encoding.Default.GetBytes(MainVM.FilePathTextBox);
            _filePathMMF = MemoryMappedFile.CreateNew("FilePathMMF", buff1.Length);
            _filePathMMFaccessor = _filePathMMF.CreateViewAccessor();

            var buff2 = Encoding.Default.GetBytes(MainVM.FilePathTextBox); // !TODO: Change to searched word
            _serchedwordMMF = MemoryMappedFile.CreateNew("SearchedWordMMF", buff2.Length);
            _serchedwordMMFaccessor = _serchedwordMMF.CreateViewAccessor();

            GC.KeepAlive(_filePathMMF);
            GC.KeepAlive(_filePathMMFaccessor);
            GC.KeepAlive(_serchedwordMMF);
            GC.KeepAlive(_serchedwordMMFaccessor);

            _filePathMMFaccessor.WriteArray<char>(0, MainVM.FilePathTextBox.ToCharArray(), 0, MainVM.FilePathTextBox.Count());
            _serchedwordMMFaccessor.WriteArray<char>(0, MainVM.FilePathTextBox.ToCharArray(), 0, MainVM.FilePathTextBox.Count());


            // !TODO Divide selected file by lines fill in collection
            MainVM.VMDeamons.Add(new VMDeamonProcess(0));
        }

        private void ProgressBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (var i in MainVM.VMDeamons)
                i.Update();
        }
    }
}
