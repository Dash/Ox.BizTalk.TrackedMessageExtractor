using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.BizTalk.Operations;
using static Ox.BizTalk.TrackedMessageExtractor.ConsoleTools;

namespace Ox.BizTalk.TrackedMessageExtractor
{
	/// <summary>
	/// Extracts messages from BizTalk tracking database
	/// </summary>
	public class Extractor
	{
		private readonly Settings _settings;
		private BizTalkOperations _btsOps;
		private TrackingDatabase _trackingDatabase;

		public Extractor(Settings settings)
		{
			this._settings = settings;
			this._btsOps = new BizTalkOperations(settings.BizTalkMgmtHost, settings.BizTalkMgmtDb);
			this._trackingDatabase = new TrackingDatabase(settings.BizTalkDTAHost, settings.BizTalkDTADb);
		}

		private string CleanFilename(string filename)
		{
			if(!String.IsNullOrEmpty(filename))
			{
				foreach(char c in Path.GetInvalidFileNameChars())
				{
					filename = filename.Replace(c.ToString(), String.Empty);
				}
			}
			return filename;
		}

		/// <summary>
		/// Saves out a message to a file, using tracked filename and timestamp where possible.
		/// </summary>
		/// <param name="messageIdentifier">Message guid</param>
		public void ExtractMessage(string messageIdentifier)
		{
			bool retry = false;

			messageIdentifier = messageIdentifier.Trim();

			Guid messageId = new Guid(messageIdentifier);

			Console.Write("Processing {0}. ", messageIdentifier);

			do
			{

				try
				{
					retry = false;

					Microsoft.BizTalk.Message.Interop.IBaseMessage msg = _btsOps.GetTrackedMessage(messageId, _trackingDatabase);

					// Build an output filename
					string saveFileName = msg.MessageID.ToString();
					string saveFileExtension = ".txt";
					string receivedFileName = (string)msg.Context.Read(_settings.FilenameProperty, _settings.FilenameSchema);

					// If we've obtained a filename from the context
					if (receivedFileName != null)
					{
						saveFileName = Path.GetFileNameWithoutExtension(receivedFileName);
						saveFileExtension = Path.GetExtension(receivedFileName);
					}

					// Loop each message part
					for (int i = 0; i < msg.PartCount; i++)
					{
						string partName;
						Microsoft.BizTalk.Message.Interop.IBaseMessagePart part = msg.GetPartByIndex(i, out partName);

						if (part != null && part.Data != null)
						{
							// Attempt to construct a file name based on part data
							string fileName = String.Empty;
							string partFileName = part.PartProperties.Read("FileName", "http://schemas.microsoft.com/BizTalk/2003/mime-properties") as String;
							string partFileExtension = saveFileExtension;

							// Use the partName if there is no mime name
							if (partFileName == null)
							{
								partFileName = partName;
							}
							else
							// Use the extension from the filename for this part
							{
								partFileExtension = Path.GetExtension(partFileName);
								partFileName = Path.GetFileNameWithoutExtension(partFileName);
							}
							
							// Handle duplicate files names with an incrementing counter
							int fileNameDuplicate = -1;
							do
							{
								fileName = CleanFilename(String.Format("{0}_{1}{3}{2}", saveFileName, partFileName, partFileExtension, fileNameDuplicate++ >= 0 ? fileNameDuplicate.ToString() : String.Empty));
							} while (File.Exists(fileName));

							Console.WriteLine("Saving message to {0}", fileName);

							using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
							{
								byte[] buffer = new byte[1024];
								int length = 0;
								while ((length = part.Data.Read(buffer, 0, buffer.Length)) > 0)
								{
									fs.Write(buffer, 0, length);
								}

								fs.Flush();
							}

							DateTime createdTime = DateTime.Now;

							// Try to get the file created time
							object prop = msg.Context.Read("FileCreationTime", "http://schemas.microsoft.com/BizTalk/2003/file-properties");

							if (prop == null)
							{
								// If there was no file created time (e.g. it was a SOAP message), get the adapter finished time
								prop = msg.Context.Read("AdapterReceiveCompleteTime", "http://schemas.microsoft.com/BizTalk/2003/messagetracking-properties");
							}

							if (prop != null)
							{
								createdTime = (DateTime)prop;
							}

							File.SetLastWriteTime(fileName, createdTime);
						}
					}
				}
				catch (Exception ex)
				{
					WriteError(ex);

					do
					{
						string prompt = PromptFor<string>("Would you like to retry this message? [Y/N]");
						switch (prompt.ToUpper().Trim())
						{
							case "N":
								return;
							case "Y":
								retry = true;
								break;
							default:
								WriteColourfulLine(CON_COLOUR_WARN, "Invalid answer, Y or N please.");
								break;
						}
					} while (!retry);	// impossible for a false retry as we'll return on a N

				}
			} while (retry);

		}
	}
}
