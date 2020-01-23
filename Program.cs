using System;
using System.IO;

using static Ox.BizTalk.TrackedMessageExtractor.ConsoleTools;

namespace Ox.BizTalk.TrackedMessageExtractor
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("BizTalk Bulk Tracked Message Extractor");
			Console.WriteLine("Copyright (C) Alastair Grant. 2013.");
			Console.WriteLine("======================================\n");

			Settings settings = new Settings();

			try
			{ 
				if (settings.ProcessArguments(args))        // Handle program arguments
				{
					settings.PromptForMissing();            // Fill in the blanks

					using (StreamReader sr = new StreamReader(new FileStream(settings.InFile, FileMode.Open, FileAccess.Read)))
					{
						Extractor extractor = new Extractor(settings);

						string line;
						while ((line = sr.ReadLine()) != null)
						{
							if (line.Trim().Length > 0)
							{
								extractor.ExtractMessage(line);
							}
						}
					}
				}
			}
			catch(Exception ex)
			{
				WriteError(ex);
			}

			if (!settings.QuitWhenDone)
			{
				WriteColourful(ConsoleColor.Green, "Program Complete.  Press ENTER to end...");
				Console.ReadLine();
			}
		}




	}
}
