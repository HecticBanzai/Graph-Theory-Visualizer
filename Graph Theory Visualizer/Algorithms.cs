using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Petzold.Media2D;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using System.Reflection.Emit;
using System.Net;

void overlay_vertex(Canvas canvas, Grid current_vertex)
{
    TextBox overlay_textbox = (TextBox)current_vertex.Children[0];

    Grid overlay_vertex = new Grid
    {
        Width = 50,
        Height = 50,
        Background = Brushes.Yellow,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
    };

    overlay_vertex.Children.Add(overlay_textbox);

    Canvas.SetLeft(overlay_vertex, Canvas.GetLeft(current_vertex) - 25);
    Canvas.SetTop(overlay_vertex, Canvas.GetLeft(current_vertex) - 25);
    Canvas.SetZIndex(overlay_vertex, 5);

    canvas.Children.Add(overlay_vertex);
}

void overlay_edge(Canvas canvas, Grid current_edge)
{

}

void DPS(Canvas canvas, Dictionary<Grid, List<Grid>> adjacency_list, Grid start_vertex, string current_mode)
{
    List<Grid> explored_vertices = new List<Grid>();
    Stack to_explore = new Stack();
    List<Grid> overlayed_verticees = new List<Grid>();


    explored_vertices.Add(start_vertex);
}