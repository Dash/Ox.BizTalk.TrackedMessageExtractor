using System;
using System.Text;

namespace Ox.BizTalk.TrackedMessageExtractor
{
	/// <summary>
	/// Console helper utilities
	/// </summary>
	internal class ConsoleTools
	{
		public const ConsoleColor CON_COLOUR_PROMPT = ConsoleColor.Cyan;
		public const ConsoleColor CON_COLOUR_ERR = ConsoleColor.Red;
		public const ConsoleColor CON_COLOUR_WARN = ConsoleColor.DarkYellow;

		/// <summary>
		/// Console prompt for input
		/// </summary>
		/// <typeparam name="T">Type of input required (don't do bool eh?)</typeparam>
		/// <param name="prompt">Text to display</param>
		/// <param name="allowEmpty">Require an input or allow blank</param>
		/// <returns>Typed value</returns>
		public static T PromptFor<T>(string prompt, bool allowEmpty = false)
		{
			return PromptFor<T>(prompt, String.Empty, allowEmpty);
		}

		/// <summary>
		/// Console prompt for input
		/// </summary>
		/// <typeparam name="T">Type of input required (don't do bool eh?)</typeparam>
		/// <param name="prompt">Text to display</param>
		/// <param name="defaultValue">Pre-populated value (Empty is fine)</param>
		/// <param name="allowEmpty">Require an input or allow blank</param>
		/// <returns>Typed value</returns>
		public static T PromptFor<T>(string prompt, string defaultValue, bool allowEmpty = false)
		{
			string value = null;
			T result = default(T);
			bool loop = true;
			do
			{
				
				try
				{
					value = Prompt(prompt, defaultValue).Trim();
					result = (T)Convert.ChangeType(value, typeof(T));
					if (!allowEmpty && result == null || result.Equals(default(T)))
						throw new InvalidOperationException("Value must be a " + typeof(T).Name);

					loop = false;
				}
				catch  
				{
					if (value.Length == 0 && allowEmpty)
					{
						// If we're allowing blanks, and no input has been provided, break gracefully.
						loop = false;
					}
					else
					{
						// This is an invalid input for the conversion, display an error.
						WriteError(String.Format("Cannot convert '{0}' to a {1}", value, typeof(T).Name));
					}
				}
			} while (loop);

			

			return result;
		}

		/// <summary>
		/// Straight prompt for something
		/// </summary>
		/// <param name="text">Display text (a : will be added)</param>
		/// <returns>Value entered, unvalidated</returns>
		public static string Prompt(string text)
		{
			WriteColourful(CON_COLOUR_PROMPT, text + ": ");
			return Console.ReadLine();
		}

		/// <summary>
		/// Prompt for something, with a pre-populated value displayed
		/// </summary>
		/// <param name="text">Display text (a : will be added)</param>
		/// <param name="defaultValue">Prepopulated text to provide</param>
		/// <returns>Value entered, unvalidated</returns>
		public static string Prompt(string text, string defaultValue)
		{
			StringBuilder input = new StringBuilder(defaultValue);

			bool loop = true;

			WriteColourful(CON_COLOUR_PROMPT, text + ": ");
			Console.Write(defaultValue);


			do
			{
				ConsoleKeyInfo keyPress = Console.ReadKey(true);
				switch (keyPress.Key)
				{
					case ConsoleKey.Backspace:
						if (input.Length > 0)
						{
							input.Remove(input.Length - 1, 1);
							if (Console.CursorLeft == 0)
							{
								Console.CursorTop = Console.CursorTop - 1;
								Console.CursorLeft = Console.BufferWidth - 1;
								Console.Write(" \b");
								Console.CursorTop = Console.CursorTop - 1;
								Console.CursorLeft = Console.BufferWidth - 1;
							}
							else
							{
								Console.Write("\b \b");
							}
						}
						break;
					case ConsoleKey.Enter:
						Console.WriteLine();
						loop = false;
						break;
					default:
						Console.Write(keyPress.KeyChar);
						input.Append(keyPress.KeyChar);
						break;
				}
			} while (loop);

			return input.ToString();
		}

		/// <summary>
		/// Prints coloured text to the console
		/// </summary>
		/// <param name="colour">Colour to use</param>
		/// <param name="text">Text to show</param>
		public static void WriteColourful(ConsoleColor colour, string text)
		{
			ConsoleColor current = Console.ForegroundColor;
			Console.ForegroundColor = colour;

			Console.Write(text);

			Console.ForegroundColor = current;
		}

		/// <summary>
		/// Prints coloured text to the console with a new line
		/// </summary>
		/// <param name="colour">Colour to use</param>
		/// <param name="text">Text to show</param>
		public static void WriteColourfulLine(ConsoleColor colour, string text)
		{
			WriteColourful(colour, text + Environment.NewLine);
		}

		/// <summary>
		/// Prints an error, in red!
		/// </summary>
		/// <param name="message">Text to display</param>
		public static void WriteError(string message)
		{
			WriteColourfulLine(CON_COLOUR_ERR, "** " + message);
		}

		/// <summary>
		/// Prints an error, in red!
		/// </summary>
		/// <param name="ex">Exception to display</param>
		public static void WriteError(Exception ex)
		{
			WriteColourful(CON_COLOUR_ERR, String.Format("** {0} **\n{1}\n", ex.Message, ex.StackTrace));
		}
	}
}
