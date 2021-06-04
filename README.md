# H3VR_Mods
All the scripts that I use for my mods are published here. Take care. They are tested but still might break your game! Also sometimes your mind!

Please credit me if you use my code!

# Magazine Tape
## How to use:
To use, put into "Assets/Plugins" Folder in your unity project (create if it doesn't already exist). Put new MagTapeProxy component as root of your double mag. Place two normal Magazines inside of that object (these should work without being inside the tape. if your mag doesn't work on its own it won't work inside the tape either!). reference the two magazines inside of the script.
Create a FVRPhysicalObject inside your Object and configure it correctly (see normal Magazines for example, or P90 mag if you want a "rotation free" grab). Reference it inside of the MagTapeProxy script, just like the mags.
Add a trigger to your object (box or capsule collider set to trigger)
Add a rigid body to your object. (configure like the ones on the mags)
After moving the Magazines how you want them to positioned, drag the colliders (usually called Phys) outside the magazines, so they are a child of the Tape instead (this is a fix for the tape dropping through the floor on spawn, IDK why this is happening)

configure FVRObject and ISID like you would with any other item you wanna put in the game, put the configured FVRObject inside your FVRPhysicalObject script (it needs this for spawnlocking).

## Putting it in the game:
Put the DLL inside of a .deli folder and make sure to edit the manifest so that it gets loaded on SETUP with deli:assembly. (Check the OtherLoader manifest, it's doing the same thing with the OtherLoader.dll)

### DISCLAIMER:
Making the Tape Mag vault correctly when inside of a firearm requires a bit more setup, but the Mag Tape should "just" work like this.
idk if I can make the whole "vaulting ready" process any easier, so if you want to know how it works, DM me on discord.
