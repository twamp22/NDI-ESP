# Twamp's (WIP) NDI ESP

## What is this?
- This is a proof of concept-turned work in progress for a network streamed ESP fuser video feed.
- ESP video feed uses an RGBA video format for alpha layer blending of ESP over gameplay without any 3rd party chroma/greenscreen software.(No black background to remove)
- This is a Work In Progress(WIP), changes may completely change the flow of the code, do not rely on stability.

## Testers welcome!
- I have only tested this on a number of my personal machines! Feel free to build it, run it and try to break it but please provide feedback to me via discord DM.

## EFT Educational Resources Discord Server
- [EFT Educational Resources Discord Server](https://discord.gg/jGSnTCekdx)

## Known Issues
- ESP Window is being rendered and hidden(terrible for FPS & rendering speed)
   - AutoFullScreen causes lag due to changing ESP window size, increasing FPS
- No way to use ESP window currently, user needs control over this in UI
- No way to disable NDI feed, user needs control over this in UI

## Future/Planned Features
- ESP Window
  - Checkbox for "Show ESP Window" (Required)
  - Checkbox for "ESP Window Clickthrough" (Required)
  - Checkbox for "ESP Window Transparency" (Required)
    - Checkbox for "ESP Transparency Chroma" (Potential)
- NDI ESP
  - Checkbox for "Stream NDI ESP" (Required)
  - Settings for NDI Stream quality (Potential)
