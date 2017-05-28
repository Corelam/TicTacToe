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

        #region Zarzadzanie przyciskami
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
            UpdateScore();
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
                ReadScoreFromFile(1);
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
                ReadScoreFromFile(2);
            }
        }

        private async void buttonResetScoreFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog()
            {
                Title = "Reset wyników",
                MaxWidth = this.ActualWidth
            };

            #region Zawartosc okna
            var panel = new StackPanel();

            panel.Children.Add(new TextBlock
            {
                Text = "Wybierz, którego gracza wyniki chcesz zresetować:",
                TextWrapping = TextWrapping.Wrap,
            });

            CheckBox checkBox1 = new CheckBox();
            checkBox1.Content = player1Name.Text;
            panel.Children.Add(checkBox1);

            CheckBox checkBox2 = new CheckBox();
            checkBox2.Content = player2Name.Text;
            panel.Children.Add(checkBox2);

            dialog.Content = panel;
            #endregion

            dialog.PrimaryButtonText = "Ok";

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile scoreFile;

                if (checkBox1.IsChecked.Equals(true))
                {
                    Debug.WriteLine("Zaznaczono " + player1Name.Text);
                    scoreFile = await storageFolder.CreateFileAsync(player1Name.Text + ".txt", CreationCollisionOption.OpenIfExists);
                    scoreFile = await storageFolder.GetFileAsync(player1Name.Text + ".txt");
                    await FileIO.WriteTextAsync(scoreFile, "0");
                }
                if (checkBox2.IsChecked.Equals(true))
                {
                    Debug.WriteLine("Zaznaczono " + player2Name.Text);
                    scoreFile = await storageFolder.CreateFileAsync(player2Name.Text + ".txt", CreationCollisionOption.OpenIfExists);
                    scoreFile = await storageFolder.GetFileAsync(player2Name.Text + ".txt");
                    await FileIO.WriteTextAsync(scoreFile, "0");
                }

                UpdateScore();
            }
        }

        #endregion

        #region Zarzadzanie interfejsem gry
        /// <summary> Pokazuje okno dialogowe do wpisywania. </summary>
        /// <param name="title"> Zapytanie do okna dialogowego. </param>
        /// <returns> Zwraca wpisany tekst. </returns>
        private async Task<string> InputTextDialog(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.Height = 32;

            ContentDialog dialog = new ContentDialog
            {
                MaxWidth = this.ActualWidth,
                PrimaryButtonText = "OK",
                Title = title,
                Content = inputTextBox
            };

            await dialog.ShowAsync();
            return inputTextBox.Text;
        }

        /// <summary> Metoda, ktora wyswietla komunikat informacyjny. </summary>
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

        /// <summary> Metoda wyswietlajaca X lub O na przycisku. </summary>
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

        /// <summary> Metoda aktywujaca przycisk. </summary>
        private void ButtonEnabler(Button button)
        {
            button.IsEnabled = true;
        }

        /// <summary> Metoda deaktywujaca przycisk. </summary>
        private void ButtonDisabler(Button button)
        {
            button.IsEnabled = false;
        }

        /// <summary> Metoda do wyswietlania tury gracza. </summary>
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

        /// <summary> Metoda aktualizujaca wyniki. </summary>
        private async void UpdateScore()
        {
            await Task.Delay(TimeSpan.FromSeconds(0.1));    //żeby przy zapisie wyniku do pliku gra miała czas na zapisanie zanim wczyta
            ReadScoreFromFile(1);
            ReadScoreFromFile(2);
        }

        /// <summary> Metoda zapisujaca wynik do pliku </summary>
        /// <param name="player"> Numer gracza </param>
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

        /// <summary> Metoda odczytujaca wynik z pliku </summary>
        /// <param name="player"> Numer gracza </param>
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

            if (savedScore.Equals(""))
            {
                await FileIO.WriteTextAsync(scoreFile, "0");
                savedScore = await FileIO.ReadTextAsync(scoreFile);
            }

            if (player.Equals(1))
            {
                score1.Text = savedScore;
            }
            else if (player.Equals(2))
            {
                score2.Text = savedScore;
            }
        }

        #endregion

        #region Glowne funkcje gry
        /// <summary> Metoda Nowa Gra. </summary>
        private void NewGame()
        {
            enabledAI = false;
            ButtonEnabler(buttonSI);
            player1Name.Text = "Gracz X";
            player2Name.Text = "Gracz O";
            UpdateScore();
            Reset();
            WhoStarts();
        }

        /// <summary> Metoda wyswietlajaca na poczatku gry komunikat do wybrania: kto zaczyna pierwszy </summary>
        private async void WhoStarts()
        {
            ContentDialog whoStarts = new ContentDialog
            {
                MaxWidth = this.ActualWidth,
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
        
        /// <summary> Metoda przechodzenia do nastepnej tury. </summary>
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
                
        /// <summary> Metoda sprawdzajaca, czy ktos wygral. </summary>
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
        
        /// <summary> Metoda, ktora wyswietla zwyciezce i dodaje mu punkt. </summary>
        private void Win(int playerNumber)
        {
            if (playerNumber == 1)
            {
                InfoDialog("Zwyciężył " + player1Name.Text);
                SaveScoreToFile(1);
            }
            else if (playerNumber == 2)
            {
                InfoDialog("Zwyciężył " + player2Name.Text);
                SaveScoreToFile(2);
            }
            else
            {
                InfoDialog("Remis");
            }
            
            Reset();
        }

        /// <summary> Metoda resetujaca plansze (oprocz punktacji). </summary>
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

        #endregion
    }
}