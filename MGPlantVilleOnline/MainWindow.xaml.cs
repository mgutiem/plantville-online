using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using MGPlantVilleOnline.Models;

/// <summary>
/// PlantVille Online Game
/// By: Miguel Gutiérrez
/// </summary>
namespace MGPlantVilleOnline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static HttpClient client = new HttpClient();
        public int money = 100;
        public int landPlots = 15;
        public int marketFee = 10;
        public string username = "";
        private const string filePath = "player_data.txt";
        private readonly HttpClient httpClient = new HttpClient();
        private readonly string apiChatUrl = "https://plantville.herokuapp.com";
        private readonly string apiTradesUrl = "https://plantville.herokuapp.com/trades";
        private readonly string apiAcceptTradesUrl = "https://plantville.herokuapp.com/accept_trade";
        public string tradingType;

        private static List<Seed> seed_list = new List<Seed>() {
            new Seed("strawberry", 2, 8, new TimeSpan(0, 0, 30)),
            new Seed("spinach", 5, 21, new TimeSpan(0, 1, 0)),
            new Seed("pears", 3, 20, new TimeSpan(0, 3, 0))
        };
        private List<Plant> garden = new List<Plant>();
        private List<Plant> inventory = new List<Plant>();
        private List<Model> chats = new List<Model>();
        private List<Model> trades = new List<Model>();
        private List<int> pendingTrades = new List<int>();
        public MainWindow()
        {
            // Calling loaders
            InitializeComponent();
            LoadLogin();
            LoadData();
            LoadChat();
            LoadGardenData();
            LoadSeedsCombo();

        }

        /// <summary>
        /// Switching grids visibility after Chat
        /// </summary>
        private void btnChat_Click(object sender, RoutedEventArgs e)
        {
           
            gridGarden.Visibility = Visibility.Hidden;
            gridSeedsEmporium.Visibility = Visibility.Hidden;
            gridInventory.Visibility = Visibility.Hidden;
            gridTradeMarket.Visibility = Visibility.Hidden;
            gridProposeTrade.Visibility = Visibility.Hidden;
            gridChat.Visibility = Visibility.Visible;
            LoadChat();
        }
        /// <summary>
        /// Switching grids visibility after Chat
        /// </summary>
        private void btnTradeMkt_Click(object sender, RoutedEventArgs e)
        {
            gridChat.Visibility = Visibility.Hidden;
            gridGarden.Visibility = Visibility.Hidden;
            gridSeedsEmporium.Visibility = Visibility.Hidden;
            gridInventory.Visibility = Visibility.Hidden;
            gridProposeTrade.Visibility = Visibility.Hidden;
            gridTradeMarket.Visibility = Visibility.Visible;
            loadTrades();
        }
        /// <summary>
        /// Switching grids visibility after Chat
        /// </summary>
        private void btnProposeTrade_Click(object sender, RoutedEventArgs e)
        {
            gridChat.Visibility = Visibility.Hidden;
            gridGarden.Visibility = Visibility.Hidden;
            gridSeedsEmporium.Visibility = Visibility.Hidden;
            gridInventory.Visibility = Visibility.Hidden;
            gridTradeMarket.Visibility = Visibility.Hidden;
            gridProposeTrade.Visibility = Visibility.Visible;
            txt_message.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Switching grids visibility after Emporium button clicked
        /// </summary>
        private void btnSeedsEmporium_Click(object sender, RoutedEventArgs e)
        {
            gridSeedsEmporium.Visibility = Visibility.Visible;
            gridChat.Visibility = Visibility.Hidden;
            gridGarden.Visibility = Visibility.Hidden;
            gridInventory.Visibility = Visibility.Hidden;
            gridTradeMarket.Visibility = Visibility.Hidden;
            gridProposeTrade.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// Switching grids visibility after Garden button clicked
        /// </summary>
        private void btnGarden_Click(object sender, RoutedEventArgs e)
        {
            gridGarden.Visibility = Visibility.Visible;
            gridSeedsEmporium.Visibility = Visibility.Hidden;
            gridChat.Visibility = Visibility.Hidden;
            gridInventory.Visibility = Visibility.Hidden;
            gridTradeMarket.Visibility = Visibility.Hidden;
            gridProposeTrade.Visibility = Visibility.Hidden;
            LoadGardenData();
        }
        /// <summary>
        /// Switching grids visibility after Inventory button clicked
        /// </summary>
        private void btnInventory_Click(object sender, RoutedEventArgs e)
        {

            gridInventory.Visibility = Visibility.Visible;
            gridGarden.Visibility = Visibility.Hidden;
            gridSeedsEmporium.Visibility = Visibility.Hidden;
            gridChat.Visibility = Visibility.Hidden;
            gridTradeMarket.Visibility = Visibility.Hidden;
            gridProposeTrade.Visibility = Visibility.Hidden;
            LoadInventoryData();
        }
        /// <summary>
        /// Cleans username textbox and puts the cursor to start the login process
        /// </summary>
        public void LoadLogin()
        {

            txtUsername.Text = "";
            txtUsername.Focus();
        }
        /// <summary>
        /// handles the click on the Sign in button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            ValidateAndSignIn();
        }
        /// <summary>
        /// Validates Sign in data from the login form fields
        /// </summary>
        private void ValidateAndSignIn()
        {
            string getUsername = txtUsername.Text.Trim();
            if (!string.IsNullOrEmpty(getUsername))
            {
                username = getUsername;
                gridSignIn.Visibility = Visibility.Hidden;
                gridMenu.Visibility = Visibility.Visible;
                gridChat.Visibility = Visibility.Visible;

            }
            else
            {
                MessageBox.Show("Please enter a username.");
                txtUsername.Focus();
            }
            UpdateStatus();
        }
        /// <summary>
        /// handles Enter key in the username login field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ValidateAndSignIn();
                e.Handled = true;
            }
        }
        /// <summary>
        /// Loads the chat data from the API
        /// </summary>
        private async void LoadChat()
        {
            try
            {
                string json = await httpClient.GetStringAsync(apiChatUrl);
                List<Model> chats = JsonConvert.DeserializeObject<List<Model>>(json);
                listBoxChat.Items.Clear();
                foreach (Model chat in chats)
                {
                    string formattedMessage = $"{chat.fields["username"]} : {chat.fields["message"]}";
                    listBoxChat.Items.Add(formattedMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving chat messages: {ex.Message}");
            }
            UpdateStatus();
        }
        /// <summary>
        /// Handles the click on the Send chat button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnChatSend_Click(object sender, RoutedEventArgs e)
        {
            string message = txtChat.Text;

            if (!string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    var chatMessage = new Dictionary<string, string>
                    {
                        { "username", username },
                        { "message", message}
                    };
                    var content = new FormUrlEncodedContent(chatMessage);
                    HttpResponseMessage response = await httpClient.PostAsync(apiChatUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Message sent successfully
                        string json_data = await response.Content.ReadAsStringAsync();
                        List<Model> chats = JsonConvert.DeserializeObject<List<Model>>(json_data);
                        txtChat.Text = "";
                        LoadChat();

                    }
                    else
                    {
                        MessageBox.Show("Failed to send message.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error sending message: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Your message is empty, try again!");
                txtChat.Focus();
            }
        }
        /// <summary>
        /// Handles the Enter key in the chat textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                btnChatSend_Click(sender, (RoutedEventArgs)e);
                e.Handled = true;
            }
        }
        /// <summary>
        /// Loads the trades data from the API
        /// </summary>
        private async void loadTrades()
        {
            try
            {
                string json = await httpClient.GetStringAsync(apiTradesUrl);
                trades = JsonConvert.DeserializeObject<List<Model>>(json);
                listBoxTradeMkt.Items.Clear();
                foreach (Model trade in trades)
                {
                    string tradeInfo = trade.fields["state"].ToString() == "open"
                        ? $"[{trade.fields["state"]}] {trade.fields["author"]} wants to buy {trade.fields["quantity"]} {trade.fields["plant"]} for ${trade.fields["price"]}"
                        : $"[{trade.fields["state"]}] {trade.fields["author"]} bought {trade.fields["quantity"]} {trade.fields["plant"]} for ${trade.fields["price"]} from {trade.fields["accepted_by"]}";

                    listBoxTradeMkt.Items.Add(tradeInfo);

                    if (pendingTrades.Contains(trade.pk) && trade.fields["state"].ToString() == "closed")
                    {
                        processTrade(trade);
                        pendingTrades.Remove(trade.pk);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving Trades data: {ex.Message}");
            }

        }
        /// <summary>
        /// Process the trade data
        /// Validates the type of trade and shows a message box
        /// </summary>
        /// <param name="trade"></param>
        private void processTrade(Model trade)
        {
            int quantity = int.Parse(trade.fields["quantity"].ToString());
            int price = int.Parse(trade.fields["price"].ToString());
            Plant plant = FindPlant(trade.fields["plant"]);
            if (plant != null)
            {
                if (tradingType != "sell")
                {
                    MessageBox.Show($"Trade accepted! You bought {quantity} {trade.fields["plant"]} for ${price} from {trade.fields["accepted_by"]}");

                    plant.Quantity += quantity;
                    money -= price;
                }
                else
                {
                    MessageBox.Show($"Trade accepted! You sold {quantity} {trade.fields["plant"]} for ${price} to {trade.fields["author"]}");

                    plant.Quantity -= quantity;
                    money += price;
                    if (plant.Quantity <= 0)
                        inventory.Remove(plant);
                }
            }
            UpdateStatus();
            
        }
        /// <summary>
        /// Handles the click on the Accept Trade button 
        /// Triggers the Accept Trade Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAcceptTrade_Click(object sender, RoutedEventArgs e)
        {
            processAcceptTrade();

        }
        /// <summary>
        /// Handles the double click on the items of the Trade Market listbox
        /// Triggers the Accept Trade Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxTradeMkt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Do you want to accept this trade?", "Accept trade",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            processAcceptTrade();
        }

        /// <summary>
        /// The method that processes the accepted Trade
        /// </summary>
        private void processAcceptTrade() {
            if (listBoxTradeMkt.SelectedIndex == -1)
            {
                MessageBox.Show("Error: Please select a trade to accept.");
            }
            else
            {
                Model selectedTrade = trades[listBoxTradeMkt.SelectedIndex];

                if (selectedTrade.fields["author"] == username)
                {
                    MessageBox.Show("Error: You cannot accept a trade that you proposed.");
                }
                else if (selectedTrade.fields["state"] == "closed")
                {
                    MessageBox.Show("Error: Please select an open trade. You cannot accept a closed trade.");
                }
                else
                {
                    Plant plant = FindPlant(selectedTrade.fields["plant"]);
                    if (plant != null)
                    {
                        if (plant.Quantity < Convert.ToInt32(selectedTrade.fields["quantity"].ToString()))
                        {
                            MessageBox.Show($"Error: You do not have enough {selectedTrade.fields["quantity"].ToString()} in your inventory to make trade.");
                        }
                        else
                        {
                            tradingType = "sell";
                            processTrade(selectedTrade);
                            AcceptTrade(selectedTrade.pk, username);

                        }
                    }
                    else
                    {
                        MessageBox.Show($"Error: You do not have {selectedTrade.fields["plant"].ToString()} in your inventory to make trade.");
                    }

                }
            }
        }
        /// <summary>
        /// Method to accept the trade
        /// </summary>
        /// <param name="tradeId"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        private async void AcceptTrade(int tradeId, string username)
        {

            try
            {
                var tradeData = new Dictionary<string, string>
                {
                    { "trade_id", tradeId.ToString() },
                    { "accepted_by", username }
                };


                var acceptedTradeContent = new FormUrlEncodedContent(tradeData);
                HttpResponseMessage tradeResponse = await httpClient.PostAsync(apiAcceptTradesUrl, acceptedTradeContent);

                if (tradeResponse.IsSuccessStatusCode)
                {
                    string tradesJson = await tradeResponse.Content.ReadAsStringAsync();
                    // Trade accepted successfully
                    trades = JsonConvert.DeserializeObject<List<Model>>(tradesJson);
                    loadTrades();
                }
                else
                {
                    MessageBox.Show("Error: Trade can't be accepted.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}");
            }
        }

        /// <summary>
        /// Method to find a plant in the inventory by name
        /// 
        /// </summary>
        /// <param name="plantName"></param>
        /// <returns></returns>
        private Plant FindPlant(string plantName)
        {
            return inventory.Find(plant => plant.Seed.Name == plantName);
        }

        /// <summary>
        /// Handles the submit trade button click
        /// Validates that all fields are properly filled out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSubmitTrade_Click(object sender, RoutedEventArgs e)
        {
            await processSubmitTrade();
            
        }

        private async Task processSubmitTrade()
        {
            if (!ValidateComboBox())
            {
                MessageBox.Show("Please select the plant you'd like to get from the trade");
                return;
            }

            if (!ValidateIntegerTextBox(txt_quantity, "Please enter a quantity.", "Quantity should be a valid integer number.", "Quantity cannot be negative."))
            {
                return;
            }

            if (!ValidateIntegerTextBox(txt_price, "Please enter a price.", "Price should be a valid integer number.", "Price cannot be negative."))

            {
                return;
            }

            // If all fields are valid, continue 
            var plant = cb_plants.SelectedItem.ToString();
            int quantity = int.Parse(txt_quantity.Text);
            int price = int.Parse(txt_price.Text);
            int tradeIdSent = await SubmitTrade(username, plant, quantity, price);

        }
        /// <summary>
        /// Method to validate plant combobox has a selection
        /// </summary>
        /// <returns></returns>
        private bool ValidateComboBox()
        {
            return cb_plants.SelectedItem != null;
        }
        /// <summary>
        /// Validates fields in the Send Trade form
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="emptyErrorMessage"></param>
        /// <param name="invalidFormatErrorMessage"></param>
        /// <param name="negativeValueErrorMessage"></param>
        /// <returns></returns>
        private bool ValidateIntegerTextBox(TextBox textBox, string emptyErrorMessage, string invalidFormatErrorMessage, string negativeValueErrorMessage)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show(emptyErrorMessage);
                return false;
            }

            if (!int.TryParse(textBox.Text, out int value))
            {
                MessageBox.Show(invalidFormatErrorMessage);
                return false;
            }

            if (value < 0)
            {
                MessageBox.Show(negativeValueErrorMessage);
                return false;
            }

            return true;
        }
        /// <summary>
        /// Process the trade with the user's data
        /// </summary>
        /// <param name="author"></param>
        /// <param name="plant"></param>
        /// <param name="quantity"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        private async Task<int> SubmitTrade(string author, string plant, int quantity, int price)
        {
            if (money < price)
            {
                MessageBox.Show("Error: You don't have enough money to submit this trade.");
            }
            else
            {
                try
                {
                    var newTrade = new Dictionary<string, string>
                    {
                        { "author", username },
                        { "plant", plant },
                        { "quantity", quantity.ToString() },
                        { "price", price.ToString() }
                    };
                    
                    var tradeContent = new FormUrlEncodedContent(newTrade);
                    HttpResponseMessage response = await httpClient.PostAsync(apiTradesUrl, tradeContent);

                    if (response.IsSuccessStatusCode)
                    {
                        // Trade sent successfully

                        string tradeId = await response.Content.ReadAsStringAsync();
                        pendingTrades.Add(Convert.ToInt32(tradeId));

                        // Clear input fields and update UI
                        txt_quantity.Text = "";
                        txt_price.Text = "";
                        cb_plants.SelectedIndex = 0;
                        txt_message.Visibility = Visibility.Visible;
                        return Convert.ToInt32(tradeId);
                    }
                    else
                    {
                        MessageBox.Show("Failed to send message.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error sending message: {ex.Message}");
                }

            }

            return -1;
        }
        /// <summary>
        /// Load Data From the file if exists.
        /// </summary>
        private void LoadData()
        {
            if (File.Exists(filePath))
            {
                string jsonContent;
                using (StreamReader streamReader = new StreamReader(filePath))
                {
                    // Read FILE content
                    jsonContent = streamReader.ReadToEnd();
                }

                if (string.IsNullOrEmpty(jsonContent))
                    return;

                // Deserialize the JSON content into a dictionary
                Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);

                // Validates data to set it properly into lists
                if (dictionary.ContainsKey("garden"))
                {
                    string gardenJson = dictionary["garden"].ToString();
                    garden = JsonConvert.DeserializeObject<List<Plant>>(gardenJson);
                }

                if (dictionary.ContainsKey("inventory"))
                {
                    string inventoryJson = dictionary["inventory"].ToString();
                    inventory = JsonConvert.DeserializeObject<List<Plant>>(inventoryJson);
                }

                if (dictionary.ContainsKey("money"))
                {
                    money = Convert.ToInt32(dictionary["money"]);
                }
                if (dictionary.ContainsKey("pending_trades"))
                {
                    pendingTrades = JsonConvert.DeserializeObject<List<int>>(dictionary["pending_trades"].ToString());
                }
            }
            // Update the status bar after loading the data
            UpdateStatus();
        }

        /// <summary>
        /// Save Data to the file
        /// </summary>
        private void SaveData()
        {
            // Create a Dictionary to store the game data
            // Stores the Garden list, Inventory list and Money value
            var data = new Dictionary<string, object>()
            {
                { "garden", garden },
                { "inventory", inventory },
                { "money", money },
                { "pending_trades", pendingTrades }
            };

            // Convert the data dictionary to JSON format and write it to a file
            var jsonData = JsonConvert.SerializeObject(data);
            File.WriteAllText(filePath, jsonData);
        }

        /// <summary>
        /// Update Status bar info with Money and Land Plots data
        /// </summary>
        private void UpdateStatus()
        {
            lblStatus.Content = $"Hello, {username}. \nMoney: ${money}\nLand: {landPlots - garden.Count}";
        }

        /// <summary>
        /// Loads Garden data (plants) into a listbox if exists
        /// </summary>
        private void LoadGardenData()
        {
            listBoxGarden.Items.Clear();
            if (garden.Count < 1)
            {
                listBoxGarden.Items.Add("No plants in the garden.");
            }
            else
            {
                foreach (Plant plant in garden)
                {
                    string status = CheckPlantStatus(plant);
                    listBoxGarden.Items.Add($"{plant.Seed.Name} ({status})");
                }
            }
            UpdateStatus();
        }

        /// <summary>
        /// Loads Inventory data into a listbox with the already harvested plants
        /// </summary>
        private void LoadInventoryData()
        {
            listBoxInventory.Items.Clear();
            foreach (Plant plant in inventory)
            {
                listBoxInventory.Items.Add($"{plant.Seed.Name} [{plant.Quantity}] ${plant.Seed.HarvestPrice}");
            }

            if (inventory.Count == 0)
            {
                listBoxInventory.Items.Add("No fruits or vegetables harvested.");
            }

            UpdateStatus();
        }
        /// <summary>
        /// Loads the Seeds Emporioum data into a listbox with the list of available Seeds
        /// </summary>
        private void LoadSeedsCombo()
        {
            listBoxSeedsEmporium.Items.Clear();
            cb_plants.Items.Clear();
            foreach (Seed seed in seed_list)
            {
                listBoxSeedsEmporium.Items.Add(string.Format("{0} ${1}", seed.Name, seed.SeedPrice));
                cb_plants.Items.Add(string.Format(seed.Name));
            }
            UpdateStatus();
        }
        /// <summary>
        /// Validates if a plant is ready to be harvested,
        /// how much more time it will to be harvested
        /// and if is not spoiled 
        /// </summary>
        /// <param name="plant"></param>
        /// <returns></returns>
        private string CheckPlantStatus(Plant plant)
        {
            TimeSpan timeSincePlanting = DateTime.Now.Subtract(plant.HarvestTime);
            if (timeSincePlanting >= plant.Seed.HarvestDuration)
            {
                if (!plant.IsSpoiled)
                {
                    return "ready to harvest";
                }
                else
                {
                    return "spoiled";
                }
            }
            else
            {
                TimeSpan timeRemaining = plant.Seed.HarvestDuration.Subtract(timeSincePlanting);
                int secondsLeft = (int)timeRemaining.TotalSeconds;
                return $"{secondsLeft} seconds left";
            }
        }

        /// <summary>
        /// Handles the double click on the listbox items of the Seeds Emporium listbox
        /// Performs the sale process of the seeds and assign them to the available land plots
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxSeedsEmporium_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedIndex = listBoxSeedsEmporium.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < seed_list.Count)
            {
                var selectedSeed = seed_list[selectedIndex];
                if (selectedSeed.SeedPrice > money)
                {
                    MessageBox.Show("You don't have enough money.");
                }
                else if (landPlots - garden.Count < 1)
                {
                    MessageBox.Show("You don't have enough land to plant another crop.");
                }
                else
                {
                    money -= selectedSeed.SeedPrice;
                    garden.Add(new Plant(selectedSeed));
                    MessageBox.Show($"You purchased {selectedSeed.Name}");
                    ValidateMoney();
                    UpdateStatus();
                }
            }
        }
        /// <summary>
        /// Handles the double click on the listbox items of the plants on the Garden 
        /// Validates the plant status and performs the harvest process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxGarden_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int selectedIndex = listBoxGarden.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < garden.Count)
            {
                Plant selectedPlant = garden[selectedIndex];
                string plantStatus = CheckPlantStatus(selectedPlant);
                if (plantStatus == "ready to harvest")
                {
                    inventory.Add(selectedPlant);
                    garden.RemoveAt(selectedIndex);
                    MessageBox.Show($"{selectedPlant.Seed.Name} harvested.");
                    
                }
                else if (plantStatus == "spoiled")
                {
                    if (MessageBox.Show($"{ selectedPlant.Seed.Name} is spoiled and cannot be harvested. Do you want to dispose it?", "Your plant is spoiled!",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.No)
                        {
                            return;
                        }
                        garden.RemoveAt(selectedIndex);
                }
                else
                {
                    MessageBox.Show($"{selectedPlant.Seed.Name} is not ready to be harvested yet. {plantStatus}");
                }
                LoadGardenData();
            }
        }
        /// <summary>
        /// Handles the click on the Harvest button
        /// Validates each of the plants status and performs the harvest process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
  
        private void btnHarvestAll_Click(object sender, RoutedEventArgs e)
        {
            int harvestedCount = 0;
            //List<Plant> harvestedPlants = new List<Plant>();
            bool hasSpoiledPlants = false;

            foreach (Plant harvestedPlant in garden.ToList<Plant>())
            {
                bool plantGroup = false;
                //Plant plant = garden[i];
                string plantStatus = CheckPlantStatus(harvestedPlant);

                if (plantStatus == "ready to harvest")
                {
                    foreach (Plant sameTypePlant in inventory)
                    {
                        if (sameTypePlant.Seed.Name == harvestedPlant.Seed.Name)
                        {
                            plantGroup = true;
                            sameTypePlant.Quantity++;

                        }
                    }
                    if (!plantGroup)
                        inventory.Add(harvestedPlant);
                        garden.Remove(harvestedPlant);
                        harvestedCount++;
                }
                else if (plantStatus == "spoiled")
                {
                    hasSpoiledPlants = true;
                }
            }

            if (hasSpoiledPlants)
            {
                MessageBoxResult result = MessageBox.Show("One or more of your plants are spoiled. Do you want to dispose of them?", "Spoiled Plant Found!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    garden.RemoveAll(plant => CheckPlantStatus(plant) == "spoiled");
                    MessageBox.Show("Spoiled plants disposed.");
                }
            }

            if (harvestedCount > 0)
            {
                //inventory.AddRange(harvestedPlants);
                MessageBox.Show($"Harvested {harvestedCount} plants.");
                LoadGardenData();
            }
            else
            {
                MessageBox.Show("Nothing to harvest.");
                LoadGardenData();
            }
        }
        /// <summary>
        /// Handles the click on the Sell in Farmer's Market button
        /// Deducts the $10 fee of the selling inventary process
        /// Calculates each of the plants value and performs the selling process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSellInventory_Click(object sender, RoutedEventArgs e)
        {
            if (inventory.Count == 0 && 
                MessageBox.Show("The Farmer's Market fee is $10. Do you want to go without any inventory?", "You are about to lose money!!!", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            var income = inventory.Sum(plant => plant.Seed.HarvestPrice);
            var netProfit = income - marketFee;
            var sellingResult = $"Made ${income} from your plants - ${marketFee} Farmer's Market fee \n Net profit: {netProfit}";
            if (income <= 0) {
                sellingResult = $"Lost -${marketFee}";
            }
            money += netProfit;
            inventory.Clear();
            MessageBox.Show($"Cleared inventory. You {sellingResult}", "Cleared inventory");
            LoadInventoryData();
            // Validating that the player's money is not negative
            ValidateMoney();            
        }
        /// <summary>
        /// Validates if user has enough Money to keep playing
        /// If not restarts the game
        /// </summary>
        private void ValidateMoney()
        {
            if (money < 0)
            {
                MessageBoxResult result = MessageBox.Show("Oh no, you've gone bankrupt! Do you want to play again?", "Game Over!", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    // Restart the game by resetting the money and land plots
                    money = 100;
                    landPlots = 15;

                    // Clear the data file
                    ClearData();

                    // Reload the necessary data or reset the game state
                    garden.Clear();
                    inventory.Clear();

                    // Update the UI
                    UpdateStatus();
                    LoadGardenData();
                }
                else
                {
                    money = 100;
                    landPlots = 15;
                    // Exit the game and clear the data file
                    ClearData();
                    Close();
                }
            }
        }
        /// <summary>
        /// Clear all data from the file
        /// </summary>
        private void ClearData()
        {
            var jsonData = "";
            File.WriteAllText(filePath, jsonData);
        }

        /// <summary>
        /// Handles the Closing event of the App Window
        /// Saves data into the file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveData();

        }
        /// <summary>
        /// Handles the event on the Close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /*
        public void SetupControls()
        {
            
            Button triggerBtnAcceptTrade = new Button();
            triggerBtnAcceptTrade.Click += (sender, e) =>
            {
                processAcceptTrade();
            };
            ListBox listBoxAcceptTrade = new ListBox();
            listBoxAcceptTrade.MouseDoubleClick += (sender, e) =>
            {
                processAcceptTrade();
            };
        }*/

    }


}
