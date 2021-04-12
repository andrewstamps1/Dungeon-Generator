using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace DungeonGen {
class Program {

    public struct rect {
	    public int x, y, width, height;
        
        public rect(int x1, int y1, int w1, int h1){
            x = x1;
            y = y1;
            width = w1;
            height = h1;
        }
    };

    class Dungeon {
        public enum Tile {
		    Unused		= ' ',
		    Floor		= '.',
		    Corridor	= ',',
		    Wall		= '#',
		    ClosedDoor	= '+',
		    OpenDoor	= '-',
		    UpStairs	= '<',
		    DownStairs	= '>'
	    };
 
	    enum Direction	{
		    North,
		    South,
		    West,
		    East,
		    DirectionCount
	    };


		public int minCorridorLength = 4;
		public int maxCorridorLength = 4;
        public int roomChance = 50;
		public int minRoomSize = 4;
		public int maxRoomSize = 4;
        public int maxFeatures = 12;
        public int width { get; set; }
        public int height { get; set; }
        public Tile[] tiles; //= new List<Tile>();
        public List<rect> rooms = new List<rect>();
        public List<rect> exits = new List<rect>();
 
    #region "random"
        int randomInt(int min, int max) {
            return new Random().Next(min,max);
        }

        int randomInt(int max) {
            return new Random().Next(0,max);
        }

        bool randomBool() {
            int Temp = new Random().Next(0,101);
            if (Temp > 50) return true;
            return false;
        }
    #endregion
   
        public Dungeon(int newwidth, int newheight, int newFeatures, int newMinCorLen,int newMaxCorLen,int newRoomChance,int newMinRomSiz,int newMaxRomSiz) {
		    width = newwidth;
            height = newheight;

	        minCorridorLength = newMinCorLen;
	        maxCorridorLength = newMaxCorLen;
            roomChance        = newRoomChance;
	        minRoomSize       = newMinRomSiz;
	        maxRoomSize       = newMaxRomSiz;
            maxFeatures       = newFeatures;

            tiles = new Tile[width * height];

            for (int index = 0; index < tiles.Count();index += 1) {
                tiles[index] = Tile.Unused;
            }
	    }
 
	    bool generate() {
		    // place the first room in the center
            Direction dirTemp = (Direction) randomInt((int) Direction.DirectionCount);
		
            if (!makeRoom(width / 2, height / 2, dirTemp, true)) {
			    Console.Write( "Unable to place the first room.\n");
			    return false;
		    }
 
		    // we already placed 1 feature (the first room)
		    for (int i = 1; i < maxFeatures; ++i) {
			    if (!createFeature()) {
				    Console.Write( "Unable to place more features (placed " + i + ").\n");
				    break;
			    }//end if
		    }//end if
 
		    if (!placeObject(Tile.UpStairs)) {
			    Console.Write( "Unable to place up stairs.\n");
			    return false;
		    }//end if
 
		    if (!placeObject(Tile.DownStairs)) {
			    Console.Write( "Unable to place down stairs.\n");
			    return false;
		    }//end if
 
            for (int index = 0; index < tiles.Length; index += 1) {       
			    if (tiles[index] == Tile.Unused) {
				    //tiles[index] = (Tile)'.';
                } else if (tiles[index] == Tile.Floor || tiles[index] == Tile.Corridor){
				    //tiles[index] = (Tile)' ';
                }//end if
		    }//next
            return true;
	    }//end function
 
	    void print() {
		    for (int y = 0; y < height; ++y) {
			    for (int x = 0; x < width; ++x) {
				    Console.Write((char) getTile(x, y));
                }//end if
			    Console.WriteLine();
		    }//end if
	    }//end function
 
        private	Tile getTile(int x, int y) {
		    if (x < 0 || y < 0 || x >= width || y >= height) {
			    return Tile.Unused;
            }else{
		        return tiles[x + y * width];
            }//end if
	    }//end 
 
	    void setTile(int x, int y, Tile tile) {
		    tiles[x + y * width] = tile;
	    }
 
	    bool createFeature()	{
		    for (int i = 0; i < 1000; ++i) {

			    if (exits.Count == 0) {
                    break;
                }//end if
        
			    // choose a random side of a random room or corridor
			    int r = randomInt(exits.Count);
			    int x = randomInt(exits[r].x, exits[r].x + exits[r].width - 1);
			    int y = randomInt(exits[r].y, exits[r].y + exits[r].height - 1);
 
			    // north, south, west, east
			    for (int j = 0; j < (int) Direction.DirectionCount; ++j) {
				    if (createFeature(x, y, (Direction) j ))	{
					    exits.RemoveAt(r);
					    return true;
				    }//end if
			    }//end if
		    }//next i
 
		    return false;
	    }//end function
 
	    bool createFeature(int x, int y, Direction dir)	{
		    int dx = 0;
		    int dy = 0;
 
		    if (dir == Direction.North) {
               dy = 1;
            } else if (dir == Direction.South) {
               dy = -1;
            } else if (dir == Direction.West) {
               dx = 1;
            } else if (dir == Direction.East) {
               dx = -1;
            }//end if
 
		    if (getTile(x + dx, y + dy) != Tile.Floor && getTile(x + dx, y + dy) != Tile.Corridor) {
			    return false;
            }//end if
 
		    if (randomInt(100) < roomChance) {
			    if (makeRoom(x, y, dir)) {
				    setTile(x, y, Tile.ClosedDoor);
				    return true;
			    }//end if
		    }else{
			    if (makeCorridor(x, y, dir)) {
				    if (getTile(x + dx, y + dy) == Tile.Floor){
					    setTile(x, y, Tile.ClosedDoor);
                    }else{ //don't place a door between corridors
					    setTile(x, y, Tile.Corridor);
                    }//end if
				    return true;
			    }//end if
		    }//end if
 
		    return false;
	    }//end function
 
	    bool makeRoom(int x, int y, Direction dir, bool firstRoom = false) {
 		    rect room = new rect();

		    room.width  = randomInt(minRoomSize, maxRoomSize);
		    room.height = randomInt(minRoomSize, maxRoomSize);
 
		    if (dir == Direction.North) {
			    room.x = x - room.width / 2;
			    room.y = y - room.height;
		    }else if (dir == Direction.South) {
			    room.x = x - room.width / 2;
			    room.y = y + 1;
		    }else if (dir == Direction.West)	{
			    room.x = x - room.width;
			    room.y = y - room.height / 2;
		    }else if (dir == Direction.East)	{
			    room.x = x + 1;
			    room.y = y - room.height / 2;
		    }//end if
 
		    if (placeRect(room, Tile.Floor)) {
			    rooms.Add(room);
			    if (dir != Direction.South || firstRoom) {// north side     
                    exits.Add(new rect(room.x, room.y - 1, room.width ,1));
                }if (dir != Direction.North || firstRoom) {// south side              
                    exits.Add(new rect(room.x,  room.y - room.height, room.width , 1));
                }if (dir != Direction.East || firstRoom) {// west side
                    exits.Add(new rect(room.x - 1, room.y, 1 , room.height));
                }if (dir != Direction.West || firstRoom) {// east side
                    exits.Add(new rect(room.x + room.width, room.y, 1 , room.height));
                }//end if
			    return true;
		    }//end if
 
		    return false;
	    }//end function
 
	    bool makeCorridor(int x, int y, Direction dir)	{
		    rect corridor = new rect();
		    corridor.x = x;
		    corridor.y = y;
 
		    if (randomBool()) {// horizontal corridor
			    corridor.width = randomInt(minCorridorLength, maxCorridorLength);
			    corridor.height = 1;
 
			    if (dir == Direction.North) {
				    corridor.y = y - 1;
 
				    if (randomBool()) {// west
					    corridor.x = x - corridor.width + 1;
                    }//end if
			    }else if (dir == Direction.South)	{
				    corridor.y = y + 1;
 
				    if (randomBool()) {// west
					    corridor.x = x - corridor.width + 1;
                    }//end if
			    }else if (dir == Direction.West){
				    corridor.x = x - corridor.width;

                }else if (dir == Direction.East){
				    corridor.x = x + 1;
                }//end if
		    }else{ // vertical corridor
			    corridor.width = 1;
			    corridor.height = randomInt(minCorridorLength, maxCorridorLength);
 
			    if (dir == Direction.North) {
				    corridor.y = y - corridor.height;
                }else if (dir == Direction.South) {
				    corridor.y = y + 1;
                }else if (dir == Direction.West) {
				    corridor.x = x - 1;
				    if (randomBool()) {// north
					    corridor.y = y - corridor.height + 1;
                    }//end if
			    }else if (dir == Direction.East){
				    corridor.x = x + 1;
 
				    if (randomBool()) {// north
					    corridor.y = y - corridor.height + 1;
                    }//end if
			    }//end if
		    }//end if
 
		    if (placeRect(corridor, Tile.Corridor))	{
			    if (dir != Direction.South && corridor.width != 1) {// north side
                    exits.Add(new rect(corridor.x, corridor.y - 1, corridor.width , 1));
                }if (dir != Direction.North && corridor.width != 1) {// south side
                    exits.Add(new rect(corridor.x, corridor.y + corridor.height, corridor.width, 1));
                }if (dir != Direction.East && corridor.height != 1) {// west side                  
                    exits.Add(new rect(corridor.x - 1, corridor.y, 1 , corridor.height));
                }if (dir != Direction.West && corridor.height != 1) {// east side
                    exits.Add(new rect(corridor.x + corridor.width, corridor.y, 1 , corridor.height));
                }//end if

			    return true;
		    }//end if

		    return false;
	    }//end function
 
	    bool placeRect(rect Rectangle, Tile tile) {
		    if (Rectangle.x < 1 || Rectangle.y < 1 || Rectangle.x + Rectangle.width > width - 1 || Rectangle.y + Rectangle.height > height - 1) {
			    return false;
            }//end if

		    for (int y = Rectangle.y; y < Rectangle.y + Rectangle.height; ++y) {
			    for (int x = Rectangle.x; x < Rectangle.x + Rectangle.width; ++x) {
				    if ( getTile(x, y) != Tile.Unused) {
					    return false; // the area already used
                    }//end if
			    }//end if
            }//end if
 
		    for (int y = Rectangle.y - 1; y < Rectangle.y + Rectangle.height + 1; ++y) {
			    for (int x = Rectangle.x - 1; x < Rectangle.x + Rectangle.width + 1; ++x) {
				    if (x == Rectangle.x - 1 || y == Rectangle.y - 1 || x == Rectangle.x + Rectangle.width || y == Rectangle.y + Rectangle.height) {
					    setTile(x, y, Tile.Wall);
                    }else{
					    setTile(x, y, tile);
                    }//end if
			    }//end if
            }//end if    
 
		    return true;
	    }//end function
 
	    bool placeObject(Tile tile)	{
		    if (rooms.Count == 0) {
			    return false;
            }//end if
 
		    int r = randomInt(rooms.Count()); // choose a random room
		    int x = randomInt(rooms[r].x + 1, rooms[r].x + rooms[r].width - 2);
		    int y = randomInt(rooms[r].y + 1, rooms[r].y + rooms[r].height - 2);
 
		    if (getTile(x, y) == Tile.Floor) {
			    setTile(x, y, tile);
			    // place one object in one room (optional)
			    rooms.RemoveAt(r);
			    return true;
		    }//end if

		    return false;
	    }//end function
 
        static void Main(string[] args) {

            bool Done = true;
            while (true) {
                Console.Clear();
                Dungeon d = new Dungeon(32, 32, 12 ,4,4,50,4,4);
                Done = !d.generate();
                d.print(); 
                Console.ReadKey();
            }//end while
            Console.ReadKey();
        }//end main

    }//end class
}//end class

}//end namespace