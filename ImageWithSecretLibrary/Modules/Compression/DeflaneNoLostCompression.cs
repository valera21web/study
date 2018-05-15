using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    

    public class DeflaneNoLostCompression
    {
        
        public static void Compress(string sourceFile, string compressedFile)
    {
           
                                                                              // stream reading the main file
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
        {
            // string writing the main file
            using (FileStream targetStream = File.Create(compressedFile))
            {
                // GZip stream
                using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                {
                    sourceStream.CopyTo(compressionStream); // Copying bytes from one stream to another
                    Console.WriteLine("Zipping {0} done succesfully. Original weight of file: {1}  Compress: {2}.",
                        sourceFile, sourceStream.Length.ToString(), targetStream.Length.ToString());
                }
            }
        }
    }

    public static void Decompress(string compressedFile, string targetFile)
    {
        // stream reading from compressed file
        using (FileStream sourceStream = new FileStream(compressedFile, FileMode.OpenOrCreate))
        {
            // stream writing from restore file
            using (FileStream targetStream = File.Create(targetFile))
            {
                // decompression stream
                using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(targetStream);
                    Console.WriteLine("Decompressed file: {0}", targetFile);
                }
            }
        }

    }
}
}
