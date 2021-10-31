using UnityEngine;
using Panda;

namespace StoryState
{
    /*
    *   TextPrinterTasks
    *
    *   Adding a Tasks component to a GameObject with a PandaBehaviour gives the PandaBehaviour access
    *   to the included PandaTasks. If the Tasks component is associated to a specific class, an object
    *   reference may have to be set before calling the tasks from a PandaScript.
    *
    *   The most frequently used Task in TextPrinterTasks is Print, which is used to print a new line of
    *   text onto the screen. Print will use the PrintBehaviour set with SetDefaultPrintBehaviour. Using
    *   PrintAwait, PrintDelay and PrintImmediate will use the specified PrintBehaviour for a single
    *   print.
    *
    *   Participants should always be set with SetParticipants before any Print actions. The Speaker
    *   can be changed by setting a first integer parameter to any of the Print actions, or can be set
    *   on its own with SetSpeaker.
    */
    public class TextPrinterTasks : MonoBehaviour
    {
        private TextPrinter _textPrinter;
        public TextPrinter TextPrinter { get => _textPrinter; set => _textPrinter = value; }

        [Task]
        public void SetParticipants(string participantNames)
        {
            _textPrinter.Participants = Participant.SetParticipants(participantNames);
            Task.current.Succeed();
        }

        [Task]
        public void SetSpeaker(int id, string emotion)
        {
            if (TextPrinter.Participants.Length > id)
                TextPrinter.Participants[id].Emotion = emotion;
            SetSpeaker(id);
        }

        [Task]
        public void SetSpeaker(int id)
        {
            if (TextPrinter.Participants.Length > id)
            {
                TextPrinter.LastSpeaker = id;
                AnimatedPortrait.UpdatePortraits(
                    TextPrinter.Participants, TextPrinter.Participants[id]);
            }
            Task.current.Succeed();
        }

        [Task]
        public void Print(string inString)
        {
            if (_textPrinter.DefaultPrintBehaviour == TextPrinter.PrintBehaviour.await)
                PrintAwait(inString);
            else if (_textPrinter.DefaultPrintBehaviour == TextPrinter.PrintBehaviour.delay)
                PrintDelay(inString);
            else if (_textPrinter.DefaultPrintBehaviour == TextPrinter.PrintBehaviour.immediate)
                PrintImmediate(inString);
        }

        [Task]
        public void Print(int speaker, string inString)
        {
            if (_textPrinter.DefaultPrintBehaviour == TextPrinter.PrintBehaviour.await)
                PrintAwait(speaker, inString);
            else if (_textPrinter.DefaultPrintBehaviour == TextPrinter.PrintBehaviour.delay)
                PrintDelay(speaker, inString);
            else if (_textPrinter.DefaultPrintBehaviour == TextPrinter.PrintBehaviour.immediate)
                PrintImmediate(speaker, inString);
        }

        [Task]
        public void Print(string inString, float delayAfterPrint)
        {
            if (_textPrinter.DefaultPrintBehaviour == TextPrinter.PrintBehaviour.await)
                PrintAwait(inString);
            else if (_textPrinter.DefaultPrintBehaviour == TextPrinter.PrintBehaviour.delay)
                PrintDelay(inString, delayAfterPrint);
        }

        [Task]
        public void Print(int speaker, string inString, float delayAfterPrint)
        {
            if (_textPrinter.DefaultPrintBehaviour == TextPrinter.PrintBehaviour.await)
                PrintAwait(speaker, inString);
            else if (_textPrinter.DefaultPrintBehaviour == TextPrinter.PrintBehaviour.delay)
                PrintDelay(speaker, inString, delayAfterPrint);
        }

        [Task]
        public void PrintAwait(string inString)
        {
            _textPrinter.Print(-1, inString);
        }

        [Task]
        public void PrintAwait(int speaker, string inString)
        {
            _textPrinter.Print(speaker, inString);
        }

        [Task]
        public void PrintDelay(string inString)
        {
            _textPrinter.Print(-1, inString, _textPrinter.DefaultDelayAfterPrint);
        }

        [Task]
        public void PrintDelay(int speaker, string inString)
        {
            _textPrinter.Print(speaker, inString, _textPrinter.DefaultDelayAfterPrint);
        }

        [Task]
        public void PrintDelay(string inString, float delayAfterPrint)
        {
            _textPrinter.Print(-1, inString, delayAfterPrint);
        }

        [Task]
        public void PrintDelay(int speaker, string inString, float delayAfterPrint)
        {
            _textPrinter.Print(speaker, inString, delayAfterPrint);
        }

        [Task]
        public void PrintImmediate(string inString)
        {
            _textPrinter.Print(-1, inString, -2f);
        }

        [Task]
        public void PrintImmediate(int speaker, string inString)
        {
            _textPrinter.Print(speaker, inString, -2f);
        }

        [Task]
        public void SetDefaultPrintBehaviour(int behaviour)
        {
            _textPrinter.DefaultPrintBehaviour = (TextPrinter.PrintBehaviour)behaviour;
            Task.current.Succeed();
        }

        [Task]
        public void SetDefaultPrintBehaviour(string behaviour)
        {
            if (behaviour.ToLower() == "await")
                _textPrinter.DefaultPrintBehaviour = TextPrinter.PrintBehaviour.await;
            else if (behaviour.ToLower() == "delay")
                _textPrinter.DefaultPrintBehaviour = TextPrinter.PrintBehaviour.delay;
            else if (behaviour.ToLower() == "immediate")
                _textPrinter.DefaultPrintBehaviour = TextPrinter.PrintBehaviour.immediate;
            Task.current.Succeed();
        }
    }
}
