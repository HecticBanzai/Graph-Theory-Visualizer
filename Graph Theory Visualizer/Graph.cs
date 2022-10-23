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
//using System.Runtime.Remoting.Contexts;
using System.Net;

namespace Graph_Theory_Visualizer
{
    internal class Graph
    {
        public Dictionary<Grid, List<Grid>> adjacency_list;
        public Dictionary<Tuple<Grid, Grid>, int> weights;
        public Dictionary<ArrowLine, Tuple<Grid, Grid>> edge_end_points;
        public Dictionary<ArrowLine, TextBox> edge_textbox;
        public bool contains_directed_and_undirected;
        public bool contains_weighted_and_unweighted;

        public Graph()
        {
            adjacency_list = new Dictionary<Grid, List<Grid>>();
            weights = new Dictionary<Tuple<Grid, Grid>, int>();
            edge_end_points = new Dictionary<ArrowLine, Tuple<Grid, Grid>>();
            edge_textbox = new Dictionary<ArrowLine, TextBox>();
            contains_directed_and_undirected = false;
            contains_weighted_and_unweighted = false;
        }

        public override string ToString()
        {
            string vertices = $"NVERT {adjacency_list.Count}\n";
            foreach (KeyValuePair<Grid, List<Grid>> entry in adjacency_list)
            {
                vertices += $"{entry.Key.Name.Replace("_", "")}";

                foreach (Grid vertex in entry.Value)
                {
                    vertices += $" {vertex.Name.Replace("_", "")}";
                }

                vertices += "\n";
            }
            vertices += $"WEIGHTS {weights.Count}\n";

            foreach (KeyValuePair<Tuple<Grid, Grid>, int> entry in weights)
            {
                vertices += $"{entry.Key.Item1.Name.Replace("_", "")} {entry.Key.Item2.Name.Replace("_", "")} {entry.Value}\n";
            }

            vertices += "EDGE END POINTS \n";

            foreach (KeyValuePair<ArrowLine, Tuple<Grid, Grid>> entry in edge_end_points)
            {
                vertices += $"{entry.Key.Name.Replace("_", "")} ({entry.Value.Item1.Name.Replace("_", "")}, {entry.Value.Item2.Name.Replace("_", "")})\n";
            }

            vertices += "EDGE TEXTBOXES \n";

            foreach (KeyValuePair<ArrowLine, TextBox> entry in edge_textbox)
            {
                vertices += $"{entry.Key.Name.Replace("_", "")} {entry.Value.Text}\n";
            }

            return vertices;
        }

        public void add_vertex(Canvas Display, Grid vertex)
        {
            adjacency_list.Add(vertex, new List<Grid>());

            Display.Children.Add(vertex);
        }

        public void add_edge(Canvas Display, Grid start_vertex, Grid end_vertex, ArrowLine edge, Boolean directed, TextBox weight)
        {
            if (directed)
            {
                adjacency_list[start_vertex].Add(end_vertex);
            }
            else
            {
                adjacency_list[start_vertex].Add(end_vertex);
                adjacency_list[end_vertex].Add(start_vertex);
            }

            if (!string.IsNullOrEmpty(weight.Text))
            {
                weights.Add(Tuple.Create(start_vertex, end_vertex), Int32.Parse(weight.Text));
            }

            edge_end_points.Add(edge, Tuple.Create(start_vertex, end_vertex));
            edge_textbox.Add(edge, weight);

            Display.Children.Add(edge);
            Display.Children.Add(weight);
        }

        public void remove_vertex_and_connected_edges(Canvas Display, Grid vertex_to_remove)
        {
            Display.Children.Remove(vertex_to_remove);
            adjacency_list.Remove(vertex_to_remove);

            foreach (KeyValuePair<Grid, List<Grid>> entry in adjacency_list)
            {
                if (entry.Value.Contains(vertex_to_remove))
                {
                    Display.Children.Remove(entry.Value[entry.Value.IndexOf(vertex_to_remove)]);

                    entry.Value.Remove(vertex_to_remove);
                }
            }

            foreach (KeyValuePair<ArrowLine, Tuple<Grid, Grid>> entry in edge_end_points.Where(kvp => kvp.Value.Item1.Equals(vertex_to_remove) | kvp.Value.Item2.Equals(vertex_to_remove)).ToList())
            {
                Display.Children.Remove(entry.Key);
                edge_end_points.Remove(entry.Key);

                Display.Children.Remove(edge_textbox[entry.Key]);
                edge_textbox.Remove(entry.Key);
            }

            foreach (KeyValuePair<Tuple<Grid, Grid>, int> entry in weights.Where(kvp => kvp.Key.Item1.Equals(vertex_to_remove) | kvp.Key.Item2.Equals(vertex_to_remove)).ToList())
            {
                weights.Remove(entry.Key);
            }
        }

        public void remove_edge(Canvas Display, ArrowLine edge_to_remove)
        {
            Grid start_vertex = edge_end_points[edge_to_remove].Item1;
            Grid end_vertex = edge_end_points[edge_to_remove].Item2;

            adjacency_list[start_vertex].Remove(end_vertex);
            adjacency_list[end_vertex].Remove(start_vertex);

            edge_end_points.Remove(edge_to_remove);

            weights.Remove(Tuple.Create(start_vertex, end_vertex));

            Display.Children.Remove(edge_to_remove);
            Display.Children.Remove(edge_textbox[edge_to_remove]);

            edge_textbox.Remove(edge_to_remove);
        }

        public List<KeyValuePair<ArrowLine, Tuple<Grid, Grid>>> get_associated_edge(Grid vertex)
        {
            List<KeyValuePair<ArrowLine, Tuple<Grid, Grid>>> data = new List<KeyValuePair<ArrowLine, Tuple<Grid, Grid>>>();

            foreach (KeyValuePair<ArrowLine, Tuple<Grid, Grid>> entry in edge_end_points)
            {
                if (entry.Key.Name.Replace("_", "").Contains(vertex.Name.Replace("_", "")))
                {
                    data.Add(entry);
                }
            }

            return data;
        }

        public bool vertices_already_connected(Grid start_vertex, Grid end_vertex)
        {
            foreach (KeyValuePair<ArrowLine, Tuple<Grid, Grid>> entry in edge_end_points)
            {
                if (entry.Key.Name.Contains(start_vertex.Name.Replace("_", "")) & entry.Key.Name.Contains(end_vertex.Name.Replace("_", "")))
                {
                    return true;
                }
            }

            return false;
        }

        public void update_weights(TextBox textbox)
        {
            textbox.Text = textbox.Text.Replace(" ", "");

            foreach (KeyValuePair<ArrowLine, Tuple<Grid, Grid>> entry in edge_end_points)
            {
                if (entry.Key.Name.Equals(textbox.Name))
                {
                    Tuple<Grid, Grid> end_points = entry.Value;

                    if (weights.ContainsKey(end_points))
                    {
                        if (!String.IsNullOrEmpty(textbox.Text))
                        {
                            weights[end_points] = Int32.Parse(textbox.Text);
                        }
                        else
                        {
                            weights.Remove(end_points);
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(textbox.Text))
                        {
                            weights.Add(end_points, Int32.Parse(textbox.Text));
                        }
                    }
                }
            }
        }

        public void update_vertex(Canvas canvas, TextBox textbox)
        {
            textbox.Text = textbox.Text.Replace(" ", "");

            if (!string.IsNullOrEmpty(textbox.Text))
            {
                foreach (UIElement element in canvas.Children)
                {
                    if (element is Grid)
                    {
                        Grid vertex = (Grid)element;
                        if (vertex.Name.Equals(textbox.Name))
                        {
                            if (!vertex_name_being_used($"_{textbox.Text}"))
                            {
                                vertex.Name = $"_{textbox.Text}";
                            }
                            else
                            {
                                textbox.Text = textbox.Name.Replace("_", "");
                            }
                        }
                    }

                    else if (element is ArrowLine)
                    {
                        ArrowLine edge = (ArrowLine)element;

                        if (edge.Name.Contains(textbox.Name.Replace("_", "")))
                        {
                            if (vertex_name_being_used($"_{textbox.Text}"))
                            {
                                edge.Name = $"_{edge.Name.Replace(textbox.Name, textbox.Text)}";
                            }
                            else
                            {
                                textbox.Text = textbox.Name.Replace("_", "");
                            }
                        }
                    }
                }

                textbox.Name = "_" + textbox.Text;
            }
            else
            {
                textbox.Text = textbox.Name.Replace("_", "");
            }
        }

        public bool vertex_name_being_used(string vertex_name)
        {
            foreach (KeyValuePair<Grid, List<Grid>> entry in adjacency_list)
            {
                if (entry.Key.Name.Equals(vertex_name))
                {
                    return true;
                }
            }

            return false;
        }

        public Dictionary<int, Tuple<int, int>> read_uploaded_vertices(Canvas canvas, string contents)
        {
            canvas.Children.Clear();
            adjacency_list.Clear();
            weights.Clear();
            edge_end_points.Clear();
            edge_textbox.Clear();

            List<string> lines = contents.Split('\n').ToList();

            string first_line = lines[0];

            int nvertices = Int32.Parse(first_line.Split(' ')[1]);

            lines.RemoveAll(line => line.Equals(first_line));

            Dictionary<int, Tuple<int, int>> vertices_to_make = new Dictionary<int, Tuple<int, int>>();

            for (int i = 0; i < nvertices; i++)
            {
                int start_x = 45;
                int start_y = 45;

                int delta_x = 180;
                int delta_y = 140;

                List<int> vertex_list = lines[i].Split(' ').Select(Int32.Parse).ToList();

                if (!vertices_to_make.ContainsKey(vertex_list[0]))
                {
                    vertices_to_make.Add(vertex_list[0], Tuple.Create(start_x + (delta_x * (i % 4)), start_y + (delta_y * (i / 4))));
                }
            }

            return vertices_to_make;
        }

        public Grid find_vertex_by_num(Canvas canvas, int num)
        {
            foreach (Grid grid in canvas.Children)
            {
                if (grid.Name.Equals("_" + num))
                {
                    return grid;
                }
            }

            return new Grid();
        }

        public Dictionary<Tuple<Grid, Grid>, Tuple<string, string>> read_uploaded_edges(Canvas canvas, string contents)
        {
            List<string> lines = contents.Split('\n').ToList();
            Dictionary<int, List<int>> temp_adjacency_list = new Dictionary<int, List<int>>();
            Dictionary<Tuple<int, int>, int> weighted_edges = new Dictionary<Tuple<int, int>, int>();
            Dictionary<Tuple<Grid, Grid>, Tuple<string, string>> edges_to_make = new Dictionary<Tuple<Grid, Grid>, Tuple<string, string>>();

            int nvertices = Int32.Parse(lines[0].Split(' ')[1]);
            lines.RemoveAll(line => line.Equals(lines[0]));

            for (int i = 0; i < nvertices; i++)
            {
                List<int> vertex_list = lines[i].Split(' ').Select(Int32.Parse).ToList();

                temp_adjacency_list.Add(vertex_list[0], vertex_list.Skip(1).ToList());
            }

            lines.RemoveRange(0, nvertices);

            int nweights = Int32.Parse(lines[0].Split(' ')[1]);
            lines.RemoveAll(line => line.Equals(lines[0]));

            for (int i = 0; i < nweights; i++)
            {
                List<int> endpoint_weights = lines[i].Split(' ').Select(Int32.Parse).ToList();

                weighted_edges.Add(Tuple.Create(endpoint_weights[0], endpoint_weights[1]), endpoint_weights[2]);
            }

            lines.RemoveRange(0, nweights);

            foreach (KeyValuePair<int, List<int>> entry in temp_adjacency_list)
            {
                int start_vertex = entry.Key;

                if (entry.Value.Count > 0)
                {
                    foreach (int connected_vertex in entry.Value)
                    {
                        int end_vertex = connected_vertex;

                        if (temp_adjacency_list[connected_vertex].Contains(start_vertex))
                        {
                            Tuple<int, int> end_points_1 = Tuple.Create(start_vertex, end_vertex);
                            Tuple<int, int> end_points_2 = Tuple.Create(end_vertex, start_vertex);

                            if (weighted_edges.ContainsKey(end_points_1))
                            {
                                Tuple<Grid, Grid> grid_end_points_1 = Tuple.Create(find_vertex_by_num(canvas, start_vertex), find_vertex_by_num(canvas, end_vertex));
                                Tuple<Grid, Grid> grid_end_points_2 = Tuple.Create(find_vertex_by_num(canvas, end_vertex), find_vertex_by_num(canvas, start_vertex));

                                if (!edges_to_make.ContainsKey(grid_end_points_1) & !edges_to_make.ContainsKey(grid_end_points_2))
                                {
                                    edges_to_make.Add(grid_end_points_1, Tuple.Create("undirected", weighted_edges[end_points_1].ToString()));
                                }
                            }
                            else if (weighted_edges.ContainsKey(end_points_2))
                            {
                                Tuple<Grid, Grid> grid_end_points_1 = Tuple.Create(find_vertex_by_num(canvas, start_vertex), find_vertex_by_num(canvas, end_vertex));
                                Tuple<Grid, Grid> grid_end_points_2 = Tuple.Create(find_vertex_by_num(canvas, end_vertex), find_vertex_by_num(canvas, start_vertex));

                                if (!edges_to_make.ContainsKey(grid_end_points_1) & !edges_to_make.ContainsKey(grid_end_points_2))
                                {
                                    edges_to_make.Add(grid_end_points_1, Tuple.Create("undirected", weighted_edges[end_points_2].ToString()));
                                }
                            }
                            else
                            {
                                Tuple<Grid, Grid> grid_end_points_1 = Tuple.Create(find_vertex_by_num(canvas, start_vertex), find_vertex_by_num(canvas, end_vertex));
                                Tuple<Grid, Grid> grid_end_points_2 = Tuple.Create(find_vertex_by_num(canvas, end_vertex), find_vertex_by_num(canvas, start_vertex));

                                if (!edges_to_make.ContainsKey(grid_end_points_1) & !edges_to_make.ContainsKey(grid_end_points_2))
                                {
                                    edges_to_make.Add(grid_end_points_1, Tuple.Create("undirected", ""));
                                }

                            }
                        }
                        else
                        {
                            Tuple<int, int> end_points = Tuple.Create(start_vertex, end_vertex);

                            if (weighted_edges.ContainsKey(end_points))
                            {
                                Tuple<Grid, Grid> grid_end_points = Tuple.Create(find_vertex_by_num(canvas, start_vertex), find_vertex_by_num(canvas, end_vertex));

                                if (!edges_to_make.ContainsKey(grid_end_points))
                                {
                                    edges_to_make.Add(grid_end_points, Tuple.Create("directed", weighted_edges[end_points].ToString()));
                                }
                            }
                            else
                            {
                                Tuple<Grid, Grid> grid_end_points = Tuple.Create(find_vertex_by_num(canvas, start_vertex), find_vertex_by_num(canvas, end_vertex));

                                if (!edges_to_make.ContainsKey(grid_end_points))
                                {
                                    edges_to_make.Add(Tuple.Create(find_vertex_by_num(canvas, start_vertex), find_vertex_by_num(canvas, end_vertex)), Tuple.Create("directed", ""));
                                }
                            }
                        }
                    }
                }

            }

            return edges_to_make;
        }

        public int get_next_label()
        {
            if (adjacency_list.Count == 0)
            {
                return 1;
            }
            else
            {
                int next_vertex_label = adjacency_list.Keys.Max(t => Int32.Parse(t.Name.Replace("_", ""))) + 1;

                return next_vertex_label;
            }

        }
    }
}
