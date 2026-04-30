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
public enum MAPDIRECTION { ANY = 0, PIECE = 7, NORTH = 1, EAST = 2, SOUTH = 3, WEST = 4, UP = 5, DOWN = 6 }
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
	ARCH = 15,
	CLIMBABLE = 16,
	RAILING = 17, 
	SUPPORT = 18, 
	BRIDGE = 19, 
}

public enum RAILING { LONG = 0, CORNERROUNDED = 1 }
public enum SUPPORT { LONG = 0, CORNERROUNDED = 1 }
public enum BRIDGES { LONG = 0, FOUNDATION = 1, SECTION = 2, STUB = 3, HANDRAILLONG = 4, HANDRAILSECTION = 5, HANDRAILSTUB = 6, HANDRAILLONGOPEN = 7, HANDRAILPOST = 8 }

[Flags]
public enum ROOMCONNECTIONRESPONCE { DOOR = 1, BALCONY = 2, BRIDGE = 4, STAIR = 8 }

[Obsolete]
public enum CATEGORYRULE { NONE, BUILD, LOOP, FLOOD }
public enum LOCATION { NONE, CENTER, ATTACHEDTOPREVIOUSSECTION, ATTACHEDTOSECTION, ATTACHEDTOPREVIOUSLEVEL }

/// <summary>
/// When starting a section or pulling an attachment location from existing section this<br/>
/// set what section internal height is valid for attachment.
/// </summary>
public enum ATTACHHEIGHT { SAME, RANDOM, BOTTOM, CENTER, TOP }


/// <summary>
/// Debug variant values made readable
/// </summary>
enum DEBUGVARIANTS { ERROR, ARROW, WALLFLAGGREEN, WALLFLAGRED, FAULTY, END }
