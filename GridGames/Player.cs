namespace GridGames
{
    public class Player<MarkerType>
    {
        public readonly bool emptyPlayer;
        public int Winns { get; set; }
        public string Name { get; set; }
        public byte GridId { get; set; }
        public MarkerType Marker { get; set; }


        //Creates a emptyPlayer (Is used for the defaultPlayer)
        public Player()
        {
            emptyPlayer = true;
            Winns = 0;
            Name = null;
            GridId = 0;
            Marker = default(MarkerType);
        }

        public Player(int gridId, MarkerType marker)
        {
            emptyPlayer = true;
            Winns = 0;
            Name = null;
            GridId = 0;
            Marker = marker;
        }

        //Non emptyPlayer
        public Player(string name, byte gridId, MarkerType marker)
        {
            emptyPlayer = false;
            Winns = 0;
            Name = name;
            GridId = gridId;
            Marker = marker;
        }

        

        static public Player<MarkerType> GetPlayerByGridId(Player<MarkerType>[] players, uint gridId)
        {
            foreach(Player<MarkerType> player in players)
            {
                if(player.GridId == gridId)
                {
                    return player;
                }
            }

            return new Player<MarkerType>(0, default(MarkerType));
        }
    }
}
