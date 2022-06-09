using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MancalaGame
{
    class AlphaBeta
    {
        /// <summary>
        /// Recursive method to get the values for the AlphaBeta optimization of the Minimax (Game) Tree
        /// </summary>
        /// <param name="board">The starting Board object</param>
        /// <param name="depth">The amount of levels to go down in the Minimax (Game) Tree</param>
        /// <param name="alfa">The best value found for the Maximizer</param>
        /// <param name="beta">The best value found for the Minimizer</param>
        /// <param name="player">The initial Max Player (before getting called recursively, Player.COMPUTER is always the Max player)</param>
        /// <returns>double, the value</returns>

        public static double Value(Board board, int depth, double alfa, double beta, Player player)
        {

            Player opponent = player == Player.COMPUTER ? Player.PLAYER : Player.COMPUTER;

            double value;
            if (depth == 0)
            {
                value = board.HeuristicValue(player, opponent, board);
            }
            else if (board.IsWinner(opponent, board))
            {
                value = player == Player.COMPUTER ? Program.MIN_VALUE : Program.MAX_VALUE;
            }
            else
            {
                if (player == Player.COMPUTER)
                {
                    for (int spot = 0; spot < Board.NUM_SPOTS; spot++)
                    {
                        if (board.IsValidMove(player, spot))
                        {
                            Board nextPos = GetNextPositionBoard(board, player, opponent, spot);
                            double thisVal = Value(nextPos, depth - 1, alfa, beta, opponent);
                            if (thisVal > alfa)
                            {
                                alfa = thisVal;
                            }
                            if (beta <= alfa)
                            {
                                break;
                            }
                        }
                    }
                    value = alfa;
                }
                else  // player == Player.MIN
                {
                    for (int spot = 0; spot < Board.NUM_SPOTS; spot++)
                    {
                        if (board.IsValidMove(player, spot))
                        {
                            Board nextPos = GetNextPositionBoard(board, player, opponent, spot);
                            double thisVal = Value(nextPos, depth - 1, alfa, beta, opponent);
                            if (thisVal < beta)
                            {
                                beta = thisVal;
                            }
                            if (beta <= alfa)
                            {
                                break;
                            }
                        }
                    }
                    value = beta;
                }
            }
            return value;
        }

        /// <summary>
        /// Internal method to get the next position board after making a move
        /// </summary>
        /// <param name="board">The Board object</param>
        /// <param name="player">The current Player</param>
        /// <param name="opponent">The current opponent</param>
        /// <param name="spot">The current spot being played</param>
        /// <returns>a new Board object, containing the next position after making a move</returns>
        private static Board GetNextPositionBoard(Board board, Player player, Player opponent, int spot)
        {
            // MakeMove returns a new version of the board after this player makes a move at spot
            Board nextPos = board.MakeMove(player, spot, false);

            // update nextPos board with game over move if the next position results in a player clearing all spots
            if (nextPos.AllSpotsAreEmpty(player))
            {
                nextPos.GameOverMove(nextPos, player, false);
            }
            else if (nextPos.AllSpotsAreEmpty(opponent))
            {
                nextPos.GameOverMove(nextPos, opponent, false);
            }

            return nextPos;
        }



    }

}
