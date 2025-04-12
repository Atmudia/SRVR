# SRVR (Slime Rancher VR)

SRVR is a modification that brings full Virtual Reality support to Slime Rancher, allowing you to explore the vibrant world of the Far, Far Range in an immersive VR experience.

## Features

- Full VR integration with SteamVR support
- Custom VR hand models and interactions
- Realistic VR controls for all game mechanics
- Seamless integration with the base game
- Support for VR motion controls and tracking
- Custom VR death handling system
- Optimized performance for VR

## Prerequisites

- Slime Rancher (base game)
- SteamVR
- A compatible VR headset (any SteamVR-supported headset)
- Windows 10 or later

## Installation

1. Ensure Slime Rancher is installed and updated to the latest version.
2. Install SRML.
3. Install SteamVR if you haven't already.
4. Download the latest release of SRVR from the releases page or via nexusmods.
5. Copy the DLL file to your SRML mods directory.
6. Launch the game *without* SteamVR. If you have the VR Playground DLC installed, the game will prompt you about its uninstallation. It will also ask whether you want to optimize the base game for VR (**you do not need to do this**). A console program will then run to patch the game—let it complete before exiting.
   <br>**Warning the optimize base game can cause anti-virus to false flag installer**
7. Relaunch the game with SteamVR enabled.

## Usage

1. Start SteamVR.
2. Launch Slime Rancher.
3. The game will automatically detect your VR headset and initialize VR mode.
4. Use your VR controllers to interact with the game world:
   - Move using smooth turn or snap turn VR locomotion.
   - Use your vacpack and hands to interact with slimes and objects.
   - Access your inventory and vacpack naturally in VR.
   - Grab slimes/food and throw them.

To launch game without vr mod, use *-novr* argument when launching the game

## VR Configuration

SRVR includes several configurable VR settings which are adjustable in Other Tab in Options:

- **Switch Hands** *(default: false)* – Allows switching primary hand controls.
- **Snap Turn** *(default: false)* – Enables snap turning instead of smooth turning.
- **Snap Turn Angle** *(default: 45°)* – Adjusts the snap turn angle.
- **Turn Sensitivity** *(default: 1.0)* – Adjusts the smooth turn speed.
- **Distance Grab** *(default: true)* – Enables grabbing objects from a distance.
- **Height Adjust** *(default: 0.0)* – Adjusts the height of the HMD.
- **Static UI Position** *(default: true)* – If UI should stay in one place in the world, or follow the camera.

## Known Issues

- Please report any bugs or issues on the GitHub Issues page.

## Contributing

We welcome contributions! Feel free to submit a pull request on GitHub.

## License

This project is licensed under the terms specified in the included LICENSE file.

## Credits

- Created by Atmudia, [Lionmeow](https://github.com/Lionmeow), and Whippersnatch Pumpkinpatch
- Special thanks to all testers
- Utilizes SteamVR SDK
- Utilizes Unity XR SDK
- Includes VR initialization code from [VHVR-Mod](https://github.com/brandonmousseau/vhvr-mod)

## Support

If you need help or encounter issues:
1. Check the Known Issues section.
2. Create a new issue on GitHub with a detailed description of your problem.
3. Include your system specifications and VR hardware details in your report.
4. Join the [Flat2VR Discord](http://flat2vr.com/) for additional support.

## Video Showcase
Check [AG4VR's video](https://www.youtube.com/watch?v=9egH_BYz-pU) to check how the vr mod looks in game.

## Version History

### 1.0 - Initial Release
- First official release of SRVR.
### 1.1 - Revamp of Input and small fixes
- [Pull Request](https://github.com/Atmudia/SRVR/pull/15) by **Lionmeow**
- Fixes issue with opening slimepedia
- Adds seated mode, turn sensitivity, static ui position

