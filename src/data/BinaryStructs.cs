using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace eric.coreminimal.data
{
    //In project.json, add to BuildOption "allowUnsafe" :  true
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct DemoBinary
    {
        public fixed byte Name[80];
        public int ifield1;
        public int ifield2;
        public double VolumeMin;
        public double VolumeMax;
        public fixed double FixedVolumes[11];
    }

    public class BinaryFortranInterchange
    {
        public static T FromBinaryReaderBlock<T>(BinaryReader br) where T : struct
        {
            //Read byte array

            byte[] buff = br.ReadBytes(Marshal.SizeOf<T>());
            //Make sure that the Garbage Collector doesn't move our buffer 

            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            //Marshal the bytes

            T s = (T)Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();//Give control of the buffer back to the GC 

            return s;
        }
        public static T GetStruct<T>(string FilePath, string FileName) where T : struct
        {
            T item;
            using (FileStream fs = new FileStream(Path.Combine(FilePath, FileName), FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
                item = FromBinaryReaderBlock<T>(br);
            return item;
        }
        public static T[] GetStruct<T>(string FilePath, string FileName, int nItems) where T : struct
        {
            T[] items = new T[nItems];
            using (FileStream fs = new FileStream(Path.Combine(FilePath, FileName), FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
            {
                for (int i = 0; i < nItems; i++)
                {
                    items[i] = FromBinaryReaderBlock<T>(br);
                }
            }
            return items;
        }
        public static T[,] GetStruct<T>(string FilePath, string FileName, int nItems, int nPoints) where T : struct
        {
            T[,] items = new T[nItems, nPoints];
            using (FileStream fs = new FileStream(Path.Combine(FilePath, FileName), FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
            {
                for (int j = 0; j < nPoints; j++)
                {
                    for (int i = 0; i < nItems; i++)
                    {
                        items[i, j] = FromBinaryReaderBlock<T>(br);
                    }
                }
            }
            return items;
        }
        public static void ToBinaryWriterBloc<T>(T item, BinaryWriter bw) where T : struct
        {
            byte[] buff = new byte[Marshal.SizeOf<T>()]; 
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);//Hands off GC

            //Marshal the structure
            Marshal.StructureToPtr(item, handle.AddrOfPinnedObject(), false);
            handle.Free();
            bw.Write(buff);
        }
        public static void WriteStruct<T>(T item, string FilePath, string FileName) where T : struct
        {
            using (FileStream fs = new FileStream(Path.Combine(FilePath, FileName), FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fs))
                ToBinaryWriterBloc<T>(item, bw);
        }
        public static void WriteStructNTimes<T>(T item, string FilePath, string FileName, int N) where T : struct
        {
            using (FileStream fs = new FileStream(Path.Combine(FilePath, FileName), FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fs))
                for (int i = 0; i < N; i++)
                    ToBinaryWriterBloc<T>(item, bw);
        }
        public static void WriteStruct<T>(T[] items, string FilePath, string FileName) where T : struct
        {
            int bound0 = items.GetUpperBound(0);
            using (MemoryStream ms = new MemoryStream((bound0 + 1) * Marshal.SizeOf<T>()))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    for (int i = 0; i < bound0 + 1; i++)
                    {
                        ToBinaryWriterBloc<T>(items[i], bw);
                    }
                }

                using (FileStream fs = new FileStream(Path.Combine(FilePath, FileName), FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(ms.ToArray());
                    }
                }
            }

        }
        private static void WriteStruct<T>(T[,] items, string FilePath, string FileName) where T : struct
        {
            int bound1 = items.GetUpperBound(1);
            int bound0 = items.GetUpperBound(0);
            using (MemoryStream ms = new MemoryStream((bound0 + 1) * (bound1 + 1) * Marshal.SizeOf<T>()))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {

                    for (int j = 0; j < bound1 + 1; j++)
                    {
                        for (int i = 0; i < bound0 + 1; i++)
                        {
                            ToBinaryWriterBloc<T>(items[i, j], bw);
                        }
                    }
                }

                using (FileStream fs = new FileStream(Path.Combine(FilePath, FileName), FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(ms.ToArray());
                    }
                }
            }
        }

        public static unsafe void FromBinary(string FilePath, string FileName, int nItems)
        {
            DemoBinary[] Bs = GetStruct<DemoBinary>(FilePath, FileName, nItems);
            for (int i = 0; i < nItems; i++)
            {
                DemoBinary b = Bs[i];
                char[] temp = new char[80];
                fixed (char* p = temp)
                {
                    Encoding.UTF8.GetChars((byte*)b.Name, 80, p, 80);
                }
                var BasinName = new string(temp).TrimEnd('\0').TrimEnd(' ');
                Console.WriteLine(BasinName + Environment.NewLine + b.FixedVolumes[5]); 
            }
        }
        public static unsafe void ToBinary(string FilePath, string FileName, int nItems )
        {
            DemoBinary[] Bs = new DemoBinary[nItems];
            fixed (DemoBinary* b = &Bs[0])
            {
                for (int i = 0; i < nItems; i++)
                {
                    string BasinName = "BasinName" + i.ToString();
                    //Prepping the 
                    byte[] temp = new byte[80];
                    for (int k = 0; k < 80; k++)
                        temp[k] = 32;
                    Encoding.UTF8.GetBytes(BasinName, 0, BasinName.Length, temp, 0);
                    for (int k = 0; k < 80; k++)
                        b[i].Name[k] = temp[k];
                    b[i].ifield1 = 1;
                    b[i].ifield2 = i;
                    b[i].VolumeMin = 10.0;
                    b[i].VolumeMax = 100.0 * (i+1);
                    for (int j = 0; j < 11; j++)
                        b[i].FixedVolumes[j] = ((b[i].VolumeMin + b[i].VolumeMax) / (10.0)) * j; 
                }
            }

            WriteStruct<DemoBinary>(Bs, FilePath, FileName); 

        }
    }
}
