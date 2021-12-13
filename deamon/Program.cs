using System.IO.MemoryMappedFiles;
using System.Linq;

namespace deamon {
    class deamonapp {

        static string filePath;
        static string searchedWord;
        static bool termination = false;
        static int proccessID;
        static Int64 beginLine;
        static Int64 endLine;

        static List<Tuple<int, int>> indexes;

        static int progress = 0;
        static Int64 found = 0;

        static MemoryMappedFile foundGate;
        static MemoryMappedViewAccessor foundAccessor;

        static MemoryMappedFile progressGate;
        static MemoryMappedViewAccessor progressAccessor;

        static Mutex mutex;
        public static byte[] ReadMMFAllBytes(string fileName)
        {
            using (var mmf = MemoryMappedFile.OpenExisting(fileName))
            {
                using (var stream = mmf.CreateViewStream())
                {
                    using (BinaryReader binReader = new BinaryReader(stream))
                    {
                        int c = 0;
                        var a = binReader.ReadBytes((int)stream.Length);
                        List<byte> b = new List<byte>();
                        foreach (var d in a) 
                        {
                            if (c > 3)
                                break;

                             if (d == (byte)'\0')
                                    c++;
                            else 
                            {
                                c = 0;
                                b.Add(d);
                            }
                        }

                        return b.ToArray();
                    }
                }
            }
        }



        public static void WriteStatusAndProgress() { 
            mutex.WaitOne();
            Console.WriteLine($"Setting {progress} {found}");

            if (foundAccessor.CanWrite)
            {
                var b = BitConverter.GetBytes(found);
                foundAccessor.WriteArray<byte>(0, b, 0, b.Length);
            }
            else
            { 
                termination = true;
            }

            if (progressAccessor.CanWrite)
            {
                var b = BitConverter.GetBytes(progress);
                progressAccessor.WriteArray<byte>(0, b, 0, b.Length);
            }
            else
            {
                termination = true;
            }

            mutex.ReleaseMutex();
        }

        public static void Seek(string[] src, string t) {
            for(int i = 0; i < src.Length; i++) 
            {
                var lj = 0;
                while ((lj = src[i].IndexOf(t, lj + 1)) != -1)
                    indexes.Append(new Tuple<int, int>(i, lj));
            }
        }

        public static void Main(String[] args)
        {
            proccessID = int.Parse(args[0]);
            mutex = new Mutex(false, proccessID + "Mutex");

            beginLine = Int32.Parse(args[1]);
            endLine = Int32.Parse(args[2]);
            //proccessID = 0;
            //beginLine = 0;
            //endLine = 19;
            Console.WriteLine($"{proccessID}, {beginLine}, {endLine}");

            foundGate = MemoryMappedFile.OpenExisting(proccessID + "FoundMMF");
            foundAccessor = foundGate.CreateViewAccessor();
            progressGate = MemoryMappedFile.OpenExisting(proccessID + "ProgressMMF");
            progressAccessor = progressGate.CreateViewAccessor();

            var a = ReadMMFAllBytes("FilePathMMF");
            filePath += System.Text.Encoding.Default.GetString(a);
            searchedWord = System.Text.Encoding.Default.GetString(ReadMMFAllBytes("SearchedWordMMF"));
            Console.WriteLine($"{filePath}");

            var line = File.ReadLines(filePath).Skip((int)beginLine).Take((int)(endLine - beginLine)).ToArray();
            
        }

    }
}