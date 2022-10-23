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
using Graph_Theory_Visualizer.ViewModels;
using System.Text.RegularExpressions;
using System.Windows.Media.Media3D;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;

namespace Graph_Theory_Visualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        Point m_start;
        Point offset;
        Vector m_startOffset;
        UIElement dragObject = null;
        string mode;
        Brush random_color;
        Random r;
        Graph graph;
        int label;
        Grid start_vertex;
        Grid end_vertex;

        public MainWindow()
        {
            InitializeComponent();
            mode = "Default Mode";
            ModeDisplay.Text = mode;
            r = new Random();
            graph = new Graph();
            label = graph.get_next_label();
            start_vertex = null;
        }

        private void default_mode(object sender, RoutedEventArgs e)
        {
            start_vertex = null;
            mode = "Default Mode";
            ModeDisplay.Text = mode;
        }

        private void add_vertex_mode(object sender, RoutedEventArgs e)
        {
            start_vertex = null;
            mode = "Add Vertex Mode";
            ModeDisplay.Text = mode;
        }

        private void add_edge_mode(object sender, RoutedEventArgs e)
        {
            start_vertex = null;
            mode = "Add Edge Mode";
            ModeDisplay.Text = mode;
        }

        private void remove_object_mode(object sender, RoutedEventArgs e)
        {
            start_vertex = null;
            mode = "Remove Object Mode";
            ModeDisplay.Text = mode;
        }

        private void RunAlgorithm_click(object sender, RoutedEventArgs e)
        {

        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var pos1 = e.GetPosition(Display);

            var scale = e.Delta > 0 ? 1.1 : 1 / 1.1;

            var mat = mat_trans.Matrix;
            mat.ScaleAt(scale, scale, pos1.X, pos1.Y);
            mat_trans.Matrix = mat;
            e.Handled = true;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DisplayContainer.Focus();

            if (mode.Equals("Add Vertex Mode"))
            {
                random_color = new SolidColorBrush(Color.FromRgb((byte)r.Next(122, 200), (byte)r.Next(122, 200), (byte)r.Next(122, 200)));

                TextBox new_vertex_label = new TextBox
                {
                    Text = label.ToString(),
                    Height = 20,
                    Background = new SolidColorBrush(Colors.Transparent),
                    BorderThickness = new Thickness(1, 1, 1, 1),
                    BorderBrush = Brushes.Gray,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontSize = 15,
                    IsReadOnly = false,
                    TextWrapping = TextWrapping.Wrap,
                };

                new_vertex_label.PreviewTextInput += NumberValidationTextBox;

                new_vertex_label.LostFocus += Vertex_Textbox_LostFocus;

                Grid vertex = new Grid
                {
                    Name = "_" + new_vertex_label.Text,
                    Width = 50,
                    Height = 50,
                    Background = random_color,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                new_vertex_label.Name = vertex.Name;

                vertex.Children.Add(new_vertex_label);

                Canvas.SetLeft(vertex, Mouse.GetPosition(Display).X - 25);
                Canvas.SetTop(vertex, Mouse.GetPosition(Display).Y - 25);
                Canvas.SetZIndex(vertex, 1);

                graph.add_vertex(Display, vertex);
                label = graph.get_next_label();

                Debug.WriteLine(graph);
            }
            else if (mode.Equals("Add Edge Mode"))
            {
                if (e.OriginalSource is Grid)
                {
                    Grid clicked_grid = (Grid)e.OriginalSource;

                    if (!clicked_grid.Name.Equals("DisplayContainer"))
                    {
                        if (Object.Equals(start_vertex, default(Grid)))
                        {
                            start_vertex = (Grid)e.OriginalSource;
                        }
                        else
                        {
                            end_vertex = (Grid)e.OriginalSource;

                            if (!start_vertex.Equals(end_vertex) & !graph.vertices_already_connected(start_vertex, end_vertex))
                            {
                                var viewModel = (MainWindowViewModel)DataContext;
                                bool direction;

                                if (viewModel.DirectedUndirectedCommand.CanExecute(null))
                                {
                                    viewModel.DirectedUndirectedCommand.Execute(null);
                                }

                                ArrowLine new_edge = new ArrowLine
                                {
                                    ArrowEnds = ArrowEnds.End,
                                    X1 = Canvas.GetLeft(start_vertex) + 25,
                                    X2 = Canvas.GetLeft(end_vertex) + 25,
                                    Y1 = Canvas.GetTop(start_vertex) + 25,
                                    Y2 = Canvas.GetTop(end_vertex) + 25,
                                    Stroke = Brushes.Black,
                                    StrokeThickness = 5,
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Name = $"_{start_vertex.Name.Replace("_", "")}to{end_vertex.Name.Replace("_", "")}",
                                    Fill = Brushes.Black
                                };

                                if (viewModel.to_return.ToString().Equals("directed"))
                                {
                                    new_edge.ArrowLength = 50;
                                    direction = true;
                                }
                                else
                                {
                                    new_edge.ArrowLength = 0;
                                    direction = false;
                                }

                                Canvas.SetZIndex(new_edge, 0);

                                TextBox weight = new TextBox()
                                {
                                    Foreground = Brushes.White,
                                    Background = Brushes.Black,
                                    BorderThickness = new Thickness(1, 1, 1, 1),
                                    BorderBrush = Brushes.Gray,
                                    Name = new_edge.Name
                                };

                                weight.PreviewTextInput += NumberValidationTextBox;
                                weight.LostFocus += Weight_Textbox_LostFocus;

                                Canvas.SetLeft(weight, ((new_edge.X1 + new_edge.X2) / 2) - weight.ActualWidth);
                                Canvas.SetTop(weight, ((new_edge.Y1 + new_edge.Y2) / 2) - weight.ActualHeight);
                                Canvas.SetZIndex(weight, 3);

                                graph.add_edge(Display, start_vertex, end_vertex, new_edge, direction, weight);

                                start_vertex = null;

                                Debug.WriteLine(graph);
                            }
                        }
                    }
                }
            }
            else if (mode.Equals("Remove Object Mode"))
            {
                if (e.OriginalSource is Grid)
                {
                    Grid clicked_grid = (Grid)e.OriginalSource;

                    if (!clicked_grid.Name.Equals("DisplayContainer"))
                    {
                        Grid vertex_to_remove = (Grid)e.OriginalSource;

                        graph.remove_vertex_and_connected_edges(Display, vertex_to_remove);

                        Debug.WriteLine(graph);
                    }
                }
                else if (e.OriginalSource is ArrowLine)
                {
                    ArrowLine edge_to_remove = (ArrowLine)e.OriginalSource;

                    graph.remove_edge(Display, edge_to_remove);

                    Debug.Write(graph);
                }
            }
            else if (mode.Equals("Default Mode"))
            {
                if (e.OriginalSource is Grid)
                {
                    Grid clicked_grid = (Grid)e.OriginalSource;

                    if (!clicked_grid.Name.Equals("DisplayContainer"))
                    {
                        dragObject = clicked_grid;
                        offset = e.GetPosition(DisplayContainer);
                        offset.Y -= Canvas.GetTop(dragObject);
                        offset.X -= Canvas.GetLeft(dragObject);
                    }
                }

                m_start = e.GetPosition(DisplayContainer);
                m_startOffset = new Vector(transl_trans.X, transl_trans.Y);
                Display.CaptureMouse();
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (Display.IsMouseCaptured)
            {
                if (dragObject != null)
                {
                    var position = e.GetPosition(sender as IInputElement);
                    Canvas.SetLeft(dragObject, position.X - offset.X);
                    Canvas.SetTop(dragObject, position.Y - offset.Y);

                    List<KeyValuePair<ArrowLine, Tuple<Grid, Grid>>> edge_connected_points = graph.get_associated_edge((Grid)dragObject);

                    foreach (KeyValuePair<ArrowLine, Tuple<Grid, Grid>> entry in edge_connected_points)
                    {
                        if (entry.Value.Item1.Equals((Grid)dragObject))
                        {
                            entry.Key.X1 = position.X - offset.X + 25;
                            entry.Key.Y1 = position.Y - offset.Y + 25;
                        }
                        else if (entry.Value.Item2.Equals((Grid)dragObject))
                        {
                            entry.Key.X2 = position.X - offset.X + 25;
                            entry.Key.Y2 = position.Y - offset.Y + 25;
                        }

                        var textbox = graph.edge_textbox[entry.Key];

                        Canvas.SetLeft(textbox, (entry.Key.X1 + entry.Key.X2) / 2);
                        Canvas.SetTop(textbox, (entry.Key.Y1 + entry.Key.Y2) / 2);
                    }
                }
                else
                {
                    Vector m_offset = Point.Subtract(e.GetPosition(DisplayContainer), m_start);

                    transl_trans.X = m_startOffset.X + m_offset.X;
                    transl_trans.Y = m_startOffset.Y + m_offset.Y;
                }
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dragObject = null;
            Display.ReleaseMouseCapture();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Weight_Textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;

            graph.update_weights(textbox);

            Debug.WriteLine(graph);
        }

        private void Vertex_Textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;

            graph.update_vertex(Display, textbox);
            label = graph.get_next_label();

            Debug.WriteLine(graph);
        }

        private void download_graph(object sender, RoutedEventArgs e)
        {
            string vertices = $"NVERT {graph.adjacency_list.Count}\n";
            foreach (KeyValuePair<Grid, List<Grid>> entry in graph.adjacency_list)
            {
                vertices += $"{entry.Key.Name.Replace("_", "")}";

                foreach (Grid vertex in entry.Value)
                {
                    vertices += $" {vertex.Name.Replace("_", "")}";
                }

                vertices += "\n";
            }
            vertices += $"WEIGHTS {graph.weights.Count}\n";

            foreach (KeyValuePair<Tuple<Grid, Grid>, int> entry in graph.weights)
            {
                vertices += $"{entry.Key.Item1.Name.Replace("_", "")} {entry.Key.Item2.Name.Replace("_", "")} {entry.Value}\n";
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, vertices);
        }

        private void upload_graph(object sender, RoutedEventArgs e)
        {
            string filename = "";
            Dictionary<int, Tuple<int, int>> vertices_to_make = new Dictionary<int, Tuple<int, int>>();
            Dictionary<Tuple<Grid, Grid>, Tuple<string, string>> edges_to_make = new Dictionary<Tuple<Grid, Grid>, Tuple<string, string>>();

            OpenFileDialog openfiledialog = new OpenFileDialog();
            openfiledialog.DefaultExt = ".txt";
            openfiledialog.Filter = "Text file (*.txt)|*.txt";

            if (openfiledialog.ShowDialog() == true)
            {
                filename = openfiledialog.FileName;

                vertices_to_make = graph.read_uploaded_vertices(Display, File.ReadAllText(filename));

                foreach (KeyValuePair<int, Tuple<int, int>> entry in vertices_to_make)
                {
                    random_color = new SolidColorBrush(Color.FromRgb((byte)r.Next(122, 200), (byte)r.Next(122, 200), (byte)r.Next(122, 200)));

                    TextBox new_vertex_label = new TextBox
                    {
                        Text = entry.Key.ToString(),
                        Height = 20,
                        Background = new SolidColorBrush(Colors.Transparent),
                        BorderThickness = new Thickness(1, 1, 1, 1),
                        BorderBrush = Brushes.Gray,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        FontSize = 15,
                        IsReadOnly = false,
                        TextWrapping = TextWrapping.Wrap,
                    };

                    new_vertex_label.PreviewTextInput += NumberValidationTextBox;

                    new_vertex_label.LostFocus += Vertex_Textbox_LostFocus;

                    Grid vertex = new Grid
                    {
                        Name = "_" + new_vertex_label.Text,
                        Width = 50,
                        Height = 50,
                        Background = random_color,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    };

                    new_vertex_label.Name = vertex.Name;

                    vertex.Children.Add(new_vertex_label);

                    Canvas.SetLeft(vertex, entry.Value.Item1 - 25);
                    Canvas.SetTop(vertex, entry.Value.Item2 - 25);
                    Canvas.SetZIndex(vertex, 1);

                    graph.add_vertex(Display, vertex);
                    label = graph.get_next_label();
                }

                edges_to_make = graph.read_uploaded_edges(Display, File.ReadAllText(filename));

                foreach (KeyValuePair<Tuple<Grid, Grid>, Tuple<string, string>> entry in edges_to_make)
                {
                    ArrowLine new_edge = new ArrowLine
                    {
                        ArrowEnds = ArrowEnds.End,
                        X1 = Canvas.GetLeft(entry.Key.Item1) + 25,
                        X2 = Canvas.GetLeft(entry.Key.Item2) + 25,
                        Y1 = Canvas.GetTop(entry.Key.Item1) + 25,
                        Y2 = Canvas.GetTop(entry.Key.Item2) + 25,
                        Stroke = Brushes.Black,
                        StrokeThickness = 5,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Name = $"_{entry.Key.Item1.Name.Replace("_", "")}to{entry.Key.Item2.Name.Replace("_", "")}",
                        Fill = Brushes.Black
                    };

                    if (entry.Value.Item1.Equals("directed"))
                    {
                        new_edge.ArrowLength = 50;
                    }
                    else
                    {
                        new_edge.ArrowLength = 0;
                    }

                    Canvas.SetZIndex(new_edge, 0);

                    TextBox weight = new TextBox()
                    {
                        Foreground = Brushes.White,
                        Background = Brushes.Black,
                        BorderThickness = new Thickness(1, 1, 1, 1),
                        BorderBrush = Brushes.Gray,
                        Name = new_edge.Name,
                        Text = entry.Value.Item2
                    };

                    weight.PreviewTextInput += NumberValidationTextBox;
                    weight.LostFocus += Weight_Textbox_LostFocus;

                    Canvas.SetLeft(weight, ((new_edge.X1 + new_edge.X2) / 2) - weight.ActualWidth);
                    Canvas.SetTop(weight, ((new_edge.Y1 + new_edge.Y2) / 2) - weight.ActualHeight);
                    Canvas.SetZIndex(weight, 3);

                    graph.add_edge(Display, entry.Key.Item1, entry.Key.Item2, new_edge, entry.Value.Item1.Equals("directed"), weight);
                }

                Debug.WriteLine(graph);
            }
        }

    }
}