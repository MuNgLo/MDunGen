// gone through at v1.3
using System;

namespace MDunGen.Commons;
/// <summary>
/// State enum for the viewer
/// </summary>
public enum VIEWERMODE { DUNGEON, SECTION }
/// <summary>
/// Unused has noting in it<br></br>
/// Pending has things but is open to change<br></br>
/// Locked can't be changed (Goal is to have all piece data locked by end of generation)
/// </summary>
public enum MAPPIECESTATE { UNUSED, PENDING, LOCKED }
/// <summary>
/// Direction in map.
/// </summary>
public enum MAPDIRECTION { ANY, NORTH, EAST, SOUTH, WEST, UP, DOWN }
[Flags]
public enum WALLS { N = 1, E = 2, S = 4, W = 8 }
public enum PIECEKEYS
{
	NONE = 0,
	DEBUG = 3,
	OCCUPIED = 6,
	F = 7,
	C = 9,
	W = 11,
	WD = 12,
	WDW = 13,
	WCI = 14,
	ARCH = 15
}


[Flags]
public enum ROOMCONNECTIONRESPONCE { DOOR = 1, BALCONY = 2, BRIDGE = 4, STAIR = 8 }

[Obsolete]
public enum CATEGORYRULE { NONE, BUILD, LOOP, FLOOD }
public enum LOCATION { NONE, CENTER, ATTACHEDTOPREVIOUSSECTION, ATTACHEDTOSECTION, ATTACHEDTOPREVIOUSLEVEL }


/// <summary>
/// Debug variant values made readable
/// </summary>
enum DEBUGVARIANTS { ERROR, ARROW, WALLFLAGGREEN, WALLFLAGRED, FAULTY, END }
