LudumDareBase
=============

As I was looking forward to LD30 I thought I’d make a little base code to make life easier and maybe allow more exciting designs. I've updated it a couple of times since, adding things it was missing (and spent all of LD32 adding pathfinding rather than making a game. Don't have that excuse for LD33! Maybe I'll do deferred lighting instead, or a music generator).

Included (in /assets/scripts/PriorityQueue, used for pathfinding) is BlueRaja's priority queue implementation, also available here:
https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
It's distributed under the MIT licence.
Everything else in this repository is either my work or automatically generated by Unity, and anybody can use any of it for any purpose, but giving Colthor credit would be nice.

Obviously, one can’t use the sprites and tiles for one’s compo game (and probably wouldn’t want to); they’re mostly in there as documentation and to demonstrate that/how the code works.

The important parts are:

 
Camera Script - 1:1 pixel mapping and simple 2D lighting
--------------------------------------------------------

“Assets/CameraSizer.cs” is the camera script, and it automatically adjusts Unity’s 2D camera so sprites are 1:1 mapped with the screen (or some multiple of). If the script’s PixelsPerUnit is the same as a sprite’s, it’ll be mapped 1:1. If it’s half the sprite’s the sprite will be pixel-doubled, and so on. Textures mapped to meshes (for instance, with the tilemap) or whatnot don’t work like this, o’course, so make sure you size them appropriately.

It also includes toggleable multiplicative lighting. You must manually add the layer "Lights", then everything on that layer will be rendered to a texture (I suggest additive blending) instead of with the main camera, which is then rendered to a quad in front of the scene using multiplicative blending. Lights.unity provides an example using both sprites and particles.


Tilemap with auto-tiling and pathfinding
----------------------------------------

IMPORTANT NOTE: Unity's anti-aliasing causes sub-pixel texture alignment errors in this code, and its graphics settings above "Good" include 2xAA as default. There are fudge factors (the two "Iota" parameters) available to try to work around this, but ideally just disable AA in higher quality settings (or the settings altogether). If you know a fix for this, please tell me!

USERS OF PREVIOUS VERSIONS: Tile maps are now generated the opposite way up - to match Unity's co-ordinates, rather than be upside down. I don't know why I used the other method before, but it made pathfinding a nuisance.

“Assets/TileMapScript.cs” is the tilemap. The .cs contains more thorough documentation, but it creates a Squares_X by Squares_Y tile grid, with each square Square_Size units to a side. It gets the data about which tile to use from LevelData, which is just a text string (0-9, A-F are ‘open’ tiles, X indicates a wall, all other characters should be ignored). “Assets/32tiles.png” is an example of the texture required.

The script then auto-tiles the walls, selecting the correct tiles from the given texture, and then adds box colliders for the walls as children to the attached gameobject. To reduce the number of colliders it groups them to cover multiple walls where possible – first making it as many tiles wide as possible, then as many tiles high. It would be possible to use a single polygon collider with several paths, but it would also be considerably more work, sorry ;)  (And maybe not faster.)

An example is in tileMap.unity.

Pathfinding uses A*, and is demonstrated in Pathfinding.unity. Press space to generate new start/end points, and if there's a route between them it'll be drawn in the debug mode as a multicoloured line. The yellow line indicates the direct path between the two points. Note: diagonal paths are only generated if both squares either side of the path are clear, to avoid cutting corners.

Menus
-----

Everybody hates UI coding and doesn't have time for it in a game jam, so a simple system (menus with buttons and option selectors) is included. See menu.unity for an example.




