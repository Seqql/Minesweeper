using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;

namespace Minesweeper
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            init();
        }

        private int max_rows = 10, max_cols = 10, curr_row, curr_col, bombs = 10, buttonSize = 50;

        private Tile[,] minefield;
        string[] bombSpots;

        private const string namePrefix = "Btn_";
        private const string nameSeperator = "_";

        private void init()
        {
            //Create the minefield
            minefield = new Tile[max_rows, max_cols];
            for (int i = 0; i < max_rows; i++)
            {
                for (int j = 0; j < max_cols; j++)
                {
                    minefield[i, j] = new Tile();
                }
            }
            createMinefield();

            //Create Rows
            for (int i = 0; i < max_rows; i++)
            {
                RowDefinition newRow = new RowDefinition();

                newRow.Height = GridLength.Auto;
                MineGrid.RowDefinitions.Add(newRow);

            }

            //Create Cols
            for (int i = 0; i < max_cols; i++)
            {
                ColumnDefinition newCol = new ColumnDefinition();
                newCol.Width = GridLength.Auto;
                MineGrid.ColumnDefinitions.Add(newCol);
            }

            //Create Buttons in the Grid
            for (int i = 0; i < max_rows; i++)
            {
                for (int j = 0; j < max_cols; j++)
                {

                    Button newBtn = createButton(i, j);

                    Grid.SetRow(newBtn, i);
                    Grid.SetColumn(newBtn, j);
                    MineGrid.Children.Add(newBtn);
                }
            }

            //Set Colors
            for (int i = 0; i < max_rows; i++)
            {
                for (int j = 0; j < max_cols; j++)
                {
                    Button el = (Button)MineGrid.Children[(int)(i * max_rows + j)];
                    setColor(el, minefield[i, j]);
                }
            }
        }

        private void createMinefield()
        {
            Random r = new Random();
            bombSpots = new string[bombs];
            int k = 0;

            //Create bomb position
            do
            {
                int row = r.Next(max_rows);
                int col = r.Next(max_cols);
                bool isAmbigous = false;

                foreach (string item in bombSpots)
                {
                    if (item == row.ToString() + "," + col.ToString())
                    {
                        isAmbigous = true;
                        break;
                    }
                }

                if (!isAmbigous)
                {
                    bombSpots[k] = row.ToString() + "," + col.ToString();
                    k++;
                }
            } while (k < bombs);

            //Set values around bombs
            foreach (string bomb in bombSpots)
            {
                int row = Convert.ToInt32(bomb.Split(',')[0]);
                int col = Convert.ToInt32(bomb.Split(',')[1]);


                for (int i = row - 1; i <= row + 1; i++)
                {
                    for (int j = col - 1; j <= col + 1; j++)
                    {
                        if (i >= max_rows || j >= max_cols)
                            break;

                        if (i < 0 || j < 0)
                            continue;


                        if (i == row && j == col)
                        {
                            minefield[i, j].isBomb = true;

                        }
                        else
                            minefield[i, j].value++;
                    }
                }
            }
        }

        private Button createButton(int row, int col)
        {
            Button newBtn = new Button();
            newBtn.Content = "";
            newBtn.Height = buttonSize;
            newBtn.Width = buttonSize;
            newBtn.Name = constructName(row, col); //Btn_[Row][Col]

            newBtn.Background = Brushes.DarkSlateGray;
            newBtn.BorderBrush = Brushes.NavajoWhite;
            newBtn.FontSize = 20.0;
            newBtn.FontWeight = FontWeights.Bold;

            newBtn.Click += Button_Click; //Add Buttons to the Click Event
            newBtn.MouseRightButtonDown += Button_RightClick;

            return newBtn;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button el = (Button)e.Source;

            curr_row = Grid.GetRow(el);
            curr_col = Grid.GetColumn(el);

            if (minefield[curr_row, curr_col].isFlagged)
                return;

            if (minefield[curr_row, curr_col].isBomb)
            {
                foreach (Button item in MineGrid.Children)
                {
                    if (item.Name == constructName(curr_row, curr_col))
                    {
                        item.Content = "B";
                        Lose();
                        break;
                    }
                }
                return;
            }


            checkTile(curr_row, curr_col);
            Win();
        }

        private void Button_RightClick(object sender, RoutedEventArgs e)
        {
            Button el = (Button)sender;

            curr_row = Grid.GetRow(el);
            curr_col = Grid.GetColumn(el);


            if (minefield[curr_row, curr_col].isDiscovered)
                return;
            else if (el.Content == "")
            {
                el.Content = "F";
                minefield[curr_row, curr_col].isFlagged = true;
                setColor(el, minefield[curr_row, curr_col]);
            }
            else
            {
                el.Content = "";
                minefield[curr_row, curr_col].isFlagged = false;
                setColor(el, minefield[curr_row, curr_col]);
            }

            Win();
        }
        private void checkTile(int rowToCheck, int colToCheck)
        {   
            
            //If Value > 0
            //If Bomb is there
            if (minefield[rowToCheck, colToCheck].isBomb)
            {
                return;
            }
            Button el = (Button)MineGrid.Children[rowToCheck * max_rows + colToCheck];
            setDiscoverColor(el);

            if (minefield[rowToCheck, colToCheck].value > 0)
            {
                //Expose value to user and change discoverd to true
                minefield[rowToCheck, colToCheck].isDiscovered = true;
                foreach (Button item in MineGrid.Children)
                {
                    if (item.Name == constructName(rowToCheck, colToCheck))
                    {
                        item.Content = minefield[rowToCheck, colToCheck].value.ToString();
                        break;
                    }
                }
                return;
            }

            

            //Value has to be zero if the code runs here

            for (int i = rowToCheck - 1; i <= rowToCheck + 1; i++)
            {
                for (int j = colToCheck - 1; j <= colToCheck + 1; j++)
                {
                    if (i >= max_rows || j >= max_cols)
                        continue;

                    //The Tile you clicked on
                    if (i == rowToCheck && j == colToCheck)
                    {
                        foreach (Button item in MineGrid.Children)
                        {
                            if (item.Name == constructName(i, j))
                            {
                                item.Content = minefield[i, j].value.ToString();
                                break;
                            }
                        }
                        continue;
                    }

                    if (i < 0 || j < 0)
                        continue;

                    if (minefield[i, j].isDiscovered)
                        continue;

                    minefield[i, j].isDiscovered = true;
                    
                    foreach (Button item in MineGrid.Children)
                    {
                        if (item.Name == constructName(i, j))
                        {
                            item.Content = minefield[i, j].value.ToString();
                            break;
                        }
                    }
                    
                    checkTile(i, j);
                }
            }
        }

        private string constructName(int row, int col)
        {
            return namePrefix + row + nameSeperator + col;
        }

        private void Win()
        {
            bool win = true;
            for (int i = 0; i < max_rows; i++)
            {
                for (int j = 0; j < max_cols; j++)
                {
                    Tile el = minefield[i, j];
                    if (!(el.isBomb || el.isDiscovered))
                    {
                        win = false;
                        break;
                    };
                }

                if (!win) break;
            }

            if (win)
            {
                L1.Content = "WIN";
            }
        }

        private void Lose()
        {
            //TODO: Something happens if you lose
            L1.Content = "LOSE";
        }


        private void setColor(Button el, Tile tile)
        {
            Brush setColor = Brushes.Transparent;

            switch (tile.value)
            {
                case 1:
                    setColor = Brushes.Blue;
                    break;
                case 2:
                    setColor = Brushes.Green;
                    break;
                case 3:
                    setColor = Brushes.Violet;
                    break;
                case 4:
                    setColor = Brushes.Orange;
                    break;
                case 5:
                    setColor = Brushes.Red;
                    break;
                case 6:
                    setColor = Brushes.DarkRed;
                    break;
                default:
                    setColor = Brushes.White;
                    break;
            }

            if (tile.isFlagged)
                setColor = Brushes.Yellow;
            else if (tile.isBomb)
                setColor = Brushes.Black;

            el.Foreground = setColor;
        }
    private void setDiscoverColor(Button el)
    {
        el.Background = Brushes.LightSlateGray;
    }
    }

}
