using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWISEWemExtractor
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string outputPath = Path.GetDirectoryName(args[0]) + "\\" + Path.GetFileNameWithoutExtension(Path.GetFileName(args[0])) + "_bnk_extracted" + "\\";
            Directory.CreateDirectory(outputPath);

            using (FileStream fs = new FileStream(args[0], FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                List<uint> wemIDs = new List<uint>();
                List<uint> wemOffsets = new List<uint>();
                List<uint> wemSizes = new List<uint>();

                int i = 0;
                int wemCount = -1;
                while (reader.BaseStream.Position < reader.BaseStream.Length - 4)
                {
                    i++;
                    string ChunkID = UTF8Encoding.UTF8.GetString(reader.ReadBytes(4));

                    if (ChunkID == "AKPK")
                    {
                        Console.WriteLine("AudioKinect Pack Chunk");
                        int ChunkSize = reader.ReadInt32();
                        reader.ReadBytes(ChunkSize);
                    }
                    else if (ChunkID == "BKHD")
                    {
                        Console.WriteLine("BNK Header Chunk");
                        int ChunkSize = reader.ReadInt32();
                        reader.ReadBytes(ChunkSize);
                    }
                    else if(ChunkID == "DATA")
                    {
                        int ChunkSize = reader.ReadInt32();
                        long baseOffset = reader.BaseStream.Position;

                        if (wemCount == -1)
                        {
                            Console.WriteLine("No WEM files or invalid DIDX chunk. Quitting.");
                            return;

                        }
                        else
                        {

                            for (int w = 0; w < wemCount; w++)
                            {
                                reader.BaseStream.Seek(baseOffset + (long)wemOffsets[w], SeekOrigin.Begin);
                                File.WriteAllBytes(outputPath + wemIDs[w].ToString() + ".wem", reader.ReadBytes((int)wemSizes[w]));

                            }

                        }



                    }
                    else if (ChunkID == "DIDX")
                    {
                        Console.WriteLine("WEM Info Chunk");
                        int ChunkSize = reader.ReadInt32();
                        wemCount = ChunkSize / 12;
                        for (int w = 0; w < wemCount; w++)
                        {
                            wemIDs.Add(reader.ReadUInt32());
                            wemOffsets.Add(reader.ReadUInt32());
                            wemSizes.Add(reader.ReadUInt32());
                        }

                    }

                    
                }
            }

            Console.WriteLine("Done!");
            Console.ReadLine();

        }
    }
}
