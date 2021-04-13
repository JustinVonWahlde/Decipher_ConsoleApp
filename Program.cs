using System;
using System.Collections.Generic;
using System.Linq;

namespace Decipher
{
    class Program
    {
        static void Main(string[] args)
        {
			//RepeatByNumber(100);
			RepeatByUserInput();

			//Wait for the user to respond before closing.
			Console.WriteLine("\nPress any Key to close the Decipher console app...");
			Console.ReadKey();
		}

		//This is for testing purposes
		static void RepeatByNumber(int repetitions)
		{
			Console.WriteLine("How many types of elements to chose from?");
			int n = Convert.ToInt32(Console.ReadLine());
			Console.WriteLine("How many elements to choose?");
			int r = Convert.ToInt32(Console.ReadLine());

			List<int> tally = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

			for (int i = 0;  i < repetitions; i++)
			{
				int x = Decipher(n, r);
				tally[x]++;
			}

			for (int j = 0; j < tally.Count; j++)
			{
				Console.WriteLine("The number of time it took {0} guesses: {1}", j, tally[j]);
			}
		}

		//This is for testing purposes
		static void RepeatByUserInput()
		{
			Console.WriteLine("How many types of elements to chose from?");
			int n = Convert.ToInt32(Console.ReadLine());
			Console.WriteLine("How many elements to choose?");
			int r = Convert.ToInt32(Console.ReadLine());

			string x = "x";
			while (x == "x")
			{
				Decipher(n, r);
				Console.WriteLine("\nEnter 'x' to play again or any other key to exit");
				x = Console.ReadLine();
			}
		}

		static int Decipher(int n, int r)
		{
			List<Code> codeList = GenerateCodeList(n, r);
			List<List<int>> responseList = GenerateResponses(r);

			Console.WriteLine("-----------------------------------------");
			Console.Write("The total number of codes is: ");
			Console.Write(codeList.Count);

			Console.Write("\nThe randomly selected Cipher is: ");

			List<int> cipher = codeList.Select(cl => cl.CodeElements).ToList()[new Random().Next(0, codeList.Count - 1)];

			Console.Write(IntListToString(cipher));

			List<int> response = new List<int>();
			int guessCount = 1;

			// keep guessing codes until the response is the last response in the responseList because the last response in the responseList 
			// will always be the success response
			do
			{
				Console.WriteLine("\n-----------------------------------------");

				List<int> guess = GenerateEducatedGuess(codeList.ToList(), responseList.ToList());
				Console.Write("\nEducated guess #{0} is: ", guessCount);
				Console.Write(IntListToString(guess));

				response = GetResponse(cipher.ToList(), guess.ToList());
				Console.Write("\nThe response is: ");
				string res = IntListToString(response);
				res = res.Substring(0, 1) + "R" + res.Substring(1, 1) + "W";
				Console.Write(res);

				if (response[0] == r)
				{
					Console.WriteLine("\nThe Cipher has been Deciphered!");
				}
				else
				{
					codeList = NarrowCodeList(codeList, guess, response);
					Console.WriteLine("\nThe remaining number of codes is:");
					Console.WriteLine(codeList.Count);
				}

				guessCount++;

			} while (response[0] != r || guessCount > 10);

			return guessCount;
		}

		/// <summary>
		/// Generates a complete list of Codes given the number of elements in each code and the number of types of elements
		/// </summary>
		/// <param name="n">Number of Types of Elements</param>
		/// <param name="r">Number of Elements in each Code</param>
		/// <returns>A complete list of Codes</returns>
		static List<Code> GenerateCodeList(int n, int r)
        {
			// Make a lists of integers to store all of the codes.
			List<Code> codeList = new List<Code> { };

			// The total number of codes is calculated using the formula: n ^ r 
			double codeCount = Math.Pow(n, r);

			// Now fill out the Codes with the code elements and code types
			for (int i = 0; i < codeCount; i++) //i = Current code number
			{
				Code codeToAdd = new Code();

				codeToAdd.CodeElements = GetCode(i, n, r);
				codeToAdd.CodeType = GetCodeType(codeToAdd.CodeElements.ToList());

				codeList.Add(codeToAdd);
			}

            return codeList;
        }

		/// <summary>
		/// Calculate each element of a given code using the formula: ⌊i / (n ^ (r - j - 1))⌋ % n
		/// </summary>
		/// <param name="i">Current code number</param>
		/// <param name="n">Number of Types of Elements</param>
		/// <param name="r">Number of Elements in each Code</param>
		/// <returns>A list of itengers which make up the Code Elements</returns>
		static List<int> GetCode(int i, int n, int r)
		{
			List<int> codeToAdd = new List<int>();

			for (int j = 0; j < r; j++) //j = Current element in the code
			{
				codeToAdd.Add(Convert.ToInt32(Math.Floor(i / Math.Pow(n, r - j - 1)) % n));
			}

			return codeToAdd;
		}

		/// <summary>
		/// Given the current list of Codes, the current guess, and the response to that guess, eliminate codes that don't return
		/// the same response for the guess. This process narrows down the list of codes, allowing the next guess to be better than the last.
		/// </summary>
		/// <param name="codeList">A list of Codes to narrow down from</param>
		/// <param name="guess">The current guess</param>
		/// <param name="response">The response given for the current guess</param>
		/// <returns>A narrowed down list of Codes</returns>
		static List<Code> NarrowCodeList(List<Code> codeList, List<int> guess, List<int> response)
		{
			List<Code> narrowedCodeList = new List<Code>();

			// Cycle through the Code list to get the response each Code would give when paired with the guess, then compare the response with
			// the given response. If they match, add the Code to the narrowed code list.
			for (int i = 0; i < codeList.Count; i++)
			{
				List<int> currentCode = codeList[i].CodeElements.ToList();
				List<int> currentResponse = GetResponse(currentCode.ToList(), guess.ToList());

				// If the responses are the same, add the code to the narrowed code list
				if (currentResponse[0] == response[0] && currentResponse[1] == response[1])
				{
					narrowedCodeList.Add(new Code
					{
						CodeElements = currentCode.ToList(),
						CodeType = codeList[i].CodeType
					});
				}
			}

			return narrowedCodeList;
		}

		/// <summary>
		/// Not all guesses are equal in value. Some guesses will typically narrow the code list more than others. This function determines
		/// determines which code type will typically narrow the code list the most, and returns a random code of that type.
		/// </summary>
		/// <param name="codeList">A complete list of Codes that might be the Cipher</param>
		/// <param name="responseList">A list of valid responses</param>
		/// <returns></returns>
		static List<int> GenerateEducatedGuess(List<Code> codeList, List<List<int>> responseList)
		{
			List<TypeValue> codeTypeValues = new List<TypeValue>();

			// Populate the codeTypeValues list with every unique Code Type in the Code list. 
			foreach (string type in codeList.Select(cl => cl.CodeType).Distinct().ToList())
			{
				codeTypeValues.Add(new TypeValue { CodeType = type });
			}

			// Find the Code Type Values of each Code Type
			foreach (TypeValue ctv in codeTypeValues)
			{
				int value = 0;

				// Get the first code with the given type to use as a guess
				List<int> guess = codeList.Where(c => c.CodeType == ctv.CodeType).Select(c => c.CodeElements).FirstOrDefault();

				foreach (List<int> response in responseList)
				{
					// Simulate how much a specific guess and response would narrow down the code list
					List<Code> tempCodeList = NarrowCodeList(codeList.ToList(), guess, response);

					// If the count of the simulated code is higher than the current value, set the value to the the count.
					value = tempCodeList.Count > value ? tempCodeList.Count : value;
				}
				// here the value variable is the highest count any response can give for this code type. This is the Code Type Value
				ctv.CodeTypeValue = value;
			}

			// Find the Code Type with the lowest Code Type Value
			string bestTypeByValue = codeTypeValues.Aggregate((x, y) => x.CodeTypeValue < y.CodeTypeValue ? x : y).CodeType;

			// Find the Educated Guesses, the Codes in the CodeList with Code Types matching the best Code Type. 
			List<List<int>> educatedGuesses = codeList.Where(cl => cl.CodeType == bestTypeByValue).Select(cl => cl.CodeElements).ToList();

			// Select a random Educated Guess to return
			List<int> educatedGuess = educatedGuesses[new Random().Next(0, educatedGuesses.Count - 1)];

			return educatedGuess;
		}

		/// <summary>
		/// Given a code, return the code type. A code type is a string where each character represents the types of the
		/// elements in the code. For example, a code of { 5, 2, 5, 1 } has a code type "AABC", where each 'A' represents
		/// one of the fives and the 'B' and 'C' reprecent the two and the one.
		/// </summary>
		/// <param name="codeElements">A list of integers as code elements</param>
		/// <returns>A string representing the "type" of the given code</returns>
		static string GetCodeType(List<int> code)
		{
			string codeType = string.Empty;

			// This list keeps track of how often each element appears in the code.
			List<int> frequencies = new List<int>();

			// Make a new list equal to the given Code elements so that the provided list isn't changed (since lists are passed by referance).
			List<int> codeElements = code.ToList();

			char currentChar = 'A';

			// First count the frequency of each element in the code
			for (int i = 0; i < codeElements.Count; i++)
			{
				int frequency = 1;

				// If the current element equals negative one, it has already been counted. Move to the next element
				if (codeElements[i] == -1) { continue; }

				// Only need to compare the current element to subsequent elements, as previous elements have already been counted
				for (int j = i + 1; j < codeElements.Count; j++)
				{
					// If both elements are the same, add to the frequency and change code[j] to negative one so that it will be skipped
					if (codeElements[i] == codeElements[j]) 
					{
						frequency++;
						codeElements[j] = -1;
					}
				}

				frequencies.Add(frequency);
			}

			// Now build the codeType string. Order frequencies from largest to smallest (so that the format of the codeTypes all match)
			// and add the currentChar to the codeType string for each frequency.
			foreach (int frequency in frequencies.OrderByDescending(f => f))
			{
				for (int i = 0; i < frequency; i++)
				{
					codeType += currentChar;
				}
				currentChar = (char)((int)currentChar + 1); // move the currentChar to the next letter for the next frequency
			}

			return codeType;
		}

		/// <summary>
		/// Generate a list of valid responses given the number of elements in each code. A response is an list of two integers
		/// which indicate how close a guess is to the Cipher. A response is considered valid if the sum of it's two integers
		/// is less than or equal to the given the number of elements in each code.
		/// </summary>
		/// <param name="r">Number of Elements in each Code</param>
		/// <returns>A list of valid responses</returns>
		static List<List<int>> GenerateResponses(int r)
		{
			List<List<int>> responses = new List<List<int>>();

			// The total number of responses is calculated using the formula: n ^ r 
			// except in this case 'n' is the number of elements plus one and 'r' is hardcoded as two.
			double totalResponseCount = Math.Pow(r + 1, 2);

			for (int i = 0; i < totalResponseCount; i++)
			{
				List<int> response = GetCode(i, r + 1, 2);

				// Check if the response is valid before adding it to the list of responses by comparing the summ of it's two integers 
				// with r. Additionally check if first integer is equal to r - 1 while the second integer is equal to 1. This isn't a valid 
				// response because it would mean all of the elements in the code except for one are the right type and in the right position 
				// and the one element left over is the correct type but not in the correct position, even though only one position remains.
				// Lastly, there's no need to include the response where the first integer equals r, since that means the Cipher has been deciphered.
				if (response[0] + response[1] <= r &&
					(response[0] != r - 1 && response[1] != 1) &&
					response[0] < r)
				{
					responses.Add(response);
				}
					
			}

			return responses;
		}

		/// <summary>
		/// A response is an list of two integers which indicate how close a guess is to the Cipher. The first integer of a response indicates how 
		/// many elements in the guess match both the type and position of the elements in the Cipher. The second integer of a response indicates 
		/// how many elements in the guess match only the type of the elements in the Cipher, ignoring all of the elements that have already be 
		/// counted for the first integer.
		/// </summary>
		/// <param name="cipher">The code that the Code Breaker is trying to discover</param>
		/// <param name="guess">The code that the Code Breaker has guessed, to which the Code Maker must provide a response</param>
		/// <returns>A response, a list of two integers which indicate how close a guess is to the Cipher</returns>
		static List<int> GetResponse(List<int> cipher, List<int> guess)
		{
			List<int> response = new List<int>();

			// Make a new list for both the cipher and the guess so that the provided lists aren't changed (since lists are passed by referance).
			List<int> tempCipher = cipher.ToList();
			List<int> tempGuess = guess.ToList();

			// Count the number of elements in the guess that match the type and position of the elements in the cipher, then set those elements to 
			// negative one so that they're skipped when counting how many elements in the guess match only the type of the elements in the Cipher.
			int counter = 0;
			for (int i = 0; i < tempGuess.Count; i++)
			{
				if (tempCipher[i] == tempGuess[i])
				{
					counter++;
					tempCipher[i] = -1;
					tempGuess[i] = -1;
				}
			}

			response.Add(counter);

			// Count the number of elements in the guess that match only the type of the elements in the cipher, ignoring all of the elements that
			// have already been counted and set to negative one.
			counter = 0;
			for (int i = 0; i < tempGuess.Count; i++) // i = current element of the guess
			{
				// If this element of the guess has already been counted, skip it here.
				if (tempGuess[i] == -1) { continue; }

				for (int j = 0; j < tempGuess.Count; j++) // j = current element of the Cipher
				{
					if (tempGuess[i] == tempCipher[j])
					{
						// If this element of the guess has already been counted, skip it here.
						if (tempGuess[i] == -1) { continue; }

						counter++;
						tempCipher[j] = -1;
						tempGuess[i] = -1;
						continue;
					}
				}
				
			}

			response.Add(counter);

			return response;
		}

		/// <summary>
		/// Convert a given list of integers to a string.
		/// </summary>
		/// <param name="intList">A list of integers</param>
		/// <returns>A string</returns>
		static string IntListToString(List<int> intList)
		{
			string str = string.Empty;

			for (int i = 0; i < intList.Count; i++)
			{
				str += intList[i];
			}

			return str;
		}

		/// <summary>
		/// Convert a given string to a list of integers if possible (otherwise return null). 
		/// </summary>
		/// <param name="str">A string</param>
		/// <returns>A list of integers</returns>
		static List<int> StringToIntList(string str)
		{
			List<int> intList = new List<int>();

			for (int i = 0; i < str.Length; i++)
			{
				try {
					intList.Add(Convert.ToInt32(str.Substring(i, 1)));
				}
				catch (OverflowException) { return null; }
				catch (FormatException) { return null; }
			}

			return intList;
		}
	}

	public class Code
	{
		public List<int> CodeElements { get; set; }
		public string CodeType { get; set; }
	}

	public class TypeValue
	{
		public string CodeType { get; set; }
		public int CodeTypeValue { get; set; }
	}
}
