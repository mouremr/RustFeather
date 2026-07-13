# Post-apocalypse-gaem

1/2/2025
fixed bug whre iris would still be in wall climbing position after sliding off a wall not touching the ground

12/22/2025

wall push off uses no stamina
stamina bar added
bug: delay on entering climbing state before being able to climb

19/20/2025

basic enemy added, moves towards iris, uses one raycast to detect sight and another to detect how close to punch
no attack logic for iris or hurt logic for enemies 

12/19/2025
arrhbhh matey

added scout assets and animation state machine, code not tested

moving iris her own player layer and off the ignore raycast layer
this created alot of problems
set scaffolding, pillars to climbable layer mask

important: had to change IsGrounded() to count the "climbable" and "ground" layers to both count as ground, otherwise not able to jump off tops of pillars or scaffolding.
not sure if this is the best way 

refactored and put most raycasts and wall and grounded checks into playerstate.cs as protected functions

12/17/2025
ahoy
jumping into wall disabled
but should this be a feature? what if the player is trying to jump up and wall climb more quickly


12/14/2025

Movement suite complete!
added blend tree to include a wall slide animation
jump, wall climb, wall jump, no sticky walls, roll, roll input buffering, mantle


12/13/2025 part 2
uncommented roll logic and fixed a climb flipping bug
movement suite complete  minus roll

todo list:
put roll logic into its own state


fixed mantle from jumping and wall climb!
known issues:
should have a seperate animation when the player slides down the wall with no upward input

12/13/2025 part 1
added climbing animation and cleaned up more visual bugs

known issues
mantle uses top of object to set grab point, but that gest the max height for tilemaps and not the individual pillars
transition from jump -> mantle is unclean, not sure why 
 
12/12/2025
known issues
mantle uses top of object to set grab point, but that gest the max height for tilemaps and not the individual pillars
