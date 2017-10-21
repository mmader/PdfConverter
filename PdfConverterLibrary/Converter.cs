using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ghostscript.NET;

namespace PdfConverterLibrary
{
    public class Converter
    {
		/* PdfConvertLibrary
		 * Pass a list of pdfFiles to convert into PostScript
		 * Call StartConvert() to start the process.
		 */

		#region Properties
		public GhostscriptVersionInfo[] Versions			 { get; private set; }
		public GhostscriptVersionInfo   LastInstalledVersion { get; private set; }

		public bool LibraryIsInstalled => ((Versions != null) && Versions.Any());
		public List<Ghostscript.NET.Processor.GhostscriptProcessor> Processors { get; private set; }
		public FileInfo[] PdfFiles { get; private set; }
		#endregion


		#region Construction
		public Converter(IEnumerable<string> pdfFiles, string outputFile)
		{
			if(!(SetUpVersion() && SetUpFiles((pdfFiles ?? new string[0]).Select(s => new FileInfo(s))) && SetUpProcessors()))
				throw new ArgumentException("Coult not initialize PdfConvertLibrary.Converter");
		}
		#endregion


		#region Public Members
		public void StartConvert(int pageFrom, int pageTo)
		{
			var argList = new List<string[]>();
			foreach(var file in PdfFiles) {
				argList.Add(CreateTestArgs(
						file.FullName, 
						new FileInfo(Path.Combine(file.DirectoryName, file.Name.Replace(file.Extension, ".ps"))).FullName, 
						pageFrom, 
						pageTo
					)
				);
			}

			foreach(var proc in Processors)
				ThreadPool.QueueUserWorkItem(a => proc.StartProcessing(argList[Processors.IndexOf(proc)], new ConsoleStdIO(true, true, true)));
		}
		#endregion
	

		#region Private Helpers
		private bool SetUpVersion()
		{
			var versions = GhostscriptVersionInfo.GetInstalledVersions(GhostscriptLicense.GPL | GhostscriptLicense.AFPL).ToArray();
			if(!versions?.Any() ?? false)
				return false;

			Versions = versions;
			LastInstalledVersion = GhostscriptVersionInfo.GetLastInstalledVersion(GhostscriptLicense.AFPL | GhostscriptLicense.GPL, GhostscriptLicense.GPL);

			return (Versions?.Any() ?? false) && (LastInstalledVersion != null);
		}

		private bool SetUpFiles(IEnumerable<FileInfo> pdfFiles)
		{
			PdfFiles = pdfFiles.Where(p => string.Compare(p.Extension, ".pdf", true) == 0).ToArray();
			return (PdfFiles != null) && (PdfFiles.Length != 0);
		}

		private bool SetUpProcessors()
		{
			Processors = Enumerable.Range(0, ((int)PdfFiles.Length - 1)).Select(f => new Ghostscript.NET.Processor.GhostscriptProcessor(LastInstalledVersion, true)).ToList();
			return (Processors != null) && (Processors.Count > 1);
		}

		private string[] CreateTestArgs(string inputPath, string outputPath, int pageFrom, int pageTo)
        {
            List<string> gsArgs = new List<string>();

            gsArgs.Add("-q");
            gsArgs.Add("-dSAFER");
            gsArgs.Add("-dBATCH");
            gsArgs.Add("-dNOPAUSE");
            gsArgs.Add("-dNOPROMPT");
            gsArgs.Add(@"-sFONTPATH=" + System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts));
            gsArgs.Add("-dFirstPage=" + pageFrom.ToString());
            gsArgs.Add("-dLastPage=" + pageTo.ToString());
            gsArgs.Add("-sDEVICE=png16m");
            gsArgs.Add("-r72");
            gsArgs.Add("-sPAPERSIZE=a4");
            gsArgs.Add("-dNumRenderingThreads=" + Environment.ProcessorCount.ToString());
            gsArgs.Add("-dTextAlphaBits=4");
            gsArgs.Add("-dGraphicsAlphaBits=4");
            gsArgs.Add(@"-sOutputFile=" + outputPath);
            gsArgs.Add(@"-f" + inputPath);

            return gsArgs.ToArray();
        }
		#endregion

		/*						
			Ghostscript.NET.Processor.GhostscriptProcessor processor = new Processor.GhostscriptProcessor(_gs_verssion_info, true);
			processor.Process(CreateTestArgs(input, output, startPage, endPage), new ConsoleStdIO(true, true, true));
		*/
	}

}
