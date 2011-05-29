namespace Exolutio.ViewToolkit
{
	/// <summary>
	/// Type of figure that is drawn at the end 
	/// of <see cref="Connector"/>
	/// </summary>
	public enum EConnectorCapStyle
	{
		/// <summary>
		/// No ending figure
		/// </summary>
		Straight,
		/// <summary>
		/// Arrow figure
		/// </summary>
		Arrow,
		/// <summary>
		/// Full arrow figure (black border, black fill)
		/// </summary>
		FullArrow,
		/// <summary>
		/// Diamond fugure (black border, white fill)
		/// </summary>
		Diamond,
		/// <summary>
		/// Full diamond figure (black border, black fill)
		/// </summary>
		FullDiamond,
		/// <summary>
		/// Triangle figure (black border, white fill)
		/// </summary>
		Triangle
	}
}