using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;  // Correctly displays UTF-8 characters (special symbols, accented letters, and other Unicode characters).

        // Display the banner at the start of the game
        DisplayBanner();

        // Display game instructions to guide the player on how to play
        DisplayInstructions();

        // Get player's name  
        string playerName = GetPlayerName();
        CenterTextHorizontally($"Welcome, {playerName}!");

        Exception? exception = null;
        int speedInput;   // Variable to store the user's speed input
        string prompt = $"Select speed [1], [2] (default), or [3]: "; // Prompt message
        string? input; //Variable to hold user input as a string

        // Change text color for the prompt
        Console.ForegroundColor = ConsoleColor.Green;

        // Calculate the starting position to center the prompt horizontally
        int consoleWidth = Console.WindowWidth; // Get the width of the console window
        int startPosition = (consoleWidth - prompt.Length) / 2; // Center alignment calculation

        // Print the prompt at the calculated centered position
        Console.SetCursorPosition(startPosition, Console.CursorTop); // Move the cursor to the starting position
        Console.Write(prompt); // Display the prompt message

        // Position the cursor immediately after the prompt for user input
        Console.SetCursorPosition(startPosition + prompt.Length, Console.CursorTop);

        // Read and validate user input
        while (!int.TryParse(input = Console.ReadLine(), out speedInput) || speedInput < 1 || speedInput > 3)
        {
            if (string.IsNullOrWhiteSpace(input)) // If the user presses Enter without typing anything
            {
                speedInput = 2; // Default speed is 2
                break; // Exit the loop
            }
            else
            {
                // Display an error message for invalid input
                string error = "Invalid Input. Try Again...";
                Console.SetCursorPosition((consoleWidth - error.Length) / 2, Console.CursorTop); // Center the error message
                Console.WriteLine(error); // Show the error message

                // Reprint the prompt after the error message, maintaining alignment
                Console.SetCursorPosition(startPosition, Console.CursorTop); // Move cursor back to the prompt position
                Console.Write(prompt); // Display the prompt again

                // Position the cursor after the prompt for the next user input
                Console.SetCursorPosition(startPosition + prompt.Length, Console.CursorTop);
            }
        }

        // Reset color to default after the prompt
        Console.ResetColor();

        // Define the speeds for different player selections
        int[] velocities = { 100, 70, 50 };
        int velocity = velocities[speedInput - 1];
        char[] DirectionChars = { '^', 'v', '<', '>' };  // Symbols for direction of the snake
        TimeSpan sleep = TimeSpan.FromMilliseconds(velocity);  // Adjust speed based on user input

        int width = Console.WindowWidth;  // Get the current console width
        int height = Console.WindowHeight;  // Get the current console height
        Tile[,] map = new Tile[width, height];  // Create a map for the game grid
        Direction? direction = null;  // The snake's current direction
        Queue<(int X, int Y)> snake = new();  // Queue to store the snake's body positions
        (int X, int Y) = (width / 2, height / 2);  // Start the snake at the center of the screen
        bool closeRequested = false;  // Flag to check if the game should be closed
        bool isPaused = false;  // Flag to check if the game is paused


        try
        {
            Console.CursorVisible = false;  // Hide the cursor during the game
            Console.Clear();  // Clear the screen at the beginning
            snake.Enqueue((X, Y));  // Add the starting position of the snake to the queue
            map[X, Y] = Tile.Snake;  // Mark the starting position as part of the snake
            PositionFood(map, width, height);  // Place food on the map
            Console.SetCursorPosition(X, Y);  // Set the cursor to the snake's starting position
            Console.Write('@');  // Display the snake's head on the screen

            // Wait for the player to set the direction before starting the movement
            while (!direction.HasValue && !closeRequested)
            {
                GetDirection(ref direction, ref closeRequested);  // Get the player's input for direction
            }

            // Main game loop
            while (!closeRequested)
            {
                // Check if the console window has been resized and end the game if so
                if (Console.WindowWidth != width || Console.WindowHeight != height)
                {
                    Console.Clear();
                    Console.Write("Console was resized. Snake game has ended.");
                    return;  // End the game if the console size changes
                }

                // Check if a key is pressed to change the direction or pause the game
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Enter)
                    {
                        isPaused = !isPaused;  // Toggle pause state
                    }
                    else
                    {
                        // Update direction based on key press
                        switch (key)
                        {
                            case ConsoleKey.UpArrow:
                                if (direction != Direction.Down) direction = Direction.Up; // Prevent reversing
                                break;
                            case ConsoleKey.DownArrow:
                                if (direction != Direction.Up) direction = Direction.Down; // Prevent reversing
                                break;
                            case ConsoleKey.LeftArrow:
                                if (direction != Direction.Right) direction = Direction.Left; // Prevent reversing
                                break;
                            case ConsoleKey.RightArrow:
                                if (direction != Direction.Left) direction = Direction.Right; // Prevent reversing
                                break;
                            case ConsoleKey.Escape:
                                closeRequested = true; // Allow the player to exit the game
                                break;
                        }
                    }
                }

                // Only move the snake if the game is not paused
                if (!isPaused)
                {
                    // Update the snake's position based on the current direction
                    switch (direction)
                    {
                        case Direction.Up: Y--; break;
                        case Direction.Down: Y++; break;
                        case Direction.Left: X--; break;
                        case Direction.Right: X++; break;
                    }

                    // Check if the snake collides with the walls or itself
                    if (X < 0 || X >= width ||
                    Y < 0 || Y >= height ||
                    map[X, Y] is Tile.Snake)
                    {
                        // Call DisplayGameOver when the game ends due to collision
                        DisplayGameOver(snake.Count - 1);  // Pass the score
                        return;  // End the game
                    }

                    // Draw the new position of the snake
                    Console.SetCursorPosition(X, Y);
                    Console.Write(DirectionChars[(int)direction!]);  // Display the snake's current direction

                    // Add the new position to the snake's body
                    snake.Enqueue((X, Y));

                    // If the snake eats food, generate new food and increase speed
                    if (map[X, Y] is Tile.Food)
                    {
                        PositionFood(map, width, height);  // Place new food on the map
                        velocity = Math.Max(10, velocity - 10);  // Increase speed, ensuring it doesn't go below a threshold
                        sleep = TimeSpan.FromMilliseconds(velocity);  // Update sleep time based on new speed
                    }
                    else
                    {
                        // If no food is eaten, remove the last segment of the snake's body
                        (int x, int y) = snake.Dequeue();
                        map[x, y] = Tile.Open;  // Mark the old position as empty
                        Console.SetCursorPosition(x, y);
                        Console.Write(' ');  // Clear the old position
                    }

                    // Mark the current position as part of the snake's body
                    map[X, Y] = Tile.Snake;

                    // Check if a key is pressed to change the direction or pause the game
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.Enter)
                        {
                            isPaused = true;  // Pause the game
                        }
                        else
                        {
                            GetDirection(ref direction, ref closeRequested);  // Get the new direction from user input
                        }
                    }

                    // Control the snake's speed
                    System.Threading.Thread.Sleep(sleep);  // Pause for a brief moment based on the selected speed
                }
            }
        }
        catch (Exception e)
        {
            exception = e;  // Catch any unexpected exceptions
            throw;
        }
        finally
        {
            Console.CursorVisible = true;  // Show the cursor again after the game ends
            Console.Clear();  // Clear the screen
            Console.WriteLine(exception?.ToString() ?? "Snake was closed.");  // Display any exception message or closure info
        }
    }
   

    // Display the banner with the game banner 
    static void DisplayBanner()
    {
        string banner = @"
██╗  ██╗██╗   ██╗███╗   ██╗████████╗██╗███╗   ██╗ ██████╗       ██████╗███╗   ██╗ █████╗ ██╗  ██╗███████╗
██║  ██║██║   ██║████╗  ██║╚══██╔══╝██║████╗  ██║██╔════╝      ███╔══╝ ████╗  ██║██╔══██╗██║ ██╔╝██╔════╝
███████║██║   ██║██╔██╗ ██║   ██║   ██║██╔██╗ ██║██║  ███╗      █████  ██╔██╗ ██║███████║█████╔╝ █████╗  
██╔══██║██║   ██║██║╚██╗██║   ██║   ██║██║╚██╗██║██║   ██║          ██ ██║╚██╗██║██╔══██║██╔═██╗ ██╔══╝  
██║  ██║╚██████╔╝██║ ╚████║   ██║   ██║██║ ╚████║╚██████╔╝     ███████╗██║ ╚████║██║  ██║██║  ██╗███████╗
╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═══╝   ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝      ╚══════╝╚═╝  ╚═══╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝";

        // Clear the screen
        Console.Clear();

        // Set text color to dark red
        Console.ForegroundColor = ConsoleColor.DarkRed;

        // Display the banner centered
        CenterTextOnScreen(banner);

        // Reset the default color
        Console.ResetColor();

        // Display the message below the banner, also centered
        string message = "\nPress any key to continue...";
        // Calculate vertical position after banner
        int messageVerticalPosition = Console.WindowHeight / 2 + (banner.Split('\n').Length / 2);
        CenterTextOnScreen(message, messageVerticalPosition);

        // Wait for the user to press any key to continue
        Console.ReadKey(true);
    }

    // Helper function to center text on screen
    static void CenterTextOnScreen(string text, int verticalPosition = -1)
    {
        int consoleWidth = Console.WindowWidth;
        int consoleHeight = Console.WindowHeight;

        // Split the text into lines (if multi-line)
        var lines = text.Split(new[] { '\n' }, StringSplitOptions.None);

        // If verticalPosition is not specified, calculate the vertical center
        if (verticalPosition == -1)
        {
            verticalPosition = (consoleHeight - lines.Length) / 2;
        }

        // Print each line of text centered horizontally
        foreach (var line in lines)
        {
            int horizontalCenter = (consoleWidth - line.Length) / 2;
            Console.SetCursorPosition(horizontalCenter, verticalPosition);
            Console.WriteLine(line);
            verticalPosition++;  // Move to the next line
        }
    }



    // Display the game instructions to the player
    static void DisplayInstructions()
    {
        // Clear the screen for a clean slate to display the instructions 
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkGreen;  // Set text color to dark green

        string instructions = @"
|------------------------------------------------------------------|
|                          Game Instructions:                      | 
|------------------------------------------------------------------|
1. Choose the Speed of the Snake:
    Press 1: Slow 
    Press 2: Normal  
    Press 3: Fast   
   -> If you press Enter without typing anything, the default speed (2) will be selected.

2. Control the Snake's Direction:
   Use the arrow keys on your keyboard to control the direction of the snake.
   - Up Arrow: Move Up
   - Down Arrow: Move Down
   - Left Arrow: Move Left
   - Right Arrow: Move Right

3. Game Objective:
   The snake will grow longer each time it eats food.
   Game Over Conditions: The game will end if the snake collides with the walls or runs into itself.

4. Pause and Resume:
   To pause the game, press Enter.
   To continue, press Enter again.

5. Replay the Game:
   After each game over, press Enter to restart the game.
--------------------------------------------------------
";

        // Center the text and the frame using the CenterTextOnScreen method
        CenterTextOnScreen(instructions);
        Console.ReadKey(true); // Wait for the user to press any key before selecting the speed.
    }

    // Function to center text horizontally in the console
    static void CenterTextHorizontally(string text)
    {
        int consoleWidth = Console.WindowWidth;  // Get the width of the console window
        int horizontalCenter = (consoleWidth - text.Length) / 2;  // Calculate the center position

        Console.SetCursorPosition(horizontalCenter, Console.CursorTop);  // Set the cursor at the calculated position
        Console.WriteLine(text);  // Write the text at the center
    }

    // Get the player's name
    static string GetPlayerName()
    {
        Console.Clear();

        int consoleWidth = Console.WindowWidth;
        int consoleHeight = Console.WindowHeight;

        // Calculate vertical center position for header
        int verticalCenter = consoleHeight / 2 - 4; // Adjusting to leave some space around

        // Centered header for the game prompt
        Console.ForegroundColor = ConsoleColor.Green;

        string header = "*****************************************";
        Console.SetCursorPosition((consoleWidth - header.Length) / 2, verticalCenter);
        Console.WriteLine(header);

        string title = "*        START HUNTING SNAKE GAME!         *";
        Console.SetCursorPosition((consoleWidth - title.Length) / 2, verticalCenter + 1);
        Console.WriteLine(title);

        string footer = "*****************************************";
        Console.SetCursorPosition((consoleWidth - footer.Length) / 2, verticalCenter + 2);
        Console.WriteLine(footer);

        // Prompt for player name (centered relative to the title)
        string prompt = "Please enter your name:";
        Console.SetCursorPosition((consoleWidth - prompt.Length) / 2, verticalCenter + 4);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(prompt);
        Console.ResetColor();

        // Start from the center of the screen for the name input
        Console.SetCursorPosition(consoleWidth / 2, verticalCenter + 5); // Start from the center horizontally

        // Read the player's name, with the cursor starting from the center and expanding outward
        string? playerName = Console.ReadLine();


        // Return the player's name
        return playerName;
    }

    static void GetDirection(ref Direction? direction, ref bool closeRequested)
    {
        switch (Console.ReadKey(true).Key)
        {
            case ConsoleKey.UpArrow: direction = Direction.Up; break;
            case ConsoleKey.DownArrow: direction = Direction.Down; break;
            case ConsoleKey.LeftArrow: direction = Direction.Left; break;
            case ConsoleKey.RightArrow: direction = Direction.Right; break;
            case ConsoleKey.Escape: closeRequested = true; break;  // Allow the player to exit the game by pressing Escape
        }
    }

    // Position new food on the map at an empty tile
    static void PositionFood(Tile[,] map, int width, int height)
    {
        List<(int X, int Y)> possibleCoordinates = new();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (map[i, j] is Tile.Open)  // Find open spaces on the map
                {
                    possibleCoordinates.Add((i, j));  // Add the coordinates of open tiles to the list
                }
            }
        }
        if (possibleCoordinates.Count > 0)  // If there are empty tiles available
        {
            int index = Random.Shared.Next(possibleCoordinates.Count);  // Select a random tile for the food
            (int X, int Y) = possibleCoordinates[index];  // Get the coordinates of the food
            map[X, Y] = Tile.Food;  // Mark the tile as food
            Console.SetCursorPosition(X, Y);  // Set the cursor to the food's position
            Console.Write('+');  // Display the food
        }
    }

    // Enum to define the direction of the snake
    enum Direction
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }

    // Enum to represent the tiles on the game map
    enum Tile
    {
        Open = 0,  // Empty tile
        Snake,     // Part of the snake
        Food,      // Food item
    }
    static void DisplayGameOver(int score)
    {
        Console.Clear();
        // Set the color for the "GAME OVER" text
        Console.ForegroundColor = ConsoleColor.DarkRed;
        string gameOverMessage = @"


░██████╗░░█████╗░███╗░░░███╗███████╗  ░█████╗░██╗░░░██╗███████╗██████╗░
██╔════╝░██╔══██╗████╗░████║██╔════╝  ██╔══██╗██║░░░██║██╔════╝██╔══██╗
██║░░██╗░███████║██╔████╔██║█████╗░░  ██║░░██║╚██╗░██╔╝█████╗░░██████╔╝
██║░░╚██╗██╔══██║██║╚██╔╝██║██╔══╝░░  ██║░░██║░╚████╔╝░██╔══╝░░██╔══██╗
╚██████╔╝██║░░██║██║░╚═╝░██║███████╗  ╚█████╔╝░░╚██╔╝░░███████╗██║░░██║
░╚═════╝░╚═╝░░╚═╝╚═╝░░░░░╚═╝╚══════╝  ░╚════╝░░░░╚═╝░░░╚══════╝╚═╝░░╚═╝

Your final score: " + (score == 0 ? "0 " : score.ToString());

        // Display the GAME OVER message centered on the screen
        CenterTextOnScreen(gameOverMessage);

        string prompt = "Press Enter to play again, or Press any other keys to Escape.";
        CenterTextHorizontally(prompt);

        // Wait for the player to press Enter to play again or Escape to exit
        ConsoleKey key = Console.ReadKey(true).Key;
        if (key == ConsoleKey.Enter)
        {
            // Play again: call the Main function or restart the game
            Main();
        }
        else if (key == ConsoleKey.Escape)
        {
            // Exit the game
            return;
        }
    }

}
