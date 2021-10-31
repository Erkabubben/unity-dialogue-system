using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Panda;

namespace StoryState
{
    /*
    *   TextPrinter
    *
    *   The TextPrinter asynchronously prints text onto the screen in StoryState mode. Used in both
    *   DialogueState and CinematicState.
    */
    public class TextPrinter
    {
        private ITextPrinterUI _textPrinterUI;
        public ITextPrinterUI TextPrinterUI
        {
            get => _textPrinterUI;
            set => _textPrinterUI = value;
        }
        private TMP_Text MainText => _textPrinterUI.MainText;

        // Array representing the Characters participating in the dialogue and the current emotions
        // of their portraits - see the Participant class.
        private Participant[] _participants;
        public Participant[] Participants
        {
            get => _participants;
            set => _participants = value;
        }

        public float DefaultDelayAfterPrint
        {
            get => _defaultDelayAfterPrint;
            set => _defaultDelayAfterPrint = value;
        }

        // Determines how fast letters are typed in Dialogue mode
        private const float _DefaultCharacterPrintSpeed = 0.040f;

        // The default pitch of the typing sound
        private const float _SpeechDefaultPitch = 0.5f;
        private bool _textResetsAtEndOfTask = true;

        // Stores the ID of the last speaker, so that speaker ID will only have to be given when
        // changing the current speaker.
        private int _lastSpeaker;
        public int LastSpeaker
        {
            get => _lastSpeaker;
            set => _lastSpeaker = value;
        }

        // The 
        public enum PrintBehaviour
        {
            await,  // The TextPrinter will await user input before Succeeding the Panda Task.
            delay,  // After the print has finished, the Task will Succeed after a set amount of time.
            immediate   // The text is immediately printed out and the Task is Succeeded.
        }

        private PrintBehaviour _defaultPrintBehaviour = PrintBehaviour.await;
        public PrintBehaviour DefaultPrintBehaviour
        {
            get => _defaultPrintBehaviour;
            set => _defaultPrintBehaviour = value;
        }

        // Used if PrintDelay is called with no delayAfterPrint parameter.
        private float _defaultDelayAfterPrint = 2f;
        private string _speechDefaultSoundEffectName = "default-speech";

        public TextPrinter(ITextPrinterUI textPrinterUI)
        {
            _textPrinterUI = textPrinterUI;
        }

        public class PrintTaskData
        {
            // The amount of currently visible characters - used to set the corresponding property
            // of the MainText TMP component in UpdateMainText().
            public int _maxVisibleCharacters = 0;
            // The speed at which characters are printed.
            public float _characterPrintSpeed;
            // Disables the printing sound effect (useful if a character is thinking).
            public bool _disablePrintingSoundEffect  = false;

            // Timer that counts down to zero from the typingSpeed
            public float _characterPrintTimer;

            // Determines whether the print is to be displayed and finished by a set timer, or affected
            // by user input.
            public PrintBehaviour _printBehaviour = 0;

            // Determines for how long a print will be displayed.
            public float _delayAfterPrintTimer;

            // The resulting string after modifications has been applied to the inString.
            public string _processedString;

            public Participant _speaker;

            public Dictionary<int, string> _identifiedOnPrintTags;

            public PrintTaskData(
                PrintBehaviour printBehaviour,
                float defaultTypingSpeed,
                float delayAfterPrint)
            {
                _characterPrintSpeed = defaultTypingSpeed;
                _printBehaviour = printBehaviour;
                // If Print Behaviour is delay, set the delayAfterPrintTimer.
                if (_printBehaviour == PrintBehaviour.delay)
                    _delayAfterPrintTimer = delayAfterPrint;
            }
        }

        /*
            Sets up a new PrintTaskData object at the start of a Print task.
        */
        private PrintTaskData PrintSetup(int speaker, string inString, float delayAfterPrint)
        {
            if (Task.current.isStarting)
            {
                // If set to use the last speaker (-1), use the stored _lastSpeaker variable
                if (speaker == -1)
                    speaker = _lastSpeaker;
                // Otherwise, store the speaker as _lastSpeaker.
                else
                    _lastSpeaker = speaker;
                // Set up a new TaskData item for storing data specific to the task.
                var determinedPrintBehaviour = PrintBehaviour.delay;
                if (delayAfterPrint == -1f)
                    determinedPrintBehaviour = PrintBehaviour.await;
                else if (delayAfterPrint == -2f)
                    determinedPrintBehaviour = PrintBehaviour.immediate;
                Task.current.item = new PrintTaskData(
                    determinedPrintBehaviour,
                    _DefaultCharacterPrintSpeed,
                    delayAfterPrint
                );
                PrintTaskData taskData = (PrintTaskData)Task.current.item;

                // Initializes the characterPrinterTimer.
                taskData._characterPrintTimer = taskData._characterPrintSpeed;
                // If setting the speaker character causes an exception, just set Participants to null.
                // This will at least not cause the application to crash.
                try
                {
                    if (speaker == -2 || Participants == null || Participants.Length == 0)
                        taskData._speaker = null;
                    else
                        taskData._speaker = Participants[speaker];
                }
                catch (Exception)
                {
                    taskData._speaker = null;
                }
                // If participants are set, the current speaker's name will be visible at the top of the
                // MainText from the beginning of the print.
                if (taskData._speaker != null)
                {
                    inString = taskData._speaker.Character.Shortname + "\n" + inString;
                    taskData._maxVisibleCharacters = taskData._speaker.Character.Shortname.Length + 1;
                }
                // Otherwise, just set the MainText to represent the processed string and starting
                // visible characters to be zero.
                else
                    taskData._maxVisibleCharacters = 0;

                // Modifies the indata string before printing (replaces placeholders with actual
                // character names, etc.), and sets up a dictionary for triggering OnPrintTag events
                // when the printer reaches a certain character in the text.
                var textPrinterStringAdjustmentsReturn = TextPrinterStringAdjustments.Apply(
                    this, inString);
                taskData._processedString = textPrinterStringAdjustmentsReturn.Item1;
                taskData._identifiedOnPrintTags = textPrinterStringAdjustmentsReturn.Item2;

                // If PrintBehaviour is set to "Immediate", let the entire string be printed out from
                // the start.
                if (taskData._printBehaviour == PrintBehaviour.immediate)
                {
                    taskData._maxVisibleCharacters = taskData._processedString.Length;
                    _textResetsAtEndOfTask = false;
                    UpdateMainText(taskData);
                }

                return taskData;
            }

            return (PrintTaskData)Task.current.item;
        }

        /*
            Prints text into the MainText element of the TextPrinter. If called through the PrintDelay
            task, the text will be printed and then remain a set amount of seconds. Otherwise, the
            user controls the text flow by clicking/hitting the action button.
        */
        public void Print(int speaker, string inString, float delayAfterPrint = -1f)
        {
            PrintTaskData taskData = PrintSetup(speaker, inString, delayAfterPrint);

            // Print text in dialogue window.
            if (taskData._characterPrintTimer <= 0 &&
                taskData._maxVisibleCharacters < MainText.textInfo.characterCount)
            {
                // Resets the character print timer.
                taskData._characterPrintTimer = taskData._characterPrintSpeed;
                taskData._maxVisibleCharacters++;
                // Play printing sample.
                if (!taskData._disablePrintingSoundEffect && taskData._maxVisibleCharacters % 2 == 1)
                {
                    if (taskData._speaker != null)
                    {
                        // If a character is speaking, adjust printing sample to voice pitch.
                        string printingSoundEffect = null; // TODO: GET SOUND EFFECT ASSET

                        //printingSoundEffect.Pitch = taskData._speaker.Character.VoicePitch;
                        // TODO: PLAY SOUND EFFECT ASSET
                    }
                    else
                        // If noone is speaking, the printing sample plays anyway, with its default
                        // pitch value.
                        // TODO: PLAY SOUND EFFECT ASSET
                }
            }

            // Check if there's any OnPrintTag command associated with the currently printed character,
            // and if so, triggers action.
            TextPrinterStringAdjustments.CheckForOnPrintTag(taskData, taskData._maxVisibleCharacters);

            // Update the _characterPrintTimer by decreasing the deltaTime since last frame.
            taskData._characterPrintTimer -= Time.deltaTime;

            // If set to not respond to user input, decrease _delayAfterPrintTimer if print
            // has finished.
            if (taskData._printBehaviour == PrintBehaviour.delay
                && taskData._maxVisibleCharacters >= MainText.textInfo.characterCount)
            {
                taskData._delayAfterPrintTimer -= Time.deltaTime;
            }

            CheckPrintSucceedConditions(taskData);

            UpdateMainText(taskData);
            AnimatedPortrait.UpdatePortraits(_participants, taskData._speaker);
        }

        /*
            Updates MainText (the main TMP component on TextPrinter).
        */
        private void UpdateMainText(PrintTaskData taskData)
        {
            MainText.maxVisibleCharacters = taskData._maxVisibleCharacters;
            MainText.text = taskData._processedString;
        }

        /*
            Checks the conditions for succeeding the Print task, depending on which PrintBehaviour is
            being used. If the conditions are met, the task's Succeed method is called.
        */
        private void CheckPrintSucceedConditions(PrintTaskData taskData)
        {
            // Set to true to allow the user to Succeed the Task immediately by pressing Left Ctrl -
            // useful for debugging.
            if (false && Input.GetKey("left ctrl"))
                Task.current.Succeed();
            // Checks the Succeed conditions of the PrintBehaviour.
            if (taskData._printBehaviour == PrintBehaviour.await
                && Input.GetMouseButtonDown(0) && !Task.current.isStarting)
            {
                // Task will succeed on user input if the print has already finished.
                if (MainText.maxVisibleCharacters >= MainText.textInfo.characterCount)
                    ResetAndSucceed(taskData);
                else
                {
                    // If print has not yet finished, user input will immediately display the full text.
                    while (taskData._maxVisibleCharacters < MainText.textInfo.characterCount)
                    {
                        taskData._maxVisibleCharacters++;
                        // Ensures that tag checks are still performed on each printed character.
                        TextPrinterStringAdjustments.CheckForOnPrintTag(
                            taskData, taskData._maxVisibleCharacters);
                    }
                }
            }
            // If PrintBehaviour is Delay, Succeed the Task when the delayPrintTimer has finished.
            else if (taskData._printBehaviour == PrintBehaviour.delay
                && taskData._delayAfterPrintTimer < 0f)
            {
                ResetAndSucceed(taskData);
            }
            // The Task will always be Succeeded immediately if PrintBehaviour is set to Immediate.
            else if (taskData._printBehaviour == PrintBehaviour.immediate
                && MainText.maxVisibleCharacters >= MainText.textInfo.characterCount)
            {
                ResetAndSucceed(taskData);
            }
        }

        /*
            Resets the string and Succeeds the task.
        */
        private void ResetAndSucceed(PrintTaskData taskData)
        {
            if (_textResetsAtEndOfTask)
                taskData._processedString = "";

            Task.current.Succeed();
        }
    }
}
