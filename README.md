# H3VR_Mods
All the scripts that I use for my mods are published here. Take care. They are tested but still might break your game! Also sometimes your mind!

Please credit me if you use my code!

# MagazineTapeMK2
## How to use:
Put Editor DLL (found in Releases/unity_code folder) to your Unity Project (Assets/Plugins)
Make a magazine that works on its own in game. Place the script as a new component on the magazine. Put a second magazine as a child inside of the first, primary magazine. Fill out the fields for primary and secondary magazine. if you have a GameObject with the tape mesh you can place it in there as well.
Use the context menu (cogwheel icon in the top right corner of the component) to calculate relative positions.

To make Vaulting work, duplicate the magazine and drag the secondary magazine out. Now place the primary magazine in it as a child object. Make a new FVRObject and ISID for it.
