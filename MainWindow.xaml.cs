using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using System.Windows.Controls.Primitives;
using static System.Text.Json.JsonElement;
using System.Diagnostics;

// let's be clear: [■] is filled, [ ] is empty and [.] is crossed

namespace Nonogram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // some sort of global variables
        private string levelName = "level1";
        private bool saved = false;
        private List<Rectangle> tiles = new List<Rectangle>();
        private List<Label> numbers = new List<Label>();
        Brush stroke;

        public MainWindow()
        {
            InitializeComponent();
            tiles.Add(R00); // that's a thing I missed before
            Dictionary<DependencyProperty, object> tileProperties = new Dictionary<DependencyProperty, object>(); // all the properties a tile should have
            tileProperties.Add(HeightProperty, 20D);
            tileProperties.Add(WidthProperty, 20D);
            tileProperties.Add(Shape.FillProperty, R00.Fill);
            tileProperties.Add(Shape.StrokeProperty, R00.Stroke);
            tileProperties.Add(Shape.StrokeThicknessProperty, R00.StrokeThickness);
            stroke = R00.Stroke; // for later use
            for (int row = 0; row < 15; row++) // init these tiles
            {
                for (int column = 0; column < 15; column++)
                {
                    if (row + column == 0)
                    {
                        continue;
                    }
                    Rectangle newTile = new Rectangle();
                    foreach (var entry in tileProperties)
                    {
                        newTile.SetValue(entry.Key, entry.Value);
                    }
                    newTile.SetValue(NameProperty, "R" + Convert.ToString(row, 16) + Convert.ToString(column, 16));
                    newTile.SetValue(Grid.RowProperty, row);
                    newTile.SetValue(Grid.ColumnProperty, column);
                    newTile.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Paint));
                    newTile.AddHandler(MouseRightButtonDownEvent, new MouseButtonEventHandler(Cross));
                    newTile.AddHandler(MouseEnterEvent, new MouseEventHandler(Hover));
                    MainGrid.Children.Add(newTile); // render it!
                    tiles.Add(newTile);
                }
            }
            numbers.Add(NA00); // that's another one I missed
            Dictionary<DependencyProperty, object> numberProperties = new Dictionary<DependencyProperty, object>(); // all the properties a number should have
            numberProperties.Add(HeightProperty, 20D);
            numberProperties.Add(WidthProperty, 20D);
            numberProperties.Add(ContentProperty, NA00.Content);
            numberProperties.Add(HorizontalContentAlignmentProperty, NA00.HorizontalContentAlignment);
            numberProperties.Add(VerticalContentAlignmentProperty, NA00.VerticalContentAlignment);
            numberProperties.Add(PaddingProperty, NA00.Padding);
            numberProperties.Add(FontSizeProperty, NA00.FontSize);
            for (int column = 0; column < 15; column++) // init above ones
            {
                for (int row = 6; row >= 0; row--)
                {
                    if (row == 6 && column == 0)
                    {
                        continue;
                    }
                    Label newNumber = new Label();
                    foreach (var entry in numberProperties)
                    {
                        newNumber.SetValue(entry.Key, entry.Value);
                    }
                    newNumber.SetValue(NameProperty, "NA" + Convert.ToString(6 - row, 16) + Convert.ToString(column, 16));
                    newNumber.SetValue(Grid.RowProperty, row);
                    newNumber.SetValue(Grid.ColumnProperty, column);
                    AboveNumbers.Children.Add(newNumber);
                    numbers.Add(newNumber);
                }
            }
            for (int row = 0; row < 15; row++) // init left ones
            {
                for (int column = 6; column >= 0; column--)
                {
                    // note there's no ignorance!
                    Label newNumber = new Label();
                    foreach (var entry in numberProperties)
                    {
                        newNumber.SetValue(entry.Key, entry.Value);
                    }
                    newNumber.SetValue(NameProperty, "NL" + Convert.ToString(row, 16) + Convert.ToString(6 - column, 16));
                    newNumber.SetValue(Grid.RowProperty, row);
                    newNumber.SetValue(Grid.ColumnProperty, column);
                    LeftNumbers.Children.Add(newNumber);
                    numbers.Add(newNumber);
                }
            }
        }

        // left click a tile
        private void Paint(object sender, MouseButtonEventArgs e)
        {
            foreach (var tile in tiles) // reset strokes
            {
                tile.Stroke = stroke;
            }
            Brush brush = ((Rectangle)e.Source).Fill;
            if (brush is SolidColorBrush)
            {
                Color color = ((SolidColorBrush)brush).Color;
                if (color.Equals(Color.FromRgb(14, 44, 92))) // filled
                {
                    ((Rectangle)e.Source).Fill = Brushes.White;
                }
                else // empty
                {
                    ((Rectangle)e.Source).Fill = new SolidColorBrush(Color.FromRgb(14, 44, 92));
                }
            }
            else if (brush is DrawingBrush) // crossed
            {
                ((Rectangle)e.Source).Fill = Brushes.White;
            }
            saved = false; // made changes
        }

        // right click a tile
        private void Cross(object sender, MouseButtonEventArgs e)
        {
            foreach (var tile in tiles)
            {
                tile.Stroke = stroke;
            }
            Brush brush = ((Rectangle)e.Source).Fill;
            if (brush is DrawingBrush) // crossed
            {
                ((Rectangle)e.Source).Fill = new SolidColorBrush(Colors.White);
            }
            else if (brush is SolidColorBrush) // filled or empty
            {
                DrawingBrush dbrush = new DrawingBrush();
                dbrush.Stretch = Stretch.None;
                GeometryDrawing drawing = new GeometryDrawing();
                drawing.Brush = Brushes.Gray;
                drawing.Pen = new Pen(Brushes.Gray, 0);
                Geometry circle = new EllipseGeometry(new Point(9, 9), 4, 4);
                drawing.Geometry = circle;
                dbrush.Drawing = drawing;
                ((Rectangle)e.Source).Fill = dbrush;
            }
            saved = false; // made changes
        }

        // hover a tile
        private void Hover(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) // left mouse button pressed
            {
                MouseButtonEventArgs buttonArgs = new MouseButtonEventArgs((MouseDevice)e.Device, e.Timestamp, MouseButton.Left);
                buttonArgs.RoutedEvent = MouseLeftButtonDownEvent; 
                buttonArgs.Source = e.Source;
                ((Rectangle)e.Source).RaiseEvent(buttonArgs);
            }
            else if (e.RightButton == MouseButtonState.Pressed) // left mouse button pressed
            {
                MouseButtonEventArgs buttonArgs = new MouseButtonEventArgs((MouseDevice)e.Device, e.Timestamp, MouseButton.Right);
                buttonArgs.RoutedEvent = MouseRightButtonDownEvent;
                buttonArgs.Source = e.Source;
                ((Rectangle)e.Source).RaiseEvent(buttonArgs);
            }
        }

        /// <summary>
        /// Load a level from a file.
        /// </summary>
        /// <example>
        ///      2  1  2
        ///      1  1  1  3
        ///   3 [■][■][■][ ]
        /// 1 2 [■][ ][■][■]
        ///   1 [ ][■][ ][■]
        /// 1 2 [■][ ][■][■]
        /// <code>
        /// {
        ///     "above": [
        ///         [ 1, 2 ],
        ///         [ 1, 1 ],
        ///         [ 1, 2 ],
        ///         [ 3 ]
        ///     ],
        ///     "left": [
        ///         [ 3 ],
        ///         [ 2, 1 ],
        ///         [ 1 ],
        ///         [ 2, 1 ]
        ///     ]
        /// }
        /// </code>
        /// Note that the numbers are reversed.
        /// </example>
        private void LoadLevel(object sender, RoutedEventArgs e) // load level from ./level/%levelname%.json
        {
            Reset(sender, e); // reset all
            foreach (var number in numbers)
            {
                number.Content = " ";
            }
            string levelFileName = ".\\levels\\" + FilenameInput.Text + ".json";
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(levelFileName, FileMode.Open, FileAccess.Read);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Level file not found", "Exception");
                return;
            }
            levelName = FilenameInput.Text; // save the name only if the level json is found
            StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8);
            string levelJson = streamReader.ReadToEnd(); // raw json text
            streamReader.Close();
            fileStream.Close();
            JsonElement rootElement = JsonDocument.Parse(levelJson).RootElement; // parse json
            JsonElement above; // two sections of numbers
            JsonElement left;
            try
            {
                above = rootElement.GetProperty("above");
                left = rootElement.GetProperty("left");
            }
            catch (KeyNotFoundException)
            {
                MessageBox.Show("Invalid level file: missing 'above' or 'left'", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int aboveCount = above.GetArrayLength(); // used to check if valid
            int leftCount = left.GetArrayLength();
            if (aboveCount != 15)
            {
                MessageBox.Show("Invalid level file: 'above' array length is not 15", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (leftCount != 15)
            {
                MessageBox.Show("Invalid level file: 'left' array length is not 15", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ArrayEnumerator aboveArray = above.EnumerateArray(); // the actual arrays
            ArrayEnumerator leftArray = left.EnumerateArray();
            aboveArray.MoveNext(); // move to the first element (fun fact: I have no idea about this at the first time and spent half an hour on it)
            leftArray.MoveNext();
            for (int column = 0; column < aboveCount; column++) // above ones
            {
                JsonElement columnElement = aboveArray.Current;
                ArrayEnumerator dataArray = columnElement.EnumerateArray(); // the actual array of this column
                int arrayCount = columnElement.GetArrayLength();
                if (arrayCount > 7 || arrayCount <= 0) // if not valid
                {
                    MessageBox.Show("Invalid level file: too many numbers in a column", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                    foreach (Label number in LeftNumbers.Children) // reset
                    {
                        number.Content = " ";
                    }
                    return;
                }
                dataArray.MoveNext();
                for (int i = 0; i < arrayCount; i++) // write the numbers
                {
                    Label number = GetNumber("NA" + Convert.ToString(i, 16) + Convert.ToString(column, 16));
                    number.Content = dataArray.Current.GetInt32();
                    dataArray.MoveNext();
                }
                aboveArray.MoveNext();
            }
            for (int row = 0; row < leftCount; row++) // left ones
            {
                JsonElement rowElement = leftArray.Current;
                ArrayEnumerator dataArray = rowElement.EnumerateArray(); // the acutal array of this row
                int arrayCount = rowElement.GetArrayLength();
                if (arrayCount > 7 || arrayCount <= 0) // if not valid
                {
                    MessageBox.Show("Invalid level file: too many numbers in a row", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                    foreach (Label number in LeftNumbers.Children) // reset
                    {
                        number.Content = " ";
                    }
                    return;
                }
                dataArray.MoveNext();
                for (int i = 0; i < arrayCount; i++) // write the numbers
                {
                    Label number = GetNumber("NL" + Convert.ToString(row, 16) + Convert.ToString(i, 16));
                    number.Content = dataArray.Current.GetInt32();
                    dataArray.MoveNext();
                }
                leftArray.MoveNext();
            }
        }

        /// <summary>
        /// Load your progress from a file.
        /// </summary>
        /// <see cref="Save"/>
        private void LoadProgress(object sender, RoutedEventArgs e) // load your progress in ./saves/%levelname%.json
        {
            string saveFileName = ".\\saves\\" + levelName + ".json";
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(saveFileName, FileMode.Open, FileAccess.Read);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Save file not found", "Exception");
                return;
            }
            StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8);
            string saveJson = streamReader.ReadToEnd(); // raw json text
            streamReader.Close();
            fileStream.Close();
            JsonElement rootElement = JsonDocument.Parse(saveJson).RootElement; // parse json
            ArrayEnumerator dataArray = rootElement.EnumerateArray(); // the actual array
            dataArray.MoveNext();
            for (int row = 0; row < rootElement.GetArrayLength(); row++) // iterate through all tiles with their unique names
            {
                JsonElement rowElement = dataArray.Current;
                ArrayEnumerator rowArray = rowElement.EnumerateArray();
                rowArray.MoveNext();
                for (int column = 0; column < rowElement.GetArrayLength(); column++)
                {
                    Rectangle tile = GetTile("R" + Convert.ToString(row, 16) + Convert.ToString(column, 16));
                    try
                    {
                        switch (rowArray.Current.GetInt32()) // follow the rules
                        {
                            case 0:
                                tile.Fill = Brushes.White;
                                break;
                            case 1:
                                tile.Fill = new SolidColorBrush(Color.FromRgb(14, 44, 92));
                                break;
                            case 2:
                                DrawingBrush dbrush = new DrawingBrush();
                                dbrush.Stretch = Stretch.None;
                                GeometryDrawing drawing = new GeometryDrawing();
                                drawing.Brush = Brushes.Gray;
                                drawing.Pen = new Pen(Brushes.Gray, 0);
                                Geometry circle = new EllipseGeometry(new Point(9, 9), 4, 4);
                                drawing.Geometry = circle;
                                dbrush.Drawing = drawing;
                                tile.Fill = dbrush;
                                break;
                            default:
                                MessageBox.Show("Invalid save file: invalid value in save file", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        MessageBox.Show("Invalid save file: invalid value in save file", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    rowArray.MoveNext();
                }
                dataArray.MoveNext();
            }
        }

        private void Reset(object sender, RoutedEventArgs e) // make all tiles white
        {
            foreach (var tile in tiles)
            {
                tile.Fill = Brushes.White;
                tile.Stroke = stroke;
            }
        }

        private Rectangle GetTile(string name) // get a tile from names like "R00" or "R3A"
        {
            foreach (var element in MainGrid.Children)
            {
                if (element is Rectangle)
                {
                    if (((Rectangle)element).Name == name)
                    {
                        return (Rectangle)element;
                    }
                }
            }
            return new Rectangle();
        }
        
        private Label GetNumber(string name) // get a number from names like "NAB1" or "NL38"
        {
            foreach (Label label in AboveNumbers.Children)
            {
                if (label.Name == name)
                {
                    return label;
                }
            }
            foreach (Label label in LeftNumbers.Children)
            {
                if (label.Name == name)
                {
                    return label;
                }
            }
            return new Label();
        }

        /// <summary>
        /// Save your progress to a file.
        /// </summary>
        /// <example>
        ///      2  1  2
        ///      1  1  1  3
        ///   3 [■][■][■][ ]
        /// 1 2 [■][ ][■][■]
        ///   1 [ ][■][.][■]
        /// 1 2 [■][.][■][■]
        /// <code>
        /// [
        ///     [ 1, 1, 1, 0 ],
        ///     [ 1, 0, 1, 1 ],
        ///     [ 0, 1, 2, 1 ],
        ///     [ 1, 2, 1, 1 ]
        /// ]
        /// </code>
        /// 0 is empty, 1 is filled and 2 is crossed.
        /// </example>
        private void Save(object sender, RoutedEventArgs e) // save your progress in ./saves/%levelname%.json
        {
            // please inform that your progress is saved in level1.json if the level name is not defined!
            string filename = ".\\saves\\" + levelName + ".json";
            FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            Utf8JsonWriter jsonWriter = new Utf8JsonWriter(fileStream); // json writer
            jsonWriter.WriteStartArray();
            for (int row = 0; row < 15; row++) // iterate through all tiles with their unique names
            {
                jsonWriter.WriteStartArray();
                for (int column = 0; column < 15; column++)
                {
                    Rectangle tile = GetTile("R" + Convert.ToString(row, 16) + Convert.ToString(column, 16));
                    if (tile.Fill is SolidColorBrush) // follow the rules
                    {
                        if (tile.Fill.Equals(Brushes.White))
                        {
                            jsonWriter.WriteNumberValue(0);
                        }
                        else
                        {
                            jsonWriter.WriteNumberValue(1);
                        }
                    }
                    else
                    {
                        jsonWriter.WriteNumberValue(2);
                    }
                }
                jsonWriter.WriteEndArray();
            }
            jsonWriter.WriteEndArray();
            jsonWriter.Flush();
            fileStream.Close();
            saved = true; // successfully saved
        }

        private void Examine(object sender, RoutedEventArgs e) // examine whether your solution is right and mark your mistakes (this took me a lot of energy)
        {
            bool correct = true; // solution correct or not
            for (int row = 0; row <= 14; row++) // each row
            {
                List<int> goal = new List<int>(); // numbers on the left (reversed)
                for (int column = 0; column <= 6; column++)
                {
                    Label number = GetNumber("NL" + Convert.ToString(row, 16) + Convert.ToString(column, 16));
                    try
                    {
                        goal.Add(Convert.ToInt32(number.Content));
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
                List<Rectangle> tiles = new List<Rectangle>(); // row tiles
                List<int> solution = new List<int>(); // numbers which represents your solution (also reversed)
                int currentValue = 0; // number
                for (int column = 14; column >= 0; column--) // scan through the row
                {
                    Rectangle tile = GetTile("R" + Convert.ToString(row, 16) + Convert.ToString(column, 16));
                    tiles.Add(tile);
                    if (tile.Fill is SolidColorBrush && ((SolidColorBrush)tile.Fill).Color.Equals(Color.FromRgb(14, 44, 92)))
                    {
                        currentValue++;
                    }
                    else if (currentValue != 0)
                    {
                        solution.Add(currentValue);
                        currentValue = 0;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (currentValue != 0) // add end value
                {
                    solution.Add(currentValue);
                }
                if (!haveSameContents(goal, solution)) // mark if wrong
                {
                    foreach (Rectangle tile in tiles)
                    {
                        tile.Stroke = Brushes.Red;
                    }
                }
                correct = correct && haveSameContents(goal, solution); // merge to get the final result
            }
            for (int column = 0; column <= 14; column++) // each column
            {
                List<int> goal = new List<int>(); // numbers on the top (reversed)
                for (int row = 0; row <= 6; row++)
                {
                    Label number = GetNumber("NA" + Convert.ToString(row, 16) + Convert.ToString(column, 16));
                    try
                    {
                        goal.Add(Convert.ToInt32(number.Content));
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
                List<Rectangle> tiles = new List<Rectangle>(); // column tiles
                List<int> solution = new List<int>(); // numbers which represents your solution (also reversed)
                int currentValue = 0;
                for (int row = 14; row >= 0; row--) // scan through the column
                {
                    Rectangle tile = GetTile("R" + Convert.ToString(row, 16) + Convert.ToString(column, 16));
                    tiles.Add(tile);
                    if (tile.Fill is SolidColorBrush && ((SolidColorBrush)tile.Fill).Color.Equals(Color.FromRgb(14, 44, 92)))
                    {
                        currentValue++;
                    }
                    else if (currentValue != 0)
                    {
                        solution.Add(currentValue);
                        currentValue = 0;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (currentValue != 0) // add end value
                {
                    solution.Add(currentValue);
                }
                if (!haveSameContents(goal, solution)) // mark if wrong
                {
                    foreach (Rectangle tile in tiles)
                    {
                        tile.Stroke = Brushes.Red;
                    }
                }
                correct = correct && haveSameContents(goal, solution); // merge to get the final result
            }
            if (correct) // check if correct
            {
                MessageBox.Show("Congratulations! You have successfully solved this level!", "Level solved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

            private bool haveSameContents<T>(List<T> a, List<T> b) // compare two lists
        {
            if (a.Count != b.Count)
            {
                return false;
            }
            for (int i = 0; i < a.Count; i++)
            {
#pragma warning disable CS8602 // NPE
                if (!a[i].Equals(b[i])) // this warning is stupid...
                {
                    return false;
                }
#pragma warning restore CS8602 // NPE
            }
            return true;
        }
        
        private void ClosingWindow(object sender, System.ComponentModel.CancelEventArgs e) // check the progress is unsaved before closing
        {
            if (!saved)
            {
                string msg = "Save your progress before closing?";
                MessageBoxResult result =
                  MessageBox.Show(
                    msg,
                    "Save Progress",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    RoutedEventArgs args = new RoutedEventArgs(ButtonBase.ClickEvent);
                    args.Source = SaveProgressButton;
                    SaveProgressButton.RaiseEvent(args);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void Link(object sender, RoutedEventArgs e)
        {
            switch (((Button)e.Source).Name) {
                case "IntroductionLinkButton":
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://zh.wikipedia.org/wiki/%E6%95%B8%E7%B9%94",
                        UseShellExecute = true
                    });
                    break;
                case "IntroductionLinkBaiduButton":
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://www.csdn.net/tags/MtTaEg0sNDA0NTcwLWJsb2cO0O0O.html",
                        UseShellExecute = true
                    });
                    break;
            }
        }
    }
}
