The Project described in the report is a Unity Project which simulates the vehicle-to-vehicle interaction.  
The project is designed to be run using Unity, meaning if the user would like to make changes, or compile and  
run the code on their computer, the Unity engine will have to be installed. However, an exe file is supplied for those  
who wish to skip the compilation step.(Program has been tested only in Windows)

Instructions for compiling and running the code:  
1.Ensure that the Unity engine is installed on the computer  
2.Place the V2V_Sim folder in the Documents directory of the computer(i.e. path: ~/Documents).  
	This ensures the Unity engine can see the project folder.  
3.Start up Unity and select the V2V_Sim project folder  
4.When the project successfully boots up, from the Unity toolbar, access File->Build & Run.  
5.Select the appropriate platform from the build settings window and select Build & Run.  

An executable should be produced and prompt to play the simulation should show. To exit the program, the user should  
press the escape key.  

Information related to simulation:  
As the simulation runs, packets are generated from a staticly positioned car at the bottom left side of the map.
A car carrying a packet is denoted by being surrounded by a yellow sphere, whereas a car surrounded by a blue sphere
denotes a car that has arrived at the packet destination. A black sphere denotes a car that is not carrying a packet.
In order to change the destination of the packet, go into the Unity engine and in the hierarchy window on the left side 
of the IDE, navigate through V2V sim->Network->StartPacket. Then on the left side in the Inspector window, scroll to Initiate Packet(Script)
and change the xDest,zDest, and DestRange accordingly to change the broadcast destination and acceptable range away from the destination.
