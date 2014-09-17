using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 3 dimensional list implementation.
/// Gets used to store chunk data.
/// </summary>
public class List3D<T>
{

	/// <summary>
	/// The list data.
	/// </summary>
	private Dictionary<ListIndex<int>, T> listData;

	public Dictionary<ListIndex<int>, T> listSource
	{
		get { return this.listData; }
	}

	public T this[ListIndex<int> index]
	{
		get { return this.listData[index]; }
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="List3D`1"/> class.
	/// </summary>
	public List3D()
	{
		this.listData = new Dictionary<ListIndex<int>, T> ();
	}

	/// <summary>
	/// Add the specified data at the position of x, y, z.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	/// <param name="data">Data.</param>
	public void Add(int x, int y, int z, T data)
	{
		ListIndex<int> indexObject = new ListIndex<int> (x, y, z);

		if (! this.listData.ContainsKey(indexObject))
		{
			this.listData.Add(indexObject, data);
		}
	}

	/// <summary>
	/// Get the specified x, y and z.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public T Get(int x, int y, int z)
	{
		return this.listData [new ListIndex<int> (x, y, z)];
	}

	/// <summary>
	/// Checks if the list contains the given x, y, z.
	/// </summary>
	/// <returns><c>true</c>, if key was containsed, <c>false</c> otherwise.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public bool ContainsKey(int x, int y, int z)
	{
		return this.listData.ContainsKey (new ListIndex<int> (x, y, z));
	}

	/// <summary>
	/// Remove the specified index.
	/// </summary>
	/// <param name="index">Index.</param>
	public void Remove(ListIndex<int> index)
	{
		this.listData.Remove (index);
	}
}

/// <summary>
/// The ListIndex class is a generic class for use as multi-dimensional list index key.
/// </summary>
public class ListIndex<T>
{
	public T x;
	public T y;
	public T z;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="List3D`1+ListIndex`1"/> class.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public ListIndex(T x, T y, T z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
	
	/// <summary>
	/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="List3D`1+ListIndex`1"/>.
	/// </summary>
	/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="List3D`1+ListIndex`1"/>.</param>
	/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
	/// <see cref="List3D`1+ListIndex`1"/>; otherwise, <c>false</c>.</returns>
	public override bool Equals(object obj)
	{
		ListIndex<T> other = obj as ListIndex<T>; 
		
		return this.x.Equals(other.x) && this.y.Equals(other.y) && this.z.Equals(other.z);
	}
	
	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 17;
			// Maybe nullity checks, if these are objects not primitives!
			hash = hash * 23 + this.x.GetHashCode();
			hash = hash * 23 + this.y.GetHashCode();
			hash = hash * 23 + this.z.GetHashCode();
			return hash;
		}
	}
}