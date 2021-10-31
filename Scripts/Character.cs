using System;
using UnityEngine;

namespace StoryState
{
    /*
    *   Character
    *
    *   Representes a character with a more or less prominent role in the game's storyline. Name
    *   fields, textures and voice pitch allows the Character to participate in dialogue sequences.
    */
    [CreateAssetMenu(fileName = "new-character", menuName = "Character")]
    [Serializable]
    public class Character : ScriptableObject
    {
        private enum ShortName { firstname, lastname, nickname, fullname }

        [SerializeField] private string _firstname;
        [SerializeField] private string _lastname;
        [SerializeField] private string _nickname;
        [SerializeField] private Texture2D _defaultPortrait;
        [SerializeField][Range(.35f, 2.15f)] private float _voicePitch = 1f;
        [SerializeField] private ShortName _shortName;

        public static string ConvertCharacterNameToFilename(string fullname)
            => fullname.ToLower().Trim().Replace(' ', '-');

        public string Firstname => _firstname;
        public string Lastname => _lastname;
        public string Nickname => _nickname;
        public string Fullname => _firstname + " " + _lastname;

        public string Filename => ConvertCharacterNameToFilename(Fullname);

        public string Shortname
        {
            get
            {
                return _shortName switch
                {
                    ShortName.firstname => _firstname,
                    ShortName.lastname => _lastname,
                    ShortName.nickname => _nickname,
                    ShortName.fullname => _firstname + " " + _lastname,
                    _ => _firstname,
                };
            }
        }

        public float VoicePitch => _voicePitch;

        public Texture2D GetPortrait(string emotion = "neutral", int style = 0)
        {
            Texture2D portrait = null; // TODO: GET TEXTURE2D ASSET
            return portrait != null ? portrait : _defaultPortrait;
        }
    }
}
