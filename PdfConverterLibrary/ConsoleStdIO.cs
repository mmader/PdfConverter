using System;
using System.Linq;
using System.Text;
using Ghostscript.NET;

namespace PdfConverterLibrary
{
	internal class ConsoleStdIO : GhostscriptStdIO
	{
		public ConsoleStdIO(bool handleStdIn, bool handleStdOut, bool handleStdErr) : base(handleStdIn, handleStdOut, handleStdErr) { }

		public override void StdError(string error) => Console.Error.WriteLine(error);

		public override void StdIn(out string input, int count)
		{
			var userinput = Enumerable.Range(0 , count).Select(s => Console.ReadLine());
			var sb = new StringBuilder();
			foreach(var ui in userinput)
				sb.AppendLine(ui);

			input = sb.ToString();
		}

		public override void StdOut(string output) => Console.WriteLine(output);
	}
}