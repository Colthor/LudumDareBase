LudumDareBase
=============

As I was looking forward to LD30 I thought I’d make a little base code to make life easier and maybe allow more exciting designs.

Anybody can use anything in this repository for any purpose, but giving Colthor credit would be nice.

Obviously, one can’t use the sprites and tiles for one’s compo game (and probably wouldn’t want to); they’re mostly in there as documentation and to demonstrate that/how the code works.

The important parts are:

 
Camera Script

“Assets/CameraSizer.cs” is the camera script, and it automatically adjusts Unity’s 2D camera so sprites are 1:1 mapped with the screen (or some multiple of). If the script’s PixelsPerUnit is the same as a sprite’s, it’ll be mapped 1:1. If it’s half the sprite’s the sprite will be pixel-doubled, and so on. Textures mapped to meshes (for instance, with the tilemap) or whatnot don’t work like this, o’course, so make sure you size them appropriately.


Tilemap with auto-tiling

“Assets/TileMapScript.cs” is the tilemap. The .cs contains more thorough documentation, but it creates a Squares_X by Squares_Y tile grid, with each square Square_Size units to a side. It gets the data about which tile to use from LevelData, which is just a text string (0-9, A-F are ‘open’ tiles, X indicates a wall, all other characters should be ignored). “Assets/32tiles.png” is an example of the texture required.

The script then auto-tiles the walls, selecting the correct tiles from the given texture, and then adds box colliders for the walls as children to the attached gameobject. To reduce the number of colliders it groups them to cover multiple walls where possible – first making it as many tiles wide as possible, then as many tiles high. It would be possible to use a single polygon collider with several paths, but it would also be considerably more work, sorry ;)  (And maybe not faster.)

The gameobject’s origin is the bottom left corner of the map, and LevelData is mapped in the same way as the tile texture; the first character is the top left, the second row will start – assuming no ignored characters – at Squares_X, and the bottom right will be character (Squares_X * Squares_Y – 1).

 

I’ll try to back-port any improvements or bugfixes made over the course of LD30 for future jams!
