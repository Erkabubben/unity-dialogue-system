using System.Collections.Generic;

namespace StoryState
{
    /*
    *   Participant
    *
    *   Class used by the TextPrinter to retrieve an array of Characters based on a string value.
    *   The referenced Characters of the array are set as participants in a subsequent dialogue, and can
    *   then be referred to by their array ID in TextPrinterTasks (such as Print or PrintAwait).
    *   They can also be assigned emotions so that their AnimatedPortraits will change during dialogues.
    *
    *   Note that when using SetParticipants to set only two Participants, only the name of the NPC
    *   needs to be passed as the ParticipantNames parameter, since this is the most common use case.
    *   "None" should be passed if you wish to print out a text without Characters present. To set the
    *   player character to be the only participant, pass "player", "playeronly" or "player only".
    *
    *   For a conversation with more than two participants, however, "player" should be included
    *   if the player Character is to be present.
    */
    public class Participant
    {
        private Character _character;
        public Character Character { get => _character; set => _character = value; }

        // Determines which Character portrait will be displayed.
        private string _emotion;
        public string Emotion { get => _emotion; set => _emotion = value; }

        /*
            Constructor.
        */
        public Participant(Character character) => _character = character;

        /*
            Takes a string of comma-separated names, searches asset locations for the named characters,
            and returns them as an array to be used in TextPrinter dialogues.
        */
        public static Participant[] SetParticipants(string participantNames)
        {
            // Set up reference to player character.
            Character playerCharacter = null; // = TODO: GET CHARACTER
            // Parse participantNames string to array and trim names.
            string[] names = participantNames.Split(',');
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = names[i].Trim();
            }
            // If only one name is recognized, it's either a monologue or two-way conversation.
            if (names.Length == 1)
                return ConvertCharacterArrayToParticipants(
                    GetParticipantsFromOneName(names, playerCharacter));
            // Otherwise, it's three Characters or more participating.
            else
                return ConvertCharacterArrayToParticipants(
                    GetParticipantsFromMultipleNames(names, playerCharacter));
        }

        /*
            Takes an array of Characters and returns an array of Participants (all portraits will be
            set to default).
        */
        private static Participant[] ConvertCharacterArrayToParticipants(Character[] chars)
        {
            Participant[] participantsArray;
            if (chars != null && chars.Length > 0)
            {
                participantsArray = new Participant[chars.Length];
                for (int i = 0; i < chars.Length; i++)
                    participantsArray[i] = new Participant(chars[i]);
            }
            else
                participantsArray = new Participant[0];
            return participantsArray;
        }

        /*
            A string with only one name means it's either a player monologue, a two-way conversation or
            just a text being printed out without any Character portraits being displayed. See the class
            comment for more info.
        */
        private static Character[] GetParticipantsFromOneName(
            string[] names,
            Character playerCharacter)
        {
            // If the only participant name is "player" or "player only", set the player character
            // to be the lone participant.
            if (names[0].ToLower() == "player" || names[0].ToLower() == "playeronly"
                || names[0].ToLower() == "player only")
            {
                return new[] { playerCharacter };
            }
            else if (names[0].ToLower() == "none") // No participants
            {
                return null;
            }
            else
            {
                Character secondParticipant = null; // = TODO: GET CHARACTER
                // If the named character is found, set participants to the player and the second 
                // participant.
                if (secondParticipant != null)
                {
                    return new Character[]{
                        playerCharacter,
                        secondParticipant
                    };
                }
                // Else, set to player only.
                else
                {
                    return new[]{
                        playerCharacter
                    };
                }
            }
        }

        /*
            Returns an array of Participants based on the names in the participantNames string. "Player"
            should be included if player Character is to be present. See class comment for more info.
        */
        private static Character[] GetParticipantsFromMultipleNames(
            string[] names,
            Character playerCharacter)
        {
            var validParticipants = new List<Character>();
            // Iterate the names of the parsed participantNames string.
            foreach (var name in names)
            {
                // If name is "player" or "player only", add player to the valid participants list.
                if (name.ToLower() == "player" || name.ToLower() == "playeronly"
                || name.ToLower() == "player only")
                {
                    // Check that player character is not already in list.
                    if (!validParticipants.Contains(playerCharacter))
                    {
                        validParticipants.Add(playerCharacter);
                    }
                }
                else
                {
                    Character newParticipant = null; // = TODO: GET CHARACTER
                    // If the named character is found and not already in the participants list,
                    // add to valid participants list.
                    if (newParticipant != null && !validParticipants.Contains(newParticipant))
                        validParticipants.Add(newParticipant);
                }
            }
            if (validParticipants.Count > 0)
                return validParticipants.ToArray();
            else
                return null;
        }
    }
}
