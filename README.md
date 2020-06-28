# Aircraft-Physics
Fixed wing aircraft physics for games.

[__Watch video tutorial__](https://youtu.be/p3jDJ9FtTyM)
![Thumbnail](https://i.imgur.com/Xx7fCtX.png "Video thumbnail")

## How to make a plane
*These instructions are WIP.*

Check out the scene in the example to see how to set up the system. Here are also some tips!

1. Add `Rigidbody` to your plane. Set linear and angular drag to zero. Set the mass to something around the mass of the real aircraft of similar size. 
Or you can make it much lighter so it will be easier to control. 
2. Add collision. For gears I use simple spheres with slippery material. You can try to set up the wheel colliders instead, but it can be tricky. 
3. Add `AircraftPhysics` component. Orange gizmo shows the position of the center of mass. Set thrust value to about the weight of the rigidbody to make the plane easier to control. Or search for realistic thrust to weight ratio. 
4. How to add aerodynamic surfaces. Create a gameobject and add `AeroSurface` component. Add `AeroSurface` to the list in `AircraftPhysics`. `AeroSurface` requires a config file with parameters. Create `AeroSurfaceConfig` scriptable object and assign it to `AeroSurface`. 
5. Create wings and horizontal tail stabiliser surfaces. You don’t need to touch parameters other than chord, aspect ratio and flapFraction. Make sure surfaces are placed perfectly symmetrically. 
6. The blue gizmo shows the point of application of the sum of aerodynamic forces or aerodynamic center. For the plane to be stable around the pitch axis aerodynamic center should be slightly behind the center of mass. If the center of mass is behind the aerodynamic center you can tweak stabilizer position or size. Or you can tweak the colliders to change your plane’s center of mass. To make small tweaks you can also change zeroLiftAoA of the stabiliser a little.
7. Create a vertical stabilizer. Then make sure to add at least two surfaces for the body, one behind and one in front of the center of mass.
8. Add control surfaces. Setup ailerons, elevator and rudder as shown on screenshot. Ailerons control roll, elevator controls pitch and rudder controls yaw. You can check how your aircraft is going to react to different angles of attack and control inputs using the display settings file.
9. Add `AircraftController` to apply control inputs.

