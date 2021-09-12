using System;
using System.Collections.Generic;

namespace GridGames
{
    public class Grid
    {
        private uint maxGridLength = 200;

        private uint width;
        public uint Width { get { return width; } private set { width = Math.Clamp(value, 1, maxGridLength); } }
        private uint height;
        public uint Height { get { return height; } private set { height = Math.Clamp(value, 1, maxGridLength); } }

        public byte DefaultFieldValue { get; private set; }

        private byte[] gridArray;
        public byte[] GridArray
        {
            get { return gridArray; }
            private set { gridArray = value; }
        }

        public byte this[uint index]
        {
            get
            {
                return gridArray[index];
            }

            set
            {
                gridArray[index] = value;
                FieldChangedEvent?.Invoke(this, index);
                foreach (byte item in gridArray)
                {
                    if (item == DefaultFieldValue)
                        return;
                }

                GridIsFullEvent?.Invoke(this);
            }
        }

        //delegates
        public delegate void FieldChangedHandler(Grid senderGrid, uint fieldIndex);
        public delegate void GridIsFullHandler(Grid senderGrid);
        public delegate uint? GetField(uint fieldIndex);

        //events
        public event FieldChangedHandler FieldChangedEvent; //Event wont be triggered through ClearGrid()
        public event GridIsFullHandler GridIsFullEvent; //Event is triggered when the field does not contain any defaultFieldValues

        //Constructor
        public Grid(uint width, uint height, byte defaultFieldValue = default(byte))
        {
            Width = width;
            Height = height;
            DefaultFieldValue = defaultFieldValue;
            GridArray = new byte[width * height];
            ClearGrid();
        }

        //Writes the whole Grid to the defaultFieldValue
        public void ClearGrid()
        {
            for(int i = 0; i < GridArray.Length; i++)
            {
                gridArray[i] = DefaultFieldValue;
            }
        }

        //Methods return the fieldIndex beside the given input Index
        public uint? GetUpperField(uint fieldIndex)
        {
            uint newIndex = fieldIndex - Width;
            if(newIndex < GridArray.Length && newIndex >= 0)
            {
                return newIndex;
            }
            return null;
        }
        
        public uint? GetLowerField(uint fieldIndex)
        {
            uint newIndex = fieldIndex + Width;
            if(newIndex < GridArray.Length && newIndex >= 0)
            {
                return newIndex;
            }
            return null;
        }

        public uint? GetLeftField(uint fieldIndex)
        {
            long newIndex = fieldIndex - 1;
            if (newIndex < GridArray.Length && newIndex >= 0)
            {
                long tempIndex = Width - 1;
                while (true)
                {
                    if (fieldIndex <= tempIndex)
                    {
                        break;
                    }
                    tempIndex += Width;
                }
                if(newIndex > tempIndex - Width && newIndex <= tempIndex)
                {
                    return (uint)newIndex;
                }
            }
            return null;
        }

        public uint? GetRightField(uint fieldIndex)
        {
            long newIndex = fieldIndex + 1;
            if (newIndex < GridArray.Length && newIndex >= 0)
            {
                long tempIndex = Width - 1;
                while (true)
                {
                    if (fieldIndex <= tempIndex)
                    {
                        break;
                    }
                    tempIndex += Width;
                }
                if (newIndex >= tempIndex - Width && newIndex <= tempIndex)
                {
                    return (uint)newIndex;
                }
            }
            return null;
        }

        private uint? GetCombinedField(uint fieldIndex, GetField firstField, GetField secondField)
        {
            uint? firstFieldIndex = firstField(fieldIndex);
            uint? secondFieldIndex = null;
            if (firstFieldIndex != null)
                secondFieldIndex = secondField(firstFieldIndex ?? default(int));

            if (secondFieldIndex != null)
                return secondFieldIndex;

            return null;
        }

        public uint? GetUpperLeftField(uint fieldIndex)
        {
            return GetCombinedField(fieldIndex, GetUpperField, GetLeftField);
        }

        public uint? GetUpperRightField(uint fieldIndex)
        {
            return GetCombinedField(fieldIndex, GetUpperField, GetRightField);
        }

        public uint? GetLowerLeftField(uint fieldIndex)
        {
            return GetCombinedField(fieldIndex, GetLowerField, GetLeftField);
        }

        public uint? GetLowerRightField(uint fieldIndex)
        {
            return GetCombinedField(fieldIndex, GetLowerField, GetRightField);
        }
        
        //Creates a sorted list(with the indexes of the line) starting from the firstFieldDirection to the secondFieldDirection
        public List<uint> GetLine(uint fieldIndex, GetField firstField, GetField secondField)
        {
            uint? tempFieldIndex = fieldIndex;
            uint movingIndex = fieldIndex;
            List<uint> lane = new List<uint>();
            while(true)
            {
                tempFieldIndex = firstField(movingIndex);
                if (tempFieldIndex == null)
                    break;
                movingIndex = tempFieldIndex ?? default(int);
                lane.Insert(0, movingIndex);
            }
            lane.Add(fieldIndex);
            movingIndex = fieldIndex;
            while(true)
            {
                tempFieldIndex = secondField(movingIndex);
                if (tempFieldIndex == null)
                    break;
                movingIndex = tempFieldIndex ?? default(int);
                lane.Add(movingIndex);
            }

            return lane;
        }

        //returns all possible lines from a given fieldIndex
        public List<uint>[] GetAllLanes(uint fieldIndex)
        {
            List<uint>[] lanes = new List<uint>[4];

            lanes[0] = GetLine(fieldIndex, GetLeftField, GetRightField);
            lanes[1] = GetLine(fieldIndex, GetUpperField, GetLowerField);
            lanes[2] = GetLine(fieldIndex, GetLowerLeftField, GetUpperRightField);
            lanes[3] = GetLine(fieldIndex, GetUpperLeftField, GetLowerRightField);

            return lanes;
        }
    }
}
