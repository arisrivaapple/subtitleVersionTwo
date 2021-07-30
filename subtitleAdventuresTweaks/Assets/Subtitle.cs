using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace SubtitleSystem
{
	public class Subtitle
	{
		public String startTime; //times will be imported as strings from srt file to begin with anyway
		public int startTimeInt;
		public String endTime;
		public int endTimeInt;
		public String speakerName;
		public String text;

		public Subtitle()
		{
			startTime = null;
			startTimeInt = -1;
			endTime = null;
			endTimeInt = -1;
			speakerName = null;
			text = null;
		}

		public void inputTimeInfo(String timeInfo)
		{
			startTime = timeInfo.Substring(0, 12);
			endTime = timeInfo.Substring(17, 12);
			int hoursStart = Int32.Parse(timeInfo.Substring(0, 2));
			int minutesStart = Int32.Parse(timeInfo.Substring(3, 2));
			int secondsStart = Int32.Parse(timeInfo.Substring(6, 2));
			int milisecondsStart = Int32.Parse(timeInfo.Substring(9, 3));
			int hoursEnd = Int32.Parse(timeInfo.Substring(17, 2));
			int minutesEnd = Int32.Parse(timeInfo.Substring(20, 2));
			int secondsEnd = Int32.Parse(timeInfo.Substring(23, 2));
			int milisecondsEnd = Int32.Parse(timeInfo.Substring(26, 3));
			startTimeInt = hoursStart * 3600000 + minutesStart * 60000 + secondsStart * 1000 + milisecondsStart;
			endTimeInt = hoursEnd * 3600000 + minutesEnd * 60000 + secondsEnd * 1000 + milisecondsEnd;

		}

		public void inputTextInfo(String textInfo)
		{
			
			int lastBracketIndex = -1; //this is before it gets assigned in the loop. will throw exception if there is no last bracket (do I need this assignment?)
			
			//split with space between bracketed names or notes
			//so that double brackets liek two names or [/?][Name] (the function to keep a name annoymous
			//will not be processed as part of the text
			//ps i think I should make it so / for most cases just prints the text, not the special functiojn
			for (int i = 0; i < textInfo.Length; i++)
			{
				if (textInfo[i] == ']')
				{
					lastBracketIndex = i;
				}
			}
			speakerName = textInfo.Substring(1, lastBracketIndex - 1);
			text = textInfo.Substring(lastBracketIndex + 2, textInfo.Length - lastBracketIndex - 2);

			if (text.Substring(text.Length - 3, 2).Equals("\r"))
			//get to make sure there's not an even number of backslashes in a row, because then the backslash and r are intentional and should be displayed
			{
				text = text.Substring(0, text.Length - 2);
			}
		}

		public int getStartTimeInt()
		{
			return startTimeInt;
		}

		public int getEndTimeInt()
		{
			return endTimeInt;
		}

		public string getSpeaker()
		{
			return speakerName;
		}

		public string getText()
		{
			return text;
		}
	}
}
