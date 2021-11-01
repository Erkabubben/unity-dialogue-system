using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StoryState
{
    /*
    *   AnimatedPortrait
    *
    *   When attached to a GameObject with a RawImage and an optional Shadow child GO, the
    *   AnimatedPortrait will update the RawImage to display the current portrait of its associated
    *   Participant. If a Shadow is used, the portrait will be shadowed if its associated Participant
    *   is not the current Speaker.
    */
    [RequireComponent(typeof(RawImage))]
    public class AnimatedPortrait : MonoBehaviour
    {
        private static List<AnimatedPortrait> _activePortraits = new List<AnimatedPortrait>();
        private RawImage _rawImage;
        private Texture _texture;
        private GameObject _shadow;
        private int _participantID;
        public int ParticipantID { get => _participantID; set => _participantID = value; }

        /*
            Call this method to update all AnimatedPortraits to represent the state of the TextPrinter
            and Participants array.
        */
        public static void UpdatePortraits(
            Participant[] participants,
            Participant speakerParticipant)
        {
            foreach (var portrait in _activePortraits)
            {
                if (portrait._participantID < participants.Length)
                {
                    var participantAssociatedToPortrait = participants[portrait._participantID];
                    // Update emotion of speaker.
                    if (participantAssociatedToPortrait == speakerParticipant)
                    {
                        portrait._rawImage.texture = participantAssociatedToPortrait.Character.GetPortrait(
                            speakerParticipant.Emotion);
                    }
                    // Activate or deactivate shadow depending on whether the iterated portrait
                    // belongs to the current speaker.
                    if (portrait._shadow != null)
                    {
                        if (participantAssociatedToPortrait == speakerParticipant)
                            portrait._shadow.GetComponent<CanvasGroup>().alpha = 0f;
                        else
                            portrait._shadow.GetComponent<CanvasGroup>().alpha = 1f;
                    }
                    if (portrait._rawImage.texture == null)
                        portrait._rawImage.texture = participantAssociatedToPortrait.Character.GetPortrait();
                }
            }
        }

        void Awake()
        {
            // Automatically adds the portrait to the activePortraits list.
            if (_activePortraits == null)
                _activePortraits = new List<AnimatedPortrait>();
            _activePortraits.Add(this);
            _rawImage = GetComponent<RawImage>();
            _texture = _rawImage.texture;
            _shadow = transform.Find("shadow").gameObject;

            if (gameObject.name.EndsWith("0"))
            {
                _participantID = 0;
            }
            else if (gameObject.name.EndsWith("1"))
            {
                _participantID = 1;
            }
        }

        void OnDestroy() => _activePortraits.Remove(this);
    }
}
