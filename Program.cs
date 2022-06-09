using System;
using System.Configuration;

namespace MancalaGame
{
    public class Program
    {

        static int MAX_DEPTH = 4;  // default value reset inside ProcessConfiguration
        static bool ALPHA_BETA = false;

        public static readonly double MIN_VALUE = -1.0;
        public static readonly double MAX_VALUE = 1.0;

        private static Board board;

        private static Player firstPlayer;
        private static int opponentDifficultyLevel = 2;

        static void Main(string[] args)
        {
            board = new Board();

            ProcessConfiguration();

            MenuOpponentDifficulty();

            MenuPlayerGoesFirst();

            board.DisplayBoard();

            PlayGame();

            GetWinner(); 

        }

        /// <summary>
        /// Prints the opponent difficulty level menu, and sets the setting based on user response (default = MEDIUM)
        /// </summary>
        private static void MenuOpponentDifficulty()
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine("|Select Opponent Difficulty Level|");
            Console.WriteLine("|--------------------------------|");
            Console.WriteLine("| 1.            Easy             |");
            Console.WriteLine("| 2.           Medium            |");
            Console.WriteLine("| 3.            Hard             |");
            Console.WriteLine("----------------------------------");
            string computerDifficultyResponse = Console.ReadLine();

            switch (computerDifficultyResponse)
            {
                case "1":
                    opponentDifficultyLevel = 1;
                    Console.WriteLine("Opponent difficulty level is currently EASY.");
                    break;
                case "2":
                    opponentDifficultyLevel = 2;
                    Console.WriteLine("Opponent difficulty level is currently MEDIUM.");
                    break;
                case "3":
                    opponentDifficultyLevel = 3;
                    Console.WriteLine("Opponent difficulty level is currently HARD.");
                    break;
                default:
                    opponentDifficultyLevel = 2;
                    Console.WriteLine("Incorrect input. Playing with default opponent difficulty level MEDIUM.");
                    break;
            }
        }

        /// <summary>
        /// Displays menu option for first player, and sets setting based on user response
        /// </summary>
        private static void MenuPlayerGoesFirst()
        {
            Console.WriteLine("Do you want to go first? (Enter 'y' if yes)");
            string response = Console.ReadLine();
            firstPlayer = (response == "Y".ToLower() || response == "YES".ToLower()) ? Player.PLAYER : Player.COMPUTER;
        }

        /// <summary>
        /// Method to play game, keeps a while loop running until game is over
        /// If Player.PLAYER goes first, Player.PLAYER moves first, then Player.COMPUTER
        /// Otherwise, Player.COMPUTER moves first, then Player.PLAYER
        /// </summary>
        private static void PlayGame()
        {
            while (!board.gameOver)
            {
                if (firstPlayer == Player.PLAYER)
                {
                    PlayerMove();
                    board.DisplayBoard();
                    if (!board.gameOver)
                    {
                        ComputerMove();
                        board.DisplayBoard();
                    }
                }

                else
                {
                    ComputerMove();
                    board.DisplayBoard();
                    if (!board.gameOver)
                    {
                        PlayerMove();
                        board.DisplayBoard();
                    }
                }

            }
        }


        /// <summary>
        /// Method for Player.PLAYER's move
        /// </summary>
        private static void PlayerMove()
        {
            int move = PlayerChooseMove();
            if (move == -1)
            {
                board.gameOver = true;
            }

            if (!board.gameOver)
            {
                Board newBoard = board.MakeMove(Player.PLAYER, move, true);

                board.computerRow = newBoard.computerRow;
                board.playerRow = newBoard.playerRow;
                board.landedInMancala = newBoard.landedInMancala;
                board.gameOver = newBoard.gameOver;
            }


            if (board.AllSpotsAreEmpty(Player.PLAYER))
            {
                board.GameOverMove(board.playerRow, board.computerRow, true);
            }
            else if (board.AllSpotsAreEmpty(Player.COMPUTER))
            {
                board.GameOverMove(board.computerRow, board.playerRow, true);
            }
            else if (board.landedInMancala) 
            {
                board.landedInMancala = false;
                PlayerMove();
            }
        }

        /// <summary>
        /// Method for Player.PLAYER to choose move
        /// </summary>
        /// <returns>int, Player.PLAYER's chosen move</returns>
        private static int PlayerChooseMove()
        {
            if (board.AllSpotsAreEmpty(Player.PLAYER))
            {
                return -1;
            }
            Console.WriteLine("Choose your value");
            string value = Console.ReadLine();
            int spot;
            while (!int.TryParse(value, out spot) || !(spot >= 1 && spot <= Board.NUM_SPOTS) || board.playerRow[spot - 1] == 0)
            {
                Console.WriteLine("Choose a valid spot");
                value = Console.ReadLine();
            }

            Console.WriteLine("You chose " + spot);

            return spot - 1;
        }

        /// <summary>
        /// Method for computer's move
        ///     * If computer's difficulty level is easy, chooses first available move
        ///     * If computer's difficulty level is medium, randomly chooses an available move
        ///     * If computer's difficulty level is hard, uses Minimax (Game) Tree [Or AlphaBeta pruning, depending on AppConfig] to calculate move
        /// </summary>
        public static void ComputerMove()
        {
            Console.WriteLine("Computer's Turn");
            int bestMove;

            switch (opponentDifficultyLevel)
            {
                case 1:
                    bestMove = ComputerChooseFirstAvailableMove();
                    if (bestMove == -1)
                    {
                        board.gameOver = true;
                    }
                    break;

                case 3:
                     bestMove = ComputerChooseMove();
                    if (bestMove == -1)
                    {
                       board.gameOver = true;
                    }
                    break;
                default: // case 2, medium
                    bestMove = ComputerRandomlyChooseFirstAvailableMove();
                    break;   
            }

            if (!board.gameOver)
            {
                Console.WriteLine("Computer chose " + (bestMove + 1));


                Board newBoard = board.MakeMove(Player.COMPUTER, bestMove, true);
                board.computerRow = newBoard.computerRow;
                board.playerRow = newBoard.playerRow;
                board.landedInMancala = newBoard.landedInMancala;
                board.gameOver = newBoard.gameOver;

            }

            if (board.AllSpotsAreEmpty(Player.COMPUTER))
            {
                board.GameOverMove(board.computerRow, board.playerRow, true);
            }
            else if (board.AllSpotsAreEmpty(Player.PLAYER))
            {
                board.GameOverMove(board.playerRow, board.computerRow, true);
            }
            else if (board.landedInMancala)
            {
                board.landedInMancala = false;
                ComputerMove();
            }
        }

        /// <summary>
        /// Choose first available spot (Computer difficulty level EASY, or Computer level HARD desperation move)
        /// </summary>
        /// <returns>int, the first available move</returns>
        private static int ComputerChooseFirstAvailableMove()
        {
            int currSpot = -1;
            for (int spot = 0; spot < Board.NUM_SPOTS; spot++)
            {
                if (board.IsValidMove(Player.COMPUTER, spot))
                {
                    currSpot = spot;
                    break; 
                }
            }
            return currSpot;
        }

        /// <summary>
        /// Randomly choose first available move (Computer difficulty level MEDIUM)
        /// </summary>
        /// <returns>int, a random available move</returns>
        private static int ComputerRandomlyChooseFirstAvailableMove()
        {
            Random random = new Random();
            int currSpot = -1;

            List<int> availableMoves = new List<int>();
            for (int spot = 0; spot < Board.NUM_SPOTS; spot++)
            {
                if (board.IsValidMove(Player.COMPUTER, spot))
                {
                    availableMoves.Add(spot);
                }
            }
            if (availableMoves.Count > 0)
            {
                int randomIndex = random.Next(availableMoves.Count);
                currSpot = availableMoves[randomIndex];
            }

            return currSpot;
        }

        /// <summary>
        /// Computer chooses move based on Minimax (Game) Tree [or AlphaBeta pruning, depending on AppConfig]
        /// </summary>
        /// <returns>int, the computer's best move</returns>
        private static int ComputerChooseMove()
        {
            double highVal = -1.0;
            int bestMove = 0;
            double alfa = -1.0;
            double beta = 1.0;

            Dictionary<Board, int> childrenAndSpots = board.GetPossibleNextBoardsAndTheSpotToCreateThem(Player.COMPUTER); 

            foreach (Board possibleBoard in childrenAndSpots.Keys)
            { 
                double thisVal;
                if (possibleBoard.landedInMancala)
                {
                    thisVal = ALPHA_BETA
                        ? AlphaBeta.Value(possibleBoard, MAX_DEPTH, alfa, beta, Player.COMPUTER)
                        : GameTree.Value(possibleBoard, MAX_DEPTH, Player.COMPUTER);
                }
                else
                {
                    thisVal = ALPHA_BETA
                        ? AlphaBeta.Value(possibleBoard, MAX_DEPTH - 1, alfa, beta, Player.PLAYER)
                        : GameTree.Value(possibleBoard, MAX_DEPTH - 1, Player.PLAYER);
                }
                if (thisVal > highVal)
                {
                    bestMove = childrenAndSpots[possibleBoard];
                    highVal = thisVal;
                }
                
            }
           

            if (highVal == -1)
            {
                bestMove = ComputerChooseFirstAvailableMove();
            }
            Console.WriteLine($"Computer best move is {(bestMove + 1)}    (subj. value {highVal})");
            return bestMove;
        }

        /// <summary>
        /// Get the winner and print the winner to the Console
        /// </summary>
        private static void GetWinner()
        {
            Player winningPlayer = board.playerRow[6] > board.computerRow[6] ? Player.PLAYER : Player.COMPUTER;
            List<int> winningPlayerRow = winningPlayer == Player.PLAYER ? board.playerRow : board.computerRow;
            if (winningPlayerRow[6] == 24)
            {
                Console.WriteLine("Tie game!");
            }
            else Console.WriteLine($"{winningPlayer} won!");
        }

        
        /// <summary>
        /// Determines settings using AppConfig file
        /// </summary>
        private static void ProcessConfiguration()
        {

            string strDepth = ConfigurationManager.AppSettings["Depth"];
            var depth = 0;
            if (int.TryParse(strDepth, out depth))
            {
                if (depth > 1 && depth < 10)
                {
                    MAX_DEPTH = depth;
                }
            }

            string strAB = ConfigurationManager.AppSettings["AlphaBeta"];

            ALPHA_BETA = strAB.CompareTo("AB") == 0;

            if (ALPHA_BETA)
            {
                Console.WriteLine("Using AlphaBeta pruning.");
            }
            else
            {
                Console.WriteLine("Using Minimax (Game) Tree.");
            }
        }


    }
}