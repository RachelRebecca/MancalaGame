using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MancalaGame
{
    class Board
    {
        public static readonly int NUM_SPOTS = 6;
        public static readonly int CUP_INDEX_VAL = 6;

        public static readonly int NUM_CUPS = 12;
        public static readonly double TOTAL_MARBLES = 48.0;

        public List<int> playerRow;
        public List<int> computerRow;

        public bool gameOver { get; set; } 
        public bool landedInMancala { get; set; }

        public Board()
        {
            playerRow = new List<int>();
            computerRow = new List<int>();
            InitializeBoard();

            gameOver = false;
            landedInMancala = false;
        }

        public Board(List<int> playerRow, List<int> computerRow)
        {
            this.playerRow = new List<int>();
            this.computerRow = new List<int>();

            for (int i = 0; i < playerRow.Count; i++)
            {
                this.playerRow.Add(playerRow[i]);
                this.computerRow.Add(computerRow[i]);
            }

            gameOver = false;
            landedInMancala = false;
        }

        /// <summary>
        /// Initialize board with starting values : 4 4 4 4 4 4 0
        /// </summary>
        private void InitializeBoard()
        {
            int startingValue = (int) TOTAL_MARBLES / NUM_CUPS;
            for (int i = 0; i < NUM_SPOTS; i++)
            {
                playerRow.Add(startingValue);
                computerRow.Add(startingValue);
            }
            playerRow.Add(0);
            computerRow.Add(0);
        }

        /// <summary>
        /// Display the board (computer row displays backwards to create parallel game)
        /// </summary>
        public void DisplayBoard()
        {
            Console.WriteLine("    6 5 4 3 2 1");

            Console.WriteLine("-------------------");
            Console.Write("|  |");
            for (int i = 5; i >= 0; i--)
            {
                Console.Write(computerRow[i] + "|");
            }

            Console.Write("  |");
            Console.WriteLine();
            Console.WriteLine(" " + computerRow[CUP_INDEX_VAL] + " ------------- " + playerRow[CUP_INDEX_VAL]);


            Console.Write("|  |");
            for (int i = 0; i < NUM_SPOTS; i++)
            {
                Console.Write(playerRow[i] + "|");
            }
            Console.Write("  |");
            Console.WriteLine();
            Console.WriteLine("-------------------");
            Console.WriteLine("    1 2 3 4 5 6");
        }

        /// <summary>
        /// Method to check if the move the current Player wants to make is valid,
        ///         validity is determined if spot contain more than zero marbles
        /// </summary>
        /// <param name="player">The player enum (PLAYER or COMPUTER) of the current player</param>
        /// <param name="spot">The spot the player wants to take marbles from</param>
        /// <returns>bool, whether or not the spot is a valid move for the current Player's row</returns>
        public bool IsValidMove(Player player, int spot)
        {
            bool retVal;
            if (player == Player.PLAYER)
            {
                retVal = (playerRow[spot] > 0);
            }
            else
            {
                retVal = (computerRow[spot] > 0);
            }
            return retVal;
        }

        /// <summary>
        /// Returns a dictionary containing each next possible board and the spot the Player enum would land on to achieve that board
        /// </summary>
        /// <param name="player">The current Player enum (Player.PLAYER or Player.COMPUTER)</param>
        /// <returns>Dictionary containing the next possible board and the spot used to create the board</returns>
        public Dictionary<Board, int> GetPossibleNextBoardsAndTheSpotToCreateThem(Player player) 
        {
            Dictionary<Board, int> childrenAndSpots = new Dictionary< Board, int>();

            for (int spot = 0; spot < NUM_SPOTS; spot++)
            {
                if (this.IsValidMove(player, spot))
                {
                    Board nextPos = this.MakeMove(player, spot, false);

                    // update nextPos board with game over move if the next position results in game over
                    if (nextPos.AllSpotsAreEmpty(Player.COMPUTER))
                    {
                        nextPos.GameOverMove(nextPos, Player.COMPUTER, false);
                    }
                    else if (nextPos.AllSpotsAreEmpty(Player.PLAYER))
                    {
                        nextPos.GameOverMove(nextPos, Player.PLAYER, false);
                    }

                    childrenAndSpots.Add(nextPos, spot);
                }
            }

            return childrenAndSpots;
        }

        /// <summary>
        /// Method to actually play the move, calls DiminishPool() method until all marbles in the spot selected have been re-distributed
        /// </summary>
        /// <param name="player">The current Player enum (PLAYER or COMPUTER) making the move</param>
        /// <param name="move">the spot the player wants to take marbles from</param>
        /// <param name="isActualMove">bool, if set to false, avoids unneccessary Console.WriteLines</param>
        /// <returns>new Board object, with the updated player and computer rows post-move</returns>
        public Board MakeMove(Player player, int move, bool isActualMove)
        {
            List<int> currPlayer;
            List<int> opposingPlayer;

            Board newBoard = new Board(playerRow, computerRow);
            GetPlayerListsFromCurrentPlayer(player, out currPlayer, out opposingPlayer, newBoard); 

            int pool = currPlayer[move];
            if (isActualMove)
            {
                Console.WriteLine("current pool: " + pool);
            }

            currPlayer[move] = 0;


            pool = newBoard.DiminishPool(newBoard, currPlayer, opposingPlayer, pool, move, isActualMove);

            while (pool != 0)
            {
                pool = newBoard.DiminishPool(newBoard, currPlayer, opposingPlayer, pool, -1, isActualMove);
            }

            return newBoard;
        }

        /// <summary>
        /// Internal method to diminish the "pool" of marbles from the spot chosen by the Player
        /// </summary>
        /// <param name="newBoard">The new Board object</param>
        /// <param name="currPlayerList">The row of the player making the move</param>
        /// <param name="otherPlayerList">The row of the opposing player</param>
        /// <param name="pool">The number of marbles in the spot chosen by the Player</param>
        /// <param name="move">The spot chosen by the Player</param>
        /// <param name="isActualMove">bool, if set to false, avoids unneccessary Console.WriteLines</param>
        /// <returns>int value of the pool after circling both sides of the board (currPlayer and otherPlayer) once
        ///          If pool empties before finishing currPlayer, returns before moving to other side</returns>
        private int DiminishPool(Board newBoard, List<int> currPlayerList, List<int> otherPlayerList, int pool, int move, bool isActualMove)
        {
            for (int i = move + 1; i < currPlayerList.Count; i++)
            {
                pool--;

                currPlayerList[i]++;

                if (pool == 0)
                {
                    LastSpotPlayed(newBoard, currPlayerList, otherPlayerList, isActualMove, i); 

                    return pool;  // makes a clean exit when last spot finishes on current player's side
                }
            }

            int initPool = pool;
            for (int i = 0; i < initPool; i++)
            {
                if (i == CUP_INDEX_VAL)
                {
                    break;
                }
                else
                {
                    pool--;
                    otherPlayerList[i]++;
                }
            }

            return pool;
        }

        /// <summary>
        /// If pool empties out before moving on to other player's row, this method is called.
        /// It contains the actions to be taken if the last spot lands in current player's row:
        ///     1) If lands in Mancala, Player will be allowed to take another turn
        ///     2) Otherwise, if lands in a formerly empty spot, and there are values in the opposing player's corresponding spot, 
        ///       both spots empty, and Player's Mancala takes all the values. 
        /// </summary>
        /// <param name="newBoard">the new Board object</param>
        /// <param name="currPlayerList">the row containg the current player's values</param>
        /// <param name="otherPlayerList">the other player's row</param>
        /// <param name="isActualMove">bool, if set to false, avoids unneccessary Console.WriteLines</param>
        /// <param name="i">the index of the landed spot in the current player's row</param>
        private void LastSpotPlayed(Board newBoard, List<int> currPlayerList, List<int> otherPlayerList, bool isActualMove, int i)
        {
            List<int> currentRow = (currPlayerList.Equals(newBoard.playerRow) ? newBoard.playerRow : newBoard.computerRow);

            Player who = (currPlayerList.Equals(newBoard.playerRow) ? Player.PLAYER : Player.COMPUTER);
            Player other = (who == Player.PLAYER ? Player.COMPUTER : Player.PLAYER);

            int correspondingValue = GetCorrespondingValue(i);

            if (i == CUP_INDEX_VAL && currPlayerList.Equals(currentRow))
            {
                if (!AllSpotsAreEmpty(currentRow))
                {
                    newBoard.landedInMancala = true;
                    if (isActualMove)
                    {
                        Console.WriteLine($"{who} landed in Mancala. Take another turn.");
                        DisplayBoard();
                    }
                }
            }
            else if (currPlayerList[i] == 1 && i != CUP_INDEX_VAL && otherPlayerList[correspondingValue] > 0)
            {
                if (isActualMove)
                {
                    Console.WriteLine($"taking from {other}'s " + (correspondingValue + 1) + 
                        " pile containing " + otherPlayerList[correspondingValue]);
                }
                currPlayerList[i] = 0;
                currPlayerList[CUP_INDEX_VAL] += 1 + otherPlayerList[correspondingValue];
                otherPlayerList[correspondingValue] = 0;

                if (AllSpotsAreEmpty(otherPlayerList) || AllSpotsAreEmpty(currPlayerList))
                {
                    newBoard.gameOver = true;
                }
            }
        }

        /// <summary>
        /// Internal method to calculate the corresponding spot of the current player's landed spot
        /// </summary>
        /// <param name="i">The index of the current player's landing spot</param>
        /// <returns>int, the index of the other player's corresponding spot</returns>
        private int GetCorrespondingValue(int i)
        {
            // corresponding rows are calculated by playerRowIndex + computerRowIndex = NUM_SPOTS - 1

            return (NUM_SPOTS - 1 - i);
        }

        /// <summary>
        /// Checks if all the spots in the Player's row are empty, to determine game over.
        /// </summary>
        /// <param name="player">The Player enum (PLAYER or COMPUTER) who's getting checked if all his cups are empty</param>
        /// <returns>bool, are all his spots empty or not</returns>
        public bool AllSpotsAreEmpty(Player player) 
        {
            bool retVal;
            if (player == Player.PLAYER)
            {
                retVal = AllSpotsAreEmpty(playerRow);
            }    
            else
            {
                retVal = AllSpotsAreEmpty(computerRow);
            }
            return retVal;
        }

        /// <summary>
        /// Internal method to calculate if all of the Player's cups are empty
        /// </summary>
        /// <param name="currPlayer">The row containing the values of the Player</param>
        /// <returns>bool, if all spots are empty or not</returns>
        private bool AllSpotsAreEmpty(List<int> currPlayer) 
        {
            bool allEmpty = true;

            for (int i = 0; i < NUM_SPOTS; i++)
            {
                if (currPlayer[i] != 0)
                {
                    allEmpty = false;
                    break;
                }
            }

            return allEmpty;
        }

        /// <summary>
        /// The move to make after game over, calls other GameOverMove() method
        /// </summary>
        /// <param name="board">the Board object</param>
        /// <param name="who">the Player who has emptied out all his cup values</param>
        /// <param name="isActualMove">bool, if set to false, avoids unneccessary Console.WriteLines</param>
        public void GameOverMove(Board board, Player who, bool isActualMove) 
        {

            List<int> currPlayer = (who == Player.PLAYER) ? board.playerRow : board.computerRow;
            List<int> opposingPlayer = (who == Player.PLAYER) ? board.computerRow : board.playerRow;

            GameOverMove(currPlayer, opposingPlayer, isActualMove); 

        }

        /// <summary>
        /// The move to make after game over -> The player that empties all cups gets all the values of the opposite side
        /// </summary>
        /// <param name="currPlayer">the row of the Player who emptied out all cups</param>
        /// <param name="opposingPlayer">the row of the opposing Player</param>
        /// <param name="isActualMove">bool, if set to false, avoids unneccessary Console.WriteLines</param>
        public void GameOverMove(List<int> currPlayer, List<int> opposingPlayer, bool isActualMove)
        {
            if (isActualMove)
            {
                DisplayBoard();
            }

            gameOver = true; 

            int toBeAdded = 0;

            for (int i = 0; i < NUM_SPOTS; i++)
            {
                toBeAdded += opposingPlayer[i];
                opposingPlayer[i] = 0;
            }

            currPlayer[CUP_INDEX_VAL] += toBeAdded;
        }

        /// <summary>
        /// Method to get the Heuristic value
        /// </summary>
        /// <param name="player">The current Player</param>
        /// <param name="opponent">The current opponent</param>
        /// <param name="board">The new Board object</param>
        /// <returns>double, the heuristic value</returns>
        public double HeuristicValue(Player player, Player opponent, Board board)
        {
            List<int> currPlayer, opposingPlayer;
            GetPlayerListsFromCurrentPlayer(player, out currPlayer, out opposingPlayer, board);

            double retVal;
            if (IsWinner(player, board) || currPlayer[CUP_INDEX_VAL] > TOTAL_MARBLES / 2)
            {
                retVal = 1.0;
            }
            else if (IsWinner(player, board) || opposingPlayer[CUP_INDEX_VAL] > TOTAL_MARBLES / 2)
            {
                retVal = -1.0;
            }
            else
            {
                retVal = CalculateHeuristic(currPlayer, opposingPlayer);
            }
            return retVal;
        }

        /// <summary>
        /// Internal method to calculate heuristic value if there is no clear winner
        /// Formula: The current player's Mancala minus the opposing player's Mancala divided by total number of marbles
        /// </summary>
        /// <param name="currPlayer">The current player's row</param>
        /// <param name="opposingPlayer">The opposing player's row</param>
        /// <returns>double, the Heuristic value</returns>
        private double CalculateHeuristic(List<int> currPlayer, List<int> opposingPlayer)
        {
            double heuristic = (currPlayer[CUP_INDEX_VAL] - opposingPlayer[CUP_INDEX_VAL]) / TOTAL_MARBLES;
            return Math.Round(heuristic, 3);
        }

        /// <summary>
        /// Method to determine if Player wins, given the board
        /// </summary>
        /// <param name="player">The current Player</param>
        /// <param name="board">The given Board</param>
        /// <returns>bool, whether player is a winner or not</returns>
        public bool IsWinner(Player player, Board board)
        {
            bool winner = false;
            List<int> currPlayer, opposingPlayer;
            GetPlayerListsFromCurrentPlayer(player, out currPlayer, out opposingPlayer, board);

            if (AllSpotsAreEmpty(player) && currPlayer[CUP_INDEX_VAL] > opposingPlayer[CUP_INDEX_VAL])
            {
                winner = true;
            }

            return winner;
        }


        /// <summary>
        /// Internal method to determine current player's row and opposing player's row, based on current Player enum (PLAYER or COMPUTER)
        /// </summary>
        /// <param name="player">The current Player</param>
        /// <param name="currPlayer">The current player's row</param>
        /// <param name="opposingPlayer">The opposing player's row</param>
        /// <param name="newBoard">the Board object</param>
        private void GetPlayerListsFromCurrentPlayer(Player player, out List<int> currPlayer, out List<int> opposingPlayer, Board newBoard)
        {
            if (player == Player.PLAYER)
            {
                currPlayer = newBoard.playerRow;
                opposingPlayer = newBoard.computerRow;
            }
            else // (player == Player.COMPUTER)
            {
                currPlayer = newBoard.computerRow;
                opposingPlayer = newBoard.playerRow;
            }
        }
    }
}
