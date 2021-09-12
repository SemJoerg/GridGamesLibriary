using System;
using System.Collections.Generic;

namespace GridGames
{
    public class Game <PlayerMarkerType>
    {
        private Grid grid;
        public uint GridWidth { get { return grid.Width; } }
        public uint GridHeight { get { return grid.Height; } }
        private uint winnLineLength;
        public uint WinnLineLength 
        {
            get { return winnLineLength; }
            set
            {
                if(value == 0)
                {
                    winnLineLength = 1;
                }
                else
                {
                    winnLineLength = value;
                }
            }
        }
        public Player<PlayerMarkerType>[] Players { get; private set; }
        public Player<PlayerMarkerType> CurrentPlayer { get; private set; }
        public Player<PlayerMarkerType> NextPlayer { get; private set; }
        private int currentPlayerIndex = 1;

        public delegate void OutputGrid(PlayerMarkerType[] markerGrid, uint gridWidth, uint gridHeight, PlayerMarkerType changedFieldMarker, uint? changedFieldIndex = null);
        private OutputGrid outputGrid;
        public delegate void DetectedWinnerHandler(Game<PlayerMarkerType> sender, Player<PlayerMarkerType> winner, uint[] winningFields);
        public event DetectedWinnerHandler DetectedWinner;

        public Game(uint width, uint height, uint _winnLineLength, Player<PlayerMarkerType>[] players, Player<PlayerMarkerType> emptyPlayer, OutputGrid _outputGrid, Grid.GridIsFullHandler gridIsFull)
        {
            grid = new Grid(width, height, emptyPlayer.GridId);
            WinnLineLength = _winnLineLength;
            
            Players = new Player<PlayerMarkerType>[players.Length + 1];
            Players[0] = emptyPlayer;
            for(int i = 1; i < Players.Length; i++)
            {
                Players[i] = players[i - 1];
            }
            NextPlayer = Players[currentPlayerIndex];

            outputGrid = _outputGrid;
            grid.FieldChangedEvent += GridChangedDebug;
            grid.FieldChangedEvent += GridFieldChanged;
            grid.GridIsFullEvent += gridIsFull;
        }

        //printsout all possible lines of the changed fieldIndex
        private void GridChangedDebug(Grid senderGrid, uint fieldIndex)
        {
            List<uint>[] lanes = senderGrid.GetAllLanes(fieldIndex);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Lanes: ");
            foreach(List<uint> lane in lanes)
            {
                foreach(int item in lane)
                {
                    Console.Write(item);
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
            Console.ResetColor();
            Console.ReadKey();
        }
        
        //Checks for winner
        private void GridFieldChanged(Grid senderGrid, uint fieldIndex)
        {
            DoGridOutput(fieldIndex);
            List<uint>[] lanes = senderGrid.GetAllLanes(fieldIndex);
            
            foreach(List<uint> lane in lanes)
            {
                if (lane.Count < WinnLineLength)
                    continue;
                uint currenntIndex = 0;
                uint[] winningFields = new uint[WinnLineLength];
                Player<PlayerMarkerType> winner;
                foreach(uint gridIndex in lane)
                {
                    winner = Player<PlayerMarkerType>.GetPlayerByGridId(Players, grid[gridIndex]);
                    if (winningFields[currenntIndex] == grid[gridIndex] && grid[gridIndex] != grid.DefaultFieldValue)
                    {
                        currenntIndex++;
                        winningFields[currenntIndex] = grid[gridIndex];
                        if (currenntIndex + 1 >= WinnLineLength)
                        {
                            DetectedWinner?.Invoke(this, winner, winningFields);
                            break;
                        }
                    }
                    else
                    {
                        currenntIndex = 0;
                        winningFields = new uint[WinnLineLength];
                        winningFields[currenntIndex] = grid[gridIndex];

                        if (winningFields[currenntIndex] != grid.DefaultFieldValue && currenntIndex + 1 >= WinnLineLength)
                        {
                            DetectedWinner?.Invoke(this, winner, winningFields);
                            break;
                        }
                    }
                }
            }
        }

        //Convertes byte GridArray into a MarkerArray(char)
        private PlayerMarkerType[] GetConvertedGridArray()
        {
            PlayerMarkerType[] convertedGridArray = new PlayerMarkerType[grid.GridArray.Length];
            for(int i = 0; i < grid.GridArray.Length; i++)
            {
                convertedGridArray[i] = Player<PlayerMarkerType>.GetPlayerByGridId(Players, grid.GridArray[i]).Marker;
            }
            return convertedGridArray;
        }

        //starts the outputGrid (Method/delegate)
        public void DoGridOutput(uint? changedFieldIndex = null)
        {
            PlayerMarkerType changedFieldMarker = new Player<PlayerMarkerType>().Marker;
            if(changedFieldIndex != null)
            {
                changedFieldMarker = Player<PlayerMarkerType>.GetPlayerByGridId(Players, changedFieldIndex ?? Players[0].GridId).Marker;
            }

            outputGrid(GetConvertedGridArray(), grid.Width, grid.Height, changedFieldMarker, changedFieldIndex);
        }

        public void ResetGame()
        {
            grid.ClearGrid();
            currentPlayerIndex = 1;
            NextPlayer = Players[currentPlayerIndex];
            DoGridOutput();
        }

        //Tics the next field (for the currentPlayer) if Field is already taken by another Player the Method returns false (if Field was not taken true)
        public bool SetNextField(uint fieldIndex)
        {
            CurrentPlayer = Players[currentPlayerIndex];

            if (fieldIndex >= grid.GridArray.Length || fieldIndex < 0 || grid[fieldIndex] != grid.DefaultFieldValue)
                return false;

            if (currentPlayerIndex >= Players.Length - 1)
            {
                currentPlayerIndex = 1;
            }
            else
            {
                currentPlayerIndex++;
            }
            NextPlayer = Players[currentPlayerIndex];

            grid[fieldIndex] = CurrentPlayer.GridId;
            return true;
        }
        
    }
}
