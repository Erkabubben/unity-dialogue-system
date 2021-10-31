# unity-dialogue-system
![alt text](https://jolly-liskov-e9f154.netlify.app/public/screenshots/31.jpg "Screenshot - Startle Response")
This dialogue system for Unity is intended to be used along with Text Mesh Pro and Eric Begue's Behaviour Tree engine Panda BT. I created it for my Unity project Startle Response, and it has been quite thoroughly refactored and bug tested during the development of the title. Its core functionality is to asynchronously print text onto the screen - two common use cases is for displaying subtitles in cinematic sequences, or RPG-style dialogue boxes.

![alt text](https://jolly-liskov-e9f154.netlify.app/public/screenshots/17.jpg "Screenshot - Startle Response")
While this may sound like a simple mechanic to implement, it can turn out to be surprisingly time-consuming. This solution provides a great number of features beyond the bare minimum, but is still very straight-forward and should be easy to follow and understand, at least for a user who is already familiar with Panda BT.

Some features:
- Includes three separate modes for how the text will be printed.
- Soft-coded dialogue participants.
- Pitch of the print sound can be adjusted to represent the speaker's voice pitch.
- Included Character class deriving from ScriptableObject makes it easy to create and define a new character.
- Name of the speaker is displayed on top of the text currently being printed.
- OnPrint tags can be placed in the text to trigger behaviour when the print reaches the tag's position.
- An AnimatedPortrait can be assigned to a dialogue participant, which can be updated by OnPrint tags to represent the character's emotional reactions.
- Works perfectly with Text Mesh Pro's tag system, so sections of the printed text can easily be set to be colored, bold or italics.

All included classes are thouroughly documented, and the system should be easy to modify for your own project's specific needs. After copying the script files into your project, search them for "TODO:" to find the places where the system needs to communicate with your other game systems.