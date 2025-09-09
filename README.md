# DECO3801 - SLATES
Repository for the team SLATES in DECO3801.

CURRENT LATEST BUILD ON `network-refactor`
VR IS NOT YET IMPLEMENTED ON THE MVP BUILD

# Project Description
## Introduction
## Concept
The team is developing a co-op puzzle-platformer to enhance communication and team building. One VR player embodies a trapped mythical being who has access to a wealth of knowledge, while non-VR players act as worshippers who traverse through rooms to solve puzzles to free the being. The VR player’s main role is to provide information to the non-VR players but can also shrink to assist in certain tasks. The game is designed to have structured objectives that the players must work together to complete and make it to the end.

It is designed for broad audiences including teens (13+) and adults who want a social co-op activity. Specifically for team-building in workplaces, classrooms and casual parties where one VR player and one or more PC/laptop players join and participate.

## Setup Instructions

## Manual & Gameplay Instructions
### High Level Description
The gameplay focuses on team-building through asymmetric puzzle design, which requires the host (VR player/Giant) to communicate and assist the clients (non-VR players).
The asymmetric design primarily occurs through a knowledge difference between the host & client, with the host having access to various tomes of knowledge, and using these tomes to decipher, and understand the puzzles and challenges faced by the clients. Some puzzles will also make use of the host’s unique perspective in the world (i.e., that of a bird’s eye view of the puzzle). Other puzzles will require the host to shrink to a miniscule size to view & interact with items/elements hidden from view of the clients (e.g., shrinking down to coax a mouse from its den for the clients/kbm to catch).

The clients interact with the world through complex movement systems, and physics interactions. In particular, the clients will have to solve platforming puzzles, move physics objects to complete puzzles, etc. However, they will not have access to all information, and will need to communicate with the host to solve riddles, decipher clues, or view the world from a perspective they otherwise wouldn’t be able to view.

### VR/Giant Player
You can play the game as the giant by hosting a new game from the main menu. If the player is in VR, this will automatically be detected (WIP). The giant is spawned in the centre of the world, surrounded by a layer of fog. In one direction, there is an island w/ stepping stones leading from the KBM/Non-VR players' spawn. To complete the tutorial, the VR/Giant player will need to summon the book of knowledge, using `Space` on keyboard/mouse, or using the palm up gesture in VR (WIP). On the first page, there is a code for the lobby, so that the other players can join. They can then read the riddle to the non-vr/kbm players and assist them through verbal communication to complete the initial puzzle. Pages of the book can be turned using right click & drag.

### Non-VR Player
To play as a non-VR player, join the game using the code that the VR/Giant player provides. The player will be spawned into the world on a platform. Players can move using `WASD` and jump using `Spacebar`. There are stairs leading up to the tutorial area, which contain some boxes and some pressure plates. Players can pick up and move the boxes by left clicking when looking directly at the boxes. Left clicking again will drop the carried box. The tutorial requires the 4 pressure plates to be activated (using either boxes, or by standing on them) in a particular order, which is determined by the VR/Giant player's riddle. The smaller players should communicate with the larger player to solve the tutorial puzzle and move to the first island.

# Technical Details
## Libraries & Frameworks
## Functionality & Implementation Details
