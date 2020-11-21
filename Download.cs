using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Corona {
    public static class Download {

        /// <summary>
        /// Gets a file and caches it in the users temp path on a daily basis
        /// </summary>
        /// <param name="u">Uri to the file</param>
        /// <returns>Path to the cached file</returns>
        public static async Task<string> GetCachedAsync(Uri u) {
            string f = $@"{Path.GetTempPath()}{DateTime.Today:yyMMdd}.{u.Segments[^1]}";

            FileInfo fi = new FileInfo(f);
            if(!fi.Exists || fi.Length == 0)

                // Create file
                using(HttpClient cli = new HttpClient())
                    using(FileStream fs = File.Create(f)) {

                        // Create a FileInfo object to set the file's attributes
                        fi = new FileInfo(f);

                        // Set the Attribute property of this file to Temporary. 
                        // Although this is not completely necessary, the .NET Framework is able 
                        // to optimize the use of Temporary files by keeping them cached in memory.
                        fi.Attributes = FileAttributes.Temporary;

                        // Dowload file from RKI into tempoary file
                        HttpResponseMessage rm = await cli.GetAsync(u);
                        Stream stm = await rm.Content.ReadAsStreamAsync();
                        await stm.CopyToAsync(fs);
                    }

            return f;
        }

        /// <summary>
        /// Gets a file and caches it in the users temp path on a daily basis
        /// </summary>
        /// <param name="sUri">Uri to the file as string</param>
        /// <returns>Path to the cached file</returns>
        public static async Task<string> GetCachedAsync(string sUri) => await GetCachedAsync(new Uri(sUri));
    }
}