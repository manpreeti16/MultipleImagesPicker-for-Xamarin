using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace App7.DependencyServices
{
    public interface ICompressMediaFile
    {
        byte[] GetCompressedBytes(FileStream fs);

        object CompressVideoBytes(object sourceFilePath);
      
    }
}
