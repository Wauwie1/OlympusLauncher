using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Threading.Tasks;
using Xdelta;

namespace OlympusLauncher
{
    class DeltaPatcher
    {
        public static void Patch(string sourceName, string patchName, string targetName)
        {

            using (FileStream source = OpenForRead(sourceName))
            using (FileStream patch = OpenForRead(patchName))
            using (FileStream target = CreateForWriteAndRead(targetName))
                new Decoder(source, patch, target).Run();
        }

        private static FileStream OpenForRead(string filePath)
        {
            return new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,   // Default buffer, it seems bigger does not affect time
                FileOptions.RandomAccess);
        }

        private static FileStream CreateForWriteAndRead(string filePath)
        {
            return new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Read,
                4096,   // Default buffer, it seems bigger does not affect time
                FileOptions.RandomAccess);
        }
    }
}
