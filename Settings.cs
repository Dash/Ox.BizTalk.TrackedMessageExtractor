using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.BizTalk.ExplorerOM;
using System.IO;
using static Ox.BizTalk.TrackedMessageExtractor.ConsoleTools;

namespace Ox.BizTalk.TrackedMessageExtractor
{
	/// <summary>
	/// Setings for the application
	/// </summary>
	public class Settings
	{

		public string InFile { get; set; }
		public string OutputDirectory { get; set; }
		public string FilenameProperty { get; set; }
		public string FilenameSchema { get; set; }

		public string BizTalkMgmtDb { get; set; }
		public string BizTalkMgmtHost { get; set; }
		public string BizTalkDTADb { get; set; }
		public string BizTalkDTAHost { get; set; }

		public bool QuitWhenDone { get; set; }

		public Settings()
		{
			this.FilenameProperty = "ReceivedFileName";
			this.FilenameSchema = "http://schemas.microsoft.com/BizTalk/2003/file-properties";
			this.QuitWhenDone = false;

			try
			{
				BizTalkWmiSearcher.PopulateWmiSettings(this);
			}
			catch { }
		}

		/// <summary>
		/// Checks for missing settings and prompts for them.
		/// </summary>
		public void PromptForMissing()
		{
			if (String.IsNullOrEmpty(this.InFile))
			{
				do
				{
					var file = PromptFor<String>("Message id file");
					if (!File.Exists(file))
					{
						WriteColourfulLine(CON_COLOUR_WARN, "File does not exist.");
					}
					this.InFile = file;
				} while (String.IsNullOrEmpty(this.InFile));
			}

			if(String.IsNullOrEmpty(this.OutputDirectory))
			{
				do
				{
					var dir = PromptFor<String>("Output directory", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
					if (!Directory.Exists(dir))
					{
						WriteColourfulLine(CON_COLOUR_WARN, "Directory does not exist.");
					}
					this.OutputDirectory = dir;
				} while (String.IsNullOrEmpty(this.OutputDirectory));
			}

			if (String.IsNullOrEmpty(this.BizTalkMgmtHost))
				this.BizTalkMgmtHost = PromptFor<String>("SQL Host for BizTalkMgmtDb");

			if (String.IsNullOrEmpty(this.BizTalkMgmtDb))
				this.BizTalkMgmtDb = PromptFor<String>("Database name for BizTalkMgmtDb", "BizTalkMgmtDb");

			if (String.IsNullOrEmpty(this.BizTalkDTAHost))
				this.BizTalkDTAHost = PromptFor<String>("SQL Host for BizTalkDTADb", this.BizTalkMgmtHost);

			if (String.IsNullOrEmpty(this.BizTalkDTADb))
				this.BizTalkDTADb = PromptFor<String>("Database name for BizTalkDTADb", this.BizTalkDTADb);
		}

		/// <summary>
		/// Handles external program arguments and converts them to settings
		/// </summary>
		/// <param name="args">External arguments</param>
		/// <returns>False means exit gracefully</returns>
		public bool ProcessArguments(string[] args)
		{
			bool runprogram = true;

			foreach (var arg in args)
			{
				string key = arg;
				string value = arg;
				// Split out arguments from values
				if (arg.Contains("="))
				{
					var split = arg.Split('=');
					if (split.Length != 2)
					{
						throw new ArgumentException("Argument '{0}' not understood.", arg);
					}
					key = split[0];
					value = split[1];
				}

				switch (key.ToLower())
				{
					case "--help":
					case "-h":
					case "/?":
						string about = "Bulk saves BizTalk messages from the BizTalkDTADb health and tracking database (if they exist).\nTracking needs to be enabled in BizTalk to save messages.\n\nMessages will be saved to the current working directory unless overridden.\n\nWMI will be used to locate the server, if available.";
						Console.WriteLine(about + Environment.NewLine);
						Console.WriteLine("Usage:");
						Console.WriteLine("  --in=PATH            Line delimetered file of BizTalk message ids");
						Console.WriteLine("  --mgmthost=SQLHOST   Hostname of Mgmt SQL server/instance to connect to");
						Console.WriteLine("  --dtahost=SQLHOST    Hostname of the DTA SQL server/isntance");
						Console.WriteLine("  --mgmtdb=DB          Database name of BizTalkMgmtDb");
						Console.WriteLine("  --dtadb=DB           Database name of the BizTalkDTADb");
						Console.WriteLine("  --out=PATH           Alternative output directory");
						Console.WriteLine("  --nameschema=URI     Context namespace to use for deriving original filename");
						Console.WriteLine("  --nameproperty=NAME  Context property name to use for deriving original filename");
						Console.WriteLine("  --quit               Do not prompt for program closure at end");

						Console.WriteLine("\nExamples:");

						Console.WriteLine("  --in=messages.txt --out=c:\\ --nameschema=http://schemas.microsoft.com/BizTalk/2003/file-properties --nameproperty=ReceivedFileName --mgmthost=localhost --mgmtdb=BizTalkMgmtDb --dtahost=localhost --dtadb=BizTalkDTADb");

						runprogram = false;
						break;

					case "--in":
						if(!File.Exists(value))
						{
							throw new ArgumentException("Invalid input file " + value);
						}

						this.InFile = value;
						break;

					case "--mgmthost":
						this.BizTalkMgmtHost = value;
						break;

					case "--dtahost":
						this.BizTalkDTAHost = value;
						break;

					case "--mgmtdb":
						this.BizTalkMgmtDb = value;
						break;

					case "--dtadb":
						this.BizTalkDTADb = value;
						break;

					case "--out":
						if (!Directory.Exists(value))
						{
							throw new ArgumentException("Invalid output directory " + value);
						}
						this.OutputDirectory = value;
						break;

					case "--nameschema":
						this.FilenameSchema = value;
						break;

					case "--nameproperty":
						this.FilenameProperty = value;
						break;

					case "--quit":
						this.QuitWhenDone = true;
						break;

					}
			}
			return runprogram;

		}
	}
}

