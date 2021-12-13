using System.IO.MemoryMappedFiles;

namespace deamon {
    class deamonapp {

        //TODO: Read proccess id from args and file from mmf
        public static void main(String[] args)
        {

            using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("Process 0mmf"))
            {
                bool mutexCreated;
                Mutex mutex = new Mutex(true, "testmapmutex", out mutexCreated);
                using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write((Int64)59);
                }
                mutex.ReleaseMutex();

            }

        }

    }
}