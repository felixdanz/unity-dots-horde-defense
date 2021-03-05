# unity-dots-horde-defense

I created this project to learn how to use Unity's Data-Oriented Technology Stack.
My aim is to create a simple game where you have to defend yourself against endless hordes of enemies while building up your base.

## Current Features
	- Large amount of units with A* pathfinding.
	- Buildings:
		- Turrets
		- Walls
		- (Unit-)Factories
	- Units:
		- 1 player type
		- 1 enemy type

## Work in Progress
	- Pathfinding Improvements
	- Unit Attacks
	
## Known Problems
	- Units can stack creating increasingly performance draining CreateContactsJobs

## Planned Features
	- Win/Lose conditions
	- More buildings
	- More unit types
	- Resource Farming -> Build costs
	- Better Visuals and Effects

## Unity and Package Versions used
	- Unity 2020.2.0f1
	- Burst 1.4.3
	- Mathematics 1.2.1
	- Collections 0.14.0-preview.16
	- Jobs 0.7.0-preview.17
	- Entities 0.16.0-preview.21
	- Unity Physics 0.5.1-preview.2
	- Hybrid Renderer 0.10.0.preview-21