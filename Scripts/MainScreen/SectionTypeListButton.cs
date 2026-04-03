// Gone through at v1.3
#if TOOLS
using Godot;
using MDunGen.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace MDunGen.MS;

[Tool, GlobalClass]
public partial class SectionTypeListButton : OptionButton
{
	[Export] MainScreen screen;
	public EventHandler<Type> OnSectionTypeSelected;
	public override void _EnterTree()
	{
		Clear();
		foreach (Type type in GetList())
		{
			if (type.Name.Contains("<>")) { continue; }
			if (type.GetInterface(nameof(ISection)) == null) { continue; }
			AddItem(type.Name);
		}
		Select(0); // SELECT 0 DEFAULT ONE for starters
				   //GD.Print($"SectionTypeListButton::_EnterTree() GetList.Count[{GetList().Count}] itemCount[{ItemCount}]");
	}
	public override void _ExitTree()
	{
		ItemSelected -= WhenItemSelected;
	}
	public override void _Ready()
	{
		ItemSelected += WhenItemSelected;
	}
	private void WhenItemSelected(long index)
	{
		RaiseSectionTypeChanged();
	}
	private void RaiseSectionTypeChanged()
	{
		EventHandler<Type> evt = OnSectionTypeSelected;
		if (evt is not null)
		{
			Type T = GetSelectedType();
			evt(this, T);
		}
	}

	private List<Type> GetList()
	{
		
		string nameSpace = "MDunGen.Sections";
		IEnumerable<Type> q = from t in Assembly.GetExecutingAssembly().GetTypes()
							  where t.IsClass && t.Namespace == nameSpace
							  select t;
		return q.ToList();
	}

	public Type GetSelectedType()
	{
		string nameSpace = "MDunGen.Sections";
		IEnumerable<Type> q = from t in Assembly.GetExecutingAssembly().GetTypes()
							  where t.IsClass && t.Namespace == nameSpace
							  select t;

		string selectedText = GetItemText(Selected);
		return q.ToList().Find(p => p.Name == selectedText);
	}
}// EOF CLASS
#endif