// Gone through at v1.3
using System.Linq;
using Godot;
using MDunGen.Commons;
using MDunGen.Sections;

namespace MDunGen;

/// <summary>
/// Static Class that has helper methods for commonly needed things.
/// </summary>
static internal class DungeonUtils
{
	/// <summary>
	/// The offset used for everything position calculation from map data to engine position
	/// </summary>
	public static Vector3 globalOffset = Vector3.Zero;

	/// <summary>
	/// An array with the coordinates for all surrounding locations
	/// </summary>
	/// <param name="coord"></param>
	/// <returns></returns>
	static public MapCoordinate[] NeighbourCoordinates(MapCoordinate coord)
	{
		return [coord + MAPDIRECTION.NORTH,
				coord + MAPDIRECTION.EAST,
				coord + MAPDIRECTION.SOUTH,
				coord + MAPDIRECTION.WEST,
				coord + MAPDIRECTION.NORTH + MAPDIRECTION.EAST,
				coord + MAPDIRECTION.SOUTH + MAPDIRECTION.EAST,
				coord + MAPDIRECTION.SOUTH + MAPDIRECTION.WEST,
				coord + MAPDIRECTION.NORTH + MAPDIRECTION.WEST];
	}

	/// <summary>
	/// Flips the direction. Does this by changing N/E/S/W counterclockwise twice through the TwistLeft()
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	static public MAPDIRECTION Flip(MAPDIRECTION direction)
	{
		return TwistLeft(TwistLeft(direction));
	}
	/// <summary>
	/// Rotates the direction. Does this by changing N/E/S/W counterclockwise one step
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	static public MAPDIRECTION TwistLeft(MAPDIRECTION direction)
	{
		switch (direction)
		{
			case MAPDIRECTION.NORTH:
				return MAPDIRECTION.WEST;
			case MAPDIRECTION.WEST:
				return MAPDIRECTION.SOUTH;
			case MAPDIRECTION.SOUTH:
				return MAPDIRECTION.EAST;
			case MAPDIRECTION.EAST:
				return MAPDIRECTION.NORTH;
		}
		return direction;
	}
	/// <summary>
	/// Rotates the direction. Does this by changing N/E/S/W clockwise one step
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	static public MAPDIRECTION TwistRight(MAPDIRECTION direction)
	{
		switch (direction)
		{
			case MAPDIRECTION.NORTH:
				return MAPDIRECTION.EAST;
			case MAPDIRECTION.EAST:
				return MAPDIRECTION.SOUTH;
			case MAPDIRECTION.SOUTH:
				return MAPDIRECTION.WEST;
			case MAPDIRECTION.WEST:
				return MAPDIRECTION.NORTH;
		}
		return direction;
	}
	/// <summary>
	/// Returns the global position in engine space for the coord of the map piece
	/// </summary>
	/// <param name="piece"></param>
	/// <returns></returns>
	internal static Vector3 GlobalPosition(MapPiece piece)
	{
		return GlobalPosition(piece.Coord);
	}
	/// <summary>
	/// Unverified untested TODO verify!
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	internal static Vector3 GlobalPosition(Vector3I pos)
	{
		return GlobalPosition(GlobalSnapCoordinate(pos));
	}
	/// <summary>
	/// Returns the global position in engine space for the coord of the map piece
	/// </summary>
	/// <param name="piece"></param>
	/// <returns></returns>
	internal static Vector3 GlobalPosition(MapCoordinate Coord)
	{
		return new Vector3(Coord.x * 6, Coord.y * 6, Coord.z * 6) + globalOffset;
	}

	internal static Vector3 GlobalSnapPosition(Vector3 pos)
	{
		return GlobalPosition(GlobalSnapCoordinate((Vector3I)pos));
	}
	/// <summary>
	/// TODO figure out when this is used for what and comment it up
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	internal static MapCoordinate GlobalSnapCoordinate(Vector3I pos)
	{
		pos += new Vector3I(pos.X < 0 ? -3 : 3, 0, pos.Z < 0 ? -3 : 3);
		Vector3I c = pos == Vector3I.Zero ? Vector3I.Zero : (pos / 6);
		return new MapCoordinate(c.X, c.Y, c.Z);
	}

	/// <summary>
	/// Returns the vec3 rotation in degrees for the given orientation
	/// </summary>
	/// <param name="orientation"></param>
	/// <returns></returns>
	internal static Vector3 ResolveRotation(MAPDIRECTION orientation)
	{
		Vector3 rot = Vector3.Zero;
		switch (orientation)
		{
			case MAPDIRECTION.NORTH:
				rot.Y = 0;
				break;
			case MAPDIRECTION.EAST:
				rot.Y = -90;
				break;
			case MAPDIRECTION.SOUTH:
				rot.Y = -180;
				break;
			case MAPDIRECTION.WEST:
				rot.Y = -270;
				break;
		}
		return rot;
	}
	internal static Vector3 GlobalDirection(MAPDIRECTION orientation)
	{
		switch (orientation)
		{
			case MAPDIRECTION.EAST:
				return Vector3.Right;
			case MAPDIRECTION.SOUTH:
				return Vector3.Back;
			case MAPDIRECTION.WEST:
				return Vector3.Left;
			case MAPDIRECTION.UP:
				return Vector3.Up;
			case MAPDIRECTION.DOWN:
				return Vector3.Down;
		}
		return Vector3.Forward;
	}

	/// <summary>
	/// TODO make this work better
	/// </summary>
	/// <param name="wall"></param>
	/// <param name="materials"></param>
	public static void ApplyMaterialOverrides(Node3D wall, Material[] materials)
	{
		if (materials is null || materials.Length < 1) { return; }
		// Find first mesh instance
		MeshInstance3D meshInstance = null;
		for (int i = 0; i < wall.GetChildCount(); i++)
		{
			if (wall.GetChild(i) is MeshInstance3D m) { meshInstance = m; }
		}
		// If we found one apply the materials
		if (meshInstance is not null)
		{
			for (int i = 0; i < meshInstance.Mesh.GetSurfaceCount(); i++)
			{
				meshInstance.SetSurfaceOverrideMaterial(i, i < materials.Length ? materials[i] : materials[0]);
			}
		}
	}

	/// <summary>
	/// TODO fix this rotation
	/// </summary>
	/// <param name="section"></param>
	internal static void BuildWaterPlane(ISection section)
	{
		// create the mesh visuals
		MeshInstance3D surface = new MeshInstance3D();
		PlaneMesh plane = new PlaneMesh();
		surface.Mesh = plane;
		// set the material
		surface.MaterialOverride = section.WaterMaterial;

		// Get orientation of section
		MAPDIRECTION orientation = section.Orientation;

		// set the size of the plane
		int sizeDepth = Mathf.Abs(section.MaxCoord.x - section.MinCoord.x) * 6 + 6;
		int sizeWidth = Mathf.Abs(section.MaxCoord.z - section.MinCoord.z) * 6 + 6;
		plane.Size = new Vector2(sizeDepth, sizeWidth);

		//plane.SubdivideWidth = sizeX;
		//plane.SubdivideDepth = sizeZ;
		
		surface.Name = "Water";
		section.SectionContainer.AddChild(surface);
		
		Vector3 height = Vector3.Up * section.WaterLevel;
		//Vector3 depth = GlobalDirection(orientation) * sizeDepth * 0.5f + GlobalDirection(orientation) * 9.0f;
		//Vector3 width = GlobalDirection(TwistLeft(orientation)) * -3.0f;
		//surface.Position = height + depth + width;

		//surface.Position += GlobalPosition(section.Coord);
		surface.GlobalPosition = SectionWorldCenter(section) + height;
	}

	internal static Vector3 SectionWorldCenter(ISection section)
	{
		Vector3 min = GlobalPosition(section.MinCoord);
		Vector3 max = GlobalPosition(section.MaxCoord);
		Vector3 center = (max - min)*0.5f + min;
		return center;
	}
}// EOF CLASS

