using UnityEngine;
using System;

namespace Pathea
{
	namespace Text
	{
		public static class Text
		{
			// Capital first words or every word in a short string
			public static string CapitalWords (string s, bool everyword = false)
			{
				string temp = "";
				bool done = false;
				foreach ( char c in s )
				{
					if ( everyword && c == ' ' )
					{
						done = false;
					}
					if ( c >= 'A' && c <= 'Z' )
					{
						done = true;
					}
					if ( !done && c >= 'a' && c <= 'z' )
					{
						temp += (char)(c - 32);
						done = true;
					}
					else
					{
						temp += c;
					}
				}
				return temp;
			}
			
			// Make a multi-line string single line, return true if the string is singleline
			public static string SingleLine( string s )
			{
				s = s.Replace("\r\n", " ");
				return s.Replace('\n', ' ');
			}

			// Make a string to a valid filename, return the valid filename
			public static string MakeFileName( string name )
			{
				string retval = "";
				for ( int i = 0; i < name.Length; i++ )
				{
					if ( name[i] == '/' || name[i] == '\\' || name[i] == ':' || name[i] == '*' || name[i] == '\r' || name[i] == '\n' ||
					    name[i] == '?' || name[i] == '\"' || name[i] == '<' || name[i] == '>' || name[i] == '|' || name[i] == '\b' )
					{
						retval = retval + " ";
					}
					else
					{
						retval = retval + name[i];
					}
				}
				return retval;
			}
		}
	}
}
