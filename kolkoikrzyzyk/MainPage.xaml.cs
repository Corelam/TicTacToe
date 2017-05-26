using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Diagnostics;

namespace kolkoikrzyzyk
{
    public sealed partial class MainPage : Page
    {
        private int turn;
        private int player1Score;
        private int player2Score;
        private bool player1;    // gracz X
        private bool player2;    // gracz O
        private List<Button> buttonList;

        // Prosta sztuczna inteligencja:
        private Random randomAI = new Random();
        private bool enabledAI = false;

        public MainPage()
        {
            this.InitializeComponent();
            NewGame();
        }

        /// <summary> Funkcja Nowa Gra. </summary>
        private void NewGame()
        {
            enabledAI = false;
            ButtonEnabler(buttonSI);
            player1Name.Text = "Gracz X";
            player2Name.Text = "Gracz O";
            player1Score = 0;
            player2Score = 0;
            Reset();
            WhoStarts();
        }

        private async void WhoStarts()
        {
            ContentDialog whoStarts = new ContentDialog
            {
                Title = "Kto zaczyna?",
                Content = "Wybierz gracza:",
                PrimaryButtonText = player1Name.Text,
                SecondaryButtonText = player2Name.Text
            };

            ContentDialogResult result = await whoStarts.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                player1 = true;    // gracz X
                player2 = false;   // gracz O
            }
            else
            {
                player1 = false;
                player2 = true;
            }

            Display();
        }

        #region Obsluga przyciskow
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            SetButtonContent(button1);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            SetButtonContent(button2);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            SetButtonContent(button3);
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            SetButtonContent(button4);
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            SetButtonContent(button5);
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            SetButtonContent(button6);
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            SetButtonContent(button7);
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            SetButtonContent(button8);
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            SetButtonContent(button9);
        }
        
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
            MySplitView.SetValue(Canvas.ZIndexProperty, 1);

        }

        private void MySplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            MySplitView.SetValue(Canvas.ZIndexProperty, 0);
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        private void buttonSI_Click(object sender, RoutedEventArgs e)
        {
            enabledAI = true;
            ButtonDisabler(buttonSI);
            player2Name.Text = "Komputer";
            SimpleAI();
        }
        
        private async void buttonPlayer1Name_Click(object sender, RoutedEventArgs e)
        {
            string nickname = await InputTextDialog("Wybierz imię dla gracza X:");
            if (nickname.Length > 12 || nickname.Length == 0)
            {
                InfoDialog("Nieprawidłowa liczba znaków.");
            }
            else
            {
                player1Name.Text = nickname;
            }
        }

        private async void buttonPlayer2Name_Click(object sender, RoutedEventArgs e)
        {
            string nickname = await InputTextDialog("Wybierz imię dla gracza O:");
            if (nickname.Length > 12 || nickname.Length == 0)
            {
                InfoDialog("Nieprawidłowa liczba znaków.");
            }
            else
            {
                player2Name.Text = nickname;
            }
        }
        
        private void buttonPlayer1Score_Click(object sender, RoutedEventArgs e)
        {
            ReadScoreFromFile(1);
        }

        private void buttonPlayer2Score_Click(object sender, RoutedEventArgs e)
        {
            ReadScoreFromFile(2);
        }
        #endregion

        /// <summary> Pokazuje okno dialogowe do wpisywania. </summary>
        /// <param name="title"> Zapytanie do okna dialogowego. </param>
        /// <returns> Zwraca wpisany tekst. </returns>
        private async Task<string> InputTextDialog(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.Height = 32;

            ContentDialog dialog = new ContentDialog
            {
                PrimaryButtonText = "OK",
                Title = title,
                Content = inputTextBox
            };

            await dialog.ShowAsync();
            return inputTextBox.Text;
        }

        /// <summary> Prosta (losujaca) sztuczna inteligencja. </summary>
        private void SimpleAI()
        {
            if (player2)
            {
                if (buttonList.Count.Equals(0))
                {
                    CheckWin();
                }

                int drawButton = randomAI.Next(buttonList.Count);
                //await Task.Delay(TimeSpan.FromSeconds(0.35));   // opoznienie "wciskania" przycisku przez sztuczna inteligencje       /// jak gracz wcisnie przycisk w trakcie wykonywania ruchu SI, wywala blad, nie odblokowywac
                SetButtonContent(buttonList[drawButton]);
            }
        }

        /// <summary> Funkcja wyswietlajaca X lub O na przycisku. </summary>
        private void SetButtonContent(Button button)
        {
            if (!button.Content.ToString().Equals(""))
            {
                return;
            }

            if (player1 && !player2)        // zeby zawsze zaczynal X -> if (turn % 2 == 0)
            {
                button.Content = "X";
                button.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            }
            else
            {
                button.Content = "O";
                button.Background = new SolidColorBrush(Windows.UI.Colors.DarkCyan);
            }

            buttonList.Remove(button);
            NextTurn();
        }

        /// <summary> Funkcja aktywujaca przycisk. </summary>
        private void ButtonEnabler(Button button)
        {
            button.IsEnabled = true;
        }

        /// <summary> Funkcja deaktywujaca przycisk. </summary>
        private void ButtonDisabler(Button button)
        {
            button.IsEnabled = false;
        }

        /// <summary> Funkcja przechodzenia do nastepnej tury. </summary>
        private void NextTurn()
        {
            CheckWin();
            // System.Diagnostics.Debug.WriteLine(buttonList.Count);
            
            // Zmiana aktywnego gracza na przeciwnego
            player1 = !player1;
            player2 = !player2;

            if (enabledAI)
            {
                SimpleAI();
            }

            Display();
            turn++;
        }

        /// <summary> Funkcja do wyswietlania tury gracza. </summary>
        private void Display()
        {
            if (player1 && !player2)
            {
                displayturn.Text = "Tura gracza:\n <-";
            }
            else if (!player1 && player2)
            {
                displayturn.Text = "Tura gracza:\n ->";
            }
        }

        /// <summary> Funkcja aktualizujaca wyniki. </summary>
        private void UpdateScore()
        {
            score1.Text = player1Score.ToString();
            score2.Text = player2Score.ToString();
        }
        
        /// <summary> Funkcja sprawdzajaca, czy ktos wygral. </summary>
        private void CheckWin()
        {
            if (
                // Poziomo gracz X
                   (button1.Content.Equals("X") && button2.Content.Equals("X") && button3.Content.Equals("X"))
                || (button4.Content.Equals("X") && button5.Content.Equals("X") && button6.Content.Equals("X"))
                || (button7.Content.Equals("X") && button8.Content.Equals("X") && button9.Content.Equals("X"))

                // Pionowo gracz X
                || (button1.Content.Equals("X") && button4.Content.Equals("X") && button7.Content.Equals("X"))
                || (button2.Content.Equals("X") && button5.Content.Equals("X") && button8.Content.Equals("X"))
                || (button3.Content.Equals("X") && button6.Content.Equals("X") && button9.Content.Equals("X"))

                // Skosnie gracz X
                || (button1.Content.Equals("X") && button5.Content.Equals("X") && button9.Content.Equals("X"))
                || (button3.Content.Equals("X") && button5.Content.Equals("X") && button7.Content.Equals("X"))
                )
            {
                Win(1);
            }

            else if (
                // Poziomo gracz O
                   (button1.Content.Equals("O") && button2.Content.Equals("O") && button3.Content.Equals("O"))
                || (button4.Content.Equals("O") && button5.Content.Equals("O") && button6.Content.Equals("O"))
                || (button7.Content.Equals("O") && button8.Content.Equals("O") && button9.Content.Equals("O"))

                // Pionowo gracz O
                || (button1.Content.Equals("O") && button4.Content.Equals("O") && button7.Content.Equals("O"))
                || (button2.Content.Equals("O") && button5.Content.Equals("O") && button8.Content.Equals("O"))
                || (button3.Content.Equals("O") && button6.Content.Equals("O") && button9.Content.Equals("O"))

                // Skosnie gracz O
                || (button1.Content.Equals("O") && button5.Content.Equals("O") && button9.Content.Equals("O"))
                || (button3.Content.Equals("O") && button5.Content.Equals("O") && button7.Content.Equals("O"))
                )
            {
                Win(2);
            }

            else if (buttonList.Count.Equals(0))
            {
                Win(0);
            }
        }

        /// <summary> Funkcja, ktora wyswietla komunikat informacyjny. </summary>
        private async void InfoDialog(string text)
        {
            ContentDialog dialog = new ContentDialog
            {
                MaxWidth = this.ActualWidth,
                PrimaryButtonText = "OK",
                Content = new TextBlock
                {
                    Text = text,
                    FontSize = 24
                }
            };
            await dialog.ShowAsync();
        }

        /// <summary> Funkcja, ktora wyswietla zwyciezce i dodaje mu punkt. </summary>
        private void Win(int playerNumber)
        {
            if (playerNumber == 1)
            {
                InfoDialog("Zwyciężył " + player1Name.Text);
                player1Score += 1;
                SaveScoreToFile(1);
            }
            else if (playerNumber == 2)
            {
                InfoDialog("Zwyciężył " + player2Name.Text);
                player2Score += 1;
                SaveScoreToFile(2);
            }
            else
            {
                InfoDialog("Remis");
            }
            
            Reset();
        }

        /// <summary> Funkcja resetujaca plansze (oprocz punktacji). </summary>
        private void Reset()
        {
            buttonList = new List<Button> { button1, button2, button3, button4, button5, button6, button7, button8, button9 };

            foreach (Button button in buttonList)
            {
                ButtonEnabler(button);
                button.Content = String.Empty;
                button.Background = new SolidColorBrush(Windows.UI.Colors.Gray);
            }

            turn = 0;
            UpdateScore();
            Display();
        }
        
        private async void SaveScoreToFile(int player)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile scoreFile;

            if (player.Equals(1))
            {
                scoreFile = await storageFolder.CreateFileAsync(player1Name.Text + ".txt", CreationCollisionOption.OpenIfExists);
                scoreFile = await storageFolder.GetFileAsync(player1Name.Text + ".txt");
            }
            else if (player.Equals(2))
            {
                scoreFile = await storageFolder.CreateFileAsync(player2Name.Text + ".txt", CreationCollisionOption.OpenIfExists);
                scoreFile = await storageFolder.GetFileAsync(player2Name.Text + ".txt");
            }
            else
            {
                throw new Exception("Błędny numer gracza (SaveScoreToFile).");
            }
            
            string oldScore = await FileIO.ReadTextAsync(scoreFile);

            if (!oldScore.Equals(""))
            {
                int newScore = Int32.Parse(oldScore) + 1;
                await FileIO.WriteTextAsync(scoreFile, newScore.ToString());
            }
            else
            {
                await FileIO.WriteTextAsync(scoreFile, "1");
            }
        }

        private async void ReadScoreFromFile(int player)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile scoreFile;

            if (player.Equals(1))
            {
                scoreFile = await storageFolder.CreateFileAsync(player1Name.Text + ".txt", CreationCollisionOption.OpenIfExists);
                scoreFile = await storageFolder.GetFileAsync(player1Name.Text + ".txt");
            }
            else if (player.Equals(2))
            {
                scoreFile = await storageFolder.CreateFileAsync(player2Name.Text + ".txt", CreationCollisionOption.OpenIfExists);
                scoreFile = await storageFolder.GetFileAsync(player2Name.Text + ".txt");
            }
            else
            {
                throw new Exception("Błędny numer gracza (ReadScoreFromFile).");
            }
            
            string savedScore = await FileIO.ReadTextAsync(scoreFile);

            Debug.WriteLine(savedScore);
        }
    }
}

//DO ZROBIENIA:
// - Po wpisaniu imienia żeby automatycznie aktualizowało wynik na ten z pliku (jeśli istnieje! albo żeby tworzyło plik (if file != null, else ...))
// - Jeśli będzie samo aktualizowało wynik, usunąć 2 przyciski do wyświetlania.