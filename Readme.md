# Project: The Temporal Classroom (Team 9)

## Target Device: Android (Google cardboard VR)

## Primary Scene: Main Scene

## GitHub Repository

URL: [https://github.com/Preet-Sojitra/Team-9-Temporal-Classroom-VR](https://github.com/Preet-Sojitra/Team-9-Temporal-Classroom-VR)

Branch: `final-final` contains final code. 

## Interaction Techniques

- **Raycast Pointer:** A physics-based raycasting system implemented on both "Past" and "Future" characters.

- **Gaze/Controller Interaction:** Users interact using the A (controller button).

- Highlighting: Objects of interest (Key, Chest, Pedestals, Projector) utilize an Outline component that activates upon hover.

- Cross-Temporal Teleportation: A custom sequence where placing an object on the "Past Pedestal" triggers a networked teleportation to the "Future Pedestal" after a 15-second delay.

## Multiplayer Operations

This application uses Photon Fusion (Shared Mode) for networking and voice.

- **Launch:** Open two instances of the application (or one Build and one Editor).
- **Session:** Both clients automatically join the session TemporalClassroomVoiceTest.
- **Roles:** The first player to join is assigned the Past Room. The second player is assigned the Future Room.
- **Sync:** Positions of the Key are synced across the network.

## Video Demonstration

YouTube Link: [https://youtu.be/rRPuHZfhgmc](https://youtu.be/rRPuHZfhgmc)