// Gone through at v1.3
using System;
using System.Collections.Generic;
using MDunGen.Sections;

namespace MDunGen.Pathfinding;

internal class PathAnswer
{
    internal readonly SectionConnection startConnection;
    internal readonly SectionConnection endConnection;
    internal List<PathLocation> path = new List<PathLocation>();
    internal List<int> connectionPath = new List<int>();
    internal PathAnswer(SectionConnection startConnection, SectionConnection endConnection, int tempConnection=-1, int tempConnection2=-1){
        this.startConnection = startConnection;
        this.endConnection = endConnection;
    }
     internal PathAnswer(PathQuery query){
        startConnection = query.startConnection;
        endConnection = query.endConnection;
    }
}// EOF CLASS