using System;
using System.Collections.Generic;

namespace StoryState
{
    /*
    *   TextPrinterStringAdjustments
    *
    *   This class is responsible for replacing tags with actual Participant names, which allows for
    *   more softcoded dialogue scripts. Name tags look like this:
    *   
    *   [0.firstname] = Tag that gets replaced by the first name of Participant Character 0.
    *   [1.lastname] = Tag that gets replaced by the last name of Participant Character 1.
    *
    *   OnPrint tags can also be defined, which are removed before the text is displayed but will
    *   trigger behaviour when when the Print Task reaches the position of tag. Currently, supported
    *   OnPrint tags are [silent], which disables the Print sound effect, and tags for changing the
    *   Participant portrait (available Emotion portraits for the Speaker are automatically checked
    *   against the tag contents if they haven't been recognized as a command such as "silent").
    *
    *   More OnPrint tag commands can easily be defined below - see the CheckForOnPrintTag and
    *   TriggerOnPrintTagCommand methods below.
    */
    public static class TextPrinterStringAdjustments
    {
        /*
            Makes changes to a dialogue string before printing, such as collecting custom tags
            for print-time commands, and replacing placeholder tags with Participant names.
        */
        public static (string, Dictionary<int, string>) Apply(TextPrinter textPrinter, string inString)
        {
            char tagEndChar = ']';  // Determines the end character of a custom tag.
            char tagStartChar = '[';    // Determines the start character of a custom tag.
            string returnString = inString;   // The processed string that this method will return.
            int tagStart = -1;
            int tagEnd = -1;

            Dictionary<int, string> identifiedOnPrintTags = new Dictionary<int, string>();;

            // Check for tags in string. The storedTags dictionary will be checked when each letter is
            // being printed. Can be used for adding effects in mid-dialogue or changing the typing
            // speed, for example.
            for (int i = returnString.Length - 1; i >= 0; i--)
            {
                if (returnString[i] == tagEndChar && tagEnd == -1)
                    tagEnd = i;
                else if (returnString[i] == tagStartChar && tagEnd != -1)
                {
                    tagStart = i;
                    var tagContent = returnString.Substring(tagStart + 1, (tagEnd) - (tagStart + 1));
                    if (Char.IsDigit(returnString[tagStart + 1]))
                    {
                        var stringToInsert = ReplaceTagWithParticipantName(textPrinter, tagContent);
                        // Deletes the tag from the string.
                        returnString = returnString.Substring(0, Math.Max(0, tagStart))
                            + returnString.Substring(tagEnd + 1);
                        // Inserts name.
                        returnString = returnString.Insert(tagStart, stringToInsert);
                        tagStart = -1;
                        tagEnd = -1;
                    }
                    else
                    {
                        // Adds the tag to the storedTags dictionary.
                        identifiedOnPrintTags.Add(tagStart, tagContent);

                        // Deletes the tag from the string.
                        returnString = returnString.Substring(0, Math.Max(0, tagStart))
                            + returnString.Substring(tagEnd + 1);
                        tagStart = -1;
                        tagEnd = -1;
                    }
                }
            }

            return (returnString, identifiedOnPrintTags);
        }

        /*
            If a tag has been recognized as a Participant name tag, this method will return the name
            that the tag is to be replaced with.
        */
        private static string ReplaceTagWithParticipantName(TextPrinter textPrinter, string tagContent)
        {
            var splitTagContent = tagContent.Split('.');
            var participantID = (int)Char.GetNumericValue(tagContent[0]);
            string stringToInsert = "";
            if (participantID < textPrinter.Participants.Length)
            {
                var character = textPrinter.Participants[participantID].Character;
                switch (splitTagContent[1].ToLower())
                {
                    case "name":
                        stringToInsert = character.Shortname;
                        break;
                    case "fullname":
                        stringToInsert = character.Fullname;
                        break;
                    case "firstname":
                        stringToInsert = character.Firstname;
                        break;
                    case "lastname":
                        stringToInsert = character.Lastname;
                        break;
                    case "nickname":
                        stringToInsert = character.Nickname;
                        break;
                }
            }

            return stringToInsert;
        }

        /*
            Called by the TextPrinter every time a new character is printed. If a tag with a valid
            command has been registered to the identifiedOnPrintTags dictionary, its associated
            behaviour will be triggered.
        */
        public static void CheckForOnPrintTag(TextPrinter.PrintTaskData taskData, int characterToCheckAt)
        {
            // Check dictionary for OnPrint tag associated with the position of the current Print.
            if (taskData._identifiedOnPrintTags.ContainsKey(characterToCheckAt))
            {
                string[] splitTag = taskData._identifiedOnPrintTags[characterToCheckAt].Split('=');
                string tagContent = splitTag[0];
                string[] tagParams = null;
                if (splitTag.Length > 1)
                    tagParams = splitTag[1].Split(';');

                TriggerOnPrintTagCommand(taskData, tagContent, tagParams);

                // Uncomment the string below to also print the detected tag to the console.
                //Debug.Log("TextPrinter: OnPrintTag triggered "
                //    + tagName + "(" + characterToCheckAt + ")");

                // The tag is removed from the dictionary after its behaviour has been triggered.
                taskData._identifiedOnPrintTags.Remove(characterToCheckAt);
            }
        }

        /*
            Triggers OnPrint tag behaviour.
        */
        private static void TriggerOnPrintTagCommand(
            TextPrinter.PrintTaskData taskData,
            string tagContent,
            string[] tagParams)
        {
            // Disables the printing sound effect for the duration of the current print.
            if (tagContent == "silent")
                taskData._disablePrintingSoundEffect = true;
            // If the tag contents are none of the above, check if it's a Speaker portrait
            // and change the current portrait if it's a match.
            else
            {
                if (taskData._speaker.Character.GetPortrait(tagContent) != null)
                    taskData._speaker.Emotion = tagContent;
            }
        }
    }
}
