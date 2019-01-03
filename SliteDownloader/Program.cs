using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SliteDownloader
{
    class Program
    {
        private static string SliteFolderURL = @"https://storage.googleapis.com/slite-api-files-production";

        private static string urlRegEx =
            @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?";

       
        static void Main(string[] args)
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            //Create Download Folder
            var downloadFolder = System.IO.Path.Combine(currentDirectory, "Downloaded Files");
          ;

            Console.WriteLine("I am a Slite Back Up Downloader. Put me into the root folder of the Slite Extracted database, containing Markdown or text files.");
            Console.WriteLine("Do you want to proceed?");
            if (!GetYorN())
            {
                return;
            }

            //Get all md files from the execution folder
            List<string> mdFiles = System.IO.Directory
                .EnumerateFiles(currentDirectory, "*.md", SearchOption.AllDirectories).ToList();
            List<string> txtFiles = System.IO.Directory
                .EnumerateFiles(currentDirectory, "*.txt", SearchOption.AllDirectories).ToList();
            mdFiles = mdFiles.Concat(txtFiles).ToList();

            //Read all md files
            List<string> links = new List<string>();
            foreach (string mdFile in mdFiles)
            {
                string content = File.ReadAllText(mdFile);

                //Detect potential file links
                links.AddRange(GetFileLinks(content));
            }

       

            Console.WriteLine($"{links.Count} file links detected.");
            //links.ForEach(x=>Console.WriteLine(x));

            Console.WriteLine("Do you want to download those files?");

            if (GetYorN())
            {
                //clean current folder
                if(Directory.Exists(downloadFolder)) System.IO.Directory.Delete(downloadFolder, true);
                System.IO.Directory.CreateDirectory(downloadFolder);

                Dictionary<string, string> ReplacementDictionary = new Dictionary<string, string>();

                int counter = 0;

                Console.WriteLine("Downloading in progress, do not quit.");
                using (var client = new WebClient())
                {
                    foreach (string mdFile in mdFiles)
                    {
                        string currentChannel = (new DirectoryInfo(Path.GetDirectoryName(mdFile))).Name;
                        string currentMDFileName = Path.GetFileNameWithoutExtension(mdFile);

                        string content = File.ReadAllText(mdFile);
                        var fileLinks = GetFileLinks(content);

                        foreach (string fileLink in fileLinks)
                        {
                            Uri uri = new Uri(fileLink);

                            string filename = System.IO.Path.GetFileName(uri.Segments.Last());
                            var decoded = WebUtility.UrlDecode(filename);
                            decoded = WebUtility.UrlDecode(decoded);

                            string filePath = Path.Combine(downloadFolder, currentChannel, currentMDFileName, decoded);

                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                            //Check if file already exists
                            if (File.Exists(filePath))
                            {
                                filePath = AutoIndentFile(filePath);
                            }

                            try
                            {
                                client.DownloadFile(uri, filePath);
                                ReplacementDictionary.Add(uri.ToString(), filePath);
                                counter += 1;
                                Console.Write(".");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Couldn't download file {uri}. {ex.Message}");
                            }


                        }

                    }
                }

                Console.WriteLine($"{counter} out of {links.Count} files succesfully downloaded.");
                Console.WriteLine("Do you want to replace links inside source files by local links?");

                if (GetYorN())
                {
                    foreach (string mdFile in mdFiles)
                    {
                        string content = File.ReadAllText(mdFile);
                        foreach (KeyValuePair<string, string> pair in ReplacementDictionary)
                        {
                            content = content.Replace(pair.Key, pair.Value);
                        }

                        File.WriteAllText(mdFile, content);
                    }
                }
            }


            Console.WriteLine("Press any key to exit");
            Console.Read();
        }

        /// <summary>
        /// Find Slite file links into markdown files
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        static List<string> GetFileLinks(string content)
        {
            Regex linksRX = new Regex(urlRegEx);
            List<string> links = new List<string>();

            if (linksRX.IsMatch(content))
            {
                foreach (Match match in linksRX.Matches(content))
                {
                    links.Add(match.Groups[0].Value);
                }

            }

            return links.Where(x=>x.StartsWith(SliteFolderURL)).ToList();
        }

        /// <summary>
        /// Get a Y/N answer from the console
        /// </summary>
        /// <returns></returns>
        static bool GetYorN()
        {
            ConsoleKey response; // Creates a variable to hold the user's response.

            do
            {
                while (Console.KeyAvailable) // Flushes the input queue.
                    Console.ReadKey();

                Console.Write("Y or N? "); // Asks the user to answer with 'Y' or 'N'.
                response = Console.ReadKey().Key; // Gets the user's response.
                Console.WriteLine(); // Breaks the line.
            } while (response != ConsoleKey.Y && response != ConsoleKey.N); // If the user did not respond with a 'Y' or an 'N', repeat the loop.

            /* 
             * Return true if the user responded with 'Y', otherwise false.
             * 
             * We know the response was either 'Y' or 'N', so we can assume 
             * the response is 'N' if it is not 'Y'.
             */
            return response == ConsoleKey.Y;
        }

        /// <summary>
        /// Increment the file path if the folder already contains a file with the same name
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static string AutoIndentFile(string filePath)
        {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string path = Path.GetDirectoryName(filePath);
            string newFullPath = filePath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }

            return newFullPath;
        }

    }
}