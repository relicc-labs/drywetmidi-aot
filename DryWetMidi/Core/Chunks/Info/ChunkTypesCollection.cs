using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Collection of <see cref="ChunkType"/> objects which provide identity information of a chunk.
    /// </summary>
    public sealed class ChunkTypesCollection : IEnumerable<ChunkType>
    {
        #region Fields

        private readonly Dictionary<string, Func<MidiChunk>> _factories = new Dictionary<string, Func<MidiChunk>>();
        private readonly Dictionary<Type, string> _ids = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

        #endregion

        #region Methods

        /// <summary>
        /// Adds chunk type along with the corresponding ID.
        /// </summary>
        /// <param name="type">Type of chunk.</param>
        /// <param name="id">ID of chunk.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="type"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="id"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">Chunk type specified by <paramref name="type"/> and
        /// <paramref name="id"/> already exists in the <see cref="ChunksCollection"/>.</exception>
        public void Add(Type type, string id)
        {
            _ids.Add(type, id);
            _types.Add(id, type);
        }

        /// <summary>
        /// Adds a chunk type, its corresponding ID, and the factory used to create it while reading.
        /// </summary>
        /// <typeparam name="TChunk">Type of chunk.</typeparam>
        /// <param name="id">ID of chunk.</param>
        /// <param name="factory">Factory that creates a chunk before its content is read.</param>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> or <paramref name="factory"/> is
        /// <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Chunk type specified by <typeparamref name="TChunk"/> or
        /// <paramref name="id"/> already exists in this collection.</exception>
        public void Add<TChunk>(string id, Func<TChunk> factory)
            where TChunk : MidiChunk
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            Add(typeof(TChunk), id);
            _factories.Add(id, factory);
        }

        /// <summary>
        /// Gets the chunk type associated with the specified ID.
        /// </summary>
        /// <param name="id">ID of the chunk type to get.</param>
        /// <param name="type">When this method returns, contains the chunk type associated with
        /// the specified ID, if ID is found; otherwise, <c>null</c>. This parameter is passed
        /// uninitialized.</param>
        /// <returns><c>true</c> if the <see cref="ChunkTypesCollection"/> contains a chunk type with the
        /// specified ID; otherwise, <c>false</c>.</returns>
        public bool TryGetType(string id, out Type type)
        {
            return _types.TryGetValue(id, out type);
        }

        /// <summary>
        /// Gets the ID associated with the specified chunk type.
        /// </summary>
        /// <param name="type">Chunk type to get ID for.</param>
        /// <param name="id">When this method returns, contains the ID associated with the specified
        /// chunk type, if the type is found; otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the <see cref="ChunkTypesCollection"/> contains an ID for the
        /// specified chunk type; otherwise, <c>false</c>.</returns>
        public bool TryGetId(Type type, out string id)
        {
            return _ids.TryGetValue(type, out id);
        }

        /// <summary>
        /// Creates the custom chunk registered for reading with the specified ID.
        /// </summary>
        /// <param name="id">ID of the chunk to create.</param>
        /// <param name="chunk">The created chunk, or <c>null</c> when the ID is not registered.</param>
        /// <returns><c>true</c> when the ID is registered; otherwise, <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">The ID has no reading factory, or its factory returned
        /// <c>null</c>.</exception>
        internal bool TryCreate(string id, out MidiChunk chunk)
        {
            Type type;
            if (!_types.TryGetValue(id, out type))
            {
                chunk = null;
                return false;
            }

            Func<MidiChunk> factory;
            if (!_factories.TryGetValue(id, out factory))
                throw new InvalidOperationException($"Custom chunk type '{type}' has no reading factory.");

            chunk = factory();
            if (chunk == null)
                throw new InvalidOperationException($"The reading factory for custom chunk type '{type}' returned null.");

            return true;
        }

        #endregion

        #region IEnumerable<ChunkType>

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ChunkType> GetEnumerator()
        {
            return _ids.Select(kv => new ChunkType(kv.Key, kv.Value))
                               .GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
