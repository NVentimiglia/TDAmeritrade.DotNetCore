using System;
using System.IO;
using System.Runtime.Serialization;

namespace TDAmeritrade.Serialization
{
    /// <summary>
    /// Binary formatter.
    /// Serialize and Deserialize binary representations.
    /// </summary>
    public class BinarySerializer
	{

		#region Fields

		/// <summary>
		/// The surrogate selector.
		/// </summary>
		protected ISurrogateSelector m_SurrogateSelector;

		/// <summary>
		/// The context.
		/// </summary>
		protected StreamingContext m_Context;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the surrogate selector.
		/// </summary>
		/// <value>The surrogate selector.</value>
		public virtual ISurrogateSelector surrogateSelector
		{
			get
			{
				return m_SurrogateSelector;
			}
			set
			{
				m_SurrogateSelector = value;
			}
		}

		/// <summary>
		/// Gets or sets the context.
		/// </summary>
		/// <value>The context.</value>
		public virtual StreamingContext context
		{
			get
			{
				return m_Context;
			}
			set
			{
				m_Context = value;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="BayatGames.Serialization.Formatters.Json.JsonFormatter"/> class.
		/// </summary>
		public BinarySerializer () : this ( null, new StreamingContext ( StreamingContextStates.All ) )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BayatGames.Serialization.Formatters.Json.JsonFormatter"/> class.
		/// </summary>
		/// <param name="selector">Selector.</param>
		/// <param name="context">Context.</param>
		public BinarySerializer ( ISurrogateSelector selector, StreamingContext context )
		{
			m_SurrogateSelector = selector;
			m_Context = context;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Serialize the specified value.
		/// </summary>
		/// <param name="value">Value.</param>
		public byte[] Serialize ( object value )
		{
			using ( MemoryStream output = new MemoryStream () )
			{
				Serialize ( output, value );
				return output.ToArray ();
			}
		}

		/// <summary>
		/// Serialize the specified output and value.
		/// </summary>
		/// <param name="output">Output.</param>
		/// <param name="value">Value.</param>
		public void Serialize ( Stream output, object value )
		{
			using ( BinaryObjectWriter writer = new BinaryObjectWriter ( output, m_SurrogateSelector, m_Context ) )
			{
				writer.Write ( value );
			}
		}

		/// <summary>
		/// Deserialize the specified input.
		/// </summary>
		/// <param name="input">Input.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T Deserialize<T> ( Stream input )
		{
			return ( T )Deserialize ( input, typeof ( T ) );
		}

		/// <summary>
		/// Deserialize the specified buffer.
		/// </summary>
		/// <param name="buffer">Buffer.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T Deserialize<T> ( byte [] buffer )
		{
			return ( T )Deserialize ( buffer, typeof ( T ) );
		}

		/// <summary>
		/// Deserialize the specified buffer and type.
		/// </summary>
		/// <param name="buffer">Buffer.</param>
		/// <param name="type">Type.</param>
		public object Deserialize ( byte [] buffer, Type type )
		{
			using ( MemoryStream input = new MemoryStream ( buffer ) )
			{
				return Deserialize ( input, type );
			}
		}

		/// <summary>
		/// Deserialize the specified input.
		/// </summary>
		/// <param name="input">Input.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public object Deserialize ( Stream input, Type type )
		{
			using ( BinaryObjectReader reader = new BinaryObjectReader ( input, m_SurrogateSelector, m_Context ) )
			{
				return reader.Read ( type );
			}
		}

		/// <summary>
		/// Serializes the object.
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="value">Value.</param>
		public static byte[] SerializeObject ( object value )
		{
			BinarySerializer formatter = new BinarySerializer ();
			return formatter.Serialize ( value );
		}

		/// <summary>
		/// Serializes the object.
		/// </summary>
		/// <param name="output">Output.</param>
		/// <param name="value">Value.</param>
		public static void SerializeObject ( Stream output, object value )
		{
			BinarySerializer formatter = new BinarySerializer ();
			formatter.Serialize ( output, value );
		}

		/// <summary>
		/// Deserializes the object.
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="buffer">Buffer.</param>
		/// <param name="type">Type.</param>
		public static object DeserializeObject ( byte [] buffer, Type type )
		{
			BinarySerializer formatter = new BinarySerializer ();
			return formatter.Deserialize ( buffer, type );
		}

		/// <summary>
		/// Deserializes the object.
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="buffer">Buffer.</param>
		public static TModel DeserializeObject<TModel>(byte[] buffer)
		{
			BinarySerializer formatter = new BinarySerializer();
			return (TModel)formatter.Deserialize(buffer, typeof(TModel));
		}

		/// <summary>
		/// Deserializes the object.
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="input">Input.</param>
		/// <param name="type">Type.</param>
		public static object DeserializeObject ( Stream input, Type type )
		{
			BinarySerializer formatter = new BinarySerializer ();
			return formatter.Deserialize ( input, type );
		}

		/// <summary>
		/// Deserializes the object.
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="input">Input.</param>
		public static TModel DeserializeObject<TModel>(Stream input)
		{
			BinarySerializer formatter = new BinarySerializer();
			return (TModel)formatter.Deserialize(input, typeof(TModel));
		}

		#endregion

	}
}