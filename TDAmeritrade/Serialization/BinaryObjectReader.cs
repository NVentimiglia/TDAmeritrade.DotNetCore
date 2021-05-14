using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;
using System.Linq;

namespace TDAmeritrade.Serialization
{

	/// <summary>
	/// Binary object reader.
	/// </summary>
	public class BinaryObjectReader : IDisposable
	{

		#region Fields

		/// <summary>
		/// The reader.
		/// </summary>
		protected BinaryReader m_Reader;

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
		/// Gets the reader.
		/// </summary>
		/// <value>The reader.</value>
		public virtual BinaryReader reader
		{
			get
			{
				return m_Reader;
			}
		}

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
		/// Initializes a new instance of the <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectReader"/> class.
		/// </summary>
		/// <param name="stream">Stream.</param>
		public BinaryObjectReader ( Stream stream ) : this ( stream, null, new StreamingContext ( StreamingContextStates.All ) )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectReader"/> class.
		/// </summary>
		/// <param name="reader">Reader.</param>
		public BinaryObjectReader ( BinaryReader reader ) : this ( reader, null, new StreamingContext ( StreamingContextStates.All ) )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectReader"/> class.
		/// </summary>
		/// <param name="stream">Stream.</param>
		/// <param name="selector">Selector.</param>
		/// <param name="context">Context.</param>
		public BinaryObjectReader ( Stream stream, ISurrogateSelector selector, StreamingContext context ) : this ( new BinaryReader ( stream ), selector, context )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectReader"/> class.
		/// </summary>
		/// <param name="reader">Reader.</param>
		/// <param name="selector">Selector.</param>
		/// <param name="context">Context.</param>
		public BinaryObjectReader ( BinaryReader reader, ISurrogateSelector selector, StreamingContext context )
		{
			m_Reader = reader;
			m_SurrogateSelector = selector;
			m_Context = context;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Read this instance.
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public virtual T Read<T> ()
		{
			return ( T )Read ( typeof ( T ) );
		}

		/// <summary>
		/// Read the specified type.
		/// </summary>
		/// <param name="type">Type.</param>
		public virtual object Read ( Type type )
		{
			object result = null;
			if ( type == null )
			{
				result = null;
			}
			else if ( type.IsPrimitive || type == typeof ( string ) || type == typeof ( decimal ) )
			{
				if ( type == typeof ( string ) )
				{
					result = m_Reader.ReadString ();
				}
				else if ( type == typeof ( decimal ) )
				{
					result = m_Reader.ReadDecimal ();
				}
				else if ( type == typeof ( short ) )
				{
					result = m_Reader.ReadInt16 ();
				}
				else if ( type == typeof ( int ) )
				{
					result = m_Reader.ReadInt32 ();
				}
				else if ( type == typeof ( long ) )
				{
					result = m_Reader.ReadInt64 ();
				}
				else if ( type == typeof ( ushort ) )
				{
					result = m_Reader.ReadUInt16 ();
				}
				else if ( type == typeof ( uint ) )
				{
					result = m_Reader.ReadUInt32 ();
				}
				else if ( type == typeof ( ulong ) )
				{
					result = m_Reader.ReadUInt64 ();
				}
				else if ( type == typeof ( double ) )
				{
					result = m_Reader.ReadDouble ();
				}
				else if ( type == typeof ( float ) )
				{
					result = m_Reader.ReadSingle ();
				}
				else if ( type == typeof ( byte ) )
				{
					result = m_Reader.ReadByte ();
				}
				else if ( type == typeof ( sbyte ) )
				{
					result = m_Reader.ReadSByte ();
				}
				else if ( type == typeof ( char ) )
				{
					result = m_Reader.ReadChar ();
				}
				else if ( type == typeof ( bool ) )
				{
					result = m_Reader.ReadBoolean ();
				}
			}
			else if ( type.IsEnum )
			{
				result = Enum.Parse ( type, m_Reader.ReadString () );
			}
			else if ( type == typeof ( DateTime ) )
			{
				result = DateTime.FromBinary ( m_Reader.ReadInt64 () );
			}
			else if ( type == typeof ( TimeSpan ) )
			{
				result = TimeSpan.Parse ( m_Reader.ReadString () );
			}
			else if ( type.IsArray )
			{
				Type elementType = type.GetElementType ();
				int rank = m_Reader.ReadInt32 ();
				int [] lengths = new int[rank];
				for ( int i = 0; i < rank; i++ )
				{
					lengths [ i ] = m_Reader.ReadInt32 ();
				}
				Array array = Array.CreateInstance ( elementType, lengths );
				int [] indices = new int[rank];
				for ( int i = 0; i < rank; i++ )
				{
					for ( int j = 0; j < lengths [ i ]; j++ )
					{
						indices [ i ] = j;
						array.SetValue ( Read ( elementType ), indices );
					}
				}
				result = array;
			}
			else if ( type.IsGenericType && type.GetGenericTypeDefinition () == typeof ( KeyValuePair<,> ) )
			{
				PropertyInfo key = type.GetProperty ( "Key" );
				PropertyInfo value = type.GetProperty ( "Value" );
				key.SetValue ( result, Read ( key.PropertyType ), BindingFlags.Default, null, null, null );
				value.SetValue ( result, Read ( value.PropertyType ), BindingFlags.Default, null, null, null );
			}
			else if ( type.IsGenericType && type.GetGenericTypeDefinition () == typeof ( List<> ) )
			{
				Type [] genericArgs = type.GetGenericArguments ();
				IList list = ( IList )type.GetConstructor ( Type.EmptyTypes ).Invoke ( null );
				int length = m_Reader.ReadInt32 ();
				for ( int i = 0; i < length; i++ )
				{
					list.Add ( Read ( genericArgs [ 0 ] ) );
				}
				result = list;
			}
			else if ( type.IsGenericType && type.GetGenericTypeDefinition () == typeof ( Dictionary<,> ) )
			{
				Type [] genericArgs = type.GetGenericArguments ();
				IDictionary dictionary = ( IDictionary )type.GetConstructor ( Type.EmptyTypes ).Invoke ( null );
				int length = m_Reader.ReadInt32 ();
				for ( int i = 0; i < length; i++ )
				{
					dictionary.Add ( Read ( genericArgs [ 0 ] ), Read ( genericArgs [ 1 ] ) );
				}
				result = dictionary;
			}
			else
			{
				result = ReadObject ( type );
			}
			if ( result is IDeserializationCallback )
			{
				( result as IDeserializationCallback ).OnDeserialization ( this );
			}
			return result;
		}

		/// <summary>
		/// Reads the object.
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="type">Type.</param>
		protected virtual object ReadObject ( Type type )
		{
			object result = null;
			if ( type.IsValueType )
			{
				result = Activator.CreateInstance ( type );
			}
			else
			{
				result = FormatterServices.GetUninitializedObject ( type );
			}
			ISurrogateSelector selector = null;
			SerializationInfo info = null;
			ISerializationSurrogate surrogate = null;
			if ( m_SurrogateSelector != null )
			{
				surrogate = m_SurrogateSelector.GetSurrogate ( type, m_Context, out selector );
				if ( surrogate != null )
				{
					info = new SerializationInfo ( type, new FormatterConverter () );
				}
			}
			if ( result != null )
			{
				var properties = type.GetProperties();
				var propids = properties.Select(o => ((BinaryFieldAttribute)o.GetCustomAttribute(typeof(BinaryFieldAttribute))).Index).ToArray();
				var fields = type.GetFields();
				var fieldIds = fields.Select(o => ((BinaryFieldAttribute)o.GetCustomAttribute(typeof(BinaryFieldAttribute))).Index).ToArray();

				int length = m_Reader.ReadInt32 ();
				for ( int i = 0; i < length; i++ )
				{
					int id = m_Reader.ReadInt32();
					int index = Array.IndexOf(fieldIds, id);
					FieldInfo field = fields[index];
					if ( field != null )
					{
						if ( info != null )
						{
							info.AddValue (field.Name, Read ( field.FieldType ), field.FieldType );
						}
						else
						{
							field.SetValue ( result, Read ( field.FieldType ) );
						}
					}
				}
				length = m_Reader.ReadInt32 ();
				for ( int i = 0; i < length; i++ )
				{
					int id = m_Reader.ReadInt32();
					int index = Array.IndexOf(propids, id);
					PropertyInfo property = properties[index];
					if ( property != null )
					{
						if ( info != null )
						{
							info.AddValue (property.Name, Read ( property.PropertyType ), property.PropertyType );
						}
						else
						{
							property.SetValue ( result, Read ( property.PropertyType ), BindingFlags.Default, null, null, null );
						}
					}
				}
			}
			if ( surrogate != null )
			{
				surrogate.SetObjectData ( result, info, m_Context, selector );
			}
			return result;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the
		/// <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectReader"/>. The <see cref="Dispose"/> method leaves the
		/// <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectReader"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the
		/// <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectReader"/> so the garbage collector can reclaim the memory
		/// that the <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectReader"/> was occupying.</remarks>
		public virtual void Dispose ()
		{
			if ( m_Reader != null )
			{
				m_Reader.Close ();
			}
		}

		#endregion
		
	}

}