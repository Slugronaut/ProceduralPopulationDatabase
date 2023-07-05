using UnityEngine;

namespace ProceduralPopulationDatabase
{

    /// <summary>
    /// 
    /// </summary>
    public class EmptyPopulationException : UnityEngine.UnityException
    {
        public EmptyPopulationException() : base("The resulting population from your query is empty.") { }
    }

    /// <summary>
    /// 
    /// </summary>
    public class InaccessabePopulationException : UnityException
    {
        readonly public int Id;
        readonly public IndexRange Range;
        public InaccessabePopulationException(int id, IndexRange range) : base($"The given id does not exist within the range of the node give. The id index is '{id}' but the range is from '{range.StartIndex}' to '{range.EndIndex}' inclusive.")
        {
            Id = id;
            Range = range;
        }

        public InaccessabePopulationException(int id) : base($"The given id does not exist within the range of the node give. The id index is '{id}'.")
        {
            Id = id;
        }
    }
}
