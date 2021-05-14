using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;

namespace TDAmeritrade.Serialization
{

	/// <summary>
	/// Binary object writer.
	/// </summary>
	public class BinaryObjectWriter : IDisposable
	{

		#region Fields

		/// <summary>
		/// The writer.
		/// </summary>
		protected BinaryWriter m_Writer;

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
		/// Gets the writer.
		/// </summary>
		/// <value>The writer.</value>
		public virtual BinaryWriter writer
		{
			get
			{
				return m_Writer;
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
		/// Initializes a new instance of the <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectWriter"/> class.
		/// </summary>
		/// <param name="stream">Stream.</param>
		public BinaryObjectWriter ( Stream stream ) : this ( new BinaryWriter ( stream ), null, new StreamingContext ( StreamingContextStates.All ) )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectWriter"/> class.
		/// </summary>
		/// <param name="writer">Writer.</param>
		public BinaryObjectWriter ( BinaryWriter writer ) : this ( writer, null, new StreamingContext ( StreamingContextStates.All ) )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectWriter"/> class.
		/// </summary>
		/// <param name="stream">Stream.</param>
		/// <param name="selector">Selector.</param>
		/// <param name="context">Context.</param>
		public BinaryObjectWriter ( Stream stream, ISurrogateSelector selector, StreamingContext context ) : this ( new BinaryWriter ( stream ), selector, context )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectWriter"/> class.
		/// </summary>
		/// <param name="writer">Writer.</param>
		/// <param name="selector">Selector.</param>
		/// <param name="context">Context.</param>
		public BinaryObjectWriter ( BinaryWriter writer, ISurrogateSelector selector, StreamingContext context )
		{
			m_Writer = writer;
			m_SurrogateSelector = selector;
			m_Context = context;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Write the specified value.
		/// </summary>
		/// <param name="value">Value.</param>
		public virtual void Write ( object value )
		{
			if ( value == null )
			{
				m_Writer.Write ( 0 );
			}
			else
			{
				Type type = value.GetType ();
				if ( type.IsPrimitive || type == typeof ( string ) || type == typeof ( decimal ) )
				{
					if ( type == typeof ( string ) )
					{
						m_Writer.Write ( ( string )value );
					}
					else if ( type == typeof ( decimal ) )
					{
						m_Writer.Write ( ( decimal )value );
					}
					else if ( type == typeof ( short ) )
					{
						m_Writer.Write ( ( short )value );
					}
					else if ( type == typeof ( int ) )
					{
						m_Writer.Write ( ( int )value );
					}
					else if ( type == typeof ( long ) )
					{
						m_Writer.Write ( ( long )value );
					}
					else if ( type == typeof ( ushort ) )
					{
						m_Writer.Write ( ( ushort )value );
					}
					else if ( type == typeof ( uint ) )
					{
						m_Writer.Write ( ( uint )value );
					}
					else if ( type == typeof ( ulong ) )
					{
						m_Writer.Write ( ( ulong )value );
					}
					else if ( type == typeof ( double ) )
					{
						m_Writer.Write ( ( double )value );
					}
					else if ( type == typeof ( float ) )
					{
						m_Writer.Write ( ( float )value );
					}
					else if ( type == typeof ( byte ) )
					{
						m_Writer.Write ( ( byte )value );
					}
					else if ( type == typeof ( sbyte ) )
					{
						m_Writer.Write ( ( sbyte )value );
					}
					else if ( type == typeof ( char ) )
					{
						m_Writer.Write ( ( char )value );
					}
					else if ( type == typeof ( bool ) )
					{
						m_Writer.Write ( ( bool )value );
					}
				}
				else if ( type.IsEnum )
				{
					m_Writer.Write ( value.ToString () );
				}
				else if ( type == typeof ( DateTime ) )
				{
					m_Writer.Write ( ( ( DateTime )value ).ToBinary () );
				}
				else if ( type == typeof ( TimeSpan ) )
				{
					m_Writer.Write ( ( ( TimeSpan )value ).ToString () );
				}
				else if ( value is ISerializable )
				{
					SerializationInfo info = new SerializationInfo ( type, new FormatterConverter () );
					ISerializable serializable = value as ISerializable;
					serializable.GetObjectData ( info, m_Context );
					WriteSerializationInfo ( info );
				}
				else if ( type.IsSerializable && !type.IsArray )
				{
					WriteObject ( value, type );
				}
				else if ( type.IsArray )
				{
					Array array = value as Array;
					m_Writer.Write ( array.Rank );
					for ( int i = 0; i < array.Rank; i++ )
					{
						m_Writer.Write ( array.GetLength ( i ) );
					}
					int [] indices = new int[array.Rank];
					for ( int i = 0; i < array.Rank; i++ )
					{
						for ( int j = 0; j < array.GetLength ( i ); j++ )
						{
							indices [ i ] = j;
							Write ( array.GetValue ( indices ) );
						}
					}
				}
				else if ( value is IEnumerable && value is ICollection )
				{
					IEnumerable enumerable = value as IEnumerable;
					ICollection collection = value as ICollection;
					IEnumerator e = enumerable.GetEnumerator ();
					m_Writer.Write ( collection.Count );
					while ( e.MoveNext () )
					{
						Write ( e.Current );
					}
				}
				else if ( type.IsGenericType && type.GetGenericTypeDefinition () == typeof ( KeyValuePair<,> ) )
				{
					Write ( type.GetProperty ( "Key" ).GetValue ( value, BindingFlags.Default, null, null, null ) );
					Write ( type.GetProperty ( "Value" ).GetValue ( value, BindingFlags.Default, null, null, null ) );
				}
				else
				{
					ISerializationSurrogate surrogate = null;
					if ( m_SurrogateSelector != null )
					{
						ISurrogateSelector selector;
						surrogate = m_SurrogateSelector.GetSurrogate ( type, m_Context, out selector );
						if ( surrogate != null )
						{
							SerializationInfo info = new SerializationInfo ( type, new FormatterConverter () );
							surrogate.GetObjectData ( value, info, m_Context );
							WriteSerializationInfo ( info );
						}
					}
					if ( surrogate == null )
					{
						WriteObject ( value, type );
					}
				}
			}
		}

		/// <summary>
		/// Writes the object.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="type">Type.</param>
		protected virtual void WriteObject ( object value, Type type )
		{
			FieldInfo [] fields = type.GetFields ();
			PropertyInfo [] properties = type.GetProperties ();
			Write ( fields.Length );
			for ( int i = 0; i < fields.Length; i++ )
			{
				if ( fields [ i ].IsPublic && !fields [ i ].IsStatic && !fields [ i ].IsLiteral && !fields [ i ].IsNotSerialized )
				{
					var id = ((BinaryFieldAttribute)fields[i].GetCustomAttribute(typeof(BinaryFieldAttribute))).Index;
					Write (id);
					Write ( fields [ i ].GetValue ( value ) );
				}
			}
			Write ( properties.Length );
			for ( int i = 0; i < properties.Length; i++ )
			{
				if ( properties [ i ].CanRead && properties [ i ].CanWrite )
				{
					var id = ((BinaryFieldAttribute)properties[i].GetCustomAttribute(typeof(BinaryFieldAttribute))).Index;
					Write (id);
					Write ( properties [ i ].GetValue ( value, BindingFlags.Default, null, null, null ) );
				}
			}
		}

		/// <summary>
		/// Writes the serialization info.
		/// </summary>
		/// <param name="info">Info.</param>
		protected virtual void WriteSerializationInfo ( SerializationInfo info )
		{
			var e = info.GetEnumerator ();
			while ( e.MoveNext () )
			{
				Write ( e.Name );
				Write ( e.Value );
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the
		/// <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectWriter"/>. The <see cref="Dispose"/> method leaves the
		/// <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectWriter"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the
		/// <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectWriter"/> so the garbage collector can reclaim the memory
		/// that the <see cref="BayatGames.Serialization.Formatters.Binary.BinaryObjectWriter"/> was occupying.</remarks>
		public virtual void Dispose ()
		{
			if ( m_Writer != null )
			{
				m_Writer.Close ();
			}
		}

		#endregion
		
	}

}